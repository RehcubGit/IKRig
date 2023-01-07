using UnityEditor;
using UnityEngine;

namespace Rehcub
{
    public class ExtendedEditorWindow : EditorWindow
    {
        protected SerializedObject serializedObject;
        protected SerializedProperty currentProperty;

        protected SerializedProperty selectedProperty;
        private string selectedPropertyPath;

        protected System.Action<SerializedProperty> onSelectProperty;

        protected void DrawProperty(SerializedProperty property, bool drawChildren)
        {
            string lastPropertyPath = string.Empty;

            foreach (SerializedProperty prop in property)
            {
                if(prop.isArray && prop.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.BeginHorizontal();
                    prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.displayName);
                    EditorGUILayout.EndHorizontal();

                    if (prop.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawProperty(prop, false);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(lastPropertyPath) == false && prop.propertyPath.Contains(lastPropertyPath))
                        continue;
                    lastPropertyPath = prop.propertyPath;
                    EditorGUILayout.PropertyField(prop, drawChildren);

                }
            }
        }

        protected void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
        {
            if (settings != null)
            {
                foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    if (foldout)
                    {
                        Editor.CreateCachedEditor(settings, null, ref editor);
                        editor.OnInspectorGUI();
                    }
                    if (check.changed)
                    {
                        //generator.ActiveNoiseSettingsChanged();
                    }
                }
            }
        }
        protected void DrawSettingsEditor(Object settings, ref Editor editor)
        {
            if (settings != null)
            {
                Editor.CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
            }
        }
        protected void Separator(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
        
        protected void Separator(int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, Color.gray);
        }

        protected void DrawSideBar(SerializedProperty property)
        {
            foreach (SerializedProperty prop in property)
            {
                using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
                {
                    if (GUILayout.Button(prop.displayName))
                    {
                        selectedPropertyPath = prop.propertyPath;
                    }
                    if (GUILayout.Button("-", GUILayout.MaxWidth(17f)))
                    {
                        //Delete Element;
                    }
                }
            }

            if (string.IsNullOrEmpty(selectedPropertyPath) == false)
                selectedProperty = serializedObject.FindProperty(selectedPropertyPath);
        }

        protected void DrawSideBar(SerializedProperty property, System.Func<SerializedProperty, string> getName)
        {
            bool buttonResult = false;
            foreach (SerializedProperty prop in property)
            {
                if (GUILayout.Button(getName(prop)))
                {
                    selectedPropertyPath = prop.propertyPath;
                    buttonResult = true;
                }
            }

            if (string.IsNullOrEmpty(selectedPropertyPath) == false && buttonResult)
            {
                selectedProperty = serializedObject.FindProperty(selectedPropertyPath);
                onSelectProperty?.Invoke(selectedProperty);
            }
        }

        //https://gist.github.com/bzgeb/3800350
        public object[] DropZone(int width, int height, System.Type type)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            //GUILayout.Box(title, GUILayout.Width(width), GUILayout.Height(height));
            //GUILayout.Box("Test", "PosX: " + drop_area.x + "\nPosY: " + drop_area.y + "\nWidth: " + drop_area.width + "\nHeight: " + drop_area.height);
            if (drop_area.Contains(evt.mousePosition) == false)
                return null;

            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                if (DragAndDrop.objectReferences[0].GetType() != type)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (eventType == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        isAccepted = true;
                    }
                }
                Event.current.Use();
            }

            return isAccepted ? DragAndDrop.objectReferences : null;
        }
        public object[] DropZone()
        {
            //GUILayout.Box(title, GUILayout.Width(w), GUILayout.Height(h));

            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }
                Event.current.Use();
            }

            return isAccepted ? DragAndDrop.objectReferences : null;
        }
        public object[] DropZone(System.Type type)
        {
            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                if(DragAndDrop.objectReferences[0].GetType() != type)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (eventType == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        isAccepted = true;
                    }
                }
                Event.current.Use();
            }

            return isAccepted ? DragAndDrop.objectReferences : null;
        }

        protected void DrawGrid(Rect position, int spaceing)
        {
            EditorGUI.DrawRect(position, new Color(0.16f, 0.16f, 0.16f, 1f));

            int lineCountX = (int) position.width / spaceing;
            lineCountX /= 2;

            for (int i = -lineCountX; i <= lineCountX; i++)
            {
                Handles.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                if (i == 0)
                    Handles.color = Color.white;
                float x = i * spaceing + position.center.x;
                Vector2 a = new Vector2(x, position.yMin);
                Vector2 b = new Vector2(x, position.yMax);
                Handles.DrawLine(a, b);
            }

            int lineCountY = (int) position.height / spaceing;
            lineCountY /= 2;

            for (int i = -lineCountY; i <= lineCountY; i++)
            {
                Handles.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                if (i == 0)
                    Handles.color = Color.white;
                float y = i * spaceing + position.center.y;
                Vector2 a = new Vector2(position.xMin, y);
                Vector2 b = new Vector2(position.xMax, y);
                Handles.DrawLine(a, b);
            }
        }


        protected void Apply() => serializedObject.ApplyModifiedProperties();
    }
    public class MyHandles
    {
        // internal state for DragHandle()
        static int s_DragHandleHash = "DragHandleHash".GetHashCode();
        static Vector2 s_DragHandleMouseStart;
        static Vector2 s_DragHandleMouseCurrent;
        static Vector3 s_DragHandleWorldStart;
        static float s_DragHandleClickTime = 0;
        static int s_DragHandleClickID;
        static float s_DragHandleDoubleClickInterval = 0.5f;
        static bool s_DragHandleHasMoved;

        // externally accessible to get the ID of the most resently processed DragHandle
        public static int lastDragHandleID;

        public enum DragHandleResult
        {
            none = 0,

            LMBPress,
            LMBClick,
            LMBDoubleClick,
            LMBDrag,
            LMBRelease,

            RMBPress,
            RMBClick,
            RMBDoubleClick,
            RMBDrag,
            RMBRelease,
        };

        public static Vector3 DragHandle(Vector3 position, float handleSize, Handles.CapFunction capFunc, Color colorSelected, out DragHandleResult result)
        {
            int id = GUIUtility.GetControlID(s_DragHandleHash, FocusType.Passive);
            lastDragHandleID = id;

            Vector3 screenPosition = Handles.matrix.MultiplyPoint(position);
            Matrix4x4 cachedMatrix = Handles.matrix;

            result = DragHandleResult.none;

            switch (Event.current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == id && (Event.current.button == 0 || Event.current.button == 1))
                    {
                        GUIUtility.hotControl = id;
                        s_DragHandleMouseCurrent = s_DragHandleMouseStart = Event.current.mousePosition;
                        s_DragHandleWorldStart = position;
                        s_DragHandleHasMoved = false;

                        Event.current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);

                        if (Event.current.button == 0)
                            result = DragHandleResult.LMBPress;
                        else if (Event.current.button == 1)
                            result = DragHandleResult.RMBPress;
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && (Event.current.button == 0 || Event.current.button == 1))
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);

                        if (Event.current.button == 0)
                            result = DragHandleResult.LMBRelease;
                        else if (Event.current.button == 1)
                            result = DragHandleResult.RMBRelease;

                        if (Event.current.mousePosition == s_DragHandleMouseStart)
                        {
                            bool doubleClick = (s_DragHandleClickID == id) &&
                                (Time.realtimeSinceStartup - s_DragHandleClickTime < s_DragHandleDoubleClickInterval);

                            s_DragHandleClickID = id;
                            s_DragHandleClickTime = Time.realtimeSinceStartup;

                            if (Event.current.button == 0)
                                result = doubleClick ? DragHandleResult.LMBDoubleClick : DragHandleResult.LMBClick;
                            else if (Event.current.button == 1)
                                result = doubleClick ? DragHandleResult.RMBDoubleClick : DragHandleResult.RMBClick;
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        s_DragHandleMouseCurrent += new Vector2(Event.current.delta.x, -Event.current.delta.y) * EditorGUIUtility.pixelsPerPoint; ;

                        Vector3 position2 = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(s_DragHandleWorldStart));
                        position2 += (Vector3)(s_DragHandleMouseCurrent - s_DragHandleMouseStart);
                        position = Handles.matrix.inverse.MultiplyPoint(Camera.current.ScreenToWorldPoint(position2));

                        if (Camera.current.transform.forward == Vector3.forward || Camera.current.transform.forward == -Vector3.forward)
                            position.z = s_DragHandleWorldStart.z;
                        if (Camera.current.transform.forward == Vector3.up || Camera.current.transform.forward == -Vector3.up)
                            position.y = s_DragHandleWorldStart.y;
                        if (Camera.current.transform.forward == Vector3.right || Camera.current.transform.forward == -Vector3.right)
                            position.x = s_DragHandleWorldStart.x;

                        if (Event.current.button == 0)
                            result = DragHandleResult.LMBDrag;
                        else if (Event.current.button == 1)
                            result = DragHandleResult.RMBDrag;

                        s_DragHandleHasMoved = true;

                        GUI.changed = true;
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    Color currentColour = Handles.color;
                    if (id == GUIUtility.hotControl && s_DragHandleHasMoved)
                        Handles.color = colorSelected;

                    Handles.matrix = Matrix4x4.identity;
                    capFunc(id, screenPosition, Quaternion.identity, handleSize, EventType.Repaint);
                    Handles.matrix = cachedMatrix;

                    Handles.color = currentColour;
                    break;

                case EventType.Layout:
                    Handles.matrix = Matrix4x4.identity;
                    HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(screenPosition, handleSize));
                    Handles.matrix = cachedMatrix;
                    break;
            }

            return position;
        }
    }

    public class Dragable2D
    {
        private EditorWindow _window;
        private bool dragging;
        private Vector2 offset = Vector2.zero;
        private Vector2 mouseStart;

        public Dragable2D(EditorWindow window)
        {
            _window = window;
        }

        public Rect Drag(Rect position)
        {
            Event evt = Event.current;
            EventType eventType = evt.type;

            if (position.Contains(evt.mousePosition))
                if (eventType == EventType.MouseDown)
                    dragging = true;

            if (eventType == EventType.MouseDrag && dragging)
            {
                position.x = evt.mousePosition.x;
                _window.Repaint();
            }

            if (eventType == EventType.MouseUp)
                dragging = false;

            return position;
        }

        public Rect Drag(Rect position, Vector2 snap)
        {
            Event evt = Event.current;
            EventType eventType = evt.type;

            Rect rect = position;
            rect.x += offset.x * snap.x;

            if (rect.Contains(evt.mousePosition))
                if (eventType == EventType.MouseDown)
                {
                    dragging = true;
                    mouseStart = evt.mousePosition;
                    mouseStart.x -= rect.x;
                }

            if (eventType == EventType.MouseDrag && dragging)
            {
                Debug.Log(evt.mousePosition.x);
                offset.x = Mathf.FloorToInt(evt.mousePosition.x / snap.x);
                offset.x -= Mathf.FloorToInt(mouseStart.x / snap.x);
                _window.Repaint();
            }

            if (eventType == EventType.MouseUp)
                dragging = false;

            position.x += offset.x * snap.x;

            return position;
        }
    }
}
