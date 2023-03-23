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
    }
}
