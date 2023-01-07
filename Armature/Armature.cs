using System.Collections.Generic;
using UnityEngine;

namespace Rehcub 
{
    public enum SourceBone
    {
        HIP, 
        UPPER_LEG, LOWER_LEG, FOOT, TOE,
        SHOULDER, UPPER_ARM, LOWER_ARM, HAND,
        INDEX_01, INDEX_02, INDEX_03, INDEX_04,
        MIDDLE_01, MIDDLE_02, MIDDLE_03, MIDDLE_04,
        RING_01, RING_02, RING_03, RING_04,
        PINKY_01, PINKY_02, PINKY_03, PINKY_04,
        THUMB_01, THUMB_02, THUMB_03, THUMB_04,
        SPINE_01, SPINE_02, SPINE_03, SPINE_04, SPINE_05, SPINE_06,
        NECK, HEAD,
        CHAIN,
        SIMULATE,
        NONE
    }
    public enum SourceChain
    {
        LEG, ARM, SPINE, INDEX, MIDDLE, RING, PINKY, THUMB, NONE
    }
    public enum SourceSide
    {
        LEFT, RIGHT, MIDDLE
    }


    [System.Serializable]
    public class Armature
    {
        [SerializeField] private BoneTransformDictionary _transforms;
        [SerializeField] private List<Chain> _chains;

        public Bone root;

        public Vector3 scale = Vector3.one;
        public BindPose bindPose;
        public Pose currentPose;

        public bool fixHip;

        public Armature(List<Transform> boneTransforms, List<Bone> bones, List<Chain> chains)
        {
            _transforms = new BoneTransformDictionary(boneTransforms);
            _chains = chains;

            bindPose = new BindPose(bones);
            currentPose = CreatePose(bones);
        }

        public void UpdatePose()
        {
            foreach (string name in _transforms.Keys)
            {
                currentPose[name].Update(_transforms[name]);
            }
        }

        public Pose CreatePose(List<Bone> bones) 
        {
            Pose pose = new Pose();
            foreach (Bone bone in bones)
            {
                pose.AddBone(new Bone(bone));
            }
            return pose;
        }

        public Pose CreatePose() 
        {
            Pose pose = new Pose();
            foreach (Transform bone in _transforms.Values)
            {
                pose.AddBone(new Bone(bone));
            }
            return pose;
        }


        public void ApplyPose() => ApplyPose(currentPose);
        public void ApplyBindPose()
        {
            foreach (string boneName in _transforms.Keys)
            {
                BoneTransform world = bindPose.GetModelTransform(boneName);
                world.position.Scale(scale);
                _transforms[boneName].SetPositionAndRotation(world.position, world.rotation);
            }
        }

        public void ApplyPose(Pose pose)
        {
            currentPose = pose;
            foreach (Bone bone in pose.GetBones())
            {
                BoneTransform model = pose.CalculateParentModelTransform(bone) + bone.local;
                bone.model = model;

                model.position.Scale(scale);
                BoneTransform world = pose.ModelToWorld(model);
                _transforms[bone.boneName].SetPositionAndRotation(world.position, world.rotation);
            }
        }

        public Transform GetTransform(string boneName) => _transforms[boneName];

        public Bone[] GetChildren(Bone bone)
        {
            Transform transform = _transforms[bone.boneName];

            Bone[] children = new Bone[transform.childCount];

            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = currentPose[transform.GetChild(i).name];
            }

            return children;
        }

        public Chain[] GetAllChains() => _chains.ToArray();

        public Chain[] GetChains(SourceChain sourceChain, SourceSide side = SourceSide.MIDDLE)
        {
            List<Chain> matchingChains = new List<Chain>();

            foreach (Chain chain in _chains)
            {
                if (chain.side == side && chain.source == sourceChain)
                    matchingChains.Add(chain);
            }

            return matchingChains.ToArray();
        }

        public Bone[] GetBones(SourceBone sourceBone, SourceSide side = SourceSide.MIDDLE)
        {
            List<Bone> matchingBones = new List<Bone>();

            foreach (Bone bone in currentPose.GetBones())
            {
                if (bone.side == side && bone.source == sourceBone)
                    matchingBones.Add(bone);
            }

            return matchingBones.ToArray();
        }

        public bool HasParent(Bone bone)
        {
            return _transforms.TryGetValue(bone.parentName, out _);
        }
    }
}
