using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public abstract class Solver
    {
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

        protected Quaternion AimBone(BindPose bindPose, BoneTransform parentTransform, Axis axis)
        {
            BoneTransform bindLocal = bindPose.GetLocalTransform(_chain.First());
            BoneTransform childTransform = parentTransform + bindLocal;

            Quaternion rotation = childTransform.rotation;

            Vector3 direction = rotation * _chain.alternativeForward;
            rotation = Quaternion.FromToRotation(direction, axis.forward) * rotation;

            direction = rotation * _chain.alternativeUp;
            rotation = Quaternion.FromToRotation(direction, axis.up) * rotation;
            return rotation;
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

            for (int i = 0; i < _chain.count - 1; i++)
            {
                Bone poseBone = pose[_chain[i].boneName];

                Quaternion rotation = parentTransform.rotation;
                if (i == 0)
                    rotation = AimBone(bindPose, parentTransform, axis);

                //TODO: Get Rotation in which the bone forward vectors point in the same direction!
                rotation = Quaternion.Inverse(parentTransform.rotation) * rotation;

                pose.SetBoneLocal(poseBone.boneName, rotation);
                parentTransform = pose.GetModelTransform(_chain[i]);
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

        protected Axis GetAlternativeAxis(BindPose bindPose, BoneTransform parentTransform)
        {
            BoneTransform bindLocal = bindPose.GetLocalTransform(_chain.First());
            BoneTransform childTransform = parentTransform + bindLocal;

            Quaternion q = childTransform.rotation;

            Vector3 forward = q * _chain.alternativeForward;
            Vector3 up = q * _chain.alternativeUp;

            return new Axis(forward, up);
        }

        protected Axis GetAlternativeAxis(BindPose bindPose, BoneTransform parentTransform, Vector3 direction)
        {
            BoneTransform bindLocal = bindPose.GetLocalTransform(_chain.First());
            BoneTransform childTransform = parentTransform + bindLocal;

            Quaternion q = childTransform.rotation;

            Vector3 forward = q * _chain.alternativeForward;
            q = Quaternion.FromToRotation(forward, direction) * q;
            Vector3 up = q * _chain.alternativeUp;

            return new Axis(forward, up);
        }

        protected Axis GetAxis(IKChain ikChain) => new Axis(ikChain.direction, ikChain.jointDirection);

        protected float GetLength(IKChain ikChain) => _chain.length * ikChain.lengthScale;
    }
}
