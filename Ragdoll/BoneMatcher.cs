using UnityEngine;

namespace Rehcub
{
    public class BoneMatcher
    {
        private Transform anchor;
        private Bone bone;

        ConfigurableJoint joint;
        Rigidbody rigidbody;

        private PDController pdContoller;
        private PDController pdRotation;
        private SODController sodPosition;
        private SODController sodRotation;

        bool positionMatching;
        bool rotationMatching;

        private bool enabled = true;
        private bool isKinematic;

        public float muscleStrength = 1f;

        private Quaternion startRotation; 
        BoneTransform previousPose;
        Vector3 angularVelocity;

        public BoneMatcher(Armature armature, Bone bone, bool positionMatching = false, bool rotationMatching = true, bool kinematic = false)
        {
            this.bone = bone;
            this.anchor = armature.GetTransform(bone.boneName);

            rigidbody = anchor.GetComponent<Rigidbody>();
            joint = anchor.GetComponent<ConfigurableJoint>();
            startRotation = anchor.localRotation;

            isKinematic = kinematic;
            rigidbody.isKinematic = kinematic;

            rigidbody.solverIterations = 12;
            rigidbody.solverVelocityIterations = 12;
            rigidbody.maxAngularVelocity = 20;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            rigidbody.useGravity = true;

            this.pdContoller = new PDController();
            this.pdRotation = new PDController();

            sodPosition = new SODController();
            sodRotation = new SODController();

            this.positionMatching = positionMatching;
            this.rotationMatching = rotationMatching;
        }

        public void CalculateAngularVelocity(Pose pose, float dt)
        {
            BoneTransform newPose = pose[bone.boneName].local;
            Quaternion deltaRotation = newPose.rotation * Quaternion.Inverse(previousPose.rotation);
            deltaRotation.ToAngleAxis(out float deltaAngle, out Vector3 axis);

            if (deltaAngle > 180)
            {
                deltaAngle -= 360f;
            }

            angularVelocity = Mathf.Deg2Rad * deltaAngle / dt * axis.normalized;
            previousPose = newPose;
        }

        public static void SetTargetRotationLocal(ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
        {
            if (joint.configuredInWorldSpace)
            {
                Debug.LogError("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
            }
            SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
        }

        private static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
        {
            Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
            Vector3 up = Vector3.Cross(forward, joint.axis).normalized;
            Quaternion quaternion = Quaternion.LookRotation(forward, up);
            Quaternion targetRotation2 = Quaternion.Inverse(quaternion);
            if (space == Space.World)
            {
                targetRotation2 *= startRotation * Quaternion.Inverse(targetRotation);
            }
            else
            {
                targetRotation2 *= Quaternion.Inverse(targetRotation) * startRotation;
            }

            targetRotation2 *= quaternion;
            joint.targetRotation = targetRotation2;
        }

        private static float GetSpringStiffnessFromAlpha(float alpha, float mass, float dt)
        {
            return mass * alpha / (dt * dt);
        }

        private static float GetSpringDampingFromDampingRatio(float dampingRatio, float k, float mass)
        {
            return dampingRatio * (2f * Mathf.Sqrt(k * mass));
        }

        public static Vector3 GetAccelerationFromPositionSpring(Vector3 currentPos, Vector3 targetPos, Vector3 currentLinearVel, Vector3 targetLinearVel, float alpha, float dampingRatio, float mass, float dt)
        {
            float k = GetSpringStiffnessFromAlpha(alpha, mass, dt);
            float d = GetSpringDampingFromDampingRatio(dampingRatio, k, mass);

            Vector3 positionDifference = currentPos - targetPos;
            Vector3 velocityDifference = currentLinearVel - targetLinearVel;

            Vector3 acceleration = (-k / mass * positionDifference) - (d / mass * velocityDifference);
            return acceleration;
        }

        public Vector3 GetAccelerationFromRotationSpring(Quaternion rotation, Quaternion target, Vector3 currentAngularVel, float alpha, float dampingRatio, float mass, float dt)
        {
            /*
            float k = GetSpringStiffnessFromAlpha(alpha, mass, dt);
            float d = GetSpringDampingFromDampingRatio(dampingRatio, k, mass);

            Vector3 v = new Vector3(currentAngularVel.x, currentAngularVel.y, currentAngularVel.z);
            
            Vector3 r = new Vector3(rotation[0], rotation[1], rotation[2]);
            Vector3 t = new Vector3(target[0], target[1], target[2]);

            float tempK = -k * pTorque * dt / mass;
            float tempD = d * dTorque * dt * dt / mass;

            Vector3 a = tempK * (r - t) - tempD * v;
            return a;
            */

            float k = alpha;
            float d = dampingRatio;


            Vector4 r = new Vector4(rotation[0], rotation[1], rotation[2], rotation[3]);
            Vector4 t = new Vector4(target[0], target[1], target[2], target[3]);

            Quaternion vQ = Quaternion.AngleAxis(Mathf.Rad2Deg * currentAngularVel.magnitude, currentAngularVel.normalized);

            //Vector4 v = new Vector4(currentAngularVel.x, currentAngularVel.y, currentAngularVel.z, 0);
            Vector4 v = new Vector4(vQ[0], vQ[1], vQ[2], vQ[3]);
            Vector4 a = -k * (r - t) - d * v;

            v += a * dt;

            /*float dSqrDt = d * d * dt;
            float n2 = 1 + d * dt;
            float n2Sqr = n2 * n2;

            v = (v - (r - t) * dSqrDt) / n2Sqr;*/

            Quaternion newRotation = rotation;
            newRotation[0] += v[0] * dt;
            newRotation[1] += v[1] * dt;
            newRotation[2] += v[2] * dt;
            newRotation[3] += v[3] * dt;
            newRotation.Normalize();

            Quaternion deltaRotation = newRotation * Quaternion.Inverse(rotation);
            deltaRotation.ToAngleAxis(out float deltaAngle, out Vector3 axis);

            if (deltaAngle > 180)
            {
                deltaAngle -= 360f;
            }

            Debug.Log(deltaAngle);
            Debug.Log(axis);

            return Mathf.Deg2Rad * deltaAngle * axis.normalized;

        }

        public void PinBone(Armature armature, Pose pose, float alpha, float damping)
        {
            float dt = Time.fixedDeltaTime;

            if(alpha == 0)
            {
                pdContoller.Reset();
                pdRotation.Reset();
                
                SetUnpoweredDrive();
                return;
            }

            if (isKinematic || enabled == false)
            {
                BoneTransform target = pose.GetWorldTransform(bone);

                rigidbody.MovePosition(target.position);
                rigidbody.MoveRotation(target.rotation);
                return;
            }


            if(positionMatching)
                MatchPosition(armature, pose, alpha, damping, dt);

            if (rotationMatching)
                MatchRotation(armature, pose, alpha, damping, dt);
            else
                SetUnpoweredDrive();
        }

        private void MatchRotation(Armature armature, Pose pose, float alpha, float damping, float dt)
        {
            if (joint == null)
            {
                MatchRotationRigidbody(armature, pose, alpha, damping, dt);
                return;
            }

            Quaternion target = pose.GetLocalTransform(bone).rotation;

            SetTargetRotationLocal(joint, target, startRotation);
            //SetXYZDrive();
            SetSlerpDrive(alpha, damping, rigidbody.mass, dt, 10000);
        }

        private void MatchRotationRigidbody(Armature armature, Pose pose, float alpha, float damping, float dt)
        {
            Quaternion current = anchor.localRotation;
            Quaternion target = pose.GetLocalTransform(bone).rotation;

            Vector3 acceleration = pdRotation.Update(dt, current, target, true);
            acceleration *= alpha;
            acceleration *= muscleStrength;

            rigidbody.AddTorque(acceleration, ForceMode.Acceleration);
        }

        private void MatchPosition(Armature armature, Pose pose, float alpha, float damping, float dt)
        {
            Vector3 targetPosition = pose.GetWorldTransform(bone).position;
            Vector3 currentPosition = anchor.position;

            Vector3 acceleration = pdContoller.Update(dt, currentPosition, targetPosition);

            acceleration *= alpha;
            acceleration *= muscleStrength;

            rigidbody.AddForce(acceleration, ForceMode.Acceleration);
        }

        public void SetControllers(PDController positionController, PDController rotationController)
        {
            pdContoller.CopySettings(positionController);
            pdRotation.CopySettings(rotationController);

            /*if (isKinematic == false)
                return;

            float k = GetSpringStiffnessFromAlpha(1f, rigidbody.mass, Time.fixedDeltaTime);
            float d = GetSpringDampingFromDampingRatio(0.33f, pdContoller.proportionalGain, rigidbody.mass);

            pdContoller.proportionalGain = k;
            pdContoller.derivativeGain = d;*/
        }
        public void SetControllers(SODController positionController, SODController rotationController)
        {
            sodPosition.UpdateParameter(positionController.f, positionController.z, positionController.r);
            sodRotation.UpdateParameter(rotationController.f, rotationController.z, rotationController.r);
            //pdRotation.CopySettings(rotationController);
        }

        private void SetXYZDrive()
        {
            if (joint == null)
                return;

            JointDrive drive = default;
            drive.positionSpring = 1500;
            drive.positionDamper = 10;
            drive.maximumForce = float.PositiveInfinity;

            joint.rotationDriveMode = RotationDriveMode.XYAndZ;
            joint.angularXDrive = drive;
            joint.angularYZDrive = drive;
        }

        public void SetSlerpDrive(float alpha, float dampingRatio, float mass, float dt, float maxAcceleration)
        {
            if (joint == null)
                return;

            float springStiffnessFromAlpha = GetSpringStiffnessFromAlpha(alpha, mass, dt) * muscleStrength;
            float springDampingFromDampingRatio = GetSpringDampingFromDampingRatio(dampingRatio, springStiffnessFromAlpha, mass) * muscleStrength;

            JointDrive drive = default;
            drive.positionSpring = springStiffnessFromAlpha;
            drive.positionDamper = springDampingFromDampingRatio;
            //drive.maximumForce = maxAcceleration * mass;
            drive.maximumForce = float.PositiveInfinity;

            /*joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;*/
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.slerpDrive = drive;
        }
        public void SetUnpoweredDrive()
        {
            if (joint == null)
                return;

            JointDrive drive = default;
            drive.positionSpring = 0f;
            drive.positionDamper = 0f;
            drive.maximumForce = float.PositiveInfinity;

            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;

            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.slerpDrive = drive;
        }


        public void EnableBone()
        {
            enabled = true;

            pdContoller.Reset();
            pdRotation.Reset();

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.isKinematic = false;
        }

        public void DisableBone()
        {
            enabled = false;
            rigidbody.isKinematic = true;
            SetUnpoweredDrive();
        }

        public void SnapToTargetPose(Pose pose)
        {
            pdContoller.Reset();
            pdRotation.Reset();

            BoneTransform transform = pose[bone.boneName].model;
            rigidbody.position = transform.position;
            rigidbody.rotation = transform.rotation;

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            
            if (joint == null)
                return;

            joint.targetAngularVelocity = Vector3.zero;
            joint.slerpDrive = new JointDrive()
            {
                maximumForce = 0,
                positionDamper = 0,
                positionSpring = 0
            };
        }

    }
}
