using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class Animation
    {
        public string name;
        public int FrameCount => _keyframes.Length;
        [SerializeField] private Pose[] _keyframes;

        public bool HasRootMotion { get => _hasRootMotion; }
        private bool _hasRootMotion;

        public Animation(float length, float frameRate)
        {
            int frameCount = (int)(length * frameRate);
            _keyframes = new Pose[frameCount + 1];
        }

        public Animation(int frameCount)
        {
            _keyframes = new Pose[frameCount];
        }

        public void AddKeyframe(Pose pose, int frame)
        {
            _keyframes[frame] = pose;
        }
        public Pose GetFrame(int frame) => _keyframes[frame % _keyframes.Length];

        public Vector3 GetDeltaRootMotion(int frame)
        {
            Pose pose;
            Pose lastPose;
            if (frame == 0)
            {
                pose = _keyframes[FrameCount - 1];
                lastPose = _keyframes[FrameCount - 2];
            }
            else
            {
                pose = _keyframes[frame];
                lastPose = _keyframes[frame - 1];
            }

            Vector3 deltaRootMotion = pose.rootTransform.position - lastPose.rootTransform.position;
            

            return deltaRootMotion;
        }

        public Vector3 GetRootMotion(int frame)
        {
            Vector3 rootMotion = Vector3.zero;

            for (int i = 0; i < frame; i++)
            {
                rootMotion += GetDeltaRootMotion(i);
            }

            return rootMotion;
        }
    }
}
