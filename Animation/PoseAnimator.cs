using UnityEngine;

namespace Rehcub 
{
    public class PoseAnimator : MonoBehaviour
    {
        private const int FPS = 30;
        [SerializeField] private IKRig _rig;
        [SerializeField] private IKAnimationData _currentAnimation;
        private float _startTime;
        private int _currentFrame = 0;

        public System.Action OnAnimationFinished;

        private void Awake()
        {
            _startTime = Time.time;
            _currentFrame = 0;
        }

        private void Update()
        {
            ApplyCurrentPose();
        }

        public void Play(IKAnimationData ikAnimation)
        {
            if (ikAnimation == _currentAnimation)
                return;
            _currentAnimation = ikAnimation;
            _startTime = Time.time;
            _currentFrame = 0;
        }

        public IKPose GetCurrentPose()
        {
            float time = Time.time - _startTime;
            int frame = Mathf.FloorToInt(time * FPS);
            float blend = ((time * FPS) - frame).Frac();

            /*if (frame == _currentFrame)
                return null;*/

            _currentFrame = frame;

            IKPose ikPose = _currentAnimation.animation.GetFrame(frame);
            ikPose = IKPose.Slerp(ikPose, _currentAnimation.animation.GetFrame(frame + 1), blend);
            return ikPose;
        }

        public void ApplyCurrentPose()
        {
            float time = Time.time - _startTime;
            int frame = Mathf.FloorToInt(time * FPS) % _currentAnimation.animation.FrameCount;

            if (frame == _currentFrame)
                return;

            _currentFrame = frame;

            _rig.ApplyIkPose(_currentAnimation, _currentFrame);
        }

        public IKPose GetNextFrame() => _currentAnimation.animation.GetFrame(_currentFrame++);
    }
}
