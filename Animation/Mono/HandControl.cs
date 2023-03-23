using UnityEngine;

namespace Rehcub 
{
    [ExecuteInEditMode]
    public class HandControl : MonoBehaviour
    {
        [SerializeField] private IKRig _rig;
        [SerializeField] private SourceSide _targetSide;
        private Transform _transform;

        [HideInInspector]
        [SerializeField] private BoneTransform _thumbTarget = BoneTransform.zero;
        [HideInInspector]
        [SerializeField] private BoneTransform _indexTarget = BoneTransform.zero;
        [HideInInspector]
        [SerializeField] private BoneTransform _middleTarget = BoneTransform.zero;
        [HideInInspector]
        [SerializeField] private BoneTransform _ringTarget = BoneTransform.zero;
        [HideInInspector]
        [SerializeField] private BoneTransform _pinkyTarget = BoneTransform.zero;


        [HideInInspector]
        [SerializeField] private Chain _index;
        [HideInInspector]
        [SerializeField] private Chain _middle;
        [HideInInspector]
        [SerializeField] private Chain _ring;
        [HideInInspector]
        [SerializeField] private Chain _pinky;
        [HideInInspector]
        [SerializeField] private Chain _thumb;

        private void OnEnable()
        {
            if (_rig == null)
                return;

            _transform = transform;

            _index = _rig.Armature.GetChains(SourceChain.INDEX, _targetSide)[0];
            _index.solver = _rig.Armature.GetChains(SourceChain.INDEX, _targetSide)[0].solver;
            _middle = _rig.Armature.GetChains(SourceChain.MIDDLE, _targetSide)[0];
            _ring = _rig.Armature.GetChains(SourceChain.RING, _targetSide)[0];
            _pinky = _rig.Armature.GetChains(SourceChain.PINKY, _targetSide)[0];
            _thumb = _rig.Armature.GetChains(SourceChain.THUMB, _targetSide)[0];
        }

        public void Apply()
        {
            ApplyFinger(_index, GetIKChain(_index, _indexTarget));
            ApplyFinger(_middle, GetIKChain(_middle, _middleTarget));
            ApplyFinger(_ring, GetIKChain(_ring, _ringTarget));
            ApplyFinger(_pinky, GetIKChain(_pinky, _pinkyTarget));
            ApplyFinger(_thumb, GetIKChain(_thumb, _thumbTarget));
        }

        private IKChain GetIKChain(Chain chain, BoneTransform target)
        {
            BoneTransform start = _rig.Armature.currentPose.GetModelTransform(chain.First());
            BoneTransform hand = _rig.Armature.currentPose.GetParentModelTransform(chain.First());
            //BoneTransform transform = new BoneTransform(_transform);

            Axis axis = _rig.Armature.bindPose.GetAxis(chain.First().parentName);
            axis = Axis.Rotate(axis, hand.rotation);
            Quaternion rotation = axis.GetRotation();

            BoneTransform transform = new BoneTransform(hand.position, rotation);

            target.position = transform.TransformPoint(target.position);
            target.rotation = transform.rotation * target.rotation;
            //target = transform + target;

            Vector3 direction = target.position - start.position;
            Vector3 jointDirection = target.TransformPoint(Vector3.up) - start.position;

            return new IKChain(direction, jointDirection, chain.length);
        }

        private void ApplyFinger(Chain chain) => ApplyFinger(chain, 1f);
        private void ApplyFinger(Chain chain, float percent) => ApplyFinger(chain, null, percent);
        private void ApplyFinger(Chain chain, IKChain ikChain, float percent = 1f)
        {
            if (chain.solver is PercentSolver solver)
                solver.Percent = percent;

            chain.Solve(_rig.Armature, ikChain);
        }

        private void Reset()
        {
            _transform = transform;

            _index = _rig.Armature.GetChains(SourceChain.INDEX, _targetSide)[0];
            _middle = _rig.Armature.GetChains(SourceChain.MIDDLE, _targetSide)[0];
            _ring = _rig.Armature.GetChains(SourceChain.RING, _targetSide)[0];
            _pinky = _rig.Armature.GetChains(SourceChain.PINKY, _targetSide)[0];
            _thumb = _rig.Armature.GetChains(SourceChain.THUMB, _targetSide)[0];

            ResetFingers();
        }

        [ContextMenu("Reset Fingers")]
        private void ResetFingers()
        {
            ResetFinger(_index, ref _indexTarget);
            ResetFinger(_middle, ref _middleTarget);
            ResetFinger(_ring, ref _ringTarget);
            ResetFinger(_pinky, ref _pinkyTarget);
            ResetFinger(_thumb, ref _thumbTarget);
        }

        private void ResetFinger(Chain chain, ref BoneTransform target)
        {
            BoneTransform handBind = _rig.Armature.bindPose.GetParentModelTransform(chain.First());
            BoneTransform hand = _rig.Armature.currentPose.GetParentModelTransform(chain.First());
            BoneTransform targetBind = _rig.Armature.bindPose.GetModelTransform(chain.Last());

            Vector3 localBind = targetBind.position - handBind.position;
            localBind = Quaternion.Inverse(handBind.rotation) * localBind;
            localBind = hand.rotation * localBind;

            target.position = _transform.InverseTransformDirection(localBind);

            BoneTransform fingerBaseBind = _rig.Armature.bindPose.GetModelTransform(chain.First());

            Axis axis = new Axis(chain.alternativeForward, chain.alternativeUp);
            axis = Axis.Rotate(axis, fingerBaseBind.rotation);

            Vector3 up = Quaternion.Inverse(handBind.rotation) * axis.up;
            up = hand.rotation * up;
            up = _transform.InverseTransformDirection(up);

            target.rotation =  Quaternion.FromToRotation(Vector3.up, up);
        }
    }
}
