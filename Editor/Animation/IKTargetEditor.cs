﻿using UnityEngine;
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
            IKRig rig = serializedObject.FindProperty("_rig").objectReferenceValue as IKRig;
            SerializedProperty hintProperty = serializedObject.FindProperty("_hintPosition");

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            Vector3 hintPosition = hintProperty.vector3Value;
            hintPosition = rig.transform.rotation * hintPosition;
            hintPosition += targetObject.transform.position;

            hintPosition = Handles.PositionHandle(hintPosition, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetObject, "Set Hint Destinations");
                hintPosition -= targetObject.transform.position;
                hintPosition = Quaternion.Inverse(rig.transform.rotation) * hintPosition;
                hintProperty.vector3Value = hintPosition;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

        }
    }
}
