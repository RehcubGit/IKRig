using UnityEngine;

namespace Rehcub 
{
    //https://mechanicaldesign101.com/wp-content/uploads/2017/12/T1-Four-bar-Linkage-Analysis-revised.pdf
    //https://www.upet.ro/annals/mechanical/pdf/2010/Annals-Mechanical-Engineering-2010-a27.pdf
    [System.Serializable]
    public class ZigZagSolver : Solver
    {
        [SerializeField] private bool _cross;
        [Range(0f, 1f)]
        [SerializeField] private float _limit = 0.5f;


        public ZigZagSolver(Chain chain) : base(chain)
        {

        }

        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
            Axis axis = GetAxis(ikChain);
            float length = GetLength(ikChain);

            bool outOfReach = HandleOutOfReach(bindPose, pose, parentTransform, axis, length);
            if (outOfReach)
                return;

            BoneTransform bindALocal = bindPose.GetLocalTransform(_chain[0]);
            BoneTransform bindBLocal = bindPose.GetLocalTransform(_chain[1]);
            BoneTransform bindCLocal = bindPose.GetLocalTransform(_chain[2]);

            Bone poseA = pose[_chain[0].boneName];
            Bone poseB = pose[_chain[1].boneName];
            Bone poseC = pose[_chain[2].boneName];

            float r1 = poseA.length;
            float r2 = poseB.length;
            float r3 = poseC.length;
            float r4 = length;

            Vector2 limits = GetLimits(r1, r2, r3, r4);
            float rad = Mathf.Lerp(limits.x, limits.y, _limit);

            Vector2 p2 = new Vector2(-r1 * Mathf.Cos(rad), r1 * Mathf.Sin(rad));
            Vector2 p4 = Vector2.right * r4;

            float r12 = r1 * r1;
            float r22 = r2 * r2;
            float r32 = r3 * r3;
            float r42 = r4 * r4;

            float A = Mathf.Sin(rad);
            float B = (r4 / r1) + Mathf.Cos(rad);
            float C = (r4 / r3) * Mathf.Cos(rad) + ((r12 - r22 + r32 + r42) / (2f * r1 * r3));

            float A2 = A * A;
            float B2 = B * B;
            float C2 = C * C;

            float phi;
            if(_cross)
                phi = 2f * Mathf.Atan((A - Mathf.Sqrt(A2 + B2 - C2)) / (B + C));
            else
                phi = 2f * Mathf.Atan((A + Mathf.Sqrt(A2 + B2 - C2)) / (B + C));

            Vector2 p3 = new Vector2(-r3 * Mathf.Cos(phi), r3 * Mathf.Sin(phi)) + p4;
            float deg1 = -(Mathf.PI - rad) * Mathf.Rad2Deg;
            float deg2 = -Vector2.SignedAngle(p2, p3 - p2);
            float deg3 = -Vector2.SignedAngle(p3 - p2, p4 - p3);

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Bone A 
            Quaternion aimRotation = AimBone(bindPose, parentTransform, axis);

            Quaternion rot = Quaternion.AngleAxis(deg1, axis.left) * aimRotation;
            pose.SetBoneModel(poseA.boneName, rot);
            parentTransform = pose.GetModelTransform(poseA);

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Bone B

            rot = parentTransform.rotation * bindPose.GetLocalTransform(_chain[1]).rotation; //Get the model bind rotation
            // align to parent bone helpful when the chain is not straight in the bind pose, as normaly in quadrupeds
            rot = Quaternion.FromToRotation(rot * _chain[1].axis.forward, parentTransform.rotation * _chain[0].axis.forward) * rot; 
            rot = Quaternion.AngleAxis(deg2, axis.left) * rot; // rotate the computed ik angle
            pose.SetBoneModel(_chain[1].boneName, rot);
            parentTransform = pose.GetModelTransform(_chain[1]);

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Bone C

            rot = parentTransform.rotation * bindPose.GetLocalTransform(_chain[2]).rotation;
            rot = Quaternion.FromToRotation(rot * _chain[2].axis.forward, parentTransform.rotation * _chain[1].axis.forward) * rot;
            rot = Quaternion.AngleAxis(deg3, axis.left) * rot;
            pose.SetBoneModel(poseC.boneName, rot);
        }

        private Vector2 GetLimits(float r1, float r2, float r3, float r4)
        {
            float r12 = r1 * r1;
            float r42 = r4 * r4;
            float min = ((r2 - r3) * (r2 - r3) - r12 - r42) / (2 * r1 * r4);
            float max = ((r2 + r3) * (r2 + r3) - r12 - r42) / (2 * r1 * r4);

            min = Mathf.Acos(min);
            max = Mathf.Acos(max);

            if (float.IsNaN(max))
                max = 0;
            if (float.IsNaN(min))
                min = Mathf.PI;

            return new Vector2(min, max);
        }


        /*
        public override void Solve(Pose bindPose, Pose pose, BoneTransform parentTransform, Axis axis, float length)
        {
            //------------------------------------
            // Get the length of the bones, the calculate the ratio length for the bones based on the chain length
            // The 3 bones when placed in a zig-zag pattern creates a Parallelogram shape. We can break the shape down into two triangles
            // By using the ratio of the Target length divided between the 2 triangles, then using the first bone + half of the second bound
            // to solve for the top 2 joiints, then uing the half of the second bone + 3rd bone to solve for the bottom joint.
            // If all bones are equal length,  then we only need to use half of the target length and only test one triangle and use that for
            // both triangles, but if bones are uneven, then we need to solve an angle for each triangle which this function does.	

            //------------------------------------

            Bone bind_a = bindPose.bones[_chain[0].boneName];
            Bone bind_b = bindPose.bones[_chain[1].boneName];
            Bone bind_c = bindPose.bones[_chain[2].boneName];

            Bone pose_a = pose.bones[_chain[0].boneName];
            Bone pose_b = pose.bones[_chain[1].boneName];
            Bone pose_c = pose.bones[_chain[2].boneName];

            float a_len = bind_a.length;
            float b_len = bind_b.length;
            float c_len = bind_c.length;
            float bh_len = bind_b.length * 0.5f;

            float t_ratio = (a_len + bh_len) / (a_len + b_len + c_len);
            float ta_len = length * t_ratio;
            float tb_len = length - ta_len;

            Debug.Log($"Lengths: a {a_len}  b {b_len} c {c_len} bh {bh_len}");
            Debug.Log($"Lengths: ta {ta_len}  tb {tb_len}");


            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Bone A 
            Quaternion rot = AimBone(bindPose, parentTransform, axis);

            float rad = LawCosSSS(a_len, ta_len, bh_len);

            rot = Quaternion.AngleAxis(-rad * Mathf.Rad2Deg, axis.left) * rot;
            rot = Quaternion.Inverse(parentTransform.rotation) * rot;

            pose.SetBoneLocal(pose_a.boneName, rot);
            pose_a.model = parentTransform + new BoneTransform(bind_a.local.position, rot);

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Bone B
            rad = Mathf.PI - LawCosSSS(a_len, bh_len, ta_len);

            rot = pose_a.model.rotation * bind_b.local.rotation;
            rot = Quaternion.AngleAxis(rad * Mathf.Rad2Deg, axis.left) * rot;
            rot = Quaternion.Inverse(pose_a.model.rotation) * rot;

            Vector3 a = bind_b.model.position - bind_a.model.position;
            Vector3 b = bind_c.model.position - bind_b.model.position;
            Quaternion localRot = Quaternion.FromToRotation(a.normalized, b.normalized);
            localRot = Quaternion.Inverse(localRot);
            //FIX for non perfect tpose
            //rot *= localRot;

            pose.SetBoneLocal(pose_b.boneName, rot);
            pose_b.model = pose_a.model + new BoneTransform(bind_b.local.position, rot);

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Bone C
            rad = Mathf.PI - LawCosSSS(c_len, b_len - bh_len, tb_len);

            //rot *= localRot;

            a = bind_c.model.position - bind_b.model.position;
            b = bindPose.bones[_chain[3].boneName].model.position - bind_c.model.position;
            localRot = Quaternion.FromToRotation(a.normalized, b.normalized);

            rot = pose_b.model.rotation * bind_c.local.rotation;
            //FIX for non perfect tpose
            rot = Quaternion.AngleAxis(-rad * Mathf.Rad2Deg, axis.left) * rot;
            rot = Quaternion.Inverse(pose_b.model.rotation) * rot;
            rot *= localRot;

            pose.SetBoneLocal(pose_c.boneName, rot);
            pose_c.model = pose_b.model + new BoneTransform(bind_c.local.position, rot);
        }*/

    }
}
