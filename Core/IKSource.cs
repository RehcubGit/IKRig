using System.Linq;
using UnityEngine;

namespace Rehcub
{
    [SelectionBase]
    public class IKSource : MonoBehaviour
    {
        [SerializeField] private Transform _transform;

        public Armature Armature { get => _armature; }
        [SerializeField] private Armature _armature;

        [SerializeField] private AnimationClip _clip;
        public IKAnimationData AnimationData { get => _animationData; }
        [SerializeField] private IKAnimationData _animationData;

        [SerializeField] private bool _initialized;


        public void Init(Armature armature)
        {
            _transform = transform;
            _armature = armature;
            _initialized = true;
        }

        public IKAnimationData CreateIKAnimation(Vector3 rootMotionAxis, float targetAngle)
        {
            int frameCount = (int)(_clip.length * _clip.frameRate);

            bool hasRootMotion = Mathf.Abs(rootMotionAxis.sqrMagnitude) > 0f || Mathf.Abs(targetAngle) > 0f;

            IKAnimation ikAnimation = new IKAnimation(_clip.length, _clip.frameRate, hasRootMotion);

            Vector3 prevRootMovement = Vector3.zero;
            Quaternion prevRootRotation = Quaternion.identity;
            bool ignoreFurtherRotation = false;

            for (int i = 0; i <= frameCount; i++)
            {
                bool lastFrame = i == frameCount;

                SampleAnimation(i);
                GetRootMotion(targetAngle, rootMotionAxis, out Vector3 rootMovement, out Quaternion rootRotation, i, lastFrame, ref ignoreFurtherRotation);

                IKPose pose = Compute();
                ApplyRootMotionToPose(ref prevRootMovement, ref prevRootRotation, rootMovement, rootRotation, pose);
                
                ikAnimation.AddKeyframe(pose, i);
            }

            IKAnimationData data = IKAnimationData.Create(_clip.name, ikAnimation);

            _animationData = data;

            return data;
        }

        private void GetRootMotion(float targetAngle, Vector3 rootMotionAxis, out Vector3 rootMovement, out Quaternion rootRotation, int frame, bool lastFrame, ref bool ignoreFurtherRotation)
        {
            if (HasRootMotionCurve())
            {
                SampleAnimation(frame);
                rootMovement = _transform.position;
                rootRotation = _transform.rotation;
                return;
            }

            string hipName = _armature.GetBones(SourceBone.HIP).First().boneName;
            Transform hipTransform = _armature.GetTransform(hipName);

            ExtractRootMotion(frame, rootMotionAxis, out rootMovement, out float rootAngle);

            ignoreFurtherRotation |= Mathf.Abs(rootAngle) >= Mathf.Abs(targetAngle);

            if (ignoreFurtherRotation || lastFrame)
                rootAngle = targetAngle;

            rootRotation = Quaternion.AngleAxis(rootAngle, Vector3.up);
            Quaternion invRootRotation = Quaternion.Inverse(rootRotation);

            hipTransform.position -= rootMovement;
            hipTransform.localPosition = invRootRotation * hipTransform.localPosition;
            hipTransform.rotation = invRootRotation * hipTransform.rotation;
            
        }

        private static void ApplyRootMotionToPose(ref Vector3 prevRootMovement, ref Quaternion prevRootRotation, Vector3 rootMovement, Quaternion rootRotation, IKPose pose)
        {
            pose.rootMotion.position = rootMovement;
            pose.rootMotion.rotation = rootRotation;

            pose.deltaRootMotion.position = rootMovement - prevRootMovement;
            pose.deltaRootMotion.rotation = Quaternion.Inverse(prevRootRotation) * rootRotation;

            prevRootMovement = rootMovement;
            prevRootRotation = rootRotation;
        }

        private void ExtractRootMotion(int frame, Vector3 rootMotionAxis, out Vector3 rootMovement, out float rootAngle)
        {
            string hipName = _armature.GetBones(SourceBone.HIP).First().boneName;
            Transform hipTransform = _armature.GetTransform(hipName);

            SampleAnimation(0);

            Vector3 startPosition = hipTransform.position;
            Quaternion startRotation = hipTransform.rotation;

            SampleAnimation(frame);

            Vector3 endPosition = hipTransform.position;
            Quaternion endRotation = hipTransform.rotation;

            rootMovement = endPosition - startPosition;
            rootMovement.Scale(rootMotionAxis);

            Quaternion startToEndRotation = Quaternion.Inverse(startRotation) * endRotation;
            Vector3 rootForward = startToEndRotation * Vector3.forward;
            rootForward = Vector3.ProjectOnPlane(rootForward, Vector3.up).normalized;
            rootAngle = Vector3.SignedAngle(Vector3.forward, rootForward, Vector3.up);
        }

        public void ResetToBindPose()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            _armature.scale = transform.localScale;
            _armature.ApplyBindPose();
            _armature.currentPose.rootTransform = BoneTransform.zero;
            _armature.UpdatePose();
        }

        public void SampleAnimation(int frame)
        {
            float frameTime = frame / _clip.frameRate;
            _clip.SampleAnimation(gameObject, frameTime);
            _clip.SampleAnimationRoot(_transform, frameTime);
        }

        private IKPose Compute()
        {
            _armature.UpdatePose();

            IKPose ikPose = new IKPose();

            IKCompute.Hip(_armature, ikPose);

            Chain spine = _armature.GetChains(SourceChain.SPINE)[0];
            IKCompute.Chain(_armature, spine, ikPose.spine);

            Chain leftLeg = _armature.GetChains(SourceChain.LEG, SourceSide.LEFT)[0];
            IKCompute.Chain(_armature, leftLeg, ikPose.leftLeg);
            IKCompute.SimpleBone(_armature, _armature.GetBones(SourceBone.TOE, SourceSide.LEFT)[0], ikPose.leftToe);

            Chain rightLeg = _armature.GetChains(SourceChain.LEG, SourceSide.RIGHT)[0];
            IKCompute.Chain(_armature, rightLeg, ikPose.rightLeg);
            IKCompute.SimpleBone(_armature, _armature.GetBones(SourceBone.TOE, SourceSide.RIGHT)[0], ikPose.rightToe);


            IKCompute.SimpleBone(_armature, _armature.GetBones(SourceBone.SHOULDER, SourceSide.LEFT)[0], ikPose.leftShoulder);
            Chain leftArm = _armature.GetChains(SourceChain.ARM, SourceSide.LEFT)[0];
            IKCompute.Chain(_armature, leftArm, ikPose.leftArm);

            IKCompute.SimpleBone(_armature, _armature.GetBones(SourceBone.SHOULDER, SourceSide.RIGHT)[0], ikPose.rightShoulder);
            Chain rightArm = _armature.GetChains(SourceChain.ARM, SourceSide.RIGHT)[0];
            IKCompute.Chain(_armature, rightArm, ikPose.rightArm);

            Bone head = _armature.GetBones(SourceBone.HEAD)[0];
            IKCompute.SimpleBone(_armature, head, ikPose.head);

            return ikPose;
        }


        #region RootMotion

        public bool HasRootMotionCurve() => _clip.hasRootCurves || _clip.hasGenericRootTransform || _clip.hasMotionCurves;

        public bool HasBakedRootMotion() => GetRootMotionAxis().sqrMagnitude > 0.01f;
        public bool HasBakedRootRotation() => Mathf.Abs(GetRootRotationAngle()) > 0.1f;

        public Vector3 GetRootMotionAxis()
        {
            string hipName = _armature.GetBones(SourceBone.HIP).First().boneName;
            Transform hipTransform = _armature.GetTransform(hipName);

            _clip.SampleAnimation(gameObject, 0f);
            Vector3 firstPos = hipTransform.position;

            _clip.SampleAnimation(gameObject, _clip.length);
            Vector3 lastPos = hipTransform.position;

            Vector3 dist = lastPos - firstPos;
            dist.Abs();

            bool x = dist.x > 0.001f;
            bool y = dist.y > 0.001f;
            bool z = dist.z > 0.001f;

            return new Vector3(x.ToFloat(), y.ToFloat(), z.ToFloat());
        }
        public float GetRootRotationAngle()
        {
            string hipName = _armature.GetBones(SourceBone.HIP).First().boneName;
            Transform hipTransform = _armature.GetTransform(hipName);

            _clip.SampleAnimation(gameObject, 0f);
            Vector3 firstPos = hipTransform.rotation * Vector3.forward;

            _clip.SampleAnimation(gameObject, _clip.length);
            Vector3 lastPos = hipTransform.rotation * Vector3.forward;

            if ((lastPos - firstPos).sqrMagnitude < 0.1f)
                return 0f;

            firstPos = Vector3.ProjectOnPlane(firstPos, Vector3.up).normalized;
            lastPos = Vector3.ProjectOnPlane(lastPos, Vector3.up).normalized;

            float angle = Vector3.SignedAngle(firstPos, lastPos, Vector3.up);

            return angle;
        }

        #endregion
    }
}
