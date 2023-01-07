using System.Collections.Generic;
using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
    public class IKPose
    {
		public Vector3 rootMotion = Vector3.zero;
		public Vector3 deltaRootMotion = Vector3.zero;
        public IKHip hip;

        public IKChain spine;

        public IKChain leftLeg;
		public IKBone leftToe;
        public IKChain rightLeg;
		public IKBone rightToe;

		public IKBone leftShoulder;
        public IKChain leftArm;
		public IKBone rightShoulder;
        public IKChain rightArm;

		public IKBone head;

		public IKPose()
		{
			hip = new IKHip();

			spine = new IKChain();

			leftShoulder = new IKBone();
			leftArm = new IKChain();
			rightShoulder = new IKBone();
			rightArm = new IKChain();

			leftLeg = new IKChain();
			leftToe = new IKBone();
			rightLeg = new IKChain();
			rightToe = new IKBone();

			head = new IKBone();
		}

		public IKPose Copy()
		{
			IKPose newPose = new IKPose
			{
				rootMotion = rootMotion,
				deltaRootMotion = deltaRootMotion,

				hip = hip.Copy(),
				spine = spine.Copy(),

				leftShoulder = leftShoulder.Copy(),
				leftArm = leftArm.Copy(),
				rightShoulder = rightShoulder.Copy(),
				rightArm = rightArm.Copy(),

				leftLeg = leftLeg.Copy(),
				leftToe = leftToe.Copy(),
				rightLeg = rightLeg.Copy(),
				rightToe = rightToe.Copy(),

				head = head.Copy()
			};


			return newPose;
		}

        public IKPose Modify(Armature armature, IKAnimationData animationData) => Modify(armature, animationData.GetModifiers());

        public IKPose Modify(Armature armature, List<BoneConstraint> modifiers)
        {
			IKPose newPose = Copy();

			foreach (BoneConstraint constraint in modifiers)
			{
				if (constraint == null)
					continue;

				if (constraint.visible == false)
					continue;
				constraint.Apply(newPose, armature);
			}
			return newPose;
		}

		public IKPose Modify(Armature armature, BoneConstraint modifier)
        {
			IKPose newPose = Copy();
			modifier.Apply(newPose, armature);

			return newPose;
		}

		public Vector3 GetRootMotion(Armature armature, bool scaled = false)
        {
			Vector3 unscaledMotion = rootMotion;
			if (scaled == false)
				return unscaledMotion;

			Bone bind = armature.GetBones(SourceBone.HIP)[0];
			float heightScale = bind.model.position.y / hip.bindHeight;
			Vector3 scaledMotion = unscaledMotion * heightScale;
			return scaledMotion;
		}

		public Vector3 GetDeltaRootMotion(Armature armature, bool scaled = false)
        {
			Vector3 unscaledMotion = deltaRootMotion;
			if (scaled == false)
				return unscaledMotion;

			Bone bind = armature.GetBones(SourceBone.HIP)[0];
			float heightScale = bind.model.position.y / hip.bindHeight;
			Vector3 scaledMotion = unscaledMotion * heightScale;
			return scaledMotion;
		}

		public void ApplyPose(Armature armature)
		{
			ApplyHip(armature);

			ApplyChains(armature, armature.GetChains(SourceChain.SPINE), spine);

			ApplyChains(armature, armature.GetChains(SourceChain.LEG, SourceSide.LEFT), leftLeg);
			ApplyBones(armature, armature.GetBones(SourceBone.TOE, SourceSide.LEFT), leftToe);
			ApplyChains(armature, armature.GetChains(SourceChain.LEG, SourceSide.RIGHT), rightLeg);
			ApplyBones(armature, armature.GetBones(SourceBone.TOE, SourceSide.RIGHT), rightToe);

			ApplyBones(armature, armature.GetBones(SourceBone.SHOULDER, SourceSide.LEFT), leftShoulder);
			ApplyChains(armature, armature.GetChains(SourceChain.ARM, SourceSide.LEFT), leftArm);

			ApplyBones(armature, armature.GetBones(SourceBone.SHOULDER, SourceSide.RIGHT), rightShoulder);
			ApplyChains(armature, armature.GetChains(SourceChain.ARM, SourceSide.RIGHT), rightArm);

			ApplyBones(armature, armature.GetBones(SourceBone.HEAD), head);

			ApplyChains(armature, armature.GetChains(SourceChain.NONE), null);
		}

		public void ApplyHip(Armature armature)
		{
			Bone bind = armature.GetBones(SourceBone.HIP)[0];
			BoneTransform hip = CalculateHip(armature);

			BoneTransform bindLocal = armature.bindPose.GetLocalTransform(bind);
			BoneTransform bindModel = armature.bindPose.GetModelTransform(bind);

			BoneTransform parent = bindModel - bindLocal;
			BoneTransform local = parent - hip;

            if (armature.HasParent(bind))
			{
				BoneTransform poseParent = parent + (bindModel - hip);
				poseParent.position = this.hip.GetPosition(parent);
				armature.currentPose.SetBoneModel(bind.parentName, poseParent);
				return;
			}

            armature.currentPose.SetBoneLocal(bind.boneName, local);
            armature.currentPose.SetBoneModel(bind.boneName, hip);
		}

		public BoneTransform CalculateHip(Armature armature)
		{
			BoneTransform bindLocal = armature.bindPose.GetLocalTransform(SourceBone.HIP);
			BoneTransform bindModel = armature.bindPose.GetModelTransform(SourceBone.HIP);

			Quaternion childRotation = bindLocal.rotation;
			Vector3 alternativeLook = Vector3.forward;

			Quaternion inverseBind = Quaternion.Inverse(bindModel.rotation);
			alternativeLook = inverseBind * alternativeLook;

			Vector3 currentLook = childRotation * alternativeLook;

			//TODO: If the hip should be fixed to the oriantation in the rig than fix alt_look to Vector3.forward!!
			Quaternion finalRotation = Quaternion.FromToRotation(currentLook, hip.direction) * childRotation;
			
			if (hip.twist != 0) 
				finalRotation = Quaternion.AngleAxis(hip.twist, hip.direction) * finalRotation;
			Vector3 pos = hip.GetPosition(bindModel);

            return new BoneTransform(pos, finalRotation);
		}


		private void ApplyBones(Armature armature, Bone[] bones, IKBone ikBone)
		{
			foreach (Bone bone in bones)
			{
				bone.Solve(armature, ikBone);
			}
		}

		private void ApplyChain(Armature armature, Chain chain, IKChain ikChain)
		{
			if (chain.solver == null)
				return;

			if (ikChain == null)
				return;

			chain.Solve(armature, ikChain);

			if (ikChain.endEffector == null)
				return;

			chain.Last().Solve(armature, ikChain.endEffector);
        }

		private void ApplyChains(Armature armature, Chain[] chains, IKChain ikChain)
		{
            foreach (Chain chain in chains)
            {
				ApplyChain(armature, chain, ikChain);
            }
        }

		public static IKPose Lerp(IKPose from, IKPose to, float t)
		{
			IKPose newPose = new IKPose
			{
				rootMotion = Vector3.Lerp(from.rootMotion, to.rootMotion, t),
				deltaRootMotion = Vector3.Lerp(from.deltaRootMotion, to.deltaRootMotion, t),

				hip = IKHip.Lerp(from.hip, to.hip, t),
				spine = IKChain.Lerp(from.spine, to.spine, t),

				leftArm = IKChain.Lerp(from.leftArm, to.leftArm, t),
				rightArm = IKChain.Lerp(from.rightArm, to.rightArm, t),

				leftLeg = IKChain.Lerp(from.leftLeg, to.leftLeg, t),
				rightLeg = IKChain.Lerp(from.rightLeg, to.rightLeg, t),

				head = IKBone.Lerp(from.head, to.head, t)
			};

			return newPose;
		}

		public static IKPose Slerp(IKPose from, IKPose to, float t)
		{
			IKPose newPose = new IKPose
			{
				rootMotion = Vector3.Slerp(from.rootMotion, to.rootMotion, t),
				deltaRootMotion = Vector3.Slerp(from.deltaRootMotion, to.deltaRootMotion, t),

				hip = IKHip.Slerp(from.hip, to.hip, t),
				spine = IKChain.Slerp(from.spine, to.spine, t),

				leftArm = IKChain.Slerp(from.leftArm, to.leftArm, t),
				rightArm = IKChain.Slerp(from.rightArm, to.rightArm, t),

				leftLeg = IKChain.Slerp(from.leftLeg, to.leftLeg, t),
				rightLeg = IKChain.Slerp(from.rightLeg, to.rightLeg, t),

				head = IKBone.Slerp(from.head, to.head, t)
			};

			return newPose;
		}
	}
}
