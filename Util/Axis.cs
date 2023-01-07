using UnityEngine;

namespace Rehcub
{
    public struct Axis
    {
        public Vector3 pos;
        public Vector3 left;
        public Vector3 up;
        public Vector3 forward;

        public Axis(Vector3 forward, Vector3 up)
        {
            pos = Vector3.zero;
            this.forward = forward.normalized;
            left = Vector3.Cross(up, this.forward).normalized;
            this.up = Vector3.Cross(this.forward, left).normalized;
        }

        public Axis(Vector3 forward, Vector3 up, Vector3 pos)
        {
            this.pos = pos;
            this.forward = forward.normalized;
            left = Vector3.Cross(up.normalized, this.forward).normalized;
            this.up = Vector3.Cross(this.forward, left).normalized;
        }
    }
}
