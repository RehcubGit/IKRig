using UnityEngine;

namespace Rehcub 
{
    public interface IEndEffector 
    {
        void Apply();

        /// <summary>
        /// Adjust target ex.: Raycast on the ground
        /// </summary>
        /// <param name="start"> of the chain in world space</param>
        /// <param name="target"> of the chain in world space</param>
        /// <returns>The adjusted target Transform</returns>
        BoneTransform AdjustTarget(Vector3 start, BoneTransform target);
    }
}
