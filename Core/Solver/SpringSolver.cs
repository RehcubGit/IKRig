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
        [SerializeField] private Quaternion[] _rotation;
        [HideInInspector]
        [SerializeField] private Vector3[] _angularVelocity;

        public SpringSolver(Chain chain) : base(chain)
        {
            _rotation = new Quaternion[chain.count];
            _angularVelocity = new Vector3[chain.count];
            for (int i = 0; i < chain.count; i++)
            {
                _rotation[i] = Quaternion.identity;
                _angularVelocity[i] = Vector3.zero;
            }
        }

        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
            float dt = 1f / 30f;
            int iterations = 1;

            dt /= iterations;


            for (int i = 0; i < iterations; i++)
            {
                Quaternion partentRotation = parentTransform.rotation;
                for (int j = 0; j < _chain.count; j++)
                {
                    BoneTransform bindLocal = bindPose.GetLocalTransform(_chain[j]);

                    float stiffness = _stiffness + _stiffnessChange * j;
                    float damping = _damping + _dampingChange * j;

                    stiffness = Mathf.Clamp(stiffness, 0.1f, float.PositiveInfinity);
                    damping = Mathf.Clamp(damping, 0.1f, float.PositiveInfinity);



                    Quaternion current = _rotation[j];
                    Quaternion target = partentRotation * bindLocal.rotation;

                    //current = Quaternion.LookRotation(Vector3.SmoothDamp(current * Vector3.forward, target * Vector3.forward, ref _angularVelocity[j], stiffness));
                    //Quaternion target = bindPose.GetModelTransform(_chain[j]).rotation;

                    _angularVelocity[j] = SpringStep(_angularVelocity[j], current, target, dt, damping, stiffness);
                    current = current.Rotate(_angularVelocity[j], dt);

                    //Debug.Log($"{j} : {_angularVelocity[j]} : {target.eulerAngles}");

                    current.Normalize();
                    pose.SetBoneModel(_chain[j].boneName, current);
                    //target = Quaternion.Inverse(parentTransform.rotation) * Quaternion.LookRotation(Vector3.back, Vector3.down);

                    /*Quaternion rotation = SpringStep(poseModel.rotation, i, target, dt, damping, stiffness);

                    pose.SetBoneModel(bone.boneName, rotation);*/
                    partentRotation = target;
                    _rotation[j] = current;
                    //partentRotation = target;
                }
            }

        }

        private Vector3 SpringStep(Vector3 v, Quaternion rotation, Quaternion target, float dt, float damping = 5, float stiffness = 30)
        {
            float dot = Quaternion.Dot(rotation, target);
            if (dot >= 0.9999f && v.sqrMagnitude < 0.000001f)
            {
                return Vector3.zero;
            }

            if (dot < 0)
            {
                target = target.Negate();
            }
            /*rotation.ToAngleAxis(out float angle, out Vector3 axis);
            target.ToAngleAxis(out float angle2, out Vector3 axis2);

            Vector3 differance = (angle2 * axis2) - (angle * axis);*/

            (rotation * Quaternion.Inverse(target)).ToAngleAxis(out float angle, out Vector3 axis);
            //Quaternion.FromToRotation(rotation * Vector3.forward, target * Vector3.forward).ToAngleAxis(out float angle, out Vector3 axis);


            Vector3 differance = angle * axis;
            //Debug.Log(differance);

            v += (-stiffness * differance - damping * v) * dt;

            float dSqrDt = damping * damping * dt;
            float n2 = 1 + damping * dt;
            float n2Sqr = n2 * n2;

            v = (v - differance * dSqrDt) / n2Sqr;

            return v;
        }

        /*private Quaternion SpringStep(Quaternion rotation, int index, Quaternion target, float dt, float damping = 5, float stiffness = 30)
        {
            Vector4 v = _rotation[index];
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

            float dSqrDt = damping * damping * dt;
            float n2 = 1 + damping * dt;
            float n2Sqr = n2 * n2;

            v[0] = (v[0] - (rotation[0] - tq[0]) * dSqrDt) / n2Sqr;
            v[1] = (v[1] - (rotation[1] - tq[1]) * dSqrDt) / n2Sqr;
            v[2] = (v[2] - (rotation[2] - tq[2]) * dSqrDt) / n2Sqr;
            v[3] = (v[3] - (rotation[3] - tq[3]) * dSqrDt) / n2Sqr;

            //.........................................
            rotation[0] += v[0] * dt;
            rotation[1] += v[1] * dt;
            rotation[2] += v[2] * dt;
            rotation[3] += v[3] * dt;
            rotation.Normalize();
            _rotation[index] = v;

            return rotation;
        }*/
    }
}
