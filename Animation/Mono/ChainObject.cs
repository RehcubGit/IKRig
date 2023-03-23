using UnityEngine;

namespace Rehcub
{
    [ExecuteInEditMode]
    public class ChainObject : MonoBehaviour
    {
        [SerializeField] private IKRig _rig;

        [SerializeField] private SourceChain _sourceChain;
        [SerializeField] private SourceSide _sourceSide;

        [SerializeField] private Transform[] _chain;
        private float chainLength = 1;
        [SerializeField] private Transform _fixPoint;



        private void OnEnable()
        {
            //_rig.onPreApplyIkPose += ApplyIK;
            _rig.onPreApplyPose += Apply;

            chainLength = 0;
            for (int i = 1; i < _chain.Length; i++)
            {
                chainLength += Vector3.Distance(_chain[i].position, _chain[i - 1].position);
            }
        }

        private void OnDisable()
        {
            //_rig.onPreApplyIkPose -= ApplyIK;
            _rig.onPreApplyPose -= Apply;
        }

        private void Apply()
        {
            Chain chain = _rig.Armature.GetChains(_sourceChain, _sourceSide).First();

            Vector3 root = _rig.Armature.currentPose.GetModelTransform(SourceBone.HIP).position.SetY(0);

            Vector3 start = _rig.Armature.currentPose.GetModelTransform(chain.First()).position;
            Vector3 end = _rig.Armature.currentPose.GetModelTransform(chain.Last()).position;

            Vector3 direction = end - start;

            Quaternion q = Quaternion.FromToRotation(Vector3.down, direction);
            _chain.First().rotation = q;

            end = _fixPoint.position + direction + root;
            start = end - direction.normalized * chainLength;

            _chain.First().position = start;
        }

        private void ApplyIK(IKPose pose)
        {
            IKChain ikChain = GetIkChain(pose);
            Quaternion q = Quaternion.FromToRotation(Vector3.down, ikChain.direction);
            _chain.First().rotation = q;

            Vector3 direction = ikChain.direction * (chainLength * ikChain.lengthScale);
            Vector3 end = _fixPoint.position + direction;
            Vector3 start = end - ikChain.direction * chainLength;

            _chain.First().position = start;
        }

        private IKChain GetIkChain(IKPose pose)
        {
            switch (_sourceChain)
            {
                case SourceChain.LEG:
                    if (_sourceSide == SourceSide.LEFT)
                        return pose.leftLeg;
                    return pose.rightLeg;

                case SourceChain.ARM:
                    if (_sourceSide == SourceSide.LEFT)
                        return pose.leftArm;
                    return pose.rightArm;

                case SourceChain.SPINE:
                    return pose.spine;

                case SourceChain.NONE:
                default:
                    return null;
            }
        }


    }
}
