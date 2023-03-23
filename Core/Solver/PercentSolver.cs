using UnityEngine;

namespace Rehcub 
{
    public class PercentSolver : Solver
    {
        [SerializeField] private float _minLimit = -90;
        [SerializeField] private float _maxLimit;

        public float Percent { get => _percent; set => _percent = value; }
        [Range(0, 1)]
        [SerializeField] private float _percent;


        public PercentSolver(Chain chain) : base(chain)
        {
        }


        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
            BoneTransform bindLocal = bindPose.GetLocalTransform(_chain.First());

            Quaternion aimRotation = bindLocal.rotation;

            if (ikChain != null)
                aimRotation = AimBone(bindPose, parentTransform, GetAxis(ikChain));

            //float percent = Vector3.Dot(worldForward, axis.forward) * 0.5f + 0.5f;
            float angle = (_maxLimit - _minLimit) * _percent;

            Vector3 forward = _chain.alternativeForward;
            Vector3 up = _chain.alternativeUp;

            Axis axis = new Axis(forward, up);

            Quaternion q = Quaternion.AngleAxis(angle, axis.left);

            for (int i = 0; i < _chain.count; i++)
            {
                Quaternion rotation = Quaternion.identity;

                if (i == 0)
                {
                    rotation = aimRotation;
                    if (_chain.source == SourceChain.THUMB)
                    {
                        pose.SetBoneModel(_chain[i].boneName, aimRotation);
                        continue;
                    }
                }

                rotation = q * rotation;
                pose.SetBoneLocal(_chain[i].boneName, rotation);
            }
		}
    }
}
