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
            if (_blend <= 0)
                return;

            IKChain originalChain = GetOriginalChain();
            IKChain targetChain = GetTargetChain();

            targetChain = IKChain.Slerp(originalChain, targetChain, _blend);
            _chain.Solve(_rig.Armature, targetChain);

            if (_ignoreRotation)
                return;

            targetChain.endEffector.sourceAxis = Axis.FromRotation(_rig.Armature.bindPose.GetModelTransform(_chain.Last()).rotation);

            Axis axis = new Axis(targetChain.endEffector.direction, targetChain.endEffector.twist);

            _chain.Last().Solve(_rig.Armature.bindPose, _rig.Armature.currentPose, axis);

            HandControl handControl = GetComponent<HandControl>();
            if (handControl == null)
                return;
            handControl.Apply();
        }

        private IKChain GetOriginalChain()
        {
            BoneTransform start = _rig.Armature.currentPose.GetModelTransform(_chain.First());

            BoneTransform target = _rig.Armature.currentPose.GetModelTransform(_chain.Last());
            Vector3 poleTarget = _rig.Armature.currentPose.GetModelTransform(_chain[1]).position;

            Vector3 jointDirection = poleTarget - start.position;
            jointDirection.Normalize();

            Axis axis = GetAxis();
            IKBone ikBone = new IKBone(target.rotation * axis.forward, target.rotation * axis.up);
            IKChain ikChain = new IKChain(start.position, target.position, jointDirection, _chain.length, ikBone);
            return ikChain;
        }

        private IKChain GetTargetChain()
        {
            BoneTransform start = _rig.Armature.currentPose.GetModelTransform(_chain.First());

            //Vector3 startPosition = _rigTransform.TransformPoint(start.position);
            Vector3 startPosition = start.position;
            Vector3 targetPosition = _transform.position;
            Quaternion targetRotation = _transform.rotation;
            Vector3 poleTarget = _transform.TransformPoint(_hintPosition);

            targetPosition = _rigTransform.InverseTransformPoint(targetPosition);
            targetRotation = Quaternion.Inverse(_rigTransform.rotation) * targetRotation;
            poleTarget = _rigTransform.InverseTransformPoint(poleTarget);

            BoneTransform targetTransform = new BoneTransform(targetPosition, targetRotation);

            Vector3 jointDirection = poleTarget - startPosition;
            jointDirection.Normalize();

            IKBone ikBone = new IKBone(targetTransform.forward, targetTransform.up);
            IKChain ikChain = new IKChain(startPosition, targetTransform.position, jointDirection, _chain.length, ikBone);
            return ikChain;
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

            Vector3 poleTarget = _transform.TransformPoint(_hintPosition);

            Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
            Gizmos.DrawSphere(poleTarget, 0.02f);

        }
    }
}
