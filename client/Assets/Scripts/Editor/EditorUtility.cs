using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
}
