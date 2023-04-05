using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rehcub 
{
    public static class AnimationClipFactory
    {
        public static void Create(IKRig rig, IKAnimationData animationData)
        {
            IKAnimation animation = animationData.animation;
            string animationName = animationData.animationName; 
            
            AnimationClip clip = new AnimationClip
            {
                frameRate = 30
            };
            if (animationData.loop)
                clip.wrapMode = WrapMode.Loop;

            

            Dictionary<string, AnimationCurve[]> curves = CreateDictionary(rig);

            AnimationCurve[] hipPositionCurves = CreatePositionCurves();
            AnimationCurve[] rootPositionCurves = CreatePositionCurves();
            AnimationCurve[] rootRotationCurves = CreateRotationCurves();

            ProcessAnimation(rig, animationData, curves, hipPositionCurves, rootPositionCurves, rootRotationCurves);
            ApplyCurvesToClip(rig, clip, curves, hipPositionCurves, rootPositionCurves, rootRotationCurves);

            SaveClip(clip, animationName);
        }

        private static Dictionary<string, AnimationCurve[]> CreateDictionary(IKRig rig)
        {
            string[] boneNames = rig.Armature.currentPose.GetNames().ToArray();
            Dictionary<string, AnimationCurve[]> curves = new Dictionary<string, AnimationCurve[]>();

            for (int j = 0; j < boneNames.Length; j++)
            {
                string boneName = boneNames[j];
                curves[boneName] = CreateRotationCurves();
            }

            return curves;
        }

        private static AnimationCurve[] CreatePositionCurves()
        {
            AnimationCurve[] positionCurves = new AnimationCurve[3];
            positionCurves[0] = new AnimationCurve();
            positionCurves[1] = new AnimationCurve();
            positionCurves[2] = new AnimationCurve();
            return positionCurves;
        }

        private static AnimationCurve[] CreateRotationCurves()
        {
            AnimationCurve[] rotationCurves = new AnimationCurve[4];
            rotationCurves[0] = new AnimationCurve();
            rotationCurves[1] = new AnimationCurve();
            rotationCurves[2] = new AnimationCurve();
            rotationCurves[3] = new AnimationCurve();
            return rotationCurves;
        }

        private static void ProcessAnimation(IKRig rig, IKAnimationData animationData, 
            Dictionary<string, AnimationCurve[]> curves, 
            AnimationCurve[] hipPositionCurves,
            AnimationCurve[] rootPositionCurves, 
            AnimationCurve[] rootRotationCurves)
        {
            //TODO: hardcoded FPS!!!
            float frameTime = 1.0f / 30f;
            string hipName = rig.Armature.GetBones(SourceBone.HIP).First().boneName;

            for (int i = 0; i < animationData.animation.FrameCount; i++)
            {
                rig.ApplyIkPose(animationData, i);

                float time = i * frameTime;

                foreach (string boneName in curves.Keys)
                {
                    BoneTransform boneTransform = rig.Armature.currentPose.GetLocalTransform(boneName);
                    AddRotationKey(boneTransform.rotation, curves[boneName], time);
                }

                Vector3 position = rig.Armature.currentPose.GetLocalTransform(hipName).position;
                AddPositionKey(position, hipPositionCurves, time);

                BoneTransform rootTransform = rig.Armature.currentPose.rootTransform;
                AddPositionKey(rootTransform.position, rootPositionCurves, time);
                AddRotationKey(rootTransform.rotation, rootRotationCurves, time);
            }
        }

        private static void ApplyCurvesToClip(IKRig rig, AnimationClip clip,
            Dictionary<string, AnimationCurve[]> curves,
            AnimationCurve[] hipPositionCurves,
            AnimationCurve[] rootPositionCurves,
            AnimationCurve[] rootRotationCurves)
        {
            string hipName = rig.Armature.GetBones(SourceBone.HIP).First().boneName;
            Transform root = rig.transform;
            Transform bone = rig.Armature.GetTransform(hipName);
            string hipPath = GetPath(root, bone);


            //https://forum.unity.com/threads/new-animationclip-property-names.367288/
            foreach (string boneName in curves.Keys)
            {
                bone = rig.Armature.GetTransform(boneName);
                string path = GetPath(root, bone);
                AnimationCurve[] rotationCurves = curves[boneName];
                SetRotationCurve(clip, rotationCurves, path, "localRotation");
            }

            SetPositionCurve(clip, hipPositionCurves, hipPath, "localPosition");
            SetPositionCurve(clip, typeof(Animator), rootPositionCurves, "", "MotionT");
            SetRotationCurve(clip, typeof(Animator), rootRotationCurves, "", "MotionQ");
        }

        private static void SaveClip(AnimationClip clip, string name)
        {
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/AnimationClips/{name}.anim");
            AssetDatabase.CreateAsset(clip, assetPath);
            AssetDatabase.SaveAssets();
        }

        private static void AddPositionKey(Vector3 position, AnimationCurve[] curves, float time)
        {
            curves[0].AddKey(time, position.x);
            curves[1].AddKey(time, position.y);
            curves[2].AddKey(time, position.z);
        }

        private static void AddRotationKey(Quaternion rotation, AnimationCurve[] curves, float time)
        {
            curves[0].AddKey(time, rotation.x);
            curves[1].AddKey(time, rotation.y);
            curves[2].AddKey(time, rotation.z);
            curves[3].AddKey(time, rotation.w);
        }

        private static void SetPositionCurve(AnimationClip clip, AnimationCurve[] positionCurves, string path, string property)
        {
            clip.SetCurve(path, typeof(Transform), property + ".x", positionCurves[0]);
            clip.SetCurve(path, typeof(Transform), property + ".y", positionCurves[1]);
            clip.SetCurve(path, typeof(Transform), property + ".z", positionCurves[2]);
        }
        private static void SetPositionCurve(AnimationClip clip, Type type, AnimationCurve[] positionCurves, string path, string property)
        {
            clip.SetCurve(path, type, property + ".x", positionCurves[0]);
            clip.SetCurve(path, type, property + ".y", positionCurves[1]);
            clip.SetCurve(path, type, property + ".z", positionCurves[2]);
        }
        private static void SetRotationCurve(AnimationClip clip, AnimationCurve[] positionCurves, string path, string property)
        {
            clip.SetCurve(path, typeof(Transform), property + ".x", positionCurves[0]);
            clip.SetCurve(path, typeof(Transform), property + ".y", positionCurves[1]);
            clip.SetCurve(path, typeof(Transform), property + ".z", positionCurves[2]);
            clip.SetCurve(path, typeof(Transform), property + ".w", positionCurves[3]);
        }
        private static void SetRotationCurve(AnimationClip clip, Type type, AnimationCurve[] positionCurves, string path, string property)
        {
            clip.SetCurve(path, type, property + ".x", positionCurves[0]);
            clip.SetCurve(path, type, property + ".y", positionCurves[1]);
            clip.SetCurve(path, type, property + ".z", positionCurves[2]);
            clip.SetCurve(path, type, property + ".w", positionCurves[3]);
        }

        private static string GetPath(Transform root, Transform bone)
        {
            Transform parent = bone.parent;
            string path = bone.name;

            while (parent != root)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
