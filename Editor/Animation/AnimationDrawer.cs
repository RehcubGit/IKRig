using UnityEngine;
using UnityEditor;

namespace Rehcub
{
    [CustomPropertyDrawer(typeof(Animation))]
    public class AnimationDrawer : PropertyDrawer
    {
        private int _currentFrame;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            position.y += EditorGUIUtility.singleLineHeight;

            DrawProperty(position, property, "name");
            position.y += EditorGUIUtility.singleLineHeight;

            /*SerializedProperty keyframeProperty = property.FindPropertyRelative("_keyframes");
            _currentFrame = EditorGUI.IntSlider(position, _currentFrame, 0, keyframeProperty.arraySize);
            position.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, keyframeProperty.GetArrayElementAtIndex(_currentFrame), true);
            position.y += EditorGUIUtility.singleLineHeight;*/

            DrawProperty(position, property, "length");
            position.y += EditorGUIUtility.singleLineHeight;
        }

        private static void DrawProperty(Rect position, SerializedProperty property, string name)
        {
            SerializedProperty prop = property.FindPropertyRelative(name);
            EditorGUI.PropertyField(position, prop, true);
        }
    }
}
