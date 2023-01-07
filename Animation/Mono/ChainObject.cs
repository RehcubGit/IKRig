using System.Collections.Generic;
using UnityEngine;

namespace Rehcub
{
    [ExecuteInEditMode]
    public class ChainObject : MonoBehaviour
    {
        [SerializeField] private IKRig _rig;

        [SerializeField] private List<Transform> _bones;

        [SerializeField] private SourceChain _sourceChain;
        [SerializeField] private SourceSide _sourceSide;

        private Chain _chain;
        private Armature _armature;

        private Vector3 start;

        private void OnEnable()
        {
            _rig.onPreApplyIkPose += Apply;
            start = transform.position;
        }

        private void OnDisable()
        {
            _rig.onPreApplyIkPose -= Apply;
        }

        private void Start()
        {
        }

        /*private void Apply(IKPose pose)
        {
            IKChain ikChain = GetIkChain(pose);
            Quaternion q = Quaternion.FromToRotation(Vector3.down, ikChain.direction);
            transform.rotation = q;

            transform.position = start.AddY((1f - ikChain.lengthScale));
        }*/

        private void Apply(IKPose pose)
        {
            _chain.Solve(_armature, GetIkChain(pose));

            for (int i = 0; i < _bones.Count; i++)
            {
                Bone bone = _armature.currentPose[_bones[i].name];

                BoneTransform model = _armature.currentPose.CalculateParentModelTransform(bone) + bone.local;
                bone.model = model;

                _bones[i].SetPositionAndRotation(model.position, model.rotation);
            }
        }

        private IKChain GetIkChain(IKPose pose)
        {
            switch (_sourceChain)
            {
                case SourceChain.LEG:
                    if (_sourceSide == SourceSide.LEFT)
                        return pose.leftLeg;
                    return pose.rightLeg;

                case SourceChain.ARM:
                    if (_sourceSide == SourceSide.LEFT)
                        return pose.leftArm;
                    return pose.rightArm;

                case SourceChain.SPINE:
                    return pose.spine;

                case SourceChain.NONE:
                default:
                    return null;
            }
        }

        private void OnValidate()
        {
            List<Bone> bones = new List<Bone>();
            List<Chain> chains = new List<Chain>();

            for (int i = 0; i < _bones.Count; i++)
            {
                bones.Add(new Bone(_bones[i]));
            }
            _chain = new Chain(bones.ToArray());
            chains.Add(_chain);
            _armature = new Armature(_bones, bones, chains);
        }
    }
}
