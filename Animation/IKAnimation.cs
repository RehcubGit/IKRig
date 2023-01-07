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
        public StrideData LeftStride { get => _leftStride; }
        [SerializeField] private StrideData _leftStride;
        public StrideData RightStride { get => _rightStride; }
        private StrideData _rightStride;
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
/*
        public void ExtraktStrideData2(Armature armature, bool addRootMotion = false)
        {
            List<StrideFrameData> leftStrideData = new List<StrideFrameData>();
            List<StrideFrameData> rightStrideData = new List<StrideFrameData>();

            for (int i = 0; i < _keyframes.Length; i++)
            {
                IKPose pose = _keyframes[i];
                pose.ApplyHip(armature);
                //BoneTransform hip = armature.bindPose[armature.hip.boneName].model;
                BoneTransform hip = armature.currentPose[armature.hip.boneName].model;
                //hip.position = new Vector3(0f, hip.position.y, 0f);
                if(addRootMotion)
                    hip.position += pose.GetRootMotion(armature);

                Vector3 leftFootPosition = GetFootPosition(hip, armature.leftLeg, pose.leftLeg);
                Vector3 rightFootPosition = GetFootPosition(hip, armature.rightLeg, pose.rightLeg);

                StrideFrameData leftStrideFrame = new StrideFrameData(leftFootPosition, i);
                StrideFrameData rightStrideFrame = new StrideFrameData(rightFootPosition, i);

                leftStrideData.Add(leftStrideFrame);
                rightStrideData.Add(rightStrideFrame);
            }

            _leftStride = new StrideData
            {
                frames = leftStrideData.ToArray()
            };
            _leftStride.CalculatePlantedAndLifted(armature.leftLeg.Last().model.position);
            //_leftStride.CalculateStrideLength();
            _leftStride.SetRootMotion(_keyframes[_keyframes.Length - 2].GetRootMotion(armature, true));
            //_leftStride.Sort();
            _rightStride = new StrideData
            {
                frames = rightStrideData.ToArray()
            };

        }*/

        #region strideData need fix up
        /*
                public void ExtraktStrideData(Armature armature)
                {
                    List<StrideFrameData> leftStrideData = new List<StrideFrameData>();
                    List<StrideFrameData> rightStrideData = new List<StrideFrameData>();

                    bool leftFootGroundedLast = false;
                    StrideFrameData leftPlanted = new StrideFrameData(Vector3.back, 0);
                    StrideFrameData leftLifted = new StrideFrameData(Vector3.back, 0);
                    bool rightFootPlantedLast = false;

                    for (int i = 0; i < _keyframes.Length; i++)
                    {
                        IKPose pose = _keyframes[i];
                        BoneTransform hip = pose.CalculateHip(armature);
                        hip.position = new Vector3(0f, hip.position.y, 0f);

                        Vector3 leftFootPosition = GetFootPosition(armature.leftLeg, pose.leftLeg, hip);
                        bool leftFootGrounded = leftFootPosition.y <= (armature.leftLeg.Last().model.position.y + 0.01f);

                        StrideFrameData strideData = new StrideFrameData(leftFootPosition, i);
                        strideData.SetGrounded(leftFootGrounded);

                        if (leftFootGrounded == false) 
                            _hasRootMotion = true;

                        bool leftFootPlanted = leftFootGrounded == true && leftFootGroundedLast == false;
                        if (leftFootPlanted)
                        {
                            strideData.SetAsDestination();
                            leftPlanted = strideData;
                        }
                        bool leftFootLifted = leftFootGrounded == false && leftFootGroundedLast == true;
                        if (leftFootLifted)
                        {
                            leftLifted = strideData;
                        }

                        leftFootGroundedLast = leftFootGrounded;
                        leftStrideData.Add(strideData);


                        Vector3 rightFootPosition = GetFootPosition(armature.leftLeg, pose.leftLeg, hip);
                        bool rightFootGrounded = rightFootPosition.y <= armature.rightLeg.Last().model.position.y;
                        //bool rightFootLifted = rightFootGrounded == false && rightFootPlantedLast == true;
                        if (rightFootGrounded == false)
                            rightStrideData.Add(new StrideFrameData(rightFootPosition, i));

                        bool rightFootPlanted = rightFootGrounded == true && rightFootPlantedLast == false;
                        if (rightFootPlanted == false)
                        {
                            rightStrideData.Add(new StrideFrameData(rightFootPosition, i, 0, rightFootPosition));
                        }
                    }

                    *//*for (int i = 0; i < leftStrideData.Count; i++)
                    {
                        leftStrideData[i].SetPlantFrame(lastLeft);
                        IKVisualize.AddStrideData(leftStrideData[i]);
                    }*//*
                    foreach (StrideFrameData data in leftStrideData)
                    {
                        data.SetPlantFrame(leftPlanted);
                    }

                    StrideFrameData lastRight = rightStrideData[rightStrideData.Count - 1];
                    for (int i = 0; i < rightStrideData.Count - 1; i++)
                    {
                        rightStrideData[i].SetPlantFrame(lastRight);
                    }

                    Vector3 firstPos = _keyframes[leftLifted.frame].hip.movement;
                    Vector3 lastPos = _keyframes[leftPlanted.frame].hip.movement;

                    _leftStride = new StrideData
                    {
                        frames = leftStrideData.ToArray(),
                        plantedFrame = leftPlanted.frame,
                        liftedFrame = leftLifted.frame,
                        rootMotion = (lastPos - firstPos).xz()
                    };

                    _leftStride.Sort();
                    _leftStride.CalculateDeltas();
                    _leftStride.CalculateStrideLength();
                    IKVisualize.AddStrideData(_leftStride);

                    //_leftStride = leftStrideData.ToArray();
                    if (_hasRootMotion == false)
                        FixFootToTheGround(armature);
                }*/

        #endregion

        public void SetLeftStride(StrideData data) => _leftStride = data;
        public void SetRightStride(StrideData data) => _rightStride = data;

        public void ComputeRootMotion(bool avarage = false)
        {
            if(avarage == false)
            {
                IKPose last = _keyframes[_keyframes.Length - 2];
                Vector3 prevRoot0 = new Vector3(_keyframes.Last().hip.movement.x, 0, _keyframes.Last().hip.movement.z);
                Vector3 prevRoot1 = new Vector3(last.hip.movement.x, 0, last.hip.movement.z);
                Vector3 root = new Vector3(_keyframes[0].hip.movement.x, 0, _keyframes[0].hip.movement.z);
                _keyframes[0].hip.movement = new Vector3(0, _keyframes[0].hip.movement.y, 0);
                _keyframes[0].rootMotion = root;
                _keyframes[0].deltaRootMotion = prevRoot0 - prevRoot1;
                Vector3 prevRoot = root;

                for (int i = 1; i < _keyframes.Length; i++)
                {
                    root = new Vector3(_keyframes[i].hip.movement.x, 0, _keyframes[i].hip.movement.z);
                    _keyframes[i].rootMotion = root;
                    _keyframes[i].deltaRootMotion = root - prevRoot;
                    _keyframes[i].hip.movement = new Vector3(0, _keyframes[i].hip.movement.y, 0);
                    prevRoot = root;
                }
                return;
            }
            Vector3 firstPos = _keyframes.First().hip.movement;
            Vector3 lastPos = _keyframes.Last().hip.movement;
            float movementX = lastPos.x - firstPos.x;
            float movementZ = lastPos.z - firstPos.z;

            movementX /= _keyframes.Length;
            movementZ /= _keyframes.Length;

            for (int i = 0; i < _keyframes.Length; i++)
            {
                /*Vector3 lastHip = _keyframes[i - 1].hip.movement;
                Vector3 difference = _keyframes[i].hip.movement - lastHip;
                _keyframes[i].hip.movement = new Vector3(difference.x, _keyframes[i].hip.movement.y, difference.z);*/
                _keyframes[i].hip.movement = new Vector3(0, _keyframes[i].hip.movement.y, 0);
                _keyframes[i].rootMotion = new Vector3(movementX, 0f, movementZ);
            }
        }

        private Vector3 GetFootPosition(BoneTransform hip, Chain chain, IKChain ikChain)
        {
            float length = chain.length * ikChain.lengthScale;

            Vector3 startPosition = (hip + chain.First().local).position;
            Vector3 endPosition = (startPosition + ikChain.direction * length);

            return endPosition;
        }
    }

    public struct StrideFrameData
    {
        public Vector3 footPosition;
        public int frame;
        public int frameTillPlanted;
        public Vector3 toPlantDestination;
        public bool isGrounded;
        private Vector3 _delta;

        public StrideFrameData(Vector3 footPosition, int frame) : this()
        {
            this.footPosition = footPosition;
            this.frame = frame;
        }

        public StrideFrameData(Vector3 footPosition, int frame, int frameTillPlanted, Vector3 toPlantDestination) : this(footPosition, frame)
        {
            this.frameTillPlanted = frameTillPlanted;
            this.toPlantDestination = toPlantDestination;
        }

        public void SetDelta(Vector3 delta) => _delta = delta;
        public Vector3 GetDelta() => _delta;

        public void SetPlantFrame(StrideFrameData lastStride)
        {
            frameTillPlanted = lastStride.frame - frame;
            toPlantDestination = lastStride.footPosition - footPosition;
        }

        public void SetGrounded(bool grounded) => this.isGrounded = grounded;
        public void SetAsDestination() 
        {
            this.frameTillPlanted = 0;
            this.toPlantDestination = footPosition.Copy();
        }
    }


    public class StrideData
    {
        public StrideFrameData[] frames;
        public int liftedFrame;
        public int plantedFrame;
        public float strideLength;
        public Vector3 rootMotion;

        public Vector3 nextPlantedPosition;

        public void CalculateStrideLength()
        {
            //strideLength = Vector3.Distance(frames.First().footPosition.xz(), frames.Last().footPosition.xz());
            rootMotion = frames.Last().footPosition - frames.First().footPosition;
            //strideLength = rootMotion.magnitude;
        }
        public void CalculateDeltas()
        {
            for (int i = 1; i < frames.Length; i++)
            {
                if (frames[i].isGrounded)
                {
                    frames[i].SetDelta(Vector3.zero);
                    continue;
                }
                frames[i].SetDelta(frames[i].footPosition - frames[i - 1].footPosition);
            }
        }

        public void CalculatePlantedAndLifted(Vector3 modelPosition)
        {
            bool groundedLast = frames.Last().footPosition.y <= modelPosition.y;
            for (int i = 0; i < frames.Length; i++)
            {
                bool grounded = frames[i].footPosition.y <= modelPosition.y;

                if (grounded == true && groundedLast == false)
                {
                    Debug.Log($"{i} planted {modelPosition.y} | {frames[i].footPosition.y}");
                    plantedFrame = frames[i].frame;
                }
                if (grounded == false && groundedLast == true)
                {
                    Debug.Log($"{i} lifted");
                    liftedFrame = i;
                }
                groundedLast = grounded;
            }
        }

        public void Sort()
        {
            int newLength;
            if (liftedFrame < plantedFrame)
                newLength = plantedFrame - liftedFrame;
            else
                newLength = frames.Length - liftedFrame + plantedFrame;

            StrideFrameData[] sortedFrames = new StrideFrameData[newLength];

            for (int i = 0; i < newLength; i++)
            {
                int frameIndex = (liftedFrame + i) % frames.Length;
                sortedFrames[i] = frames[frameIndex];
            }


            frames = sortedFrames;
        }

        public void SetRootMotion(Vector3 rootMotion) => this.rootMotion = rootMotion;
        public void SetPlantedFrame(int frame) => this.plantedFrame = frame;
        public void SetLiftedFrame(int frame) => this.liftedFrame = frame;
    }
}
