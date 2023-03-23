using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
    public struct Axis
    {
        public Vector3 pos;
        public Vector3 right { get => -left; }
        public Vector3 left;

        public Vector3 down { get => -up; }
        public Vector3 up;

        public Vector3 back { get => -forward; }
        public Vector3 forward;

        public static Axis normal => new Axis(Vector3.forward, Vector3.up);

        public Axis(Vector3 forward, Vector3 up)
        {
            pos = Vector3.zero;
            this.forward = forward.normalized;
            left = Vector3.Cross(up, this.forward).normalized;
            this.up = Vector3.Cross(this.forward, left).normalized;

            //Vector3.OrthoNormalize(ref forward, ref up, ref left);
        }

        public static Axis FromRotation(Quaternion rotation)
        {
            Vector3 forward = rotation * Vector3.forward;
            Vector3 up = rotation * Vector3.up;

            return new Axis(forward, up);
        }

        /// <summary>
        /// Get the rotational differance between two Axes
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>The Rotation that would rotate lhs to rhs</returns>
        public static Quaternion GetRotationOfAxes(Axis lhs, Axis rhs)
        {
            Quaternion rotation = Quaternion.FromToRotation(lhs.forward, rhs.forward);

            Vector3 up = rotation * rhs.up;
            rotation = Quaternion.FromToRotation(lhs.up, up) * rotation;

            //Quaternion rotation = Quaternion.Inverse(lhs.GetRotation()) * rhs.GetRotation();

            return rotation;
        }

        public Quaternion GetRotation()
        {
            return Quaternion.LookRotation(forward, up);
        }

        public static Axis Rotate(Axis axis, Quaternion rotaion)
        {
            return new Axis(rotaion * axis.forward, rotaion * axis.up);
        }
        public void Rotate(Quaternion rotaion)
        {
            forward = rotaion * forward;
            up = rotaion * up;
        }

        public static Axis Lerp(Axis from, Axis to, float t) => new Axis(Vector3.Lerp(from.forward, to.forward, t), Vector3.Lerp(from.up, to.up, t));
        public static Axis Slerp(Axis from, Axis to, float t) => new Axis(Vector3.Slerp(from.forward, to.forward, t), Vector3.Slerp(from.up, to.up, t));
    }
}
