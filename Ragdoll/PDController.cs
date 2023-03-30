using System;
using UnityEngine;

namespace Rehcub 
{
    [Serializable]
    public class PDController
    {
        public enum DerivativeMeasurement
        {
            Velocity,
            ErrorRateOfChange
        }
        public float proportionalGain = 1500;
        public float integralGain = 30;
        public float derivativeGain = 30;

        public float tau = 1f;

        public float outputMin = -0.1f;
        public float outputMax = 10000f;
        public float integralSaturation = 15;
        public DerivativeMeasurement derivativeMeasurement;

        private Vector3 valueLast;
        private Vector3 errorLast;
        private Vector3 integrationStored;
        private float valueLastF;
        private float errorLastF;
        private float integrationStoredF;
        private bool derivativeInitialized;

        public PDController()
        {
            Reset();
            derivativeMeasurement = DerivativeMeasurement.ErrorRateOfChange;
        }

        public void SetErrorLast(Vector3 errorLast)
        {
            this.errorLast = errorLast;
            derivativeInitialized = true;
        }

        public void Reset()
        {
            derivativeInitialized = false;
            this.valueLast = Vector3.zero;
            this.errorLast = Vector3.zero;
            integrationStored = Vector3.zero;
        }

        private float AngleDifference(float a, float b) => -Mathf.DeltaAngle(a, b);

        private Vector3 AngleDifference(Vector3 a, Vector3 b)
        {
            float x = AngleDifference(a.x, b.x);
            float y = AngleDifference(a.y, b.y);
            float z = AngleDifference(a.z, b.z);
            return new Vector3(x, y, z);
        }

        public float Update(float dt, float currentValue, float targetValue, bool debug = false)
        {
            if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

            float error = targetValue - currentValue;

            //calculate P term
            float P = proportionalGain * error;

            //calculate I term
            integrationStoredF = Mathf.Clamp(integrationStoredF + (error * dt), -integralSaturation, integralSaturation);
            float I = integralGain * integrationStoredF;

            //calculate both D terms
            float errorRateOfChange = (error - errorLastF) / dt;
            errorLastF = error;

            float valueRateOfChange = (currentValue - valueLastF) / dt;
            valueLastF = currentValue;

            //choose D term to use
            float derivativeMeasure = 0;

            if (derivativeInitialized)
            {
                if (derivativeMeasurement == DerivativeMeasurement.Velocity)
                    derivativeMeasure = -valueRateOfChange;
                else
                    derivativeMeasure = errorRateOfChange;
            }
            else
            {
                derivativeInitialized = true;
            }

            if (debug)
            {
                Debug.Log("Error: " + error);
                Debug.Log("Derivative: " + derivativeMeasure);
            }
            float D = derivativeGain * derivativeMeasure;


            /*derivativeStored = -(2.0f * derivativeGain * (currentValue - valueLast)	*//* Note: derivative on measurement, therefore minus sign in front of equation! *//*
                        + (2.0f * tau - dt) * derivativeStored)
                        / (2.0f * tau + dt);
            Vector3 D = derivativeStored;*/

            float result = P + I + D;

            return Mathf.Clamp(result, outputMin, outputMax);
        }

        public Vector3 Update(float dt, Vector3 currentValue, Vector3 targetValue, bool debug = false)
        {
            if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

            Vector3 error = targetValue - currentValue;

            //calculate P term
            Vector3 P = proportionalGain * error;

            //calculate I term
            integrationStored = Vector3.ClampMagnitude(integrationStored + (error * dt), integralSaturation);
            Vector3 I = integralGain * integrationStored;

            //calculate both D terms
            Vector3 errorRateOfChange = (error - errorLast) / dt;
            errorLast = error;

            Vector3 valueRateOfChange = (currentValue - valueLast) / dt;
            valueLast = currentValue;

            //choose D term to use
            Vector3 derivativeMeasure = Vector3.zero;

            if (derivativeInitialized)
            {
                if (derivativeMeasurement == DerivativeMeasurement.Velocity)
                    derivativeMeasure = -valueRateOfChange;
                else
                    derivativeMeasure = errorRateOfChange;
            }
            else
            {
                derivativeInitialized = true;
            }

            if (debug)
            {
                Debug.Log("Error: " + error);
                Debug.Log("Derivative: " + derivativeMeasure);
            }
            Vector3 D = derivativeGain * derivativeMeasure;


            Vector3 result = P + I + D;
            return Vector3.ClampMagnitude(result, outputMax);
        }

        public Vector3 Update(float dt, Quaternion current, Quaternion target, bool debug = false)
        {
            //Quaternion error = current * Quaternion.Inverse(target);
            Quaternion errorQ = target * Quaternion.Inverse(current);
            if (errorQ.w < 0)
            {
                errorQ[0] = -errorQ[0];
                errorQ[1] = -errorQ[1];
                errorQ[2] = -errorQ[2];
                errorQ[3] = -errorQ[3];
            }

                current.ToAngleAxis(out float angle, out Vector3 axis);
            Vector3 currentValue = axis * angle;
            errorQ.ToAngleAxis(out angle, out axis);
/*
            if (angle > 180)
                angle -= 360;*/
            Vector3 error = axis * angle;

            //calculate P term
            Vector3 P = proportionalGain * error;

            //calculate I term
            integrationStored = Vector3.ClampMagnitude(integrationStored + (error * dt), integralSaturation);
            Vector3 I = integralGain * integrationStored;

            //calculate both D terms
            Vector3 errorRateOfChange = AngleDifference(error, errorLast);
            errorRateOfChange *= 1f / dt;
            errorLast = error;

            Vector3 valueRateOfChange = AngleDifference(currentValue, valueLast) / dt;
            valueLast = currentValue;

            //choose D term to use
            Vector3 derivativeMeasure = Vector3.zero;

            if (derivativeInitialized)
            {
                if (derivativeMeasurement == DerivativeMeasurement.Velocity)
                    derivativeMeasure = -valueRateOfChange;
                else
                    derivativeMeasure = errorRateOfChange;
            }
            else
            {
                derivativeInitialized = true;
            }
            Vector3 D = derivativeGain * derivativeMeasure;

            Vector3 result = P + I + D;

            return Vector3.ClampMagnitude(result, outputMax);
        }

        public Vector3 UpdateAngle(float dt, Vector3 currentValue, Vector3 targetValue, bool debug = false)
        {
            if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

            Vector3 error = AngleDifference(targetValue, currentValue);

            //calculate P term
            Vector3 P = proportionalGain * error;

            //calculate I term
            integrationStored = Vector3.ClampMagnitude(integrationStored + (error * dt), integralSaturation);
            Vector3 I = integralGain * integrationStored;

            //calculate both D terms
            Vector3 errorRateOfChange = AngleDifference(error, errorLast) / dt;
            errorLast = error;

            Vector3 valueRateOfChange = AngleDifference(currentValue, valueLast) / dt;
            valueLast = currentValue;

            //choose D term to use
            Vector3 derivativeMeasure = Vector3.zero;

            if (derivativeInitialized)
            {
                if (derivativeMeasurement == DerivativeMeasurement.Velocity)
                    derivativeMeasure = -valueRateOfChange;
                else
                    derivativeMeasure = errorRateOfChange;
            }
            else
            {
                derivativeInitialized = true;
            }

            if (debug)
            {
                Debug.Log("Error: " + error);
                Debug.Log("Derivative: " + derivativeMeasure);
            }
            Vector3 D = derivativeGain * derivativeMeasure;

            Vector3 result = P + I + D;

            //return Mathf.Clamp(result, outputMin, outputMax);
            return Vector3.ClampMagnitude(result, outputMax);
            //return result;
        }

        public void CopySettings(PDController controller)
        {
            proportionalGain = controller.proportionalGain;
            integralGain = controller.integralGain;
            derivativeGain = controller.derivativeGain;

            tau = controller.tau;

            outputMin = controller.outputMin;
            outputMax = controller.outputMax;
            integralSaturation = controller.integralSaturation;
            derivativeMeasurement = controller.derivativeMeasurement;
    }
    }

    [Serializable]
    public class SODController
    {
        private Vector3 previewsTargetPosition = Vector3.zero;
        private Vector3 currentVelocity = Vector3.zero;

        public float f = 1;
        public float z = 1;
        public float r = 0;

        private float k1;
        private float k2;
        private float k3;

        public void UpdateParameter(float f, float z, float r)
        {
            float pif = Mathf.PI * f;
            k1 = z / pif;
            float pif2 = pif * 2f;
            k2 = 1f / (pif2 * pif2);
            k3 = r * z / pif2;
        }

        public Vector3 Update(float dt, Vector3 currentPosition, Vector3 target)
        {
            Vector3 targetVelocity = (target - previewsTargetPosition) / dt;
            previewsTargetPosition = target;

            float k2Stable = Mathf.Max(k2, dt * dt * 0.5f + dt * k1 * 0.5f, dt * k1);

            currentPosition += dt * currentVelocity;
            Vector3 acceleration = (target + k3 * targetVelocity - currentPosition - k1 * currentVelocity) / k2Stable; 
            currentVelocity += dt * acceleration;

            return acceleration;
        }
    }
}
