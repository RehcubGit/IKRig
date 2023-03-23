using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public abstract class Solver
    {
        /*[HideInInspector]
        [SerializeField] */
        protected Chain _chain;

        public Solver(Chain chain)
        {
            _chain = chain;
        }

        public abstract void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform);

        public void Check(Chain chain)
        {
            _chain = chain;
        }

        public bool ValidateChain(Chain chain)
        {
            if (this is LimbSolver && chain.count != 3)
                return false;
            if (this is ZigZagSolver && chain.count != 4)
                return false;

            return true;
        }

        /// <summary>
        /// Aligns the first bone of the chain to the given Axis.
        /// </summary>
        /// <param name="bindPose">Bindpose of the Armature</param>
        /// <param name="parentTransform">Parent transform of the first bone</param>
        /// <param name="axis">Axis to match</param>
        /// <returns>Rotation of the first bone to match the given axis (in model space)</returns>
        protected Quaternion AimBone(BindPose bindPose, BoneTransform parentTransform, Axis axis)
        {
            BoneTransform bindLocal = bindPose.GetLocalTransform(_chain.First());
            BoneTransform childTransform = parentTransform + bindLocal;

            Quaternion q = childTransform.rotation;

            Vector3 direction = q * _chain.alternativeForward;
            q = Quaternion.FromToRotation(direction, axis.forward) * q;

            direction = q * _chain.alternativeUp;
            //float twist = Vector3.SignedAngle(direction, axis.up, axis.forward);
            //q = Quaternion.AngleAxis(twist, axis.forward) * q;

            q = Quaternion.FromToRotation(direction, axis.up) * q;
            return q;
        }

        /// <summary>
        /// Aligns the first bone of the chain to the given Axis.
        /// </summary>
        /// <param name="bindPose">Bindpose of the Armature</param>
        /// <param name="parentTransform">Parent transform of the first bone</param>
        /// <param name="targetAxis">Axis to match</param>
        /// <returns>Rotation of the first bone to match the given axis (in model space)</returns>
        protected Quaternion AimBone(BindPose bindPose, Bone bone, BoneTransform parentTransform, Axis targetAxis)
        {
            BoneTransform bindLocal = bindPose.GetLocalTransform(bone);
            BoneTransform childTransform = parentTransform + bindLocal;

            Quaternion q = childTransform.rotation;

            Vector3 direction = q * bone.axis.forward;
            q = Quaternion.FromToRotation(direction, targetAxis.forward) * q;

            direction = q * bone.axis.up;
            //float twist = Vector3.SignedAngle(direction, axis.up, axis.forward);
            //q = Quaternion.AngleAxis(twist, axis.forward) * q;

            q = Quaternion.FromToRotation(direction, targetAxis.up) * q;
            return q;
        }

        protected float LawCosSSS(float aLen, float bLen, float cLen)
        {
            float v = (aLen * aLen + bLen * bLen - cLen * cLen) / (2 * aLen * bLen);
            v = Mathf.Clamp(v, -1, 1);
            return Mathf.Acos(v);
        }

        public bool IsOutOfReach(IKChain ikChain) => _chain.length < GetLength(ikChain);

        protected bool HandleOutOfReach(BindPose bindPose, Pose pose, BoneTransform parentTransform, Axis axis, float length)
        {
            if (_chain.length > length)
                return false;

            Quaternion aimRotation = AimBone(bindPose, parentTransform, axis);

            for (int i = 0; i < _chain.count - 1; i++)
            {
                //TODO: this assumes that every bone in the chain has the same axis
                pose.SetBoneModel(_chain[i].boneName, aimRotation);
            }

            //TODO: Save this in the armature or the chain!
            bool isStrechy = false;

            if (isStrechy)
                StretchBones(bindPose, pose, parentTransform, axis, length);

            return true;
        }

        private void StretchBones(BindPose bindPose, Pose pose, BoneTransform parentTransform, Axis axis, float length)
        {
            float chainStretch = length - _chain.length;

            for (int i = 1; i < _chain.count; i++)
            {
                Bone parentBone = pose[_chain[i - 1].boneName];
                Bone bone = pose[_chain[i].boneName];

                float boneStretch = parentBone.length / _chain.length;
                boneStretch *= chainStretch;

                Vector3 position = bone.local.position + axis.forward * boneStretch;

                pose.SetBoneLocal(bone.boneName, position);
            }
        }

        protected Axis GetAxis(IKChain ikChain) => new Axis(ikChain.direction, ikChain.jointDirection);

        protected float GetLength(IKChain ikChain) => _chain.length * ikChain.lengthScale;
    }
}
