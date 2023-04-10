using UnityEditor;
using UnityEngine;

namespace Rehcub 
{
    public class AddModifierPopup : EditorWindow
    {
        private static AddModifierPopup window;
        private static string[] _menuItems;
        private static System.Action<int> _selectionCallback;

        public static void Init(Rect position, string[] menuItems, System.Action<int> selectionCallback)
        {
            if(window != null)
                window.Close();
            window = CreateInstance<AddModifierPopup>();
            //window = GetWindow<AddModifierPopup>();
            _menuItems = menuItems;
            window.position = new Rect(position.x, position.y, 250, EditorGUIUtility.singleLineHeight * _menuItems.Length + 20f);
            _selectionCallback = selectionCallback;
            window.ShowPopup();
        }

        void OnGUI()
        {
            if (this != window)
                Close();
            if(_menuItems == null)
            {
                if(window != null)
                    window.Close();
                return;
            }


            for (int i = 0; i < _menuItems.Length; i++)
            {
                if (GUILayout.Button(_menuItems[i], GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    _selectionCallback.Invoke(i);
                    window.Close();
                }
            }
            Event e = Event.current;
            Debug.Log(e.mousePosition);
            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition) == false)
            {
                Debug.Log("Close");
                ClosePopup();
            }
        }

        public static void ClosePopup() => window?.Close();
    }
    public class Popup : PopupWindowContent
    {
        private Vector2 _windowSize;
        private string[] _menuItems;
        private System.Action<int> _selectionCallback;

        public static Popup Create(Rect position, string[] menuItems, System.Action<int> selectionCallback)
        {
            Popup popupExample = new Popup
            {
                _menuItems = menuItems,
                _selectionCallback = selectionCallback,
            };
            popupExample._windowSize = new Vector2(position.width, (EditorGUIUtility.singleLineHeight + 2f) * popupExample._menuItems.Length + 3f);

            return popupExample;
        }

        public override Vector2 GetWindowSize() => _windowSize;

        public override void OnGUI(Rect rect)
        {
            for (int i = 0; i < _menuItems.Length; i++)
            {
                if (GUILayout.Button(_menuItems[i], GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    _selectionCallback.Invoke(i);
                    editorWindow.Close();
                }
            }
        }

        public override void OnOpen()
        {
        }

        public override void OnClose()
        {
        }
    }
}
