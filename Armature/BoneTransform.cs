using System;
using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
    public struct BoneTransform
    {
        public Vector3 position;
        public Quaternion rotation;

        public static BoneTransform zero => new BoneTransform(Vector3.zero, Quaternion.identity);

        public BoneTransform(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
        }

        public BoneTransform(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public void CopyToTransform(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        public static BoneTransform operator +(BoneTransform parent, BoneTransform child)
        {
            Vector3 pos = parent.position + (parent.rotation * child.position);
            Quaternion rot = parent.rotation * child.rotation;

            BoneTransform transform = new BoneTransform
            {
                position = pos,
                rotation = rot
            };

            return transform;
        }

        public static Vector3 operator +(BoneTransform parent, Vector3 v)
        {
            Vector3 result = parent.rotation * v + parent.position;
            return result;
        }

        public static Quaternion operator +(BoneTransform parent, Quaternion q)
        {
            Quaternion result = parent.rotation * q;
            return result;
        }

        public static BoneTransform operator -(BoneTransform parent, BoneTransform child)
        {
            Vector3 pos = parent - child.position;
            Quaternion rot = parent - child.rotation;

            BoneTransform transform = new BoneTransform
            {
                position = pos,
                rotation = rot
            };

            return transform;
        }

        public static Vector3 operator -(BoneTransform parent, Vector3 v)
        {
            Vector3 result = Quaternion.Inverse(parent.rotation) * (parent.position - v);
            return result;
        }

        public static Quaternion operator -(BoneTransform parent, Quaternion q)
        {
            Quaternion result = Quaternion.Inverse(parent.rotation) * q;
            return result;
        }


        public BoneTransform Copy()
        {
            BoneTransform transform = new BoneTransform
            {
                position = new Vector3(position.x, position.y, position.z),
                rotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w)
            };

            return transform;
        }

        public override string ToString()
        {
            return $"{position}\n{rotation}";
        }
    }
}
