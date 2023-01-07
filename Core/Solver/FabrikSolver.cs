using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class FabrikSolver : Solver
    {
        [HideInInspector]
        [SerializeField] private Vector3[] _bonePositions;

        public FabrikSolver(Chain chain) : base(chain)
        {
            _bonePositions = new Vector3[_chain.count];
        }

        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
            Axis axis = new Axis(ikChain.direction, ikChain.jointDirection);
            float length = GetLength(ikChain);

            Vector3 start = pose[_chain.First().boneName].model.position;
            Vector3 target = start + axis.forward * length;

            for (int i = 0; i < _chain.count; ++i)
            {
                _bonePositions[i] = pose[_chain[i].boneName].model.position;
            }

            int mMaxIterationAttempts = 20;
            float mSolveDistanceThreshold = 0.1f;
            float bestSolveDistance = float.PositiveInfinity;
            float lastPassSolveDistance = Vector3.Distance(start, target);
            float mMinIterationChange = 0.1f;

            for (int loop = 0; loop < mMaxIterationAttempts; ++loop)
            {
                float solveDistance = SolveFabrikIK(target, axis);
                for (int i = 1; i < _chain.count; ++i)
                {
                    Quaternion local = ModelToLocal(bindPose, pose, i);
                    pose.SetBoneLocal(_chain[i].boneName, local);
                }

                if (solveDistance < bestSolveDistance)
                {
                    bestSolveDistance = solveDistance;

                    if (solveDistance <= mSolveDistanceThreshold)
                    {
                        break;
                    }
                    continue;
                }
                else
                {
                    if (Mathf.Abs(solveDistance - lastPassSolveDistance) < mMinIterationChange)
                    {
                        break;
                    }
                }
                lastPassSolveDistance = solveDistance;
            }

            ApplyToPose(bindPose, pose, parentTransform, axis);
        }

        private void ApplyToPose(BindPose bindPose, Pose pose, BoneTransform parentTransform, Axis axis)
        {
            for (int i = 0; i < _chain.count - 1; ++i)
            {
                BoneTransform bindLocal = bindPose.GetLocalTransform(_chain[i]);
                Bone poseBone = pose[_chain[i].boneName];
                Quaternion rot = GetLocalRotation(bindPose, i, axis);
                rot = Quaternion.Inverse(parentTransform.rotation) * rot;

                pose.SetBoneLocal(poseBone.boneName, rot);
                parentTransform = pose.GetModelTransform(_chain[i]);
            }
        }

        private float SolveFabrikIK(Vector3 target, Axis axis)
        {
            Vector3 mFixedBaseLocation = getStartLocation(0);

            for (int i = _chain.count - 2; i >= 0; --i)
            {
                Bone thisBone = _chain[i];
                float thisBoneLength = thisBone.length;

                if (i == _chain.count - 2)
                {
                    setEndLocation(i, target);
                }

                target = getEndLocation(i);

                Vector3 thisBoneOuterToInnerUV = getDirectionUV(i) * -1f;

                thisBoneOuterToInnerUV = Vector3.ProjectOnPlane(thisBoneOuterToInnerUV, axis.left);

                Vector3 newStartLocation = target + thisBoneOuterToInnerUV * thisBoneLength;

                setStartLocation(i, newStartLocation);

                if (i > 0)
                {
                    setEndLocation(i - 1, newStartLocation);
                }
            }

            for (int i = 0; i < _chain.count - 1; ++i)
            {
                Bone thisBone = _chain[i];
                float thisBoneLength = thisBone.length;
                Vector3 newEndLocation;
                if (i == 0)
                {
                    setStartLocation(i, mFixedBaseLocation);
                    newEndLocation = getStartLocation(i) + getDirectionUV(i) * thisBoneLength;
                    setEndLocation(i, newEndLocation);
                    setStartLocation(1, newEndLocation);

                    continue;
                }

                Vector3 thisBoneInnerToOuterUV = getDirectionUV(i);
                //Vector3 prevBoneInnerToOuterUV = getDirectionUV(pose, chain, i - 1);

                thisBoneInnerToOuterUV = Vector3.ProjectOnPlane(thisBoneInnerToOuterUV, axis.left);

                newEndLocation = getStartLocation(i) + thisBoneInnerToOuterUV * thisBoneLength;

                setEndLocation(i, newEndLocation);

                if (i < _chain.count - 1)
                {
                    setStartLocation(i + 1, newEndLocation);
                }
            }

            return Vector3.Distance(_chain.Last().model.position, target);
        }

        private Vector3 getStartLocation(int index) => _bonePositions[index];
        private void setStartLocation(int index, Vector3 pos) => _bonePositions[index] = pos;

        private Vector3 getEndLocation(int index) => _bonePositions[index + 1];
        private void setEndLocation(int index, Vector3 pos) => _bonePositions[index + 1] = pos;

        private Quaternion ModelToLocal(BindPose bindPose, Pose pose, int index)
        {
            BoneTransform parent = pose[_chain[index - 1].boneName].model;
            BoneTransform parentBind = bindPose.GetModelTransform(_chain[index - 1]);
            BoneTransform child = pose[_chain[index].boneName].model;
            BoneTransform childBind = bindPose.GetModelTransform(_chain[index]);

            Vector3 bindDir = childBind.position - parentBind.position;
            Vector3 poseDir = child.position - parent.position;

            return Quaternion.FromToRotation(bindDir.normalized, poseDir.normalized);
        }
        private Quaternion GetLocalRotation(BindPose bindPose, int index, Axis axis)
        {
            BoneTransform bind = bindPose.GetModelTransform(_chain[index]);
            BoneTransform childBind = bindPose.GetModelTransform(_chain[index + 1]);

            Vector3 bindDir = childBind.position - bind.position;
            float angle = Vector3.SignedAngle(bindDir.normalized, getDirectionUV(index), axis.left);
            return Quaternion.AngleAxis(angle, axis.left);

            //return Quaternion.FromToRotation(bindDir.normalized, getDirectionUV(index));
        }

        private Vector3 getDirectionUV(int index)
        {
            Vector3 start = _bonePositions[index];
            Vector3 end = _bonePositions[index + 1];
            return (end - start).normalized;
        }
    }
}
