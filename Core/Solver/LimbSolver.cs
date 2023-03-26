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

            Axis axis = GetAxis(ikChain);

            float rad = LawCosSSS(aLen, cLen, bLen);
            Quaternion rot = AimBone(bindPose, parentTransform, axis);
            rot = Quaternion.AngleAxis(-rad * Mathf.Rad2Deg, axis.left) * rot;
            pose.SetBoneModel(poseA.boneName, rot);


            rad = Mathf.PI - LawCosSSS(aLen, bLen, cLen);
            rot = AlignToParent(bindPose, poseB, pose.GetModelTransform(poseA));
            rot = Quaternion.AngleAxis(rad * Mathf.Rad2Deg, axis.left) * rot;
            pose.SetBoneModel(poseB.boneName, rot);



            Bone poseC = pose[_chain[2].boneName];

            if(poseC.parentName.Equals(poseB.boneName) == false)
            {
                /*Vector3 d = bindPose.GetModelTransform(poseC).position - bindPose.GetModelTransform(poseB).position;
                Quaternion r = pose.GetModelTransform(poseB).rotation * Quaternion.Inverse(bindPose.GetModelTransform(poseB).rotation);
                d = r * d;
                d += pose.GetModelTransform(poseB).position;
                pose.SetBoneModel(poseC.boneName, d);*/
                BoneTransform poseBModel = pose.GetModelTransform(_chain[1]);
                Vector3 test = poseBModel.TransformPoint(poseB.alternativeForward * bLen);
                pose.SetBoneModel(poseC.boneName, test);
            }

        }
    }
}
