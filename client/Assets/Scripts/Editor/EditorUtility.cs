using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public static class EditorUtility
{
    public static void startAnimationMode()
    {
        if (!AnimationMode.InAnimationMode())
        {
            AnimationMode.StartAnimationMode();
        }
    }

    public static void stopAnimationMode()
    {
        if (AnimationMode.InAnimationMode())
        {
            AnimationMode.StopAnimationMode();
        }
    }

    public static void GUIEnabled(bool enabled, Action callback)
    {
        var prevEnabled = GUI.enabled;
        GUI.enabled = enabled;
        callback();
        GUI.enabled = prevEnabled;
    }

    public class ReorderableObjectList<T> where T : UnityEngine.Object
    {
        public string headerName = "";
        public string itemName = "";

        private List<T> _items = null;
        private ReorderableList _roList = null;

        public ReorderableObjectList(List<T> items)
        {
            _items = items;
            _roList = new ReorderableList(items, typeof(T), true, true, true, true);
        }

        public void enable()
        {
            _roList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, headerName);
            };

            _roList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                _items[index] = EditorGUI.ObjectField(rect, $"{itemName} {index}", _items[index], typeof(T), false) as T;
            };

            _roList.onAddCallback = (list) =>
            {
                _items.Add(null);
            };

            _roList.onRemoveCallback = (list) =>
            {
                _items.RemoveAt(list.index);
                list.index--;
            };
        }

        public void disable()
        {
            _roList.drawHeaderCallback = null;
            _roList.drawElementCallback = null;
            _roList.onAddCallback = null;
            _roList.onRemoveCallback = null;
        }

        public void drawLayout()
        {
            _roList.DoLayoutList();
        }
    }
}
