using UnityEditor;
using UnityEngine;

namespace Rehcub 
{
    public class NotificationBar
    {
        private const float MessageDisplayTime = 3f;
        private string _message;
        private float _lastMessage;

        public void Draw()
        {
            if (_lastMessage + MessageDisplayTime < (float)EditorApplication.timeSinceStartup)
                _message = "";

            using (new GUILayout.HorizontalScope("box", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
            {
                EditorGUILayout.LabelField(_message);
            }
        }

        public void Push(string message)
        {
            _message = message;
            _lastMessage = (float)EditorApplication.timeSinceStartup;
        }
    }
}
