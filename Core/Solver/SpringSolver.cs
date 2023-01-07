using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class SpringSolver : Solver
    {
        [Range(0, 100f)]
        public float _stiffness = 10f;
        [Range(-1f, 1f)]
        public float _stiffnessChange = -0.1f;

        [Range(0, 1f)]
        public float _damping = 0.95f;
        [Range(-1f, 1f)]
        public float _dampingChange = -0.05f;

        [HideInInspector]
        [SerializeField] private Vector4[] _velocitys;

        public SpringSolver(Chain chain) : base(chain)
        {
            _velocitys = new Vector4[chain.count];
            for (int i = 0; i < chain.count; i++)
            {
                _velocitys[i] = Vector4.zero;
            }
        }

        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
            float dt = 1f / 30f;
            for (int i = 0; i < _chain.count; i++)
            {
                Bone bone = _chain[i];
                BoneTransform bindLocal = bindPose.GetLocalTransform(bone);
                Bone bonePose = pose[bone.boneName];

                float s = _stiffness + _stiffnessChange * i;
                float d = _damping + _dampingChange * i;

                s = Mathf.Clamp(s, 0f, float.PositiveInfinity);
                d = Mathf.Clamp(d, 0f, float.PositiveInfinity);

                Quaternion target = parentTransform.rotation * bindLocal.rotation;

                //target = Quaternion.Inverse(parentTransform.rotation) * Quaternion.LookRotation(Vector3.back, Vector3.down);

                Quaternion q = bonePose.model.rotation;
                q = SpringStep(q, i, target, dt, d, s);
                q = Quaternion.Inverse(parentTransform.rotation) * q;

                pose.SetBoneLocal(bone.boneName, q);
                parentTransform = pose.GetModelTransform(bone);
            }
        }

        private Quaternion SpringStep(Quaternion rotation, int index, Quaternion target, float dt, float damping = 5, float stiffness = 30)
        {
            Vector4 v = _velocitys[index];
            float dot = Quaternion.Dot(rotation, target);
            if (dot >= 0.9999f && v.sqrMagnitude < 0.000001f)
            {
                return target;
            }

            Quaternion tq = target;

            if (dot < 0)
            {
                tq[0] = -target[0];
                tq[1] = -target[1];
                tq[2] = -target[2];
                tq[3] = -target[3];
            }

            v[0] += (-stiffness * (rotation[0] - tq[0]) - damping * v[0]) * dt;
            v[1] += (-stiffness * (rotation[1] - tq[1]) - damping * v[1]) * dt;
            v[2] += (-stiffness * (rotation[2] - tq[2]) - damping * v[2]) * dt;
            v[3] += (-stiffness * (rotation[3] - tq[3]) - damping * v[3]) * dt;

            /*float dSqrDt = damping * damping * dt;
            float n2 = 1 + damping * dt;
            float n2Sqr = n2 * n2;

            v[0] = (v[0] - (rotation[0] - tq[0]) * dSqrDt) / n2Sqr;
            v[1] = (v[1] - (rotation[1] - tq[1]) * dSqrDt) / n2Sqr;
            v[2] = (v[2] - (rotation[2] - tq[2]) * dSqrDt) / n2Sqr;
            v[3] = (v[3] - (rotation[3] - tq[3]) * dSqrDt) / n2Sqr;*/

            //.........................................
            rotation[0] += v[0] * dt;
            rotation[1] += v[1] * dt;
            rotation[2] += v[2] * dt;
            rotation[3] += v[3] * dt;
            rotation.Normalize();
            _velocitys[index] = v;

            return rotation;
        }
    }
}
