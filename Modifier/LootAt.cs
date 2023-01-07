using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class LootAt : BoneConstraint
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private Transform _target;

        public override void Apply(IKPose pose, Armature armature)
        {
            Bone head = armature.GetBones(SourceBone.HEAD)[0];
            Bone poseBone = armature.currentPose[head.boneName];

            Vector3 lookAtPosition = _position;
            if (_target != null)
                lookAtPosition = _target.position;

            Vector3 direction = (lookAtPosition - poseBone.model.position).normalized;
            Quaternion rotation = Quaternion.FromToRotation(pose.head.direction, direction);

            IKBone ikBoneNew = new IKBone
            {
                direction = rotation * pose.head.direction,
                twist = rotation * pose.head.twist
            };

            pose.head = ikBoneNew;
        }
    }
}
