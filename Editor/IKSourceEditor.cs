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
            bool initialized = serializedObject.FindProperty("_initialized").boolValue;
            if (initialized == false)
            {
                EditorGUILayout.HelpBox("The IKSource Component should be created with the ArmatureBuilder.", MessageType.Error);
                return;
            }

            serializedObject.Update();

            SerializedProperty currentProperty = serializedObject.FindProperty("_clip");
            EditorGUILayout.PropertyField(currentProperty);

            serializedObject.ApplyModifiedProperties();

            if(currentProperty.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Assign a AnimationClip to open the Configurator!", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Show Configurator"))
            {
                IKSourceConfigurator.ShowConfigurator((IKSource)target);
            }

        }
    }
}
