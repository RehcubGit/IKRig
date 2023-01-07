using UnityEngine;

namespace Rehcub
{
    public enum Target
    {
        Hip, Spine, LeftFoot, RightFoot, LeftHand, RightHand
    }

    [System.Serializable]
    public class CopyPosition : BoneConstraint
    {
        [SerializeField] private bool _x = true;
        [SerializeField] private bool _y = true;
        [SerializeField] private bool _z = true;
        [SerializeField] private string _owner;
        [SerializeField] private string _target;


        public override void Apply(IKPose pose, Armature armature)
        {
            Bone ownerPose = armature.currentPose[_owner];
            Bone targetPose = armature.currentPose[_target];

            Vector3 ownerPosition = ownerPose.model.position;
            Vector3 targetPosition = targetPose.model.position;
            float x = _x ? targetPosition.x : ownerPosition.x;
            float y = _y ? targetPosition.y : ownerPosition.y;
            float z = _z ? targetPosition.z : ownerPosition.z;
            Vector3 newTarget = new Vector3(x, y, z);
            armature.currentPose.SetBoneModel(_owner, newTarget);
        }
    }

    [System.Serializable]
    public class FixedPosition : BoneConstraint
    {
        public Vector3 position;
        public Vector3 hint;
        [Range(0f, 1f)]
        public float blend = 1f;
        public bool _x = true;
        public bool _y = true;
        public bool _z = true;
        public Target target;

        public Space _space;

        public void Validate(Vector3 childWorldPosition)
        {
            Vector3 direction = position - childWorldPosition;
            Vector3 jointDirection = hint - childWorldPosition;

            direction = direction.normalized;
            Vector3 left = Vector3.Cross(jointDirection, direction).normalized;
            hint = Vector3.Cross(direction, left).normalized * 0.2f + childWorldPosition;
        }
        public void Validate(Vector3 target, Vector3 pole, Vector3 childWorldPosition)
        {
            Vector3 direction = target - childWorldPosition;
            Vector3 jointDirection = pole - childWorldPosition;

            direction = direction.normalized;
            Vector3 left = Vector3.Cross(jointDirection, direction).normalized;
            hint = Vector3.Cross(direction, left).normalized * 0.2f + childWorldPosition;
        }

        public override void Apply(IKPose pose, Armature armature)
        {
            IKChain newChain;
            switch (target)
            {
                case Target.Hip:
                    Bone hip = armature.GetBones(SourceBone.HIP)[0];
                    Vector3 hipPose = armature.currentPose[hip.boneName].local.position;
                    //pose.hip.movement += position - hipPose;
                    pose.hip.bindOffset += position - hipPose;

                    //Vector3 hipPose = armature.currentPose.bones[armature.hip.boneName].local.position;
                    //Vector3 localPos = hipPose + _position;
                    //armature.currentPose.SetBoneLocal(armature.hip.boneName, localPos);
                    //armature.currentPose.SetBoneWorld(armature.hip.boneName, localPos);
                    //pose.hip.movement = _position;
                    break;
                case Target.Spine:
                    Chain spine = armature.GetChains(SourceChain.SPINE)[0];
                    newChain = CreateNewChain(armature, spine, pose.spine);
                    pose.spine = IKChain.Lerp(pose.spine, newChain, blend);
                    break;
                case Target.LeftFoot:
                    Chain leftLeg = armature.GetChains(SourceChain.LEG, SourceSide.LEFT)[0];
                    newChain = CreateNewChain(armature, leftLeg, pose.leftLeg);
                    pose.leftLeg = IKChain.Lerp(pose.leftLeg, newChain, blend);
                    break;
                case Target.RightFoot:
                    Chain rightLeg = armature.GetChains(SourceChain.LEG, SourceSide.RIGHT)[0];
                    newChain = CreateNewChain(armature, rightLeg, pose.rightLeg);
                    pose.rightLeg = IKChain.Lerp(pose.rightLeg, newChain, blend);
                    break;
                case Target.LeftHand:
                    Chain leftArm = armature.GetChains(SourceChain.ARM, SourceSide.LEFT)[0];
                    newChain = CreateNewChain(armature, leftArm, pose.leftArm);
                    pose.leftArm = IKChain.Lerp(pose.leftArm, newChain, blend);
                    break;
                case Target.RightHand:
                    Chain rightArm = armature.GetChains(SourceChain.ARM, SourceSide.RIGHT)[0];
                    newChain = CreateNewChain(armature, rightArm, pose.rightArm);
                    pose.rightArm = IKChain.Lerp(pose.rightArm, newChain, blend);
                    break;
                default:
                    break;
            }
        }

        public void Reset(Armature armature)
        {
            switch (target)
            {
                case Target.Hip:
                    Bone hip = armature.GetBones(SourceBone.HIP)[0];
                    position = armature.currentPose[hip.boneName].local.position;
                    break;
                case Target.LeftFoot:
                    Chain leftLeg = armature.GetChains(SourceChain.LEG, SourceSide.LEFT)[0];
                    ResetChain(armature, leftLeg);
                    break;
                case Target.RightFoot:
                    Chain rightLeg = armature.GetChains(SourceChain.LEG, SourceSide.RIGHT)[0];
                    ResetChain(armature, rightLeg);
                    break;
                case Target.LeftHand:
                    Chain leftArm = armature.GetChains(SourceChain.ARM, SourceSide.LEFT)[0];
                    ResetChain(armature, leftArm);
                    break;
                case Target.RightHand:
                    Chain rightArm = armature.GetChains(SourceChain.ARM, SourceSide.RIGHT)[0];
                    ResetChain(armature, rightArm);
                    break;
                default:
                    break;
            }
        }

        private void ResetChain(Armature armature, Chain chain)
        {
            BoneTransform startWorld = armature.currentPose.GetModelTransform(chain.First());
            BoneTransform middleWorld = startWorld + chain[1].local;
            BoneTransform endWorld = startWorld + chain.Last().local;
            position = endWorld.position;

            Vector3 forward = (endWorld.position - startWorld.position).normalized;
            Vector3 up = (middleWorld.position - startWorld.position).normalized;
            Vector3 left = Vector3.Cross(up, forward);
            hint = Vector3.Cross(forward, left) * 0.2f + startWorld.position;
        }

        private IKChain CreateNewChain(Armature armature, Chain chain, IKChain ikChain)
        {
            BoneTransform childModel = armature.currentPose.GetModelTransform(chain.First());
            Vector3 start = childModel.position;
            BoneTransform root = armature.currentPose.rootTransform;
            BoneTransform childWorld = root + childModel;

            float length = chain.length * ikChain.lengthScale;

            Vector3 originalTarget = start + ikChain.direction * length;
            Vector3 target = root - position;
            Vector3 pole = root - hint;
            Validate(target, pole, childWorld.position);
            pole = root - hint;

            float x = _x ? target.x : originalTarget.x;
            float y = _y ? target.y : originalTarget.y;
            float z = _z ? target.z : originalTarget.z;

            Vector3 newTarget = new Vector3(x, y, z);
            position = newTarget; 
            //Validate(start);
            Vector3 jointDirection = pole - childWorld.position;

            /*IKTarget iktarget = new IKTarget(childWorld.position, newTarget, jointDirection, chain.length);
            return iktarget.GetIKChain(ikChain);*/
            return new IKChain(start, newTarget, jointDirection, chain.length);
        }
    }
}
