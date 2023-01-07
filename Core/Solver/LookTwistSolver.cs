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
			int count = _chain.count - 1;
			Axis axis = GetAxis(ikChain);

			for (int i = 0; i <= count; i++)
			{
				BoneTransform bindLocal = bindPose.GetLocalTransform(_chain[i]);
				BoneTransform bindModel = bindPose.GetModelTransform(_chain[i]);

				float t = i / (float)count;// ** 2;

				//Vector3 ik_look = axis.forward;
				//Vector3 ik_twist = axis.up;
				//Vector3 ik_look = -ikChain.endEffector.direction;
				//Vector3 ik_twist = ikChain.endEffector.twist;
				Vector3 ik_look = Vector3.Slerp(axis.up, -ikChain.endEffector.direction, t);
				Vector3 ik_twist = Vector3.Slerp(axis.forward, ikChain.endEffector.twist, t);

				Quaternion q_inv = Quaternion.Inverse(bindModel.rotation);
				Vector3 alt_look = q_inv * _chain.alternativeUp;
				Vector3 alt_twist = q_inv * _chain.alternativeForward;

				BoneTransform childTransform = parentTransform + bindLocal;
				Vector3 now_look = childTransform.rotation * alt_look;

				Quaternion rot = Quaternion.FromToRotation(now_look, ik_look) * childTransform.rotation;
				Vector3 now_twist = rot * alt_twist;

				Quaternion q = Quaternion.FromToRotation(now_twist, ik_twist);
				rot = q * rot;

				rot = Quaternion.Inverse(parentTransform.rotation) * rot;

				pose.SetBoneLocal(_chain[i].boneName, rot);
				parentTransform += new BoneTransform(bindLocal.position, rot);
			}
		}
    }
}
