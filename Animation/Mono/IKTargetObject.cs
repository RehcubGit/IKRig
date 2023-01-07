using UnityEngine;

namespace Rehcub 
{
    [ExecuteInEditMode]
    public class IKTargetObject : MonoBehaviour
    {
        [SerializeField] private IKRig _rig;
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _poleTarget;

        [SerializeField] private SourceChain _targetChain;
        [SerializeField] private SourceSide _targetSide;

        [Range(0f, 1f)]
        [SerializeField] private float blend;

        private bool _ignoreRotation = false;


        private Transform _rigTransform;
        public Chain Chain { get => _chain; }
        private Chain _chain;

        public bool IsPlanted { get => _isPlanted; }
        private bool _isPlanted;

        public bool WasMoved { get => _wasMoved; set => _wasMoved = value; }
        private bool _wasMoved;

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

            Bone lastBone = _rig.Armature.currentPose[_chain.Last()];
            BoneTransform footWorld = _rig.Armature.currentPose.GetWorldTransform(lastBone);

            _target.position = footWorld.position;
            _target.rotation = footWorld.rotation;

            _chain = _rig.Armature.GetChains(_targetChain, _targetSide)[0];
            _rig.onPreApplyPose += Apply;
        }

        private void Awake()
        {
        }

        private void Start()
        {
            _rigTransform = _rig.transform;
        }

        private void Update()
        {
            _wasMoved = false;
#if UNITY_EDITOR
            if (_target.hasChanged)
            {
                _rig.ApplyPose();
                _wasMoved = true;
                _target.hasChanged = false;
            }
#endif
        }

        public Vector3 GetPosition() => transform.position;
        public void SetPosition(Vector3 targetPosition) => transform.position = targetPosition;
        public Vector3 GetTargetPosition() => _target.position;
        public void SetTargetPosition(Vector3 targetPosition) => _target.position = targetPosition;
        public void SetTargetRotation(Quaternion targetRotation) => _target.rotation = targetRotation;

        public bool IsOutOfReach()
        {
            Bone firstBone = _rig.Armature.currentPose[_chain.First()];
            BoneTransform start = firstBone.model;
            Vector3 target = _rigTransform.InverseTransformPoint(_target.position);

            float length = Vector3.Distance(start.position, target);

            return _chain.length < length;
        }

        public bool IsOutOfReach(out Vector3 direction)
        {
            Bone firstBone = _rig.Armature.currentPose[_chain.First()];
            BoneTransform start = firstBone.model;
            Vector3 target = _rigTransform.InverseTransformPoint(_target.position);

            direction = target - start.position;
            float length = direction.magnitude;

            float lengthDifferance = length - _chain.length;

            direction = direction.normalized * lengthDifferance;

            //direction = start + direction;

            return _chain.length < length;
        }

        public void ForcePlanted() 
        {
            blend = 1f;
            bool gotHit = Physics.Raycast(_target.position + Vector3.up, Vector3.down, out RaycastHit hit, 2f);

            float groundHeight = hit.point.y;
            float bindHeight = _rig.Armature.bindPose.GetModelTransform(_chain.Last()).position.y;

            _isPlanted = false;
            if (gotHit)
            {
                _target.position = _target.position.SetY(groundHeight + bindHeight);
                _isPlanted = true;
            }
        }

        [ContextMenu("Apply Target")]
        public void Apply()
        {
            CopyTransform copyTransform = GetComponentInChildren<CopyTransform>();
            if(copyTransform != null)
                copyTransform.ApplyTransform();

            Bone lastBone = _rig.Armature.currentPose[_chain.Last()];
            if (blend == 0)
            {
                BoneTransform footWorld = _rig.Armature.currentPose.ModelToWorld(lastBone.model);

                _target.position = footWorld.position;
                _target.rotation = footWorld.rotation;
                if (_targetChain == SourceChain.LEG)
                    CalculateToes();
                return;
            }

            Bone firstBone = _rig.Armature.currentPose[_chain.First()];
            BoneTransform start = firstBone.model;

            Vector3 target = _target.localPosition;
            Vector3 poleTarget = _poleTarget.localPosition;

            if (transform.parent != _rigTransform)
            {
                target = _rigTransform.InverseTransformPoint(_target.position);
                poleTarget = _rigTransform.InverseTransformPoint(_poleTarget.position);
            }

            //BoneTransform targetTransform = AqustTarget(start.position, target); 
            BoneTransform targetTransform = new BoneTransform(target, _target.rotation);
            /*BoneTransform targetTransform = new BoneTransform(target, Quaternion.identity);
            if (_targetChain == SourceChain.LEG)
            {
                targetTransform = AqustTarget(start.position, target);
            }*/

            Vector3 jointDirection = poleTarget - start.position;
            jointDirection.Normalize();

            IKChain ikChain = new IKChain(start.position, targetTransform.position, jointDirection, _chain.length);
            _chain.Solve(_rig.Armature, ikChain);

            if(_ignoreRotation == false)
            {
                IKBone ikBone = new IKBone(targetTransform.rotation * Vector3.up, targetTransform.rotation * Vector3.forward);
                lastBone.Solve(_rig.Armature, ikBone);
            }

            if(_targetChain == SourceChain.LEG)
                CalculateToes();
            if(_targetChain == SourceChain.ARM)
                CalculateFinger();

            //_rig.ApplyPose();
        }

        private BoneTransform AqustTarget(Vector3 start, Vector3 target)
        {
            Vector3 direction = target - start;
            float distance = direction.magnitude;
            direction = direction.normalized;

            bool gotHit = Physics.Raycast(start, direction, out RaycastHit hit, distance);

            _isPlanted = false;

            BoneTransform transform = new BoneTransform(target, Quaternion.identity);
            if (gotHit == false)
                return transform;
            _isPlanted = true;

            float hitDistance = Vector3.Distance(hit.point, start);
            /*
            float groundHeight = hit.point.y;
            float bindHeight = _rig.Armature.bindPose[_chain.Last()].model.position.y;


            if (_target.position.y < groundHeight + bindHeight)
            {
                transform.position = transform.position.SetY(groundHeight + bindHeight);
                _isPlanted = true;
            }*/

            if (distance > hitDistance)
            {
                transform.position = hit.point;
            }

            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            return transform;
        }

        private void CalculateFinger()
        {
            //Chain index = _rig.Armature.GetChains(SourceChain.INDEX, _targetSide).First();
        }


        private void CalculateToes()
        {
            Armature armature = _rig.Armature;

            Bone foot = armature.currentPose[_chain.Last().boneName];
            BoneTransform parentTransform = _rig.Armature.currentPose.GetParentWorldTransform(foot);
            BoneTransform footWorld = parentTransform + foot.local;

            Bone toeBind = armature.GetChildren(foot).First();
            Bone toe = armature.currentPose[toeBind];
            //BoneTransform toeModel = _rig.Armature.currentPose.GetWorldTransform(toe);
            BoneTransform toeModel = footWorld + toe.local;

            Physics.Raycast(toeModel.position + Vector3.up, Vector3.down, out RaycastHit hit, 2f);
            float groundHeight = hit.point.y;

            Bone toeEndBind = armature.GetChildren(toe).First();
            Bone toeEnd = armature.currentPose[toeEndBind];
            //BoneTransform toeEndModel = _rig.Armature.currentPose.GetWorldTransform(toeEnd);
            BoneTransform toeEndModel = toeModel + toeEnd.local;

            if (toeModel.position.y < groundHeight)
            {
                float a = footWorld.position.y - groundHeight;
                float c = foot.length;
                float b = Mathf.Sqrt(c * c - a * a);

                Vector3 toeLocal = toeModel.position - footWorld.position;
                Vector3 newToeLocal = new Vector3(toeLocal.x, -a, b);

                Quaternion rot = Quaternion.FromToRotation(toeLocal.normalized, newToeLocal.normalized);
                rot = foot.local.rotation * Quaternion.Inverse(rot);

                armature.currentPose.SetBoneLocal(foot.boneName, rot);

                parentTransform += foot.local;

                rot = Quaternion.LookRotation(Vector3.up, Vector3.forward);
                rot = Quaternion.Inverse(parentTransform.rotation) * rot;
                armature.currentPose.SetBoneLocal(toe.boneName, rot);

                return;
            }

            if(toeEndModel.position.y < groundHeight)
            {
                float a = toeModel.position.y - groundHeight;
                float c = toe.length;
                float b = Mathf.Sqrt(c * c - a * a);

                Vector3 toeEndLocal = toeEndModel.position - toeModel.position;
                Vector3 newToeLocal = new Vector3(toeEndLocal.x, -a, b);

                Quaternion rot = Quaternion.FromToRotation(toeEndLocal.normalized, newToeLocal.normalized);
                rot = toe.local.rotation * Quaternion.Inverse(rot);
                armature.currentPose.SetBoneLocal(toe.boneName, rot);

                return;
            }

            //Quaternion q = Quaternion.Euler(39.174f, 0, 0);
            armature.currentPose.SetBoneLocal(toe.boneName, toeBind.local.rotation);
        }

        private void OnDrawGizmos()
        {
            /*Armature armature = _rig.Armature;

            if (_chain == null)
                return;

            Bone foot = armature.currentPose[_chain.Last().boneName];
            Transform lastBoneTransform = _rig.Armature.GetTransform(foot.boneName);

            Bone hipBind = armature.GetBones(SourceBone.HIP).First();
            Bone hip = armature.currentPose[hipBind];
            Gizmos.DrawSphere(hip.model.position, 0.04f);

            Transform toeTransform = lastBoneTransform.GetChild(0);
            Bone toe = armature.currentPose[toeTransform.name];
            BoneTransform toeModel = _rig.Armature.currentPose.GetModelTransform(toe);
            BoneTransform footModel = _rig.Armature.currentPose.GetModelTransform(foot);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(toeModel.position, 0.04f);
            Gizmos.DrawWireSphere(footModel.position, 0.04f);

            if (toeModel.position.y < 0)
            {
                float a = foot.model.position.y;
                float c = foot.length;
                float b = Mathf.Sqrt(c * c - a * a);

                Vector3 toeLocal = toeModel.position - foot.model.position;
                Vector3 newToeLocal = new Vector3(toeLocal.x, -a, b);

                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(foot.model.position + toeLocal, 0.01f);
                Gizmos.DrawSphere(foot.model.position + newToeLocal, 0.02f);

                Quaternion rot;
                rot = Quaternion.FromToRotation(toeLocal.normalized, newToeLocal.normalized);
                Gizmos.DrawSphere(foot.model.position + rot * toeLocal, 0.03f);
                Gizmos.DrawRay(footModel.position, newToeLocal);
            }*/
        }
    }
}
