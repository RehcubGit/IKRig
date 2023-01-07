using System.Collections.Generic;
using UnityEngine;

namespace Rehcub
{
    [SelectionBase]
    public class ArmatureBuilder : MonoBehaviour
    {
        public bool hasShoulder;
        public bool fixHip;


        public List<Transform> boneTransforms = new List<Transform>();
        public List<Bone> bones = new List<Bone>();
        public List<Chain> chains = new List<Chain>();

        public void EditArmature(Armature armature)
        {
            boneTransforms.Clear();
            //boneTransforms.AddRange(armature.GetTransforms());
            bones.Clear();
            //bones.AddRange(armature.bindPose.GetBones());
            chains.Clear();
            chains.AddRange(armature.GetAllChains());
        }

        public void CreateIKRig()
        {
            IKRig[] oldRigs = GetComponents<IKRig>();

            foreach (IKRig oldRig in oldRigs)
            {
                DestroyImmediate(oldRig);
            }

            IKRig rig = gameObject.AddComponent<IKRig>();

            Armature armature = new Armature(boneTransforms, bones, chains);


            rig.Init(armature);

            //TODO: Remove this Component!
            //Destroy(this);
        }
        public void CreateIKSource()
        {
            IKSource[] oldRigs = GetComponents<IKSource>();

            foreach (IKSource oldRig in oldRigs)
            {
                DestroyImmediate(oldRig);
            }

            IKSource rig = gameObject.AddComponent<IKSource>();

            Armature armature = new Armature(boneTransforms, bones, chains);


            rig.Init(armature);

            //TODO: Remove this Component!
            //Destroy(this);
        }
    }
}
