using UnityEngine;
using UnityEditor;

namespace Rehcub
{
    [CustomEditor(typeof (IKSource))]
    public class IKSourceEditor : Editor 
    {
        private void OnEnable () 
        {
        	
        }

        public override void OnInspectorGUI ()
        {
            //base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty currentProperty;

            currentProperty = serializedObject.FindProperty("_tPose");
            EditorGUILayout.PropertyField(currentProperty);

            currentProperty = serializedObject.FindProperty("_clip");
            EditorGUILayout.PropertyField(currentProperty);

            if (GUILayout.Button("Show Configurator"))
            {
                IKSourceConfigurator.ShowConfigurator((IKSource)target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
