﻿#if UNITY_EDITOR

using Rehcub;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace SerializableCollections
{
    public abstract class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        bool foldout;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            var dictionary = fieldInfo.GetValue(SerializeableCollectionsPropertyHelper.GetParent(property)) as IDictionary;
            if (dictionary == null) return height;

            return (foldout)
                ? (dictionary.Count + 1) * 17f
                : 17f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var dictionary = fieldInfo.GetValue(SerializeableCollectionsPropertyHelper.GetParent(property)) as IDictionary;
            if (dictionary == null) return;

            position = new Rect(position.x, position.y, position.width, 17f);
            foldout = EditorGUI.Foldout(position, foldout, label, true);
            EditorGUI.LabelField(position, label, new GUIContent() { text = "Count:" + dictionary.Count });
            if (foldout)
            {
                EditorGUI.indentLevel++;
                // only dump:)
                foreach (DictionaryEntry item in dictionary)
                {
                    position = new Rect(position.x, position.y + 17f, position.width, position.height);
                    EditorGUI.LabelField(position, item.Key.ToString(), (item.Value == null) ? "null" : item.Value.ToString());
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}

#endif