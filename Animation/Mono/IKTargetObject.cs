using UnityEngine;

namespace Rehcub 
{
    [ExecuteInEditMode]
    public class IKTargetObject : MonoBehaviour
    {
        [SerializeField] private IKRig _rig;

        [SerializeField] private Vector3 _hintPosition;

        [SerializeField] private SourceChain _targetChain;
        [SerializeField] private SourceSide _targetSide;

        [Range(0f, 1f)]
        [SerializeField] private float _blend;

        [SerializeField] private bool _ignoreRotation;

        private Transform _transform;
        private Transform _rigTransform;

        public Chain Chain { get => _chain; }
        private Chain _chain;

        private void OnEnable()
        {
            if (_rig == null)
                return;

            _chain = _rig.Armature.GetChains(_targetChain, _targetSide)[0];
            _rig.onPreApplyPose += Apply;
            _rigTransform = _rig.transform;
        }

        private void OnDisable()
        {
            _rig.onPreApplyPose -= Apply;
        }

        public void Init()
        {
            _chain = _rig.Armature.GetChains(_targetChain, _targetSide)[0];
            ResetToPose();
            _rig.onPreApplyPose += Apply;
            _rigTransform = _rig.transform;
        }

        private void Start()
        {
            _rigTransform = _rig.transform;
        }

        public static IKTargetObject Create(IKRig rig, GameObject parent, SourceChain sourceChain, SourceSide side)
        {
            GameObject go = new GameObject($"{sourceChain} {side}");
            go.transform.SetParent(parent.transform);
            IKTargetObject target = go.AddComponent<IKTargetObject>();


            Chain chain = rig.Armature.GetChains(sourceChain, side).First();
            Vector3 pole = chain.First().model.TransformPoint(chain.alternativeUp);

            target._rig = rig;
            target.transform.position = chain.Last().model.position;
            target.transform.rotation = chain.Last().model.rotation;
            target._hintPosition = pole;
            target._targetChain = sourceChain;
            target._targetSide = side;
            target._blend = 1f;

            target.Init();

            return target;
        }

        private Axis GetAxis() => _rig.Armature.bindPose.GetAxis(_chain.Last());

        public void Apply()
        {
            BoneTransform start = _rig.Armature.currentPose.GetModelTransform(_chain.First());
            BoneTransform original = GetOriginalChain(out Vector3 poleOriginal);
            BoneTransform target = GetTargetChain(out Vector3 pole);

            BoneTransform targetTransform = BoneTransform.Slerp(original, target, _blend);

            IEndEffector endEffector = AdjustTarget(start, ref targetTransform);

            Vector3 poleTarget = Vector3.Slerp(poleOriginal, pole, _blend);

            if (_transform.IsChildOf(_rigTransform) == false)
                start.position.Scale(_rigTransform.localScale);
            

            Vector3 direction = targetTransform.position - start.position; 
            
            if (_transform.IsChildOf(_rigTransform) == false)
            {
                Vector3 reverseScaleRoot = new Vector3(1f / _rigTransform.localScale.x, 1f / _rigTransform.localScale.y, 1f / _rigTransform.localScale.z);
                direction.Scale(reverseScaleRoot);
            }

            Vector3 jointDirection = poleTarget - start.position;

            Axis axis = new Axis(direction, jointDirection);

            IKBone ikBone = new IKBone(targetTransform.forward, targetTransform.up);
            IKChain ikChain = new IKChain(direction, axis.up, _chain.length, ikBone);
            _chain.Solve(_rig.Armature, ikChain);

            if (_ignoreRotation)
                return;

            ikChain.endEffector.sourceAxis = Axis.FromRotation(_rig.Armature.bindPose.GetModelTransform(_chain.Last()).rotation);

            axis = new Axis(ikChain.endEffector.direction, ikChain.endEffector.twist);

            _chain.Last().Solve(_rig.Armature.bindPose, _rig.Armature.currentPose, axis);

            if (endEffector == null)
                return;
            endEffector.Apply();
        }

        private IEndEffector AdjustTarget(BoneTransform start, ref BoneTransform targetTransform)
        {
            IEndEffector endEffector = GetComponent<IEndEffector>();
            if (endEffector == null)
            {
                return endEffector;
            }

            BoneTransform rootTransform = new BoneTransform(_rigTransform);
            if (_transform.IsChildOf(_rigTransform))
            {
                Vector3 reverseScaleRoot = new Vector3(1f / _rigTransform.localScale.x, 1f / _rigTransform.localScale.y, 1f / _rigTransform.localScale.z);
                rootTransform.position.Scale(reverseScaleRoot);
            }

            targetTransform = endEffector.AdjustTarget(rootTransform.TransformPoint(start.position), rootTransform + targetTransform);
            targetTransform = rootTransform - targetTransform;

            return endEffector;
        }

        private BoneTransform GetOriginalChain(out Vector3 jointDirection)
        {
            BoneTransform start = _rig.Armature.currentPose.GetModelTransform(_chain.First());

            BoneTransform targetTransform = _rig.Armature.currentPose.GetModelTransform(_chain.Last());
            Axis endAxis = GetAxis();
            endAxis.Rotate(targetTransform.rotation);

            targetTransform.rotation = endAxis.GetRotation();

            jointDirection = start.TransformDirection(_chain.alternativeUp);
            jointDirection.Normalize();

            return targetTransform;
        }

        private BoneTransform GetTargetChain(out Vector3 jointDirection)
        {
            BoneTransform rootTransform = new BoneTransform(_rigTransform);
            BoneTransform targetTransform = new BoneTransform(_transform);

            jointDirection = _hintPosition;
            if (_transform.IsChildOf(_rigTransform))
            {
                Vector3 reverseScaleRoot = new Vector3(1f / _rigTransform.localScale.x, 1f / _rigTransform.localScale.y, 1f / _rigTransform.localScale.z);
                rootTransform.position.Scale(reverseScaleRoot);
                targetTransform.position.Scale(reverseScaleRoot);
                jointDirection = rootTransform.rotation * _hintPosition;
            }
            jointDirection += targetTransform.position;

            targetTransform = rootTransform - targetTransform;
            jointDirection = rootTransform.InverseTransformPoint(jointDirection);

            return targetTransform;
        }

        [ContextMenu("Reset to Bind Pose")]
        private void ResetToBindPose()
        {
            BoneTransform footWorld = _rig.Armature.bindPose.GetModelTransform(_chain.Last());

            Vector3 position = footWorld.position;
            Axis axis = GetAxis();
            Quaternion rotation = Quaternion.LookRotation(footWorld.rotation * axis.forward, footWorld.rotation * axis.up);
            _transform.SetPositionAndRotation(position, rotation);

            BoneTransform legWorld = _rig.Armature.bindPose.GetModelTransform(_chain.First());

            _hintPosition = legWorld.TransformPoint(_chain.Last().alternativeUp);

        }

        [ContextMenu("Reset to Current Pose")]
        private void ResetToPose()
        {
            BoneTransform footWorld = _rig.Armature.currentPose.GetWorldTransform(_chain.Last());

            Vector3 position = footWorld.position;
            Axis axis = GetAxis();
            Quaternion rotation = Quaternion.LookRotation(footWorld.rotation * axis.forward, footWorld.rotation * axis.up);
            _transform.SetPositionAndRotation(position, rotation);

            BoneTransform legWorld = _rig.Armature.bindPose.GetModelTransform(_chain.First());
            _hintPosition = legWorld.TransformPoint(_chain.Last().alternativeUp);
        }

        private void OnValidate()
        {
            _transform = transform;
        }

        private void OnDrawGizmos()
        {
            if (enabled == false)
                return;

            Vector3 targetPosition = _transform.position;

            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawSphere(targetPosition, 0.04f);

            Vector3 poleTarget = _hintPosition;
            if (_transform.IsChildOf(_rigTransform))
                poleTarget = _rig.transform.rotation * _hintPosition;

            poleTarget += _transform.position;

            Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
            Gizmos.DrawSphere(poleTarget, 0.02f);
        }
    }
}
