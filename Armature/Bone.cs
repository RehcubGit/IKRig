using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
    public class Bone
    {
        [ReadOnly]
        public string boneName;
        [ReadOnly]
        public string parentName;
        public BoneTransform model;
        public BoneTransform local;
        [ReadOnly]
        public float length = 0.1f;

        public SourceSide side;
        public SourceBone source;

        public Vector3 alternativeForward = Vector3.forward;
        public Vector3 alternativeUp = Vector3.up;

        public Bone(Transform transform)
        {
            boneName = transform.gameObject.name;
            parentName = transform.parent.gameObject.name;
            model = new BoneTransform(transform);
            local = new BoneTransform(transform.localPosition, transform.localRotation);

            if (transform.childCount <= 0)
                return;

            //ComputeForwardAxis(transform.GetChild(0));
            alternativeForward = transform.InverseTransformDirection((transform.GetChild(0).position - transform.position).normalized);
            Axis axis = new Axis(alternativeForward, Vector3.forward);
            alternativeUp = axis.up;
            ComputeLength(transform.GetChild(0));
        }

        public Bone(Bone bone)
        {
            boneName = bone.boneName;
            parentName = bone.parentName;
            model = bone.model;
            local = bone.local;

            length = bone.length;

            side = bone.side;
            source = bone.source;

            alternativeForward = bone.alternativeForward;
            alternativeUp = bone.alternativeUp;
        }

        public void Update(Transform transform)
        {
            model = new BoneTransform(transform);
            local = new BoneTransform(transform.localPosition, transform.localRotation);
        }

        public void ComputeForwardAxis(Bone child) => alternativeForward = (child.model.position - model.position).normalized;

        public void ComputeLength(Bone child) => length = Vector3.Distance(model.position, child.model.position);

        public void ComputeForwardAxis(Transform child) => alternativeForward = (child.position - model.position).normalized;

        public void ComputeLength(Transform child) => length = Vector3.Distance(model.position, child.position);

        //public void Solve(Armature armature, IKBone ikBone) => Solve(armature, ikBone, alternativeForward, alternativeUp);

        public void Solve(Armature armature, IKBone ikBone)
        {
            Vector3 forward = Vector3.forward;
            Vector3 up = Vector3.up;

            BoneTransform bindModel = armature.bindPose.GetModelTransform(boneName);

            // Compute our Quat Inverse Direction, using the Defined Look&Twist Direction
            Quaternion inverseBind = Quaternion.Inverse(bindModel.rotation);
            Vector3 alternativeForward = inverseBind * forward;
            Vector3 alternativeUp = inverseBind * up;

            Solve(armature, ikBone, alternativeForward, alternativeUp);
        }

        private void Solve(Armature armature, IKBone ikBone, Vector3 alternativeForward, Vector3 alternativeUp)
        {
            BoneTransform bindLocal = armature.bindPose.GetLocalTransform(boneName);
            BoneTransform parentTransform = armature.currentPose.GetParentModelTransform(this);
            BoneTransform childTransform = parentTransform + bindLocal;

            Vector3 forward = childTransform.rotation * alternativeForward;
            Quaternion rot = Quaternion.FromToRotation(forward, ikBone.direction) * childTransform.rotation;

            Vector3 up = rot * alternativeUp;
            rot = Quaternion.FromToRotation(up, ikBone.twist) * rot;

            rot = Quaternion.Inverse(parentTransform.rotation) * rot;

            armature.currentPose.SetBoneLocal(boneName, rot);
        }
    }
}
