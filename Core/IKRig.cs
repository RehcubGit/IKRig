using UnityEngine;

namespace Rehcub 
{
    [SelectionBase]
    public class IKRig : MonoBehaviour
    {
        [SerializeField] private Transform _rootBone;
        [SerializeField] private AnimationClip _tPose;

        [SerializeField] public IKAnimationData[] _animations;

        public Armature Armature { get => _armature; }
        [SerializeField] private Armature _armature;

        private Transform _transform;

        public System.Action onPreApplyPose;
        public System.Action<IKPose> onPreApplyIkPose;

        private void Awake() => Init();

        public void Init() 
        { 
            _transform = transform;
        }
        public void Init(Armature armature) 
        { 
            _transform = transform;
            _armature = armature;
        }

        private void Update()
        {
            if (_transform.hasChanged)
            {
                _armature.currentPose.rootTransform = new BoneTransform(_transform);
                ApplyPose();
                _transform.hasChanged = false;
            }
        }

        private IKPose Modify(IKAnimationData animationData, IKPose pose)
        {
            pose = pose.Modify(_armature, animationData);
            return pose;
        }
        private IKPose Modify(IKAnimationData animationData, int frame)
        {
            IKPose pose = animationData.animation.GetFrame(frame);
            pose = pose.Modify(_armature, animationData);
            return pose;
        }

        public void ApplyIkPose(IKAnimationData animationData, int frame)
        {
            IKPose pose = animationData.animation.GetFrame(frame);

            if (animationData.extrectRootMotion)
            {
                Vector3 rootMotion = pose.GetDeltaRootMotion(_armature);
                rootMotion.Scale(_transform.localScale);
                _transform.position += _transform.rotation * rootMotion;
            }

            _armature.currentPose.rootTransform = new BoneTransform(_transform);
            _armature.scale = _transform.localScale;

            pose = Modify(animationData, pose);

            onPreApplyIkPose?.Invoke(pose);
            pose.ApplyPose(_armature);
            ApplyPose();
        }

        public void ApplyIkPoseRaw(IKAnimationData animationData, int frame)
        {
            if (_transform == null)
                _transform = transform;

            IKPose pose = animationData.animation.GetFrame(frame);

            if (animationData.extrectRootMotion)
            {
                Vector3 rootMotion = pose.GetDeltaRootMotion(_armature);
                rootMotion.Scale(_transform.localScale);
                _transform.position += _transform.rotation * rootMotion;
            }

            _armature.currentPose.rootTransform = new BoneTransform(_transform);
            _armature.scale = _transform.localScale;

            onPreApplyIkPose?.Invoke(pose);
            pose.ApplyPose(_armature);
            ApplyPose();
        }

        public void ApplyIkPose(IKPose pose)
        {
            _armature.currentPose.rootTransform = new BoneTransform(_transform);
            _armature.scale = _transform.localScale;

            onPreApplyIkPose?.Invoke(pose);
            pose.ApplyPose(_armature);
            ApplyPose();
        }

        public void ApplyPose(Pose pose)
        {
            _transform.position = pose.rootTransform.position;
            _armature.currentPose = pose;
            _armature.scale = _transform.localScale;
            ApplyPose();
        }

        public void ApplyPose()
        {
            onPreApplyPose?.Invoke();
            _armature.ApplyPose();
        }

        public Animation GetAnimation(IKAnimationData animationData)
        {
            int frameCount = animationData.animation.FrameCount;
            Animation animation = new Animation(frameCount);

            for (int i = 0; i < frameCount; i++)
            {
                ApplyIkPoseRaw(animationData, i);
                Pose keyframe = new Pose(_armature.currentPose);
                animation.AddKeyframe(keyframe, i);
            }

            ResetToBindPose();

            return animation;
        }

        public void ResetToBindPose()
        {
            _transform.position = Vector3.zero;
            _transform.rotation = Quaternion.identity;
            _transform.localScale = Vector3.one;
            _armature.scale = _transform.localScale;
            _armature.ApplyBindPose();
            _armature.currentPose.rootTransform = BoneTransform.zero;
            _armature.UpdatePose();
        }

        public void ResetToTPose()
        {
            _transform.position = Vector3.zero;
            _transform.rotation = Quaternion.identity;
            _transform.localScale = Vector3.one;
            _armature.scale = _transform.localScale;
            _tPose.SampleAnimation(gameObject, 0);
            _armature.currentPose.rootTransform = BoneTransform.zero;
            _armature.UpdatePose();
        }
    }
}
