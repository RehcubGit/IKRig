using UnityEngine;
using UnityEditor;
using Bewildered.Editor;

namespace Rehcub
{
    [CustomEditor(typeof (HandControl))]
    public class HandControlEditor : Editor
    {
        private bool _edit;

        SerializedProperty selectedProperty;
        SerializedProperty selectedChainProperty;
        private float nearClipping;

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
            if (_edit == false)
                return;

            serializedObject.Update();


            DrawFingerSelector("Thumb");
            DrawFingerSelector("Index");
            DrawFingerSelector("Middle");
            DrawFingerSelector("Ring");
            DrawFingerSelector("Pinky");

            DrawFinderTarget(selectedProperty);
        }

        private void DrawFingerSelector(string targetName)
        {
            SerializedProperty hasChainProperty = serializedObject.FindProperty("_has" + targetName);
            if (hasChainProperty.boolValue == false)
                return;

            SerializedProperty targetProperty = serializedObject.FindProperty("_" + targetName.ToLower() + "Target");
            if (targetProperty == selectedProperty)
                return;

            HandControl handControl = target as HandControl;
            BoneTransform handTransform = new BoneTransform(handControl.transform);
            handTransform.scale = Vector3.one;

            SerializedProperty positionProp = targetProperty.FindPropertyRelative("position");

            Vector3 targetPosition = handTransform.TransformPoint(positionProp.vector3Value);

            Handles.color = Color.white;
            MyHandles.DragHandle(targetPosition, 0.01f, Handles.SphereHandleCap, Color.yellow, out MyHandles.DragHandleResult result);
            if (result == MyHandles.DragHandleResult.LMBPress)
            {
                selectedProperty = targetProperty; 
                selectedChainProperty = serializedObject.FindProperty("_" + targetName.ToLower());
                Repaint();
            }
        }

        private void DrawFinderTarget(SerializedProperty targetProperty)
        {
            if (targetProperty == null)
                return;

            HandControl handControl = target as HandControl;
            BoneTransform handTransform = new BoneTransform(handControl.transform);
            handTransform.scale = Vector3.one;

            SerializedProperty positionProp = targetProperty.FindPropertyRelative("position");
            SerializedProperty rotationProp = targetProperty.FindPropertyRelative("rotation");

            EditorGUI.BeginChangeCheck();

            Vector3 targetPosition = handTransform.TransformPoint(positionProp.vector3Value);
            Quaternion targetRotation = handTransform.rotation * rotationProp.quaternionValue;
            targetRotation.Normalize();

            Handles.color = Color.white;
            Handles.TransformHandle(ref targetPosition, ref targetRotation);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(handControl, $"Set {targetProperty.displayName} Target");

                positionProp.vector3Value = handTransform.InverseTransformPoint(targetPosition);
                rotationProp.quaternionValue = Quaternion.Inverse(handTransform.rotation) * targetRotation;

                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Edit Finger Targets");

                int index = _edit.ToInt() - 1;
                index = GUILayout.Toolbar(index, new[] { EditorGUIUtility.IconContent("d_Preset.Context@2x") }, "AppCommand");
                if (EditorGUI.EndChangeCheck())
                {
                    if (index == _edit.ToInt() - 1)
                        index = -1;
                    _edit = index == 0;
                    SceneView.RepaintAll();
                }
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rig"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetSide"));

            
            if (selectedChainProperty != null)
            {
                SerializedProperty solverProperty = selectedChainProperty.FindPropertyRelative("solver");
                SerializedProperty solverProperty2 = solverProperty.FindPropertyRelative("_limit");
                if(solverProperty2 != null)
                {
                    solverProperty.isExpanded = true;
                    EditorGUILayout.PropertyField(solverProperty, true);
                    EditorGUILayout.PropertyField(solverProperty2, false);
                }
            }

            serializedObject.ApplyModifiedProperties();

            /*var lastSceneView = SceneView.lastActiveSceneView;
            if (lastSceneView == null || lastSceneView.camera == null)
            {
                EditorGUILayout.HelpBox("No Scene view found", MessageType.Error);
                return;
            }
            EditorGUI.BeginChangeCheck();
            {
                nearClipping = EditorGUILayout.Slider("Near clipping", nearClipping, 0.01f, 20f);
            }
            if (EditorGUI.EndChangeCheck())
            {
                lastSceneView.size = nearClipping;
                lastSceneView.Repaint();
            }
            if (GUILayout.Button("Focus near"))
            {
                lastSceneView.LookAt(lastSceneView.camera.transform.position + lastSceneView.camera.transform.forward, lastSceneView.rotation, nearClipping);
            }*/
            //base.OnInspectorGUI();
        }
    }
}
