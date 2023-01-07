using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public abstract class BoneConstraint
    {
        [HideInInspector]
        public bool visible = true;

        public abstract void Apply(IKPose pose, Armature armature);
    }
}
