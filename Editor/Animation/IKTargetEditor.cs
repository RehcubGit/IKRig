using UnityEngine;
using UnityEditor;

namespace Rehcub
{
    [CustomEditor(typeof(IKTargetObject))]
    public class IKTargetEditor : Editor
    {
        private void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            IKTargetObject targetObject = target as IKTargetObject;
            SerializedProperty hintProperty = serializedObject.FindProperty("_hintPosition");

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            Vector3 hintPosition = hintProperty.vector3Value;
            hintPosition = targetObject.transform.TransformPoint(hintPosition);

            Vector3 hint = Handles.PositionHandle(hintPosition, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetObject, "Set Hint Destinations");
                hint = targetObject.transform.InverseTransformPoint(hint);
                hintProperty.vector3Value = hint;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

        }
    }
}
