using System.Collections.Generic;
using UnityEngine;

namespace Rehcub
{
    [CreateAssetMenu(fileName = "Animation", menuName = "Animation/IKAnimation", order = 1)]
    public class IKAnimationData : ScriptableObject
    {
        public IKAnimation animation;
        public string animationName;
        public bool loop;
        public bool mirror;
        public bool cancle;
        public bool extrectRootMotion;
        public bool applyRootMotion;

        [SerializeReference] private List<BoneConstraint> _modifiers = new List<BoneConstraint>();

        public List<BoneConstraint> GetModifiers() => _modifiers;

        public static IKAnimationData CreateIKAnimation(IKPose[] poses)
        {
            string name = "Test";

            IKAnimation ikAnimation = new IKAnimation(poses.Length)
            {
                name = name
            };

            for (int i = 0; i < poses.Length; i++)
            {
                ikAnimation.AddKeyframe(poses[i], i);
            }

            IKAnimationData data = ScriptableObject.CreateInstance<IKAnimationData>();

            data.animation = ikAnimation;
            data.animationName = name;
            return data;
        }

        public IKAnimation ApplyModifier(Armature armature, int index)
        {
            IKAnimation ikAnimation = new IKAnimation(animation.FrameCount)
            {
                name = animationName
            };
            for (int i = 0; i < animation.FrameCount; i++)
            {
                IKPose ikPose = animation.GetFrame(i);
                IKPose newIkPose = ikPose.Modify(armature, _modifiers[index]);
                ikAnimation.AddKeyframe(newIkPose, i);
            }
            animation = ikAnimation;
            return ikAnimation;
        }

        public IKAnimation ApplyCurve(Armature armature, Vector3[] positions)
        {
            throw new System.Exception("Apply curve is not implemented!");
            /*IKAnimation ikAnimation = new IKAnimation(animation.FrameCount)
            {
                name = animationName
            };

            for (int i = 0; i < animation.FrameCount; i++)
            {
                IKPose ikPose = animation.GetFrame(i);

                ikPose.ApplyHip(armature);

                Vector3 targetPosition = positions[i];
                //targetPosition.y -= hip.position.y;
                if (extrectRootMotion)
                    targetPosition -= ikPose.GetRootMotion(armature);

                Vector3 startPosition = armature.currentPose.GetModelTransform(armature.leftLeg.First()).position;
                ikPose.leftLeg = new IKChain(startPosition, targetPosition, ikPose.leftLeg.jointDirection, armature.leftLeg.length);

                ikAnimation.AddKeyframe(ikPose, i);
            }
            //animation = ikAnimation;
            return ikAnimation;*/
        }
    }
}
