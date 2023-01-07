using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
    public class LimbSolver : Solver
    {
        public LimbSolver(Chain chain) : base(chain)
        {
        }

        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
            //Bone bindA = bindPose[_chain[0].boneName];
            //Bone bindB = bindPose[_chain[1].boneName];
            Bone poseA = pose[_chain[0].boneName];
            Bone poseB = pose[_chain[1].boneName];

            float aLen = poseA.length;
            float bLen = poseB.length;
            float cLen = GetLength(ikChain);
            float rad;

            Axis axis = GetAxis(ikChain);

            Quaternion rot = AimBone(bindPose, parentTransform, axis);

            rad = LawCosSSS(aLen, cLen, bLen);
            rot = Quaternion.AngleAxis(-rad * Mathf.Rad2Deg, axis.left) * rot;
            rot = Quaternion.Inverse(parentTransform.rotation) * rot;

            pose.SetBoneLocal(poseA.boneName, rot);

            Vector3 a = bindPose.GetModelTransform(_chain[1]).position - bindPose.GetModelTransform(_chain[0]).position;
            Vector3 b = bindPose.GetModelTransform(_chain[2]).position - bindPose.GetModelTransform(_chain[1]).position;
            Quaternion localRot = Quaternion.FromToRotation(a.normalized, b.normalized);

            if (bindPose.GetLocalTransform(_chain[1]).rotation != Quaternion.identity)
                localRot = bindPose.GetLocalTransform(_chain[1]).rotation;

            rad = Mathf.PI - LawCosSSS(aLen, bLen, cLen);
            rot = poseA.model.rotation * bindPose.GetLocalTransform(_chain[1]).rotation;
            rot = Quaternion.AngleAxis(rad * Mathf.Rad2Deg, axis.left) * rot;
            rot = Quaternion.Inverse(poseA.model.rotation) * rot;
            //FIX for non perfect tpose
            rot *= Quaternion.Inverse(localRot);
            //rot *= Quaternion.Inverse(bind_b.local.rotation); 

            pose.SetBoneLocal(poseB.boneName, rot);


            Bone poseC = pose[_chain[2].boneName];

            if(poseC.parentName.Equals(poseB.boneName) == false)
            {
                Vector3 d = bindPose.GetModelTransform(poseC).position - bindPose.GetModelTransform(poseB).position;
                Quaternion r = pose.GetModelTransform(poseB).rotation * Quaternion.Inverse(bindPose.GetModelTransform(poseB).rotation);
                d = r * d;
                d += pose.GetModelTransform(poseB).position;
                pose.SetBoneModel(poseC.boneName, d);
            }

        }
    }
}
