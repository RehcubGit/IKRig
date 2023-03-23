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


        //unity serialize by value, so when we edit a bone after the chain is created the bone in the chain object is not updated
        //to fix this we update the bones manualy.
        //TODO: in the future mabey just save the bone names in the chain object and get the bone from the bindpose
        private void FixChainBoneReferace()
        {
            foreach (Chain chain in chains)
            {
                for (int i = 0; i < chain.count; i++)
                {
                    Bone bone = bones.Find((b) => chain[i].boneName.Equals(b.boneName));

                    chain[i].parentName = bone.parentName;
                    chain[i].childNames = bone.childNames;

                    chain[i].local = bone.local;
                    chain[i].model = bone.model;

                    chain[i].side = bone.side;
                    chain[i].source = bone.source;

                    chain[i].alternativeForward = bone.alternativeForward;
                    chain[i].alternativeUp = bone.alternativeUp;
                    chain[i].length = bone.length;
                }
            }
        }

        public void CreateIKRig()
        {
            IKRig[] oldRigs = GetComponents<IKRig>();

            foreach (IKRig oldRig in oldRigs)
            {
                DestroyImmediate(oldRig);
            }

            IKRig rig = gameObject.AddComponent<IKRig>();

            FixChainBoneReferace();
            Armature armature = new Armature(boneTransforms, bones, chains);


            rig.Create(armature);

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
