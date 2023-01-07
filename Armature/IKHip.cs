using System;
using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
	public class IKHip
    {
        public float bindHeight;
        public Vector3 bindOffset;
        public Vector3 movement;
        public Vector3 direction;
        public float twist;

		public IKHip()
        {
			bindOffset = Vector3.zero;
			movement = Vector3.zero;
			direction = Vector3.forward;
        }

		public IKHip Copy()
		{
			IKHip copy = new IKHip
			{
				bindHeight = bindHeight,
				bindOffset = bindOffset.Copy(),
				movement = movement.Copy(),
				direction = direction.Copy(),
				twist = twist
			};

			return copy;
		}

		public static IKHip Lerp(IKHip from, IKHip to, float t)
		{
			if (t <= 0)
				return from;
			if (t >= 1)
				return to;
			IKHip blended = new IKHip
			{
				bindHeight = Mathf.Lerp(from.bindHeight, to.bindHeight, t),
				bindOffset = Vector3.Lerp(from.bindOffset, to.bindOffset, t),
				movement = Vector3.Lerp(from.movement, to.movement, t),
				direction = Vector3.Lerp(from.direction, to.direction, t),
				twist = Mathf.Lerp(from.twist, to.twist, t)
			};

			return blended;
		}

		public static IKHip Slerp(IKHip from, IKHip to, float t)
		{
			if (t <= 0)
				return from;
			if (t >= 1)
				return to;
			IKHip blended = new IKHip
			{
				bindHeight = Mathf.Lerp(from.bindHeight, to.bindHeight, t),
				bindOffset = Vector3.Slerp(from.bindOffset, to.bindOffset, t),
				movement = Vector3.Slerp(from.movement, to.movement, t),
				direction = Vector3.Slerp(from.direction, to.direction, t),
				twist = Mathf.Lerp(from.twist, to.twist, t)
			};

			return blended;
		}

        public Vector3 GetPosition(BoneTransform bindModel)
		{
			float heightScale = bindModel.position.y / bindHeight;
			return movement * heightScale + bindModel.position + bindOffset;
		}
    }
}
