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
        private SerializedProperty _applyRootMotionProp;

        private bool _hasRootMotion;

        private void OnEnable()
        {
            IKAnimationData animationData = target as IKAnimationData;

            string assetPath = AssetDatabase.GetAssetPath(animationData.GetInstanceID());
            name = Path.GetFileNameWithoutExtension(assetPath);

            serializedObject.FindProperty("_animationName").stringValue = name;
            serializedObject.ApplyModifiedProperties();

            _animationName = name;

            _loopProp = serializedObject.FindProperty("_loop");
            _hasRootMotion = animationData.animation.HasRootMotion;
            _applyRootMotionProp = serializedObject.FindProperty("_applyRootMotion");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Name", _animationName);

            DrawToggle(_loopProp);
            if(_hasRootMotion)
                DrawToggle(_applyRootMotionProp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawToggle(SerializedProperty property) => property.boolValue = EditorGUILayout.Toggle(property.displayName, property.boolValue);
    }
}
