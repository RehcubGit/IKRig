using System.Collections.Generic;
using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class BindPose
    {
        [HideInInspector]
        [SerializeField] private BoneDictionary _bones;

        public BindPose(List<Bone> boneList)
        {
            _bones = new BoneDictionary();
            foreach (Bone bone in boneList)
            {
                AddBone(bone);
            }
        }

        private void AddBone(Bone bone) => _bones.Add(bone.boneName, bone);

        private Bone GetBone(string boneName)
        {
            if (_bones.TryGetValue(boneName, out Bone bone))
                return bone;
            Debug.LogError($"Bone {boneName} not present in the bind pose!");
            return null;
        }
        private Bone GetBone(SourceBone sourceBone, SourceSide side = SourceSide.MIDDLE)
        {
            foreach (Bone bone in _bones.Values)
            {
                if (bone.side == side && bone.source == sourceBone)
                    return bone;
            }
            Debug.LogError($"Bone {sourceBone} | {side} not present in the bind pose!");
            return null;
        }

        public BoneTransform GetLocalTransform(string boneName) => GetBone(boneName).local;
        public BoneTransform GetLocalTransform(Bone bone) => GetBone(bone.boneName).local;
        public BoneTransform GetLocalTransform(SourceBone source, SourceSide side = SourceSide.MIDDLE) => GetBone(source, side).local;

        public BoneTransform GetModelTransform(string boneName) => GetBone(boneName).model;
        public BoneTransform GetModelTransform(Bone bone) => GetBone(bone.boneName).model;
        public BoneTransform GetModelTransform(SourceBone source, SourceSide side = SourceSide.MIDDLE) => GetBone(source, side).model;

        public BoneTransform GetWorldTransform(string boneName, BoneTransform root) => root + GetBone(boneName).model;
        public BoneTransform GetWorldTransform(Bone bone, BoneTransform root) => root + GetBone(bone.boneName).model;

        public BoneTransform GetParentModelTransform(string boneName) => GetParentModelTransform(GetBone(boneName));
        public BoneTransform GetParentModelTransform(Bone bone)
        {
            if (_bones.TryGetValue(bone.parentName, out Bone parent))
                return parent.model;
            //Debug.LogError($"Parent {bone.parentName} of Bone {bone.boneName} not found!");
            return BoneTransform.zero;
        }

        public BoneTransform GetParentWorldTransform(string boneName, BoneTransform root) => root + GetParentModelTransform(boneName);
        public BoneTransform GetParentWorldTransform(Bone bone, BoneTransform root) => root + GetParentModelTransform(bone);

        public float GetLength(Bone bone) => GetBone(bone.boneName).length;

        public Axis GetAxis(Bone bone) => new Axis(GetBone(bone.boneName).alternativeForward, GetBone(bone.boneName).alternativeUp); 
        public Axis GetAxis(string boneName) => new Axis(GetBone(boneName).alternativeForward, GetBone(boneName).alternativeUp); 
    }
}
