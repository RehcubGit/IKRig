using System.Collections.Generic;
using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class IKAnimation
    {
        public string name;
        public int FrameCount => _keyframes.Length;
        [SerializeField] private IKPose[] _keyframes;
        public bool HasRootMotion { get => _hasRootMotion; }
        private bool _hasRootMotion;

        public IKAnimation(float length, float frameRate)
        {
            int frameCount = (int)(length * frameRate);
            _keyframes = new IKPose[frameCount + 1];
        }

        public IKAnimation(int frameCount)
        {
            _keyframes = new IKPose[frameCount];
        }

        public void AddKeyframe(IKPose pose, int frame)
        {
            _keyframes[frame] = pose;
        }

        public IKPose GetFrame(int frame) => _keyframes[frame % _keyframes.Length].Copy();


        public void ComputeRootMotion(bool avarage = false)
        {
            ComputePosition(avarage); 
            ComputeRootRotation();
        }

        private void ComputeRootRotation()
        {
            float rootAngle = GetRootRotationAngle();

            if (Mathf.Abs(rootAngle) < 0.1f)
                return;

            _hasRootMotion = true;
            Vector3 prevDirection = _keyframes.First().hip.direction;
            prevDirection = Vector3.ProjectOnPlane(prevDirection, Vector3.up).normalized;
            float angle = 0f;

            for (int i = 1; i < _keyframes.Length; i++)
            {
                Vector3 direction = _keyframes[i].hip.direction;
                direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
                float deltaAngle = Vector3.SignedAngle(prevDirection, direction, Vector3.up);
                angle += deltaAngle;

                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);

                _keyframes[i].rootMotion.rotation = rotation;
                _keyframes[i].deltaRootMotion.rotation = Quaternion.AngleAxis(deltaAngle, Vector3.up);
                //_keyframes[i].hip.direction = Quaternion.Inverse(rotation) * _keyframes[i].hip.direction;

                prevDirection = direction;
            }
        }

        private void ComputePosition(bool avarage)
        {
            Vector3 movement;
            Vector3 root;

            Vector3 rootAxis = GetRootMotionAxis();
            if (rootAxis.sqrMagnitude == 0)
            {
                Debug.Log("Animation has no root motion!");
                _hasRootMotion = false;
                return;
            }
            _hasRootMotion = true;
            Vector3 hipAxis = new Vector3(1 - rootAxis.x, 1 - rootAxis.y, 1 - rootAxis.z);

            if (avarage == false)
            {
                IKPose last = _keyframes[_keyframes.Length - 2];
                Vector3 prevRoot0 = _keyframes.Last().hip.movement;
                prevRoot0.Scale(rootAxis);
                Vector3 prevRoot1 = last.hip.movement;
                prevRoot1.Scale(rootAxis);

                movement = _keyframes[0].hip.movement;
                root = movement;
                root.Scale(rootAxis);
                movement.Scale(hipAxis);

                _keyframes[0].hip.movement = movement;
                _keyframes[0].rootMotion.position = root;
                _keyframes[0].deltaRootMotion.position = prevRoot0 - prevRoot1;
                Vector3 prevRoot = root;

                for (int i = 1; i < _keyframes.Length; i++)
                {
                    movement = _keyframes[i].hip.movement;
                    root = movement;
                    root.Scale(rootAxis);
                    movement.Scale(hipAxis);

                    _keyframes[i].rootMotion.position = root;
                    _keyframes[i].deltaRootMotion.position = root - prevRoot;
                    _keyframes[i].hip.movement = movement;
                    prevRoot = root;
                }
                return;
            }

            Vector3 firstPos = _keyframes.First().hip.movement;
            Vector3 lastPos = _keyframes.Last().hip.movement;

            float recipFrames = 1f / _keyframes.Length;

            root = lastPos - firstPos;
            root.Scale(rootAxis);
            root *= recipFrames;

            for (int i = 1; i < _keyframes.Length; i++)
            {
                movement = _keyframes[i].hip.movement;
                movement.Scale(hipAxis);
                _keyframes[i].hip.movement = movement;
                _keyframes[i].rootMotion.position = root * i;
                _keyframes[i].deltaRootMotion.position = root;
            }
        }

        private float GetRootRotationAngle()
        {
            Vector3 firstPos = _keyframes.First().hip.direction;
            Vector3 lastPos = _keyframes.Last().hip.direction;

            if ((lastPos - firstPos).sqrMagnitude < 0.1f)
                return 0f;

            firstPos = Vector3.ProjectOnPlane(firstPos, Vector3.up).normalized;
            lastPos = Vector3.ProjectOnPlane(lastPos, Vector3.up).normalized;

            float angle = Vector3.SignedAngle(firstPos, lastPos, Vector3.up);

            return angle;
        }

        private Vector3 GetRootMotionAxis()
        {
            Vector3 firstPos = _keyframes.First().hip.movement;
            Vector3 lastPos = _keyframes.Last().hip.movement;


            bool x = lastPos.x - firstPos.x > 0.001f;
            bool y = lastPos.y - firstPos.y > 0.001f;
            bool z = lastPos.z - firstPos.z > 0.001f;

            if (x)
                Debug.Log($"{firstPos.x} | {lastPos.x}");
            if(y)
                Debug.Log($"{firstPos.y} | {lastPos.y}");
            if(z)
                Debug.Log($"{firstPos.z} | {lastPos.z}");

            return new Vector3(x.ToFloat(), y.ToFloat(), z.ToFloat());
        }
    }
}
