using SerializableCollections;
using System.Collections.Generic;
using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class Pose
    {
        public BoneTransform rootTransform;
        [HideInInspector]
        [SerializeField] private BoneDictionary bones;

        public Bone this[string name]
        {
            get {
                if (bones.TryGetValue(name, out Bone bone))
                    return bone;
                //Debug.LogError($"Bone {name} not present in this Pose!");
                return null; 
            }
        }
        public Bone this[Bone bone]
        {
            get {
                if (bones.TryGetValue(bone.boneName, out Bone bone2))
                    return bone2;
                //Debug.LogError($"Bone {name} not present in this Pose!");
                return null; 
            }
        }

        public BoneDictionary.ValueCollection GetBones() => bones.Values;
        public BoneDictionary.KeyCollection GetNames() => bones.Keys;

        public Pose()
        {
            bones = new BoneDictionary();
            rootTransform = BoneTransform.zero;
        }

        public Pose(Pose source)
        {
            bones = new BoneDictionary();

            foreach (Bone bone in source.GetBones())
            {
                AddBone(new Bone(bone));
            }
            rootTransform = source.rootTransform;
        }

        public void AddBone(Bone bone) => bones.Add(bone.boneName, bone);
        public void AddBones(Bone[] bones)
	    {
            foreach (Bone bone in bones) AddBone(bone);
        }
        public void AddBones(List<Bone> bones)
	    {
            foreach (Bone bone in bones) AddBone(bone);
        }

        private Bone GetBone(string boneName)
        {
            if (bones.TryGetValue(boneName, out Bone bone))
                return bone;
            //Debug.LogError($"Bone {boneName} not present in the pose!");
            return null;
        }

        private Bone GetBone(SourceBone sourceBone, SourceSide side = SourceSide.MIDDLE)
        {
            foreach (Bone bone in bones.Values)
            {
                if (bone.side == side && bone.source == sourceBone)
                    return bone;
            }
            //Debug.LogError($"Bone {sourceBone} | {side} not present in the pose!");
            return null;
        }

        private Bone GetParentBone(Bone bone) => GetBone(bone.parentName);

        #region Getter

        public BoneTransform GetLocalTransform(string boneName) => GetBone(boneName).local;
        public BoneTransform GetLocalTransform(Bone bone) => GetBone(bone.boneName).local;
        public BoneTransform GetLocalTransform(SourceBone source, SourceSide side = SourceSide.MIDDLE) => GetBone(source, side).local;

        public BoneTransform GetModelTransform(string boneName) => GetBone(boneName).model;
        public BoneTransform GetModelTransform(Bone bone) => GetBone(bone.boneName).model;
        public BoneTransform GetModelTransform(SourceBone source, SourceSide side = SourceSide.MIDDLE) => GetBone(source, side).model;

        public BoneTransform GetWorldTransform(Bone bone) => rootTransform + GetModelTransform(bone);

        public BoneTransform GetParentModelTransform(Bone bone)
        {
            Bone parent = GetParentBone(bone);

            if (parent == null)
                return BoneTransform.zero;

            return parent.model;
        }

        public BoneTransform GetParentWorldTransform(Bone bone) => rootTransform + GetParentModelTransform(bone);
        public BoneTransform CalculateParentModelTransform(Bone bone)
        {
            Bone parent = GetParentBone(bone);
            if (parent == null)
                return BoneTransform.zero;

            BoneTransform parentTransform = parent.local;

            while (this.bones.TryGetValue(parent.parentName, out parent))
            {
                //because we are adding the transforms in rev. "parentTransform" is actualy the child
                parentTransform = parent.local + parentTransform;
            }

            return parentTransform;
        }

        #endregion

        #region Setter


        public void SetBoneLocal(string boneName, Quaternion rotation) => SetBoneLocal(boneName, new BoneTransform(GetBone(boneName).local.position, rotation));
        public void SetBoneLocal(string boneName, Vector3 position) => SetBoneLocal(boneName, new BoneTransform(position, GetBone(boneName).local.rotation));
        public void SetBoneLocal(string boneName, Vector3 position, Quaternion rotation) => SetBoneLocal(boneName, new BoneTransform(position, rotation));
        public void SetBoneLocal(string boneName, BoneTransform local)
        {
            Bone bone = GetBone(boneName);
            Bone parent = GetParentBone(bone);
            bone.local = local;
            if (parent == null)
            {
                bone.model = local;
                return;
            }
            bone.model = parent.model + local;
        }

        public void SetBoneModel(string boneName, Quaternion rotation) => SetBoneModel(boneName, new BoneTransform(GetBone(boneName).model.position, rotation));
        public void SetBoneModel(string boneName, Vector3 position) => SetBoneModel(boneName, new BoneTransform(position, GetBone(boneName).model.rotation));
        public void SetBoneModel(string boneName, Vector3 position, Quaternion rotation) => SetBoneModel(boneName, new BoneTransform(position, rotation));
        public void SetBoneModel(string boneName, BoneTransform model)
        {
            Bone bone = GetBone(boneName);
            Bone parent = GetParentBone(bone);
            bone.model = model;
            if (parent == null)
            {
                bone.local = model;
                return;
            }
            bone.local = parent.model - model;
        }

        #endregion

        #region Conversion
        public Vector3 WorldToModel(Vector3 world) => rootTransform - world;
        public BoneTransform WorldToModel(BoneTransform world) => rootTransform - world;
        public Vector3 ModelToWorld(Vector3 model) => rootTransform + model;
        public BoneTransform ModelToWorld(BoneTransform model) => rootTransform + model;
        #endregion
    }
}
