using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

using Request = BrainRequest;
using Response = BrainResponse;

public class TrainingWindow : EditorWindow, Brain.IObserver
{
    private Brain _brain = null;
    private bool _debug = true;
    private Vector2 _windowScrollPos = Vector2.zero;
    private DataGatherConfig _dataGatherConfig = new DataGatherConfig();
    private DataGatherSession _dataGatherSession = new DataGatherSession();
    private FitConfig _fitConfig = new FitConfig();
    private FitSession _fitSession = new FitSession();

    private bool isRunning
    {
        get => _dataGatherSession.isRunning || _fitSession.isRunning;
    }

    [MenuItem("Window/App/Training Interface")]
    private static void init()
    {
        TrainingWindow window = EditorWindow.GetWindow<TrainingWindow>();
        window.titleContent = new GUIContent("Training Interface");
        window.Show();
    }

    private void OnGUI()
    {
        _windowScrollPos = GUILayout.BeginScrollView(_windowScrollPos);

        drawBrainGUI();
        EditorGUILayout.Space();
        drawFittingGUI();
        EditorGUILayout.Space();
        drawDataGatherGUI();

        GUILayout.EndScrollView();
    }

    private void drawBrainGUI()
    {
        GUILayout.Label("General Settings", EditorStyles.boldLabel);
        EditorUtility.GUIEnabled(!isRunning, () =>
        {
            _brain = EditorGUILayout.ObjectField("Brain", _brain, typeof(Brain), true) as Brain;
            _debug = EditorGUILayout.Toggle("Debug", _debug);
        });
    }

    private void drawFittingGUI()
    {
        GUILayout.Label("Training", EditorStyles.boldLabel);

        EditorUtility.GUIEnabled(!isRunning || _fitSession.isRunning, () =>
        {
            GUI.enabled = !_fitSession.isRunning;
            EditorGUILayout.Space();
            if (GUILayout.Button("Fit (Overwrite)"))
            {

            }

            if (GUILayout.Button("Fit (Append)"))
            {

            }

            GUI.enabled = _fitSession.isRunning;
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
            _dataGatherConfig.body = EditorGUILayout.ObjectField("Body", _dataGatherConfig.body,
                typeof(BodyController), true) as BodyController;
            _dataGatherConfig.animClip = EditorGUILayout.ObjectField("Clip", _dataGatherConfig.animClip,
                typeof(AnimationClip), false) as AnimationClip;
            _dataGatherConfig.dataFrame = EditorGUILayout.ObjectField("Data Frame", _dataGatherConfig.dataFrame,
                typeof(BrainDataFrame), false) as BrainDataFrame;
            _dataGatherConfig.duration = EditorGUILayout.FloatField("Duration", _dataGatherConfig.duration);
            _dataGatherConfig.startDelay = EditorGUILayout.FloatField("Start Delay", _dataGatherConfig.startDelay);
            _dataGatherConfig.snapshotInterval = EditorGUILayout.FloatField("Snapshot Interval",
                _dataGatherConfig.snapshotInterval);
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
        Debug.Assert(_dataGatherConfig.animClip != null);
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

                var animT = (input.elapsedTime * _dataGatherConfig.speed) % _dataGatherConfig.animClip.length;
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_dataGatherConfig.body.gameObject, _dataGatherConfig.animClip, animT);
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

    private void logInfo(object msg)
    {
        if (_debug)
        {
            Debug.Log($"[Training] {msg}");
        }
    }

    private void logWarning(object msg)
    {
        if (_debug)
        {
            Debug.LogWarning($"[Training] {msg}");
        }
    }

    private void logError(object msg)
    {
        if (_debug)
        {
            Debug.LogError($"[Training] {msg}");
        }
    }

    void Brain.IObserver.onStart()
    {
        logInfo("Server started");
    }

    void Brain.IObserver.onStop()
    {
        logInfo("Server stopped");
    }

    void Brain.IObserver.onSaveModel(Response::SaveModel response)
    {
        logInfo("Model saved");
    }

    void Brain.IObserver.onLoadModel(Response::LoadModel response)
    {
        logInfo("Model loaded");
    }

    void Brain.IObserver.onAppendInstance(Response::AppendInstance response)
    {
        logInfo("Appended instance");
    }

    void Brain.IObserver.onFit(Response::Fit response)
    {
        logInfo("Fitted model");
    }

    void Brain.IObserver.onPredict(Response::Predict response)
    {
        logInfo("Predicted model");
    }

    void Brain.IObserver.onScore(Response::Score response)
    {
        logInfo("Scored model");
    }

    void Brain.IObserver.onLog(Response::Log response)
    {
        logInfo($"Received log: {response.message}");
    }

    [Serializable]
    private class DataGatherConfig
    {
        public BodyController body = null;
        public AnimationClip animClip = null;
        public BrainDataFrame dataFrame = null;
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
        public BodyController body = null;
    }

    [Serializable]
    private class FitSession
    {
        public bool isRunning = false;
        public EditorCoroutine routine = null;
    }
}
