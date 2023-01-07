using UnityEditor;
using UnityEngine;

namespace Rehcub 
{
    [CustomPropertyDrawer(typeof(Armature))]
    public class ArmatureDrawer : PropertyDrawer
    {
        [SerializeField] private bool _showTransforms = false;
        [SerializeField] private bool _showBones = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawProperty(property, "_fixHip");
            DrawProperty(property, "_hasShulders");
            DrawProperty(property, "_isInitalized");

            DrawTransforms(property);
            DrawBones(property);
        }

        private void DrawTransforms(SerializedProperty property)
        {
            _showTransforms = EditorGUILayout.Foldout(_showTransforms, "Transforms", true, EditorStyles.foldoutHeader);

            if (_showTransforms == false)
                return;

            EditorGUI.indentLevel++;

            DrawProperty(property, "_head");
            DrawProperty(property, "_neck");

            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                DrawProperty(property, "_leftShulder");
                DrawProperty(property, "_rightShulder");
            }
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                DrawProperty(property, "_leftUpperArm");
                DrawProperty(property, "_rightUpperArm");
            }
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                DrawProperty(property, "_leftLowerArm");
                DrawProperty(property, "_rightLowerArm");
            }
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                DrawProperty(property, "_leftHand");
                DrawProperty(property, "_rightHand");
            }
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                DrawProperty(property, "leftHand");
                DrawProperty(property, "rightHand");
            }
            DrawProperty(property, "_spine");
            DrawProperty(property, "_hip");

            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                DrawProperty(property, "_leftLeg");
                DrawProperty(property, "_rightLeg");
            }
            /*
            Debug.Log(customChains.arraySize);

            for (int i = 0; i < customChains.arraySize; i++)
            {
                DrawProperty(customChains.GetArrayElementAtIndex(i), $"Custom Chain {i}");
            }*/

            EditorGUI.indentLevel--;
        }

        private void DrawBones(SerializedProperty property)
        {
            _showBones = EditorGUILayout.Foldout(_showBones, "Bones & Chains", true, EditorStyles.foldoutHeader);

            if (_showBones == false)
                return;

            EditorGUI.indentLevel++;

            DrawProperty(property, "head");
            DrawProperty(property, "leftArm");
            DrawProperty(property, "rightArm");
            DrawProperty(property, "spine");
            DrawProperty(property, "hip");
            DrawProperty(property, "leftLeg");
            DrawProperty(property, "rightLeg");

            EditorGUI.indentLevel--;
        }

        private static void DrawProperty(SerializedProperty property, string name)
        {
            SerializedProperty prop = property.FindPropertyRelative(name);
            EditorGUILayout.PropertyField(prop, true);
        }
    }
}
