using UnityEditor;
using UnityEngine;

namespace Rehcub
{
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
