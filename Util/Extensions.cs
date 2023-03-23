using System.Collections.Generic;
using UnityEngine;

namespace Rehcub 
{
    public static class Extensions
    {
        public const float TAU = 6.28318530717959f;
        public static bool Intersects(this Rect source, Rect rect)
        {
            return !((source.x > rect.xMax) || (source.xMax < rect.x) || (source.y > rect.yMax) || (source.yMax < rect.y));
        }

        #region bool

        public static int ToInt(this bool item) => item ? 1 : 0;
        public static float ToFloat(this bool item) => item ? 1 : 0;
        public static int ToIntNegative(this bool item) => item ? 1 : -1;

        #endregion

        #region int

        public static int Mod(this int x, int period)
        {
            int r = x % period;
            return (r >= 0 ? r : r + period);
        }

        public static int Mod(this int x) { return x.Mod(1); }
        public static int CyclicDiff(int high, int low, int period) => CyclicDiff(high, low, period, false);
        public static int CyclicDiff(int high, int low) => CyclicDiff(high, low, 1, false);
        public static int CyclicDiff(int high, int low, int period, bool skipWrap)
        {
            if (!skipWrap)
            {
                high = Mod(high, period);
                low = Mod(low, period);
            }
            return (high >= low ? high - low : high + period - low);
        }

        public static int WithRandomSign(this int value, float negativeProbability = 0.5f) => Random.value < negativeProbability ? -value : value;

        #endregion

        #region float


        public static float Mod(this float x, float period)
        {
            float r = x % period;
            if (r >= 0)
                return r;
            return r + period;
        }
        public static float Mod(this float x) => x.Mod(1);
        public static float Frac(this float x) => x - Mathf.Floor(x);


        public static float CyclicDiff(float high, float low) => CyclicDiff(high, low, 1, false);
        public static float CyclicDiff(float high, float low, float period) => CyclicDiff(high, low, period, false);
        public static float CyclicDiff(float high, float low, float period, bool skipWrap)
        {
            if (skipWrap == false)
            {
                high = Mod(high, period);
                low = Mod(low, period);
            }

            if (high >= low)
                return high - low;
            return high + period - low;

            //return (high >= low ? high - low : high + period - low);
        }

        public static float LinearRemap(this float value, float valueRangeMin, float valueRangeMax, float newRangeMin, float newRangeMax)
        {
            return (value - valueRangeMin) / (valueRangeMax - valueRangeMin) * (newRangeMax - newRangeMin) + newRangeMin;
        }

        public static float NormalizedDeg(this float rotation) => (rotation + 180f) % 360f - 180f;
        public static float NormalizedRad(this float rotation) => (rotation + Mathf.PI) % TAU - Mathf.PI;
        public static Vector2 AngleToDirection(this float rad) => new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        public static float Copysign(this float i, float s)
        {
            if (s >= 0)
                return Mathf.Abs(i);
            return -Mathf.Abs(i);
        }

        #endregion

        #region Vector2

        public static Vector2 SetX(this Vector2 v, float x) => new Vector2(x, v.y);
        
        public static Vector2 SetY(this Vector2 v, float y) => new Vector2(v.x, y);
        
        public static Vector3 SetZ(this Vector2 v, float z) => new Vector3(v.x, v.y, z);

        public static Vector2 Abs(this Vector2 v)
        {
            v.x = Mathf.Abs(v.x);
            v.y = Mathf.Abs(v.y);
            return v;
        }

        public static Vector2 Clamp(this Vector2 v, float min, float max) => new Vector2(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max));
        public static Vector2 Mod(this Vector2 v, float modulo) => new Vector2(v.x % modulo, v.y % modulo);
        public static Vector2 Rotate(this Vector2 v, float angRad)
        {
            float ca = Mathf.Cos(angRad);
            float sa = Mathf.Sin(angRad);
            return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
        }
        public static Vector2 RotateAround(this Vector2 v, Vector2 pivot, float angRad) => Rotate(v - pivot, angRad) + pivot;
        public static float DirectionToAngle(this Vector2 v) => Mathf.Atan2(v.y, v.x);


        //https://arrowinmyknee.com/2021/03/15/some-math-about-capsule-collision/
        public static float SqrDistanceSegment(this Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ac = p - a;
            Vector2 bc = p - b;
            float e = Vector2.Dot(ac, ab);

            // Handle cases where c projects outside ab
            if (e <= 0.0f) 
                return ac.sqrMagnitude;

            float f = ab.sqrMagnitude;
            if (e >= f) 
                return bc.sqrMagnitude;
            // Handle cases where c projects onto ab
            return ac.sqrMagnitude - e * e / f;
        }

        #endregion

        #region Vector3
        public static Vector2 xy(this Vector3 v) => new Vector2(v.x, v.y);
        
        public static Vector2 xz(this Vector3 v) => new Vector2(v.x, v.z);
        //public static Vector3 xz(this Vector3 v) => new Vector3(v.x, 0f, v.z);
        public static Vector2 zy(this Vector3 v) => new Vector2(v.z, v.y);
        
        public static Vector3 SetX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);

        public static Vector3 SetY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
        
        public static Vector3 SetZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);
        
        public static Vector3 AddX(this Vector3 v, float x) => new Vector3(v.x + x, v.y, v.z);
        
        public static Vector3 AddY(this Vector3 v, float y) => new Vector3(v.x, v.y + y, v.z);
        
        public static Vector3 AddZ(this Vector3 v, float z) => new Vector3(v.x, v.y, v.z + z);

        public static Vector3 SinLerp(Vector3 a, Vector3 b, float t, float amp)
        {
            float tPi = t * Mathf.PI;

            Vector3 ab = b - a;
            return a + (ab * t) + (amp * Mathf.Sin(tPi) * Vector3.up);
        }
        public static Vector3 ConstantSlerp(Vector3 from, Vector3 to, float angle)
        {
            float value = Mathf.Min(1, angle / Vector3.Angle(from, to));
            return Vector3.Slerp(from, to, value);
        }

        public static Vector3 Copy(this Vector3 v) => new Vector3(v.x, v.y, v.z);
        
        // axisDirection - unit vector in direction of an axis (eg, defines a line that passes through zero)
        // point - the point to find nearest on line for
        public static Vector3 NearestPointOnAxis(this Vector3 axisDirection, Vector3 point, bool isNormalized = false)
        {
            if (!isNormalized) axisDirection.Normalize();
            float d = Vector3.Dot(point, axisDirection);
            return axisDirection * d;
        }

        // lineDirection - unit vector in direction of line
        // pointOnLine - a point on the line (allowing us to define an actual line in space)
        // point - the point to find nearest on line for
        public static Vector3 NearestPointOnLine(this Vector3 lineDirection, Vector3 point, Vector3 pointOnLine, bool isNormalized = false)
        {
            if (!isNormalized) lineDirection.Normalize();
            float d = Vector3.Dot(point - pointOnLine, lineDirection);
            return pointOnLine + (lineDirection * d);
        }

        public static Vector3 SetHeight(Vector3 originalVector, Vector3 referenceHeightVector, Vector3 upVector)
        {
            Vector3 originalOnPlane = Vector3.ProjectOnPlane(originalVector, upVector);
            Vector3 referenceOnAxis = Vector3.Project(referenceHeightVector, upVector);
            return originalOnPlane + referenceOnAxis;
        }

        public static Vector3 OrthogonalVector(this Vector3 v)
        {
            Vector3 result = new Vector3(v.z.Copysign(v.x), v.z.Copysign(v.y), -v.x.Copysign(v.z) - Copysign(v.y, v.z));
            return result;
        }

        public static Vector3 Rotate(this Vector3 v, float angRad, Vector3 axis)
        {
            Quaternion rotation = Quaternion.AngleAxis(angRad * Mathf.Rad2Deg, axis);
            return rotation * v;
        }

        public static Vector3 RotateAround(this Vector3 point, Vector3 pivot, Quaternion rotation)
        {
            return (rotation * (point - pivot)) + pivot;
        }

        //https://arrowinmyknee.com/2021/03/15/some-math-about-capsule-collision/
        public static float SqrDistanceSegment(this Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 ac = p - a;
            Vector3 bc = p - b;
            float e = Vector3.Dot(ac, ab);
            // Handle cases where c projects outside ab
            if (e <= 0.0f) 
                return ac.sqrMagnitude;

            float f = ab.sqrMagnitude;

            if (e >= f) 
                return bc.sqrMagnitude;

            // Handle cases where c projects onto ab
            return ac.sqrMagnitude - e * e / f;
        }

        public static float[] GetLineSphereIntersections(Vector3 lineStart, Vector3 lineDir, Vector3 sphereCenter, float sphereRadius)
        {
            /*double a = lineDir.sqrMagnitude;
            double b = 2 * (Vector3.Dot(lineStart, lineDir) - Vector3.Dot(lineDir, sphereCenter));
            double c = lineStart.sqrMagnitude + sphereCenter.sqrMagnitude - 2*Vector3.Dot(lineStart, sphereCenter) - sphereRadius*sphereRadius;
            double d = b*b - 4*a*c;
            if (d<0) return null;
            double i1 = (-b - System.Math.Sqrt(d)) / (2*a);
            double i2 = (-b + System.Math.Sqrt(d)) / (2*a);
            if (i1<i2) return new float[] {(float)i1, (float)i2};
            else       return new float[] {(float)i2, (float)i1};*/

            float a = lineDir.sqrMagnitude;
            float b = 2 * (Vector3.Dot(lineStart, lineDir) - Vector3.Dot(lineDir, sphereCenter));
            float c = lineStart.sqrMagnitude + sphereCenter.sqrMagnitude - 2 * Vector3.Dot(lineStart, sphereCenter) - sphereRadius * sphereRadius;
            float d = b * b - 4 * a * c;
            if (d < 0) return null;
            float i1 = (-b - Mathf.Sqrt(d)) / (2 * a);
            float i2 = (-b + Mathf.Sqrt(d)) / (2 * a);
            if (i1 < i2) return new float[] { i1, i2 };
            else return new float[] { i2, i1 };
        }

        public static void CalculateDirection(this Vector3 point, out int direction, out float distance)
        {
            // Calculate longest axis
            direction = LargestComponent(point);
            distance = point[direction];
        }

        public static Vector3 CalculateDirectionAxis(this Vector3 point)
        {
            CalculateDirection(point, out int direction, out float distance);
            Vector3 axis = Vector3.zero;
            if (distance > 0)
                axis[direction] = 1.0f;
            else
                axis[direction] = -1.0f;
            return axis;
        }

        public static int SmallestComponent(this Vector3 point)
        {
            int direction = 0;
            if (Mathf.Abs(point[1]) < Mathf.Abs(point[0]))
                direction = 1;
            if (Mathf.Abs(point[2]) < Mathf.Abs(point[direction]))
                direction = 2;
            return direction;
        }

        public static int LargestComponent(this Vector3 point)
        {
            int direction = 0;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
                direction = 1;
            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
                direction = 2;
            return direction;
        }

        public static string ToString(this Vector3 v, int digits)
        {
            if(digits == 2)
                return string.Format("{0:N2}, {1:N2}, {2:N2}", v.x, v.y, v.z);
            if(digits == 3)
                return string.Format("{0:N3}, {1:N3}, {2:N3}", v.x, v.y, v.z);
            if(digits == 4)
                return string.Format("{0:N4}, {1:N4}, {2:N4}", v.x, v.y, v.z);
            if(digits == 5)
                return string.Format("{0:N5}, {1:N5}, {2:N5}", v.x, v.y, v.z);
            
            return v.ToString();
        }

        #endregion

        #region Quaternion

        public static Quaternion Rotate(this Quaternion q, Vector3 angularVelocity, float deltaTime)
        {
            Vector3 vec = angularVelocity * deltaTime;
            float length = vec.magnitude;
            if (length < 1E-6F)
                return q;    // Otherwise we'll have division by zero when trying to normalize it later on

            // Convert the rotation vector to quaternion. The following 4 lines are very similar to CreateFromAxisAngle method.
            float half = length * 0.5f;
            float sin = Mathf.Sin(half);
            float cos = Mathf.Cos(half);
            // Instead of normalizing the axis, we multiply W component by the length of it. This method normalizes result in the end.
            Quaternion rot = new Quaternion(vec.x * sin, vec.y * sin, vec.z * sin, length * cos);

            rot *= q;
            rot.Normalize();
            // The following line is not required, only useful for people. Computers are fine with 2 different quaternion representations of each possible rotation.
            if (rot.w < 0) 
                rot = rot.Negate();
            return rot;
        }

        public static Quaternion Negate(this Quaternion q) => new Quaternion(-q.x, -q.y, -q.z, -q.w);

        public static Quaternion ProjectOnPlane(this Quaternion q, Vector3 n)
        {
            q.ToAngleAxis(out _, out Vector3 axis);
            Quaternion rot = Quaternion.FromToRotation(axis.normalized, n.normalized);
            return Quaternion.Inverse(rot) * q;
        }

        public static Quaternion ConstantSlerp(Quaternion from, Quaternion to, float angle)
        {
            float value = Mathf.Min(1, angle / Quaternion.Angle(from, to));
            return Quaternion.Slerp(from, to, value);
        }

        public static Quaternion Clamp(this Quaternion q, Vector3 min, Vector3 max)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, min.x, max.x);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
            angleY = Mathf.Clamp(angleY, min.y, max.y);
            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

            float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
            angleZ = Mathf.Clamp(angleZ, min.z, max.z);
            q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);

            q.Normalize();
            return q;
        }

        public static Quaternion Clamp(this Quaternion q, Vector3 up, Vector3 min, Vector3 max)
        {

            Quaternion original = Quaternion.FromToRotation(Vector3.up, up);
            Vector3 dirToTarget = q * Vector3.forward;
            Vector3 originalForward = Vector3.forward;

            Vector3 xAxis = Vector3.right;
            Vector3 dirYZ = Vector3.ProjectOnPlane(dirToTarget, xAxis);
            Vector3 forwardYZ = Vector3.ProjectOnPlane(originalForward, xAxis);
            float xAngle = Vector3.Angle(dirYZ, forwardYZ) * Mathf.Sign(Vector3.Dot(xAxis, Vector3.Cross(forwardYZ, dirYZ)));
            float xClamped = Mathf.Clamp(xAngle, min.x, max.x);
            Quaternion xRotation = Quaternion.AngleAxis(xClamped, Vector3.right);


            originalForward = xRotation * original * Vector3.forward;
            Vector3 yAxis = xRotation * original * Vector3.up; // our local x axis
            Vector3 dirXZ = Vector3.ProjectOnPlane(dirToTarget, yAxis);
            Vector3 forwardXZ = Vector3.ProjectOnPlane(originalForward, yAxis);
            float yAngle = Vector3.Angle(dirXZ, forwardXZ) * Mathf.Sign(Vector3.Dot(yAxis, Vector3.Cross(forwardXZ, dirXZ)));
            float yClamped = Mathf.Clamp(yAngle, min.y, max.y);
            Quaternion yRotation = Quaternion.AngleAxis(yClamped, original * Vector3.up);

            dirToTarget = q * Vector3.up;
            originalForward = xRotation * yRotation * original * Vector3.up;
            Vector3 zAxis = xRotation * yRotation * original * Vector3.forward; // our local x axis
            Vector3 dirXY = Vector3.ProjectOnPlane(dirToTarget, zAxis);
            Vector3 forwardXY = Vector3.ProjectOnPlane(originalForward, zAxis);
            float zAngle = Vector3.Angle(dirXY, forwardXY) * Mathf.Sign(Vector3.Dot(zAxis, Vector3.Cross(forwardXY, dirXY)));
            float zClamped = Mathf.Clamp(zAngle, min.z, max.z);
            Quaternion zRotation = Quaternion.AngleAxis(zClamped, original * Vector3.forward);


            Quaternion newRotation = xRotation * yRotation * zRotation;
            return newRotation;
        }

        public static Quaternion ClampEuler(this Quaternion q, Vector3 min, Vector3 max)
        {
            Vector3 euler = q.eulerAngles;

            if (euler.x > 180)
                euler.x -= 360;
            if (euler.y > 180)
                euler.y -= 360;
            if (euler.z > 180)
                euler.z -= 360;
            Debug.Log(euler);

            euler.x = Mathf.Clamp(euler.x, min.x, max.x);
            euler.y = Mathf.Clamp(euler.y, min.y, max.y);
            euler.z = Mathf.Clamp(euler.z, min.z, max.z);

            Debug.Log(euler);

            q = Quaternion.Euler(euler);
            return q;
        }

        public static Quaternion Clamp(this Quaternion q, Vector3 bounds)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
            angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

            float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
            angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);
            q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);

            return q;
        }

        public static Quaternion ClampMagnitude(this Quaternion q, float maxAngle)
        {
            q.ToAngleAxis(out float angle, out Vector3 axis);

            float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);

            return Quaternion.AngleAxis(clampedAngle, axis);
        }

        #endregion

        #region List

        public static T First<T>(this IList<T> list) => list[0];
        public static T Last<T>(this IList<T> list) => list[list.Count - 1];

        public static T GetRandomItem<T>(this IList<T> list) => list[Random.Range(0, list.Count)];
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 1; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static void ExecuteAll<T>(this IList<T> list, System.Action<T> action)
        {
            foreach (T item in list)
                action(item);
        }
        #endregion

        #region Transform

        public struct TransformInfo
        {
            public Vector3 posision;
            public Quaternion rotation;
            public Vector3 scale;

            public TransformInfo(Vector3 posision, Quaternion rotation, Vector3 scale)
            {
                this.posision = posision;
                this.rotation = rotation;
                this.scale = scale;
            }
        }

        public static TransformInfo CopyLocal(this Transform transform)
        {
            TransformInfo transformInfo = new TransformInfo(transform.localPosition, transform.localRotation, transform.localScale);
            return transformInfo;
        }

        public static void PasteLocal(this Transform transform, TransformInfo transformInfo)
        {
            transform.localPosition = transformInfo.posision;
            transform.localRotation = transformInfo.rotation;
            transform.localScale = transformInfo.scale;
        }

        public static TransformInfo Copy(this Transform transform)
        {
            TransformInfo transformInfo = new TransformInfo(transform.position, transform.rotation, transform.lossyScale);
            return transformInfo;
        }

        public static void Paste(this Transform transform, TransformInfo transformInfo)
        {
            transform.position = transformInfo.posision;
            transform.rotation = transformInfo.rotation;
            transform.localScale = transformInfo.scale;
        }

        public static void DestroyChildren(this Transform transform)
        {
            for (var i = 0; i < transform.childCount; ++i)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }
        public static void ResetTransformation(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        #endregion

        #region Editor

        //https://forum.unity.com/threads/drawing-capsule-gizmo.354634/
        public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
        {
            // Special case when both points are in the same position
            if (p1 == p2)
            {
                // DrawWireSphere works only in gizmo methods
                Gizmos.DrawWireSphere(p1, radius);
                return;
            }
            using (new UnityEditor.Handles.DrawingScope(UnityEditor.Handles.color, Gizmos.matrix))
            {
                Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
                Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);

                // Check if capsule direction is collinear to Vector.up
                float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
                if (c == 1f || c == -1f)
                {
                    // Fix rotation
                    p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
                }

                // First side
                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.left, p1Rotation * Vector3.down, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);

                // Second side
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.left, p2Rotation * Vector3.down, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);

                // Lines
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
            }
        }

        public static void DrawWireCapsule2D(Vector3 p1, Vector3 p2, float radius)
        {
            // Special case when both points are in the same position
            if (p1 == p2)
            {
                // DrawWireSphere works only in gizmo methods
                Gizmos.DrawWireSphere(p1, radius);
                return;
            }

            UnityEditor.Handles.color = Gizmos.color;
            using (new UnityEditor.Handles.DrawingScope(UnityEditor.Handles.color, Gizmos.matrix))
            {
                Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
                Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);

                // Check if capsule direction is collinear to Vector.up
                float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
                if (c == 1f || c == -1f)
                {
                    // Fix rotation
                    p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
                }

                UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
                UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);

                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
                UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
            }
        }

        #endregion
    }
}
