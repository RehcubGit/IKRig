using UnityEngine;

namespace Rehcub 
{
    public class LookTwistSolver : Solver
    {
        public LookTwistSolver(Chain chain) : base(chain)
        {
        }

        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
		{
			int count = _chain.count;
			Axis axis = GetAxis(ikChain);

			for (int i = 1; i <= count; i++)
			{
				int index = i - 1;
				BoneTransform bindLocal = bindPose.GetLocalTransform(_chain[index]);
				BoneTransform bindModel = bindPose.GetModelTransform(_chain[index]);

				float t = i / (float)count;

				Vector3 look = Vector3.Slerp(axis.up, -ikChain.endEffector.direction, t);
				Vector3 twist = Vector3.Slerp(axis.forward, ikChain.endEffector.twist, t);

				Quaternion inverseBind = Quaternion.Inverse(bindModel.rotation);

				Vector3 alternativeLook = inverseBind * _chain.alternativeUp;
				Vector3 alternativeTwist = inverseBind * _chain.alternativeForward;

				BoneTransform childTransform = parentTransform + bindLocal;

				Vector3 currentLook = childTransform.rotation * alternativeLook;
				Quaternion rotation = Quaternion.FromToRotation(currentLook, look) * childTransform.rotation;

				Vector3 currentTwist = rotation * alternativeTwist;
				rotation = Quaternion.FromToRotation(currentTwist, twist) * rotation;

				rotation = Quaternion.Inverse(parentTransform.rotation) * rotation;

				pose.SetBoneLocal(_chain[index].boneName, rotation);
				parentTransform += new BoneTransform(bindLocal.position, rotation);
			}
		}
    }
}
