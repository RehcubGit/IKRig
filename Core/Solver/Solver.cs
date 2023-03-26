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
        /// <param name="targetAxis">Axis to match</param>
        /// <returns>Rotation of the first bone to match the given axis (in model space)</returns>
        protected Quaternion AimBone(BindPose bindPose, BoneTransform parentTransform, Axis targetAxis)
        {
            BoneTransform bindLocal = bindPose.GetLocalTransform(_chain.First());
            BoneTransform childTransform = parentTransform + bindLocal;

            Quaternion q = childTransform.rotation;

            Vector3 direction = q * _chain.alternativeForward;
            q = Quaternion.FromToRotation(direction, targetAxis.forward) * q;

            direction = q * _chain.alternativeUp;
            float twist = Vector3.SignedAngle(direction, targetAxis.up, targetAxis.forward);
            q = Quaternion.AngleAxis(twist, targetAxis.forward) * q;

            return q;
        }

        /// <summary>
        /// Aligns the bone to the parent. (this is nessesary if the Bind pose is not a perfect T-Pose)
        /// </summary>
        /// <param name="bindPose">Bindpose of the Armature</param>
        /// <param name="bone">Bone to align</param>
        /// <param name="parentTransform">Parent transform of the bone</param>
        /// <returns>The Quaternion in model space</returns>
        protected Quaternion AlignToParent(BindPose bindPose, Bone bone, BoneTransform parentTransform)
        {
            BoneTransform bindParent = bindPose.GetParentModelTransform(bone);
            BoneTransform bindModel = bindPose.GetModelTransform(bone);

            Axis parentAxis = bindPose.GetAxis(bone.parentName);
            parentAxis.Rotate(bindParent.rotation);

            Axis boneAxis = bindPose.GetAxis(bone.boneName);

            Quaternion rot = bindModel.rotation;
            rot = Quaternion.FromToRotation(bindModel.rotation * boneAxis.forward, parentAxis.forward) * rot;
            
            float twist = Vector3.SignedAngle(rot * boneAxis.up, parentAxis.up, parentAxis.forward);
            rot = Quaternion.AngleAxis(twist, parentAxis.forward) * rot;

            rot = Quaternion.Inverse(bindParent.rotation) * rot;
            rot = parentTransform.rotation * rot;

            return rot;
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
