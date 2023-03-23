using UnityEngine;
using UnityEditor;
using System.IO;

namespace Rehcub
{
    [CustomEditor(typeof (IKAnimationData))]
    public class IKAnimationDataEditor : Editor
    {
        private string _animationName;

        private SerializedProperty _loopProp;
        private SerializedProperty _extrectRootMotionProp;

        private void OnEnable()
        {
            IKAnimationData animationData = target as IKAnimationData;

            string assetPath = AssetDatabase.GetAssetPath(animationData.GetInstanceID());
            name = Path.GetFileNameWithoutExtension(assetPath);

            serializedObject.FindProperty("animationName").stringValue = name;
            serializedObject.ApplyModifiedProperties();

            _animationName = name;

            _loopProp = serializedObject.FindProperty("loop");
            _extrectRootMotionProp = serializedObject.FindProperty("extrectRootMotion");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Name", _animationName);

            DrawToggle(_loopProp);
            DrawToggle(_extrectRootMotionProp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawToggle(SerializedProperty property) => property.boolValue = EditorGUILayout.Toggle(property.displayName, property.boolValue);
    }
}
