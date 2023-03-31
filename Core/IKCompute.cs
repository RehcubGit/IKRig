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
			ikPose.rootMotion = BoneTransform.zero;
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

			BoneTransform start = armature.currentPose.GetModelTransform(chain.First());
			BoneTransform end = armature.currentPose.GetModelTransform(chain.Last());

			Vector3 direction = end.position - start.position;
			float length = direction.magnitude;
			direction.Normalize();

			Vector3 jointDir = start.rotation * chain.alternativeUp;
			Axis axis = new Axis(direction, jointDir);

			ikChain.lengthScale = length / chain.length;
			ikChain.axis = axis;
			ikChain.direction = axis.forward;
			ikChain.jointDirection = axis.up;

			SimpleBone(armature, chain.Last(), ikChain.endEffector);
		}

		public static void SimpleBone(Armature armature, Bone bone, IKBone ikBone) => SimpleBone(armature, bone, ikBone, bone.alternativeUp, bone.alternativeForward);

		public static void SimpleBone(Armature armature, Bone bone, IKBone ikBone, Vector3 up, Vector3 forward)
		{
			BoneTransform bindModel = armature.bindPose.GetModelTransform(bone.boneName);
			BoneTransform poseModel = armature.currentPose.GetModelTransform(bone.boneName);

			Axis bindGlobalAxis = new Axis(forward, up);
			bindGlobalAxis.Rotate(bindModel.rotation);

			Axis axis = new Axis(forward, up);
			axis.Rotate(poseModel.rotation);

			ikBone.direction = axis.forward;
			ikBone.twist = axis.up;
			ikBone.axis = axis;
			ikBone.sourceAxis = bindGlobalAxis;
		}
	}
}
