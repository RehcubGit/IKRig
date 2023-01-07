using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
	public class IKChain
    {
        public float lengthScale;
        public Vector3 direction;
        public Vector3 jointDirection;

		public IKBone endEffector;

        public IKChain()
        {
			endEffector = new IKBone();
		}

        public IKChain(Vector3 position, Vector3 target, Vector3 jointDirection, float chainLength)
		{
			Vector3 direction = target - position;

			lengthScale = direction.magnitude / chainLength;

			Axis axis = new Axis(direction.normalized, jointDirection, position);
			this.direction = axis.forward;
			this.jointDirection = axis.up;

			endEffector = new IKBone();
		}

        public IKChain(Vector3 position, Vector3 target, Vector3 jointDirection, float chainLength, IKBone endEffector)
		{
			Vector3 direction = target - position;

			lengthScale = direction.magnitude / chainLength;

			Axis axis = new Axis(direction.normalized, jointDirection, position);
			this.direction = axis.forward;
			this.jointDirection = axis.up;

			this.endEffector = endEffector;
		}

		public Vector3 GetTargetPosition(Vector3 start, float chainLength)
        {
			return start + direction * (chainLength * lengthScale);
        }
		public Vector3 GetTargetPosition(Chain chain, BoneTransform hip)
        {
			Vector3 start = hip + chain.First().local.position;
			return start + direction * (chain.length * lengthScale);
        }
		public BoneTransform GetTargetTransform(Chain chain, BoneTransform hip)
        {
			Vector3 start = hip + chain.First().local.position;
			Vector3 pos = start + direction * (chain.length * lengthScale);
			return BoneTransform.zero;
        }

		public void SetTarget(Vector3 target, Vector3 start, float chainLength)
        {
			Vector3 ikDirection = target - start;
			float ikLength = ikDirection.magnitude;
			ikDirection.Normalize();

			Vector3 leftDir = Vector3.Cross(jointDirection, ikDirection).normalized;

			lengthScale = ikLength / chainLength;
			direction = ikDirection;
			jointDirection = Vector3.Cross(ikDirection, leftDir).normalized;
		}

		public IKChain Copy()
		{
			IKChain copy = new IKChain
			{
				lengthScale = lengthScale,
				direction = direction,
				jointDirection = jointDirection,
				endEffector = endEffector.Copy()
			};

			return copy;
		}

		public static IKChain Lerp(IKChain from, IKChain to, float t)
		{
			if (t <= 0)
				return from;
			if (t >= 1)
				return to;
			IKChain blended = new IKChain
			{
				direction = Vector3.Lerp(from.direction, to.direction, t),
				jointDirection = Vector3.Lerp(from.jointDirection, to.jointDirection, t),
				lengthScale = Mathf.Lerp(from.lengthScale, to.lengthScale, t),
				endEffector = IKBone.Lerp(from.endEffector, to.endEffector, t)
			};

			return blended;
		}

		public static IKChain Slerp(IKChain from, IKChain to, float t)
		{
			if (t <= 0)
				return from;
			if (t >= 1)
				return to;
			IKChain blended = new IKChain
			{
				direction = Vector3.Slerp(from.direction, to.direction, t),
				jointDirection = Vector3.Slerp(from.jointDirection, to.jointDirection, t),
				lengthScale = Mathf.Lerp(from.lengthScale, to.lengthScale, t),
				endEffector = IKBone.Slerp(from.endEffector, to.endEffector, t)
			};

			return blended;
		}
	}
}
