using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FutureRoutine
{
    public delegate Output TickCallback(Input input);

    public static IEnumerator run(Config config, TickCallback onTick = null)
    {
        double startTime = EditorApplication.timeSinceStartup;
        double elapsedTime = 0;

        while (elapsedTime < (config.duration ?? double.PositiveInfinity))
        {
            elapsedTime = EditorApplication.timeSinceStartup - startTime;

            var input = new Input();
            input.elapsedTime = (float)elapsedTime;
            input.remainingTime = config.duration != null ? (float)elapsedTime - config.duration ?? 0 : float.NaN;

            var output = onTick?.Invoke(input);
            if ((output?.stop ?? false))
            {
                break;
            }

            yield return null;
        }
    }

    public struct Config
    {
        public float? duration;
    }

    public struct Input
    {
        public float elapsedTime;
        public float remainingTime;
    }

    public struct Output
    {
        public bool? stop;
    }
}
