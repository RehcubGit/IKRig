using Bewildered.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rehcub
{
    [CustomPropertyDrawer(typeof(Chain))]
    public class ChainDrawer : PropertyDrawer
    {
        private Rect _solverPopupRect;
        private Popup _solverPopup;
        private IEnumerable<System.Type> _solverTypes;
        private string[] _solverMenuContent;
        private string _solverName;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                float y = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("alternativeForward"));
                y += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("alternativeUp"));
                y += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("solver"));

                return EditorGUIUtility.singleLineHeight * 6f + y;
            }
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty side = property.FindPropertyRelative("side");
            SerializedProperty chain = property.FindPropertyRelative("source");

            string name = side.enumDisplayNames[side.enumValueIndex] + " " + chain.enumDisplayNames[chain.enumValueIndex];

            Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, name, true, "Foldout"); 
            rect.y += EditorGUIUtility.singleLineHeight;

            if (property.isExpanded == false)
                return;

            EditorGUI.indentLevel++;
            /*SerializedProperty bonesProp = property.FindPropertyRelative("_bones");

            bonesProp.isExpanded = EditorGUILayout.Foldout(bonesProp.isExpanded, bonesProp.displayName, true);

            if (bonesProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < bonesProp.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(bonesProp.GetArrayElementAtIndex(i));
                }
                EditorGUI.indentLevel--;
            }*/

            DrawProperty(ref rect, property, "count");
            DrawProperty(ref rect, property, "length");

            DrawProperty(ref rect, property, "side");
            DrawProperty(ref rect, property, "source");

            //rect.y += EditorGUIUtility.singleLineHeight;
            DrawProperty(ref rect, property, "alternativeForward");
            DrawProperty(ref rect, property, "alternativeUp");

            DrawSolverPanel(rect, property);
            rect.y += EditorGUIUtility.singleLineHeight;
            DrawProperty(ref rect, property, "solver");
/*
            if(GUI.Button(rect, "Reset Solver"))
                property.FindPropertyRelative("solver").GetValue<Solver>().Reset();
            rect.y += EditorGUIUtility.singleLineHeight;*/

            /*object solver = property.FindPropertyRelative("solver").GetValue();
            EditorCools.Editor.ButtonsDrawer buttonsDrawer = new EditorCools.Editor.ButtonsDrawer(solver);
            buttonsDrawer.DrawButtons(solver);*/

            EditorGUI.indentLevel--;
        }

        private static void DrawProperty(SerializedProperty property, string name)
        {
            SerializedProperty prop = property.FindPropertyRelative(name);
            EditorGUILayout.PropertyField(prop, true);
        }
        private static void DrawProperty(ref Rect rect, SerializedProperty property, string name)
        {
            SerializedProperty prop = property.FindPropertyRelative(name);
            EditorGUI.PropertyField(rect, prop, true);
            rect.y += EditorGUI.GetPropertyHeight(prop);
        }

        private void DrawSolverPanel(Rect rect, SerializedProperty chainProperty)
        {
            SerializedProperty solverProperty = chainProperty.FindPropertyRelative("solver");

            if (_solverTypes == null)
            {
                _solverTypes = GetTypes(typeof(Solver));
                _solverMenuContent = _solverTypes.Select((t) => t.Name).ToArray();
            }


            _solverName = GetSolverName(solverProperty);

            //rect.x = EditorGUI.indentLevel * 15f;
            rect.xMin += EditorGUI.indentLevel * 15f;
            bool buttonResult = GUI.Button(rect, _solverName, EditorStyles.popup);

            //rect.width = Mathf.Clamp(_solverPopupRect.width, 0, 250);
            _solverPopupRect = rect;

            if (buttonResult)
            {
                SerializedProperty side = chainProperty.FindPropertyRelative("side");
                SerializedProperty sChain = chainProperty.FindPropertyRelative("source");

                string name = side.enumDisplayNames[side.enumValueIndex] + " " + sChain.enumDisplayNames[sChain.enumValueIndex];
                Debug.Log(name);
                _solverPopup = Popup.Create(_solverPopupRect, _solverMenuContent, index => SelectionCallback(index, chainProperty, solverProperty));
                PopupWindow.Show(_solverPopupRect, _solverPopup);
            }
        }

        private void SelectionCallback(int index, SerializedProperty chainProperty, SerializedProperty solverProperty)
        {
            Chain chain = chainProperty.GetValue() as Chain;
            Solver solver = System.Activator.CreateInstance(_solverTypes.ElementAt(index), chain) as Solver;


            SerializedProperty side = chainProperty.FindPropertyRelative("side");
            SerializedProperty sChain = chainProperty.FindPropertyRelative("source");

            string name = side.enumDisplayNames[side.enumValueIndex] + " " + sChain.enumDisplayNames[sChain.enumValueIndex];
            Debug.Log(name);
            string solverName = GetSolverName(solver);
            if (solver.ValidateChain(chain) == false)
            {
                Debug.LogError($"{solverName} is not supported for this chain");
                return;
            }

            _solverName = solverName;

            solverProperty.managedReferenceValue = solver;
            chainProperty.serializedObject.ApplyModifiedProperties();
        }

        private string GetSolverName(SerializedProperty solverProp)
        {
            return solverProp.managedReferenceFullTypename.Split(' ').Last().Split('.').Last();
        }
        
        private string GetSolverName(object solverObj)
        {
            return solverObj.GetType().ToString().Split('.').Last();
        }

        private IEnumerable<System.Type> GetTypes(System.Type type)
        {
            var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
            return types.Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);
        }
    }
}
