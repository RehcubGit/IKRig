using System.Collections.Generic;
using UnityEngine;

namespace Rehcub
{
    public class IKAnimationData : ScriptableObject
    {
        public IKAnimation animation => _animation;
        [SerializeField] private IKAnimation _animation;

        public string animationName => _animationName;
        [SerializeField] private string _animationName;


        public bool loop => _loop;
        [SerializeField] private bool _loop;

        public bool applyRootMotion => _applyRootMotion;
        [SerializeField] private bool _applyRootMotion;


        public static IKAnimationData Create(string name, IKAnimation animation)
        {
            IKAnimationData data = CreateInstance<IKAnimationData>();

            data._animation = animation;
            data._animationName = name;
            return data;
        }

        public static IKAnimationData Create(string name, IKPose[] poses)
        {
            IKAnimation animation = new IKAnimation(poses.Length);

            for (int i = 0; i < poses.Length; i++)
            {
                animation.AddKeyframe(poses[i], i);
            }
            return Create(name, animation);
        }
        
    }
}
