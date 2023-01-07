using UnityEditor;
using UnityEngine;

namespace Rehcub 
{

    [CustomPropertyDrawer(typeof(BoneTransform))]
    public class BoneTransformDrawer : PropertyDrawer
    {
        private int _elementWidth = 200;
        private int _margin = 10;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect pos = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            Color oldColor = GUI.contentColor;
            Color color = new Color(0.63f, 0.63f, 0.63f, 1f);
            GUI.contentColor = color;

            SerializedProperty posProp = property.FindPropertyRelative("position");
            SerializedProperty rotProp = property.FindPropertyRelative("rotation");

            Rect posIndented = EditorGUI.IndentedRect(pos);

            GUI.Label(posIndented, $"{property.displayName} Transform");

            _elementWidth = (int)posIndented.width / 5;

            EditorGUI.indentLevel++;
            posIndented = EditorGUI.IndentedRect(pos);
            posIndented.width = _elementWidth;
            EditorGUI.indentLevel--;
            posIndented.y += EditorGUIUtility.singleLineHeight;

            using (new GUILayout.HorizontalScope())
            {
                posIndented.width = 75;
                GUI.Label(posIndented, "Position");
                posIndented.width = _elementWidth;

                SerializedProperty xProp = posProp.FindPropertyRelative("x");
                SerializedProperty yProp = posProp.FindPropertyRelative("y");
                SerializedProperty zProp = posProp.FindPropertyRelative("z");

                posIndented.x += 75;
                GUI.Label(posIndented, $"X:  {xProp.floatValue}");
                posIndented.x += _elementWidth + _margin;
                GUI.Label(posIndented, $"Y:  {yProp.floatValue}");
                posIndented.x += _elementWidth + _margin;
                GUI.Label(posIndented, $"Z:  {zProp.floatValue}");
            }

            posIndented.x -= 75;
            posIndented.x -= (_elementWidth + _margin) * 2;
            posIndented.y += EditorGUIUtility.singleLineHeight;

            using (new GUILayout.HorizontalScope())
            {
                posIndented.width = 75;
                GUI.Label(posIndented, "Rotation");
                posIndented.width = _elementWidth;

                SerializedProperty xProp = rotProp.FindPropertyRelative("x");
                SerializedProperty yProp = rotProp.FindPropertyRelative("y");
                SerializedProperty zProp = rotProp.FindPropertyRelative("z");
                SerializedProperty wProp = rotProp.FindPropertyRelative("w");

                posIndented.x += 75;
                GUI.Label(posIndented, $"X:  {xProp.floatValue}");
                posIndented.x += _elementWidth + _margin;
                GUI.Label(posIndented, $"Y:  {yProp.floatValue}");
                posIndented.x += _elementWidth + _margin;
                GUI.Label(posIndented, $"Z:  {zProp.floatValue}");
                posIndented.x += _elementWidth + _margin;
                GUI.Label(posIndented, $"W:  {wProp.floatValue}");
            }

            GUI.contentColor = oldColor;
        }
    }
}
