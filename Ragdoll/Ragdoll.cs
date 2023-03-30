using System.Collections.Generic;
using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
    public class JointSettings
    {
        public Vector3 right;
        public Vector3 up;

        public ConfigurableJointMotion angularMotion;

        public float lowLimitX;
        public float highLimitX;
        public float limitY;
        public float limitZ;

        public void Apply(ConfigurableJoint joint)
        {
            //joint.axis = right;
            //joint.secondaryAxis = up;

            joint.angularXMotion = angularMotion;
            joint.angularYMotion = angularMotion;
            joint.angularZMotion = angularMotion;

            SoftJointLimit limit = new SoftJointLimit
            {
                contactDistance = 0 // default to zero, which automatically sets contact distance.
            };

            limit.limit = lowLimitX;
            joint.lowAngularXLimit = limit;

            limit.limit = highLimitX;
            joint.highAngularXLimit = limit;

            limit.limit = limitY;
            joint.angularYLimit = limit;

            limit.limit = limitZ;
            joint.angularZLimit = limit;
        }
    }

    [System.Serializable]
    public class RigidbodySettings
    {
        public bool useGravity;
        public float drag;
        public float angularDrag;

        public RigidbodyInterpolation interpolation;
        public CollisionDetectionMode collisionDetectionMode;

        public void Apply(Rigidbody rigidbody)
        {
            rigidbody.useGravity = useGravity;
            rigidbody.drag = drag;
            rigidbody.angularDrag = angularDrag;

            rigidbody.interpolation = interpolation;
            rigidbody.collisionDetectionMode = collisionDetectionMode;
        }
    }


    [RequireComponent(typeof(IKRig), typeof(PoseAnimator))]
    public class Ragdoll : MonoBehaviour
    {

        [SerializeField] private bool enable;
        [SerializeField] private bool _selfCollision;

        [SerializeField] private PDController globalPositionController;
        [SerializeField] private PDController globalRotationController;

        [SerializeField] private int _solverInterations;

        [Range(0f, 1f)]
        [SerializeField] private float _alpha = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float _damping = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float _muscleStrength = 1f;

        private Transform _transform;
        private IKRig _rig;
        private PoseAnimator _animator;
        private Armature _armature;
        private Rigidbody[] _rigidbodies;
        private ConfigurableJoint[] _joints;


        private List<BoneMatcher> _boneMatchers;

        private void Awake()
        {
            _transform = transform;
            _rig = GetComponent<IKRig>();
            _animator = GetComponent<PoseAnimator>();
            _armature = _rig.Armature;
            _rigidbodies = GetComponentsInChildren<Rigidbody>();
            _joints = GetComponentsInChildren<ConfigurableJoint>();

            Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer, !_selfCollision);
        }

        private void Start()
        {
            Bone hip = _armature.GetBones(SourceBone.HIP)[0];
            Bone head = _armature.GetBones(SourceBone.HEAD)[0];

            Chain spine = _armature.GetChains(SourceChain.SPINE)[0];

            Chain leftLeg = _armature.GetChains(SourceChain.LEG, SourceSide.LEFT)[0];
            Chain rightLeg = _armature.GetChains(SourceChain.LEG, SourceSide.RIGHT)[0];


            Chain leftArm = _armature.GetChains(SourceChain.ARM, SourceSide.LEFT)[0];
            Chain rightArm = _armature.GetChains(SourceChain.ARM, SourceSide.RIGHT)[0];

            _boneMatchers = new List<BoneMatcher>
            {
                //new BoneMatcher(_armature, hip, true, true),
                new BoneMatcher(_armature, hip),
                new BoneMatcher(_armature, spine[0]),
                //new BoneMatcher(_armature, spine[1]),
                new BoneMatcher(_armature, leftLeg[0]),
                new BoneMatcher(_armature, leftLeg[1]),
                //new BoneMatcher(_armature, leftLeg[2], default, default, true),
                new BoneMatcher(_armature, rightLeg[0]),
                new BoneMatcher(_armature, rightLeg[1]),
                //new BoneMatcher(_armature, rightLeg[2], default, default, true),
                new BoneMatcher(_armature, leftArm[0]),
                new BoneMatcher(_armature, leftArm[1]),
                //new BoneMatcher(_armature, leftArm[2], default, default, true),
                new BoneMatcher(_armature, rightArm[0]),
                new BoneMatcher(_armature, rightArm[1]),
                //new BoneMatcher(_armature, rightArm[2], default, default, true),
                new BoneMatcher(_armature, head),
            };
        }

        public void CreateBoneMatcher(List<RagdollBoneInfo> boneInfos)
        {
            _boneMatchers = new List<BoneMatcher>();

            foreach (RagdollBoneInfo info in boneInfos)
            {
                bool positionMatch = info.parent == null;
                BoneMatcher matcher = new BoneMatcher(_armature, info.bone, positionMatch);
                _boneMatchers.Add(matcher);
            }
        }

        private void Update()
        {
            if (enable)
                return;

            IKPose ikPose = _animator.GetCurrentPose();

            ikPose.ApplyPose(_armature);

            _armature.ApplyPose();
        }

        private void FixedUpdate()
        {
            if (enable == false)
                return;

            IKPose ikPose = _animator.GetCurrentPose();

            Pose pose = _armature.CreatePose();
            ikPose.ApplyPose(_armature, pose);


            PinBones(pose);
        }

        [ContextMenu("Add random force")]
        public void AddRandomForce()
        {
            Vector3 force = Random.insideUnitSphere;
            AddForce(force * 1000f);
        }

        public void AddForce(Vector3 force)
        {
            foreach (Rigidbody body in _rigidbodies)
            {
                body.AddForce(force);
            }
        }


        [ContextMenu("Snap To Pose")]
        public void SnapToPose()
        {
            _armature.ApplyPose();
            foreach (BoneMatcher matcher in _boneMatchers)
                matcher.SnapToTargetPose(_armature.currentPose);
            Debug.Break();
        }

        private void PinBones(Pose pose)
        {
            foreach (BoneMatcher matcher in _boneMatchers)
            {
                //matcher.CalculateAngularVelocity(pose, Time.fixedDeltaTime);
                matcher.SetControllers(globalPositionController, globalRotationController);
                matcher.muscleStrength = _muscleStrength;
                matcher.PinBone(_armature, pose, _alpha, _damping);
            }
        }

        private bool IsInCapsule(Vector2 p, Vector2 a, Vector2 b, float radius)
        {
            float dist = p.SqrDistanceSegment(a, b);

            return dist <= radius * radius;
        }

        private bool IsInEllipse(Vector2 p, Vector2 center, float minorRadius, float majorRadius)
        {
            Vector2 delta = p - center;

            float x = delta.x / majorRadius;
            float y = delta.y / minorRadius;

            return x * x + y * y <= 1f;
        }

        float minorRadius = 0.2f;
        float majorRadius = 0.75f;

        private bool IsBalanced(Pose pose)
        {
            Chain leftLeg = _armature.GetChains(SourceChain.LEG, SourceSide.LEFT)[0];
            Chain rightLeg = _armature.GetChains(SourceChain.LEG, SourceSide.RIGHT)[0];

            Vector3 leftFootPosition = pose.GetModelTransform(leftLeg.Last()).position;
            Vector3 rightFootPosition = pose.GetModelTransform(rightLeg.Last()).position;

            return IsInCapsule(GetCenterOfMass().xz(), leftFootPosition.xz(), rightFootPosition.xz(), minorRadius);

            Vector3 center = (leftFootPosition + rightFootPosition) * 0.5f;
            Vector2 center2D = center.xz();

            Vector3 centerOfMass = GetCenterOfMass();
            Vector2 point2D = centerOfMass.xz();

            return IsInEllipse(point2D, center2D, minorRadius, majorRadius);
        }

        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;


        private IKPose Balance(IKPose pose)
        {
            IKPose newPose = pose.Copy();

            return newPose;
        }

        public Vector3 GetCenterOfMass()
        {
            if (_rigidbodies == null)
                return Vector3.zero;
            Vector3 a = Vector3.zero;
            float b = 0f;
            foreach (Rigidbody body in _rigidbodies)
            {
                a += body.mass * body.transform.position;
                b += body.mass;
            }

            return a / b;
        }


        private Rigidbody GetBody(string name)
        {
            foreach (Rigidbody body in _rigidbodies)
            {
                if (body.gameObject.name == name)
                    return body;
            }
            return null;
        }

        private ConfigurableJoint GetJoint(string name)
        {
            foreach (ConfigurableJoint joint in _joints)
            {
                if (joint.gameObject.name == name)
                    return joint;
            }
            return null;
        }

        private void EnableRagdoll()
        {
            if (_boneMatchers != null)
                foreach (BoneMatcher matcher in _boneMatchers)
                    matcher.EnableBone();
        }

        private void DisableRagdoll()
        {
            if (_boneMatchers != null)
                foreach (BoneMatcher matcher in _boneMatchers)
                    matcher.DisableBone();
        }

        private void OnValidate()
        {
            /*if (boneMatchers != null)
                foreach (BoneMatcher matcher in boneMatchers)
                matcher.muscleStrength = muscleStrength;

            _rigidbodies = GetComponentsInChildren<Rigidbody>();
            _joints = GetComponentsInChildren<ConfigurableJoint>();
            foreach (Rigidbody body in _rigidbodies)
            {
                globalRigidbodySettings.Apply(body);
            }
            foreach (ConfigurableJoint joint in _joints)
            {
                globalJointSettings.Apply(joint);
            }*/

            if (enable)
                EnableRagdoll();
            else
                DisableRagdoll();
        }

    }
}
