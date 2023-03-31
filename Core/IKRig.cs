using UnityEngine;
using UnityEngine.Events;

namespace Rehcub 
{
    [SelectionBase]
    public class IKRig : MonoBehaviour
    {
        [SerializeField] public IKAnimationData[] _animations;

        public Armature Armature { get => _armature; }
        [SerializeField] private Armature _armature;

        private Transform _transform;
        private bool _useUnityAnimator;

        public System.Action onPreApplyPose;
        public System.Action onPostApplyPose;
        public System.Action<IKPose> onPreApplyIkPose;


        [SerializeField] private UnityEvent _unityOnPreApplyPose;

        private void Awake() => Init();

        public void Init() 
        { 
            _transform = transform;
            _useUnityAnimator = TryGetComponent<UnityEngine.Animator>(out _);
        }
        public void Create(Armature armature) 
        {
            _armature = armature;
        }

        private void LateUpdate()
        {
            if (_useUnityAnimator == false)
                return;

            _armature.UpdatePose();
            ApplyPose();
        }

        public void ApplyIkPose(IKAnimationData animationData, int frame)
        {
            IKPose pose = animationData.animation.GetFrame(frame);

            if (animationData.extrectRootMotion)
                ApplyRootMotion(pose);

            _armature.currentPose.rootTransform = new BoneTransform(_transform);
            _armature.scale = _transform.localScale;

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
                ApplyRootMotion(pose);

            _armature.currentPose.rootTransform = new BoneTransform(_transform);
            _armature.scale = _transform.localScale;

            onPreApplyIkPose?.Invoke(pose);
            pose.ApplyPose(_armature);
            ApplyPose();
        }

        public void ApplyIkPose(IKPose pose, bool extrectRootMotion = false)
        {
            if (extrectRootMotion)
                ApplyRootMotion(pose);
            

            _armature.currentPose.rootTransform = new BoneTransform(_transform);
            _armature.scale = _transform.localScale;

            onPreApplyIkPose?.Invoke(pose);
            pose.ApplyPose(_armature);
            ApplyPose();
        }

        private void ApplyRootMotion(IKPose pose)
        {
            BoneTransform rootMotion = pose.GetDeltaRootMotion(_armature);
            rootMotion.position.Scale(_transform.localScale);
            _transform.position += _transform.rotation * rootMotion.position;
            //_transform.rotation *= rootMotion.rotation;
        }

        public void ApplyPose(Pose pose, bool extrectRootMotion = false)
        {
            if (extrectRootMotion)
            {
                _transform.position += _transform.rotation * pose.rootTransform.position;
            }

            _armature.currentPose = pose;
            _armature.scale = _transform.localScale;

            ApplyPose();
        }

        public void ApplyPose()
        {
            _unityOnPreApplyPose?.Invoke();
            onPreApplyPose?.Invoke();
            _armature.ApplyPose();
            onPostApplyPose?.Invoke();
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
    }
}
