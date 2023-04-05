using System.Collections.Generic;
using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class IKAnimation
    {
        public int FrameCount => _keyframes.Length;
        [SerializeField] private IKPose[] _keyframes;
        public bool HasRootMotion { get => _hasRootMotion; }
        private bool _hasRootMotion;

        public IKAnimation(float length, float frameRate, bool hasRootMotion = false)
        {
            int frameCount = (int)(length * frameRate);
            _keyframes = new IKPose[frameCount + 1];
            _hasRootMotion = hasRootMotion;
        }

        public IKAnimation(int frameCount, bool hasRootMotion = false)
        {
            _keyframes = new IKPose[frameCount];
            _hasRootMotion = hasRootMotion;
        }

        public void AddKeyframe(IKPose pose, int frame)
        {
            _keyframes[frame] = pose;
        }

        public IKPose GetFrame(int frame) => _keyframes[frame % _keyframes.Length].Copy();
    }
}
