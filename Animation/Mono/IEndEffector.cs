using UnityEngine;

namespace Rehcub 
{
    public interface IEndEffector 
    {
        void Apply();

        BoneTransform AqustTarget(Vector3 start, BoneTransform target);
    }
}
