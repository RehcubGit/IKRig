using UnityEngine;

namespace Rehcub 
{
    public class IKCompute
    {
		public static void Hip (Armature armature, IKPose ikPose)
		{
			Bone hip = armature.GetBones(SourceBone.HIP)[0];
			Bone animatedPoseBone = armature.currentPose[hip.boneName];
			BoneTransform bindModel = armature.bindPose.GetModelTransform(hip);

			//TODO: Save Inverse into the Rig Hip's Data.
			Quaternion inverseWorldRotation = Quaternion.Inverse(bindModel.rotation); 
			Vector3 bindBoneForward = inverseWorldRotation * Vector3.forward;
			Vector3 bindBoneUp = inverseWorldRotation * Vector3.up;

			Vector3 poseBoneForward = animatedPoseBone.model.rotation * bindBoneForward;
			Vector3 poseBoneUp = animatedPoseBone.model.rotation * bindBoneUp;

			Quaternion swingRotation = Quaternion.FromToRotation(Vector3.forward, poseBoneForward);
			Vector3 swingUp = swingRotation * Vector3.up;
			float twist = Vector3.Angle(swingUp, poseBoneUp);


			ikPose.hip.bindHeight = bindModel.position.y;
			ikPose.hip.movement = animatedPoseBone.model.position - bindModel.position;
			ikPose.rootMotion = Vector3.zero;
			ikPose.hip.direction = poseBoneForward;
			ikPose.hip.twist = 0f;

			// If Less the .01 Degree, dont bother twisting.
			if (twist <= (0.01f * Mathf.PI / 180))
				return;

			Vector3 swingLeft = Vector3.Cross(swingUp, poseBoneForward);
			if (Vector3.Dot(swingLeft, poseBoneUp) >= 0)
				twist = -twist;

			ikPose.hip.twist = twist;

		}

		public static void Chain(Armature armature, Chain chain, IKChain ikChain)
		{
			// Limb IK tends to be fairly easy to determine. What you need is the direction the end effector is in
			// relation to the beginning of the limb chain, like the Shoulder for an arm chain. After you have the
			// direction, you can use it to get the distance. Distance is important to create a scale value based on
			// the length of the chain. Since an arm or leg's length can vary between models, if you normalize the 
			// distance, it becomes easy to translate it to other models. The last bit of info is we need the direction
			// that the joint needs to point. In this example, we precompute the Quaternion Inverse Dir for each chain
			// based on the bind pose. We can transform that direction with the Animated rotation to give us where the
			// joint direction has moved to.

			Bone boneA = armature.currentPose[chain.First().boneName];
			Bone boneB = armature.currentPose[chain.Last().boneName];
			Vector3 ikDirection = boneB.model.position - boneA.model.position;
			float ikLength = ikDirection.magnitude;
			ikDirection.Normalize();

			ikChain.lengthScale = ikLength / chain.length;
			ikChain.direction = ikDirection;

			Vector3 jointDir = boneA.model.rotation * chain.alternativeUp;
			Vector3 leftDir = Vector3.Cross(jointDir.normalized, ikDirection).normalized;
			ikChain.jointDirection = Vector3.Cross(ikDirection, leftDir).normalized;

			SimpleBone(armature, chain.Last(), ikChain.endEffector);
		}

		public static void SimpleBone(Armature armature, Bone bone, IKBone ik) => SimpleBone(armature, bone, ik, Vector3.forward, Vector3.up);

		public static void SimpleBone(Armature armature, Bone bone, IKBone ik, Vector3 lookDirection, Vector3 twistDirection)
		{
			BoneTransform bindModel = armature.bindPose.GetModelTransform(bone.boneName);
			BoneTransform poseModel = armature.currentPose.GetModelTransform(bone.boneName);

			Quaternion inverseModelRotation = Quaternion.Inverse(bindModel.rotation);

			Vector3 look = inverseModelRotation * lookDirection;
			Vector3 twist = inverseModelRotation * twistDirection;

			look = poseModel.rotation * look;
			twist = poseModel.rotation * twist;

			ik.direction = look;
			ik.twist = twist;
		}
	}
}
