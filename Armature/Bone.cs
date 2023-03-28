using System.Collections.Generic;
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
        [ReadOnly]
        public List<string> childNames;
        public BoneTransform model;
        public BoneTransform local;
        public float length = 0.1f;

        public SourceSide side;
        public SourceBone source;

        public Axis axis => new Axis(alternativeForward, alternativeUp);
        public Vector3 alternativeForward = Vector3.forward;
        public Vector3 alternativeUp = Vector3.up;

        public Bone(Transform transform)
        {
            boneName = transform.gameObject.name;
            parentName = transform.parent.gameObject.name;
            childNames = new List<string>();
            model = new BoneTransform(transform)
            {
                scale = Vector3.one
            };
            local = new BoneTransform(transform.localPosition, transform.localRotation)
            {
                scale = Vector3.one
            };

            if (transform.childCount <= 0)
                return;

            //ComputeForwardAxis(transform.GetChild(0));
            /*alternativeForward = transform.InverseTransformDirection((transform.GetChild(0).position - transform.position).normalized);
            Axis axis = new Axis(alternativeForward, Vector3.forward);
            alternativeUp = axis.up;
            ComputeLength(transform.GetChild(0));*/
        }

        public Bone(Bone bone)
        {
            boneName = bone.boneName;
            parentName = bone.parentName; 
            childNames = new List<string>();
            childNames.AddRange(bone.childNames);
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
            model = new BoneTransform(transform)
            {
                scale = Vector3.one
            };
            local = new BoneTransform(transform.localPosition, transform.localRotation)
            {
                scale = Vector3.one
            };
        }

        public void UpdateLocal(Transform parent)
        {
            Vector3 position = parent.InverseTransformPoint(model.position);
            Quaternion rotation = Quaternion.Inverse(parent.rotation) * model.rotation;
            local = new BoneTransform(position, rotation);
        }
        public void UpdateLocal(BoneTransform parent)
        {
            Vector3 position = parent.InverseTransformPoint(model.position);
            Quaternion rotation = Quaternion.Inverse(parent.rotation) * model.rotation;
            local = new BoneTransform(position, rotation);
        }

        public void ComputeForwardAxis(Bone child) => alternativeForward = model.InverseTransformDirection(child.model.position - model.position).normalized;

        public void ComputeLength(Bone child) => length = Vector3.Distance(model.position, child.model.position);

        public void ComputeForwardAxis(Transform child) => alternativeForward = (child.position - model.position).normalized;

        public void ComputeLength(Transform child) => length = Vector3.Distance(model.position, child.position);

        public void Solve(Armature armature, IKBone ikBone) => Solve(armature, armature.currentPose, ikBone);

        public void Solve(Armature armature, Pose pose, IKBone ikBone)
        {
            BoneTransform bindModel = armature.bindPose.GetModelTransform(boneName);

            Axis axis = new Axis(alternativeForward, alternativeUp);
            Axis globalAxis = Axis.Rotate(axis, bindModel.rotation);

            Quaternion sourceDifference = Quaternion.FromToRotation(ikBone.sourceAxis.forward, globalAxis.forward);

            axis = Axis.Rotate(new Axis(ikBone.direction, ikBone.twist), sourceDifference);
            Solve(armature.bindPose, pose, axis);
        }

        public void Solve(BindPose bindPose, Pose pose, Axis target)
        {
            Axis axis = new Axis(alternativeForward, alternativeUp);
            BoneTransform childTransform = bindPose.GetModelTransform(this);

            Vector3 forward = childTransform.rotation * axis.forward;
            Quaternion rotation = Quaternion.FromToRotation(forward, target.forward) * childTransform.rotation;

            Vector3 up = rotation * axis.up;
            rotation = Quaternion.FromToRotation(up, target.up) * rotation;

            rotation.Normalize();
            pose.SetBoneModel(boneName, rotation);
        }
    }
}
