using System;
using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
    public struct BoneTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Vector3 forward { get => TransformDirection(Vector3.forward); }
        public Vector3 back { get => TransformDirection(Vector3.back); }
        public Vector3 up { get => TransformDirection(Vector3.up); }
        public Vector3 down { get => TransformDirection(Vector3.down); }
        public Vector3 right { get => TransformDirection(Vector3.right); }
        public Vector3 left { get => TransformDirection(Vector3.left); }

        public static BoneTransform zero => new BoneTransform(Vector3.zero, Quaternion.identity);

        public BoneTransform(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.lossyScale;
        }

        public BoneTransform(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
            scale = Vector3.one;
        }

        public BoneTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public void CopyToTransform(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }


        /// <summary>
        /// Transforms a point from world space to local space. The opposite of TransformPoint
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector3 InverseTransformPoint(Vector3 point)
        {
            point.Scale(new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z));
            Vector3 result = Quaternion.Inverse(rotation) * (point - position);
            
            return result;
        }

        /// <summary>
        /// Transforms a point from local space to world space. The opposite of InverseTransformPoint
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 point)
        {
            Vector3 result = position + (rotation * point); 
            result.Scale(scale);
            return result;
        }

        /// <summary>
        /// Transforms a direction from world space to local space. The opposite of TransformDirection
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            Vector3 result = Quaternion.Inverse(rotation) * direction;
            return result;
        }

        /// <summary>
        /// Transforms a direction from local space to world space. The opposite of InverseTransformDirection
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 TransformDirection(Vector3 direction)
        {
            Vector3 result = rotation * direction;
            return result;
        }

        /// <summary>
        /// Transforms a transform from local space to world space.
        /// </summary>
        /// <param name="parent">The transform which acts as the world transform</param>
        /// <param name="child">The transform which acts as the local transform</param>
        /// <returns>A transform repesenting the child transform in the parents space</returns>
        public static BoneTransform operator +(BoneTransform parent, BoneTransform child)
        {
            Vector3 pos = parent.TransformPoint(child.position);
            Quaternion rot = parent.rotation * child.rotation;

            return new BoneTransform(pos, rot);
        }

        /// <summary>
        /// Transforms a transform from world space to local space.
        /// </summary>
        /// <param name="parent">The transform which acts as the world transform</param>
        /// <param name="child">The transform which acts as the local transform</param>
        /// <returns></returns>
        public static BoneTransform operator -(BoneTransform parent, BoneTransform child)
        {
            Vector3 pos = parent.InverseTransformPoint(child.position);
            Quaternion rot = Quaternion.Inverse(parent.rotation) * child.rotation;

            return new BoneTransform(pos, rot);
        }

        public Matrix4x4 GetMatrix()
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, rotation * Vector3.right);
            m.SetColumn(1, rotation * Vector3.up);
            m.SetColumn(2, rotation * Vector3.forward);
            m.SetColumn(3, position);
            m[3, 3] = 1;
            return m;
        }

        public static BoneTransform Lerp(BoneTransform from, BoneTransform to, float t)
        {
            if (t <= 0)
                return from;
            if (t >= 1)
                return to;

            Vector3 pos = Vector3.Lerp(from.position, to.position, t);
            Quaternion rot = Quaternion.Lerp(from.rotation, to.rotation, t);
            //Vector3 scale = Vector3.Lerp(from.scale, to.scale, t);

            return new BoneTransform(pos, rot);
        }
        public static BoneTransform Slerp(BoneTransform from, BoneTransform to, float t)
        {
            if (t <= 0)
                return from;
            if (t >= 1)
                return to;

            Vector3 pos = Vector3.Slerp(from.position, to.position, t);
            Quaternion rot = Quaternion.Slerp(from.rotation, to.rotation, t);
            //Vector3 scale = Vector3.Slerp(from.scale, to.scale, t);

            return new BoneTransform(pos, rot);
        }

        public override string ToString()
        {
            return $"{position}\n{rotation}\n{scale}";
        }
    }
}
