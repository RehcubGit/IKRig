using UnityEngine;

namespace Rehcub 
{
    public class BoneSolver : Solver
    {
        public BoneSolver(Chain chain) : base(chain)
        {
        }

        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
            Axis axis = GetAxis(ikChain);
            Quaternion rot = AimBone(bindPose, parentTransform, axis);
            pose.SetBoneModel(_chain.First().boneName, rot);
        }
    }
}
