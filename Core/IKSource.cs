using System.Linq;
using UnityEngine;

namespace Rehcub
{
    [SelectionBase]
    public class IKSource : MonoBehaviour
    {
        [SerializeField] private Transform _root;
        [SerializeField] private AnimationClip _tPose;

        public Armature Armature { get => _armature; }
        [SerializeField] private Armature _armature;

        [SerializeField] private AnimationClip _clip;
        public IKAnimationData AnimationData { get => _animationData; }
        [SerializeField] private IKAnimationData _animationData;

        public void Init(Armature armature)
        {
            //_transform = transform;
            _armature = armature;
        }

        public IKAnimationData CreateIKAnimation()
        {
            int frameCount = (int)(_clip.length * _clip.frameRate);

            IKAnimation ikAnimation = new IKAnimation(_clip.length, _clip.frameRate)
            {
                name = _clip.name
            };

            float frameTime = 1.0f / _clip.frameRate;

            for (int i = 0; i <= frameCount; i++)
            {
                _clip.SampleAnimation(gameObject, i * frameTime);
                IKPose pose = Compute();
                ikAnimation.AddKeyframe(pose, i);
            }

            ikAnimation.ComputeRootMotion();

            IKAnimationData data = ScriptableObject.CreateInstance<IKAnimationData>();

            data.animation = ikAnimation;
            data.animationName = _clip.name;

            _animationData = data;

            return data;
        }

        public void ResetToTPose()
        {
            _tPose.SampleAnimation(gameObject, 0);
        }

        public IKPose EditorDebug(int frame)
        {
            float frameTime = 1.0f / _clip.frameRate;
            _clip.SampleAnimation(gameObject, frame * frameTime);
            return Compute();
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
    }
}
