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
			Axis targetAxis = GetAxis(ikChain);

			Axis chainAxis = _chain.axis;
			chainAxis.Rotate(bindPose.GetModelTransform(_chain.First()).rotation);
			Axis lastAxis = _chain.Last().axis;
			lastAxis.Rotate(bindPose.GetModelTransform(_chain.Last()).rotation);

			float flipLastUp = Mathf.Sign(Vector3.Dot(chainAxis.up, lastAxis.up));

			Axis endEffectorAxis = new Axis(ikChain.endEffector.direction, flipLastUp * ikChain.endEffector.twist);

			for (int i = 0; i < count - 1; i++)
			{

				float t = (i + 1) / (float)(count + 1);

				Vector3 forward = Vector3.Slerp(targetAxis.forward, endEffectorAxis.forward, t);
				Vector3 up = Vector3.Slerp(targetAxis.up, endEffectorAxis.up, t);

				BoneTransform bindLocal = bindPose.GetLocalTransform(_chain[i]);
				BoneTransform childTransform = parentTransform + bindLocal;

				Vector3 currentforward = childTransform.rotation * _chain.axis.forward;
				Quaternion rotation = Quaternion.FromToRotation(currentforward, forward) * childTransform.rotation;

				Vector3 currentTwist = rotation * _chain.axis.up;
				float twist = Vector3.SignedAngle(currentTwist, up, forward);
				rotation = Quaternion.AngleAxis(twist, forward) * rotation;

				pose.SetBoneModel(_chain[i].boneName, rotation);
				parentTransform = pose.GetModelTransform(_chain[i].boneName);
			}
		}
    }
}
