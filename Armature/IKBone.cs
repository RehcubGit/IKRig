﻿using UnityEngine;

namespace Rehcub
{
    [System.Serializable]
	public class IKBone
	{
		public Vector3 direction;
		public Vector3 twist;

		public Axis axis;
		public Axis sourceAxis;

        public IKBone() {}

        public IKBone(Vector3 direction, Vector3 twist)
        {
            this.direction = direction;
            this.twist = twist;
        }

		public IKBone Copy()
		{
			IKBone copy = new IKBone
			{
				direction = direction.normalized,
				twist = twist.normalized,
				sourceAxis = sourceAxis
			};

			return copy;
		}

		public static IKBone Lerp(IKBone from, IKBone to, float t)
		{
			if (t <= 0)
				return from;
			if (t >= 1)
				return to;
			IKBone blended = new IKBone
			{
				direction = Vector3.Lerp(from.direction, to.direction, t).normalized,
				twist = Vector3.Lerp(from.twist, to.twist, t).normalized,
				sourceAxis = from.sourceAxis
			};

			return blended;
		}

		public static IKBone Slerp(IKBone from, IKBone to, float t)
		{
			if (t <= 0)
				return from;
			if (t >= 1)
				return to;
			IKBone blended = new IKBone
			{
				direction = Vector3.Slerp(from.direction, to.direction, t).normalized,
				twist = Vector3.Slerp(from.twist, to.twist, t).normalized,
				sourceAxis = from.sourceAxis
			};

			return blended;
		}
	}
}
