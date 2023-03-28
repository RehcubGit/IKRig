using UnityEngine;

namespace Rehcub 
{
    [ExecuteInEditMode]
    public class HandControl : MonoBehaviour , IEndEffector
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

        [HideInInspector]
        [SerializeField] private bool _hasIndex;
        [HideInInspector]
        [SerializeField] private bool _hasMiddle;
        [HideInInspector]
        [SerializeField] private bool _hasRing;
        [HideInInspector]
        [SerializeField] private bool _hasPinky;
        [HideInInspector]
        [SerializeField] private bool _hasThumb;

        private void OnValidate()
        {
            if (_rig == null)
                return;

            _transform = transform;

            _index = GetChain(SourceChain.INDEX);
            _hasIndex = _index != null;

            _middle = GetChain(SourceChain.MIDDLE);
            _hasMiddle = _middle != null;

            _ring = GetChain(SourceChain.RING);
            _hasRing = _ring != null;

            _pinky = GetChain(SourceChain.PINKY);
            _hasPinky = _pinky != null;

            _thumb = GetChain(SourceChain.THUMB);
            _hasThumb = _thumb != null;
        }

        public void Apply()
        {
            if (enabled == false)
                return;

            if(_hasIndex)
                ApplyFinger(_index, GetIKChain(_index, _indexTarget));

            if (_hasMiddle)
                ApplyFinger(_middle, GetIKChain(_middle, _middleTarget));

            if (_hasRing)
                ApplyFinger(_ring, GetIKChain(_ring, _ringTarget));

            if (_hasPinky)
                ApplyFinger(_pinky, GetIKChain(_pinky, _pinkyTarget));

            if (_hasThumb)
                ApplyFinger(_thumb, GetIKChain(_thumb, _thumbTarget));
        }

        public BoneTransform AdjustTarget(Vector3 start, BoneTransform target)
        {
            return target;
        }

        private IKChain GetIKChain(Chain chain, BoneTransform target)
        {
            if (chain == null)
                return null;

            BoneTransform start = _rig.Armature.currentPose.GetModelTransform(chain.First());
            BoneTransform hand = _rig.Armature.currentPose.GetParentModelTransform(chain.First());

            Axis axis = _rig.Armature.bindPose.GetAxis(chain.First().parentName);
            axis = Axis.Rotate(axis, hand.rotation);
            Quaternion rotation = axis.GetRotation();

            BoneTransform transform = new BoneTransform(hand.position, rotation);

            target.position = transform.TransformPoint(target.position);
            target.rotation = transform.rotation * target.rotation;

            Vector3 direction = target.position - start.position;
            Vector3 jointDirection = target.TransformPoint(Vector3.up) - start.position;

            return new IKChain(direction, jointDirection, chain.length);
        }

        private void ApplyFinger(Chain chain, IKChain ikChain)
        {
            if (chain == null)
                return;

            chain.Solve(_rig.Armature, ikChain);
        }

        private Chain GetChain(SourceChain sourceChain)
        {
            Chain[] chains = _rig.Armature.GetChains(sourceChain, _targetSide);
            if (chains.Length == 0)
                return null;

            return chains.First();
        }

        [ContextMenu("Reset Fingers")]
        private void ResetFingers()
        {
            if (_hasIndex)
                ResetFinger(_index, ref _indexTarget);
            if (_hasMiddle)
                ResetFinger(_middle, ref _middleTarget);
            if (_hasRing)
                ResetFinger(_ring, ref _ringTarget);
            if (_hasPinky)
                ResetFinger(_pinky, ref _pinkyTarget);
            if (_hasThumb)
                ResetFinger(_thumb, ref _thumbTarget);
        }

        private void ResetFinger(Chain chain, ref BoneTransform target)
        {
            if (chain == null)
                return;

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
