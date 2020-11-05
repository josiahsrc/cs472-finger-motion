using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(Brain))]
public class BrainEditor : Editor
{
    private DataGatherConfig _dataGatherConfig = new DataGatherConfig();
    private DataGatherSession _dataGatherSession = new DataGatherSession();
    private FitConfig _fitConfig = new FitConfig();
    private FitSession _fitSession = new FitSession();

    private bool isRunning
    {
        get => _dataGatherSession.isRunning || _fitSession.isRunning;
    }

    public override void OnInspectorGUI()
    {
        drawFittingGUI();
        EditorGUILayout.Space();
        drawDataGatherGUI();
    }

    private void drawFittingGUI()
    {
        GUILayout.Label("Training", EditorStyles.boldLabel);

        EditorUtility.GUIEnabled(!isRunning || _fitSession.isRunning, () =>
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Fit"))
            {

            }

            if (GUILayout.Button("Fit (Append)"))
            {

            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Stop"))
            {
                gatherData_stop();
            }
        });
    }

    private void drawDataGatherGUI()
    {
        GUILayout.Label("Data Gathering", EditorStyles.boldLabel);

        EditorUtility.GUIEnabled(!isRunning, () =>
        {
            _dataGatherConfig.body = EditorGUILayout.ObjectField("Body", _dataGatherConfig.body, typeof(Body), true) as Body;
            _dataGatherConfig.clip = EditorGUILayout.ObjectField("Clip", _dataGatherConfig.clip,
                typeof(AnimationClip), false) as AnimationClip;
            _dataGatherConfig.duration = EditorGUILayout.FloatField("Duration", _dataGatherConfig.duration);
            _dataGatherConfig.startDelay = EditorGUILayout.FloatField("Start Delay", _dataGatherConfig.startDelay);
            _dataGatherConfig.snapshotInterval = EditorGUILayout.FloatField("Snapshot Interval", _dataGatherConfig.snapshotInterval);
            _dataGatherConfig.speed = EditorGUILayout.FloatField("Speed", _dataGatherConfig.speed);
        });

        EditorGUILayout.Space();
        {
            var remainingTime = TimeSpan.FromSeconds(_dataGatherSession.remainingTime).ToString("mm':'ss':'ff");
            var status = _dataGatherSession.status.Length == 0 ? "N/A" : _dataGatherSession.status;
            EditorGUILayout.HelpBox(
                $"CURRENT SESSION INFO\n" +
                $"running:\t{_dataGatherSession.isRunning}\n" +
                $"time:\t{remainingTime}\n" +
                $"status:\t{status}",
                MessageType.Info
            );
        }

        EditorGUILayout.Space();
        EditorUtility.GUIEnabled(!isRunning || _dataGatherSession.isRunning, () =>
        {
            if (GUILayout.Button(_dataGatherSession.isRunning ? "Stop" : "Gather Data"))
            {
                if (_dataGatherSession.isRunning)
                {
                    gatherData_stop();
                }
                else
                {
                    gatherData_begin();
                }
            }
        });
    }

    private void gatherData_begin()
    {
        Debug.Assert(_dataGatherConfig.body != null);
        Debug.Assert(_dataGatherConfig.clip != null);
        Debug.Assert(!_dataGatherSession.isRunning);

        EditorUtility.startAnimationMode();
        _dataGatherSession.remainingTime = 0;
        _dataGatherSession.status = "";
        _dataGatherSession.isRunning = true;
        _dataGatherSession.routine = EditorCoroutineUtility.StartCoroutine(gatherData_routine(), this);
        Repaint();
    }

    private void gatherData_stop()
    {
        if (_dataGatherSession.routine != null)
        {
            EditorCoroutineUtility.StopCoroutine(_dataGatherSession.routine);
        }

        EditorUtility.stopAnimationMode();
        _dataGatherSession.remainingTime = 0;
        _dataGatherSession.status = "";
        _dataGatherSession.isRunning = false;
        _dataGatherSession.routine = null;
        Repaint();
    }

    private IEnumerator gatherData_routine()
    {
        var threshStart = _dataGatherConfig.startDelay;
        var threshGatherData = _dataGatherConfig.duration + threshStart;
        var durationTotal = threshGatherData;

        var futureConfig = new FutureRoutine.Config();
        futureConfig.duration = durationTotal;

        EditorUtility.startAnimationMode();
        yield return FutureRoutine.run(futureConfig, (input) =>
        {
            var output = new FutureRoutine.Output();

            void updateAnimationClip()
            {
                if (EditorApplication.isPlaying || !AnimationMode.InAnimationMode())
                {
                    Debug.LogError("Unable to fit.");
                    output.stop = true;
                }

                var animT = (input.elapsedTime * _dataGatherConfig.speed) % _dataGatherConfig.clip.length;
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_dataGatherConfig.body.gameObject, _dataGatherConfig.clip, animT);
                AnimationMode.EndSampling();

                SceneView.RepaintAll();
                Repaint();
            }

            if (input.elapsedTime < threshStart)
            {
                _dataGatherSession.remainingTime = threshStart - input.elapsedTime;
                _dataGatherSession.status = "Starting";
                updateAnimationClip();
            }
            else if (input.elapsedTime < threshGatherData)
            {
                _dataGatherSession.remainingTime = threshGatherData - input.elapsedTime;
                _dataGatherSession.status = "Gathering data";
                updateAnimationClip();
            }

            return output;
        });

        SceneView.RepaintAll();
        EditorUtility.stopAnimationMode();
        gatherData_stop();
        Repaint();
    }

    [Serializable]
    private class DataGatherConfig
    {
        public Body body = null;
        public AnimationClip clip = null;
        public float duration = 10;
        public float startDelay = 3;
        public float snapshotInterval = 0.1f;
        public float speed = 1;
    }

    [Serializable]
    private class DataGatherSession
    {
        public bool isRunning = false;
        public float remainingTime = 0.0f;
        public string status = "";
        public EditorCoroutine routine = null;
    }

    [Serializable]
    private class FitConfig
    {
        public Body body = null;
    }

    [Serializable]
    private class FitSession
    {
        public bool isRunning = false;
        public EditorCoroutine routine = null;
    }
}
