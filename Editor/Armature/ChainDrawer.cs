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
        private PopupExample _solverPopup;
        private IEnumerable<System.Type> _solverTypes;
        private string[] _solverMenuContent;
        private string _solverName;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty side = property.FindPropertyRelative("side");
            SerializedProperty chain = property.FindPropertyRelative("source");

            string name = side.enumDisplayNames[side.enumValueIndex] + " " + chain.enumDisplayNames[chain.enumValueIndex];

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, name, true, "Foldout");

            if (property.isExpanded == false)
                return;

            EditorGUI.indentLevel++;
            SerializedProperty bonesProp = property.FindPropertyRelative("_bones");

            bonesProp.isExpanded = EditorGUILayout.Foldout(bonesProp.isExpanded, bonesProp.displayName, true);

            if (bonesProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < bonesProp.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(bonesProp.GetArrayElementAtIndex(i));
                }
                EditorGUI.indentLevel--;
            }

            DrawProperty(property, "count");
            DrawProperty(property, "length");

            DrawProperty(property, "side");
            DrawProperty(property, "source");

            DrawProperty(property, "alternativeForward");
            DrawProperty(property, "alternativeUp");

            DrawSolverPanel(property);
            DrawProperty(property, "solver");

            EditorGUI.indentLevel--;
        }

        private static void DrawProperty(SerializedProperty property, string name)
        {
            SerializedProperty prop = property.FindPropertyRelative(name);
            EditorGUILayout.PropertyField(prop, true);
        }

        private void DrawSolverPanel(SerializedProperty chainProperty)
        {
            SerializedProperty solverProperty = chainProperty.FindPropertyRelative("solver");

            if (_solverTypes == null)
            {
                _solverTypes = GetTypes(typeof(Solver));
                _solverMenuContent = _solverTypes.Select((t) => t.Name).ToArray();
                _solverName = GetSolverName(solverProperty);
            }



            bool buttonResult;
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                buttonResult = GUILayout.Button(_solverName, EditorStyles.popup);
            }

            if (Event.current.type == EventType.Repaint)
            {
                _solverPopupRect = GUILayoutUtility.GetLastRect();
                _solverPopupRect.xMin += EditorGUI.indentLevel * 15f;
                _solverPopupRect.width = Mathf.Clamp(_solverPopupRect.width, 0, 250);
            }
            if (buttonResult)
            {
                _solverPopup = PopupExample.Init(_solverPopupRect, _solverMenuContent, index => SelectionCallback(index, chainProperty, solverProperty));
                PopupWindow.Show(_solverPopupRect, _solverPopup);
            }
        }

        private void SelectionCallback(int index, SerializedProperty chainProperty, SerializedProperty solverProperty)
        {
            Chain chain = chainProperty.GetValue() as Chain;
            Solver solver = System.Activator.CreateInstance(_solverTypes.ElementAt(index), chain) as Solver;

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
