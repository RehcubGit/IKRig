using SerializableCollections;
using System.Collections.Generic;
using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class BoneDictionary : SerializableDictionary<string, Bone>
    {
    }
    [System.Serializable]
    public class BoneTransformDictionary : SerializableDictionary<string, Transform>
    {
        public BoneTransformDictionary() : base() { }

        public BoneTransformDictionary(List<Transform> transforms) : base()
        {
            foreach (Transform transform in transforms)
            {
                Add(transform.name, transform);
            }
        }
    }
}
