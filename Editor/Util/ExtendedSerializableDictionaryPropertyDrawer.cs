#if UNITY_EDITOR

using Rehcub;
using SerializableCollections;

[UnityEditor.CustomPropertyDrawer(typeof(BoneDictionary))]
[UnityEditor.CustomPropertyDrawer(typeof(BoneTransformDictionary))]
public class ExtendedSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer
{

}

#endif
