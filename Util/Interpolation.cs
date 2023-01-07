using UnityEngine;

namespace Rehcub 
{
    public static class Interpolation
    {
        private static float Flip(float t) => 1 - t;
        private static float Square(float t) => t * t;


        public static float SmoothStart(float t) => Square(t);
        public static float SmoothStartN(float t, int n)
        {
            float temp = t;
            for (int i = 0; i < n; i++)
            {
                t *= temp;
            }

            return t;
        }

        public static float SmoothStop(float t) => Flip(Square(Flip(t)));
        public static float SmoothStopN(float t, int n)
        {
            float temp = Flip(t);
            t = temp;
            for (int i = 0; i < n; i++)
            {
                t *= temp;
            }

            return Flip(t);
        }

        public static float SmoothStep(float t) => t * t * (3 - 2 * t);

        public static float Arch(float t) => t * (1 - t) * 4f;
        public static float Bell6(float t) => SmoothStartN(t, 3) * SmoothStopN(t, 3);

        public static float Hermite01(float t, float a, float b)
        {
            float c3 = a + b - 2;
            float c2 = 3 - 2 * a - b;
            float t2 = t * t;
            float t3 = t2 * t;
            return c3 * t3 + c2 * t2 + a * t;
        }
        public static float Hermite00(float t, float a, float b)
        {
            float c3 = a + b;
            float c2 = -a - c3;
            float t2 = t * t;
            float t3 = t2 * t;
            return c3 * t3 + c2 * t2 + a * t;
        }

        public static float Hermite(float t, float m0, float m1, float p0, float p1)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float c1 = 2 * t3 - 3 * t2 + 1;
            float c2 = t3 - 2 * t2 + t;
            float c3 = -2f * t3 + 3 * t2;
            float c4 = t3 - t2;

            return c1 * p0 + c2 * m0 + c3 * p1 + c4 * m1;
        }

        //https://www.desmos.com/calculator/aksjkh9das?lang=de
        public static float NormalizedTunableSigmoid(float t, float k)
        {
            float a = t - t * k;
            float b = k - Mathf.Abs(t) * 2 * k - 1;
            return a / b;
        }
    }
}
