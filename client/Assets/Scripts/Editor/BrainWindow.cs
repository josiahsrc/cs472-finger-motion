using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

public class BrainWindow : EditorWindow
{
    [SerializeField] private FitConfig _fitConfig = new FitConfig();

    private FitSession _fitSession = new FitSession();
    private Vector2 _windowScrollPos = Vector2.zero;

    [MenuItem("Window/App/Brain")]
    private static void init()
    {
        BrainWindow window = EditorWindow.GetWindow<BrainWindow>();
        window.titleContent = new GUIContent("Brain");
        window.Show();
    }

    private void OnGUI()
    {
        _windowScrollPos = GUILayout.BeginScrollView(_windowScrollPos);

        drawTrainingGUI();

        GUILayout.EndScrollView();
    }

    private void drawTrainingGUI()
    {
        GUILayout.Label("Training", EditorStyles.boldLabel);
        _fitConfig.body = EditorGUILayout.ObjectField("Body", _fitConfig.body, typeof(Body), true) as Body;
        _fitConfig.clip = EditorGUILayout.ObjectField("Clip", _fitConfig.clip, typeof(AnimationClip), false) as AnimationClip;
        _fitConfig.duration = EditorGUILayout.FloatField("Duration", _fitConfig.duration);
        _fitConfig.startDelay = EditorGUILayout.FloatField("Start Delay", _fitConfig.startDelay);
        _fitConfig.snapshotInterval = EditorGUILayout.FloatField("Snapshot Interval", _fitConfig.snapshotInterval);
        _fitConfig.speed = EditorGUILayout.FloatField("Speed", _fitConfig.speed);

        EditorGUILayout.Space();
        {
            var remainingTime = TimeSpan.FromSeconds(_fitSession.remainingTime).ToString("mm':'ss':'ff");
            var status = _fitSession.status.Length == 0 ? "N/A" : _fitSession.status;
            EditorGUILayout.HelpBox(
                $"CURRENT SESSION INFO\n" +
                $"running:\t{_fitSession.isRunning}\n" +
                $"time:\t{remainingTime}\n" +
                $"status:\t{status}",
                MessageType.Info
            );
        }

        var prevGUIEnabled = GUI.enabled;

        GUI.enabled = !_fitSession.isRunning;
        EditorGUILayout.Space();
        if (GUILayout.Button("Fit (Overwrite)"))
        {
            fit_begin(true);
        }

        if (GUILayout.Button("Fit (Append)"))
        {
            fit_begin(false);
        }

        GUI.enabled = _fitSession.isRunning;
        EditorGUILayout.Space();
        if (GUILayout.Button("Stop"))
        {
            fit_stop();
        }

        GUI.enabled = prevGUIEnabled;
    }

    private void fit_begin(bool overwrite)
    {
        Debug.Assert(_fitConfig.body != null);
        Debug.Assert(_fitConfig.clip != null);
        Debug.Assert(!_fitSession.isRunning);

        _fitSession.remainingTime = 0;
        _fitSession.status = "";
        _fitSession.isRunning = true;
        _fitSession.routine = EditorCoroutineUtility.StartCoroutine(fit_routine(), this);
        Repaint();
    }

    private void fit_stop()
    {
        if (_fitSession.routine != null)
        {
            EditorCoroutineUtility.StopCoroutine(_fitSession.routine);
        }

        _fitSession.remainingTime = 0;
        _fitSession.status = "";
        _fitSession.isRunning = false;
        _fitSession.routine = null;
        Repaint();
    }

    private IEnumerator fit_routine()
    {
        var threshStart = _fitConfig.startDelay;
        var threshGatherData = _fitConfig.duration + threshStart;
        var durationTotal = threshGatherData;

        var futureConfig = new FutureRoutine.Config();
        futureConfig.duration = _fitConfig.duration;

        AnimationMode.StartAnimationMode();
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

                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_fitConfig.body.gameObject, _fitConfig.clip, input.elapsedTime % _fitConfig.clip.length);
                AnimationMode.EndSampling();

                SceneView.RepaintAll();
                Repaint();
            }

            if (input.elapsedTime < threshStart)
            {
                _fitSession.remainingTime = threshStart - input.elapsedTime;
                _fitSession.status = "Starting";
                updateAnimationClip();
            }
            else if (input.elapsedTime < threshGatherData)
            {
                _fitSession.remainingTime = threshGatherData - input.elapsedTime;
                _fitSession.status = "Gathering data";
                updateAnimationClip();
            }

            return output;
        });

        AnimationMode.StopAnimationMode();
        SceneView.RepaintAll();
        Repaint();
        fit_stop();
    }

    private IEnumerator fit_captureRoutine()
    {
        var futureConfig = new FutureRoutine.Config();
        futureConfig.duration = 1;
        yield return FutureRoutine.run(futureConfig);
    }

    [Serializable]
    private class FitConfig
    {
        public Body body = null;
        public AnimationClip clip = null;
        public float duration = 10;
        public float startDelay = 3;
        public float snapshotInterval = 0.1f;
        public float speed = 1;
    }

    [Serializable]
    private class FitSession
    {
        public bool isRunning = false;
        public float remainingTime = 0.0f;
        public string status = "";
        public EditorCoroutine routine = null;
    }
}
