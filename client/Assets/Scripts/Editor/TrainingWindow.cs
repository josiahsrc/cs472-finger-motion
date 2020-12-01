using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Unity.EditorCoroutines.Editor;

using Request = BrainRequest;
using Response = BrainResponse;

/// <summary>
/// A training interface.
///
/// TODO: Decompose this class, it's pretty hefty.
/// </summary>
public class TrainingWindow : EditorWindow, Brain.IObserver, Brain.ICoroutineDriver
{
    private EditorCoroutine _brainSpinRoutine = null;
    private Brain _brain = null;
    private bool _debug = true;
    private Logger _logger = new Logger("Training Window", true);
    private Vector2 _windowScrollPos = Vector2.zero;

    private DataGatherConfig _dataGatherConfig = new DataGatherConfig();
    private DataGatherSession _dataGatherSession = new DataGatherSession();

    private EditorUtility.ReorderableObjectList<BrainDataFrame> _fitRoDataFrameList = null;
    private FitConfig _fitConfig = new FitConfig();
    private FitSession _fitSession = new FitSession();

    private bool isSessionRunning
    {
        get => _dataGatherSession.isRunning || _fitSession.isRunning;
    }

    [MenuItem("Window/App/Training Interface")]
    public static void showWindow()
    {
        TrainingWindow window = EditorWindow.GetWindow<TrainingWindow>();
        window.titleContent = new GUIContent("Training Interface");
        window.Show();
    }

    private void OnEnable()
    {
        _fitRoDataFrameList = new EditorUtility.ReorderableObjectList<BrainDataFrame>(_fitConfig.dataFrames);
        _fitRoDataFrameList.headerName = "Data Frames";
        _fitRoDataFrameList.itemName = "Data Frame";
        _fitRoDataFrameList.enable();

        if (_brain != null)
        {
            _brain.addObserver(this);
            _brain.setCoroutineDriver(this);
        }
    }

    private void OnDisable()
    {
        _fitRoDataFrameList.disable();

        if (_brain != null)
        {
            _brain.removeObserver(this);
            _brain.setCoroutineDriver(null);
        }
    }

    private void OnGUI()
    {
        _windowScrollPos = GUILayout.BeginScrollView(_windowScrollPos);

        drawGeneralGUI();
        EditorGUILayout.Space();
        drawDebugGUI();

        EditorUtility.GUIEnabled(_brain && _brain.isRunning && GUI.enabled, () =>
        {
            EditorGUILayout.Space();
            drawFittingGUI();
            EditorGUILayout.Space();
            drawDataGatherGUI();
        });


        GUILayout.EndScrollView();
    }

    private void drawDebugGUI()
    {
        GUILayout.Label("Debugging", EditorStyles.boldLabel);
        _debug = EditorGUILayout.Toggle("Debug Mode", _debug);
        _logger.debug = _debug;

        if (_debug)
        {
            if (GUILayout.Button("Log Status Report"))
            {
                if (!_brain)
                {
                    _logger.info("No brain assigned");
                }
                else
                {
                    _logger.info(_brain.statusReport());
                }
            }

            EditorGUILayout.Space();
            {
                var brainRunningMessage = _brain ? _brain.isRunning.ToString() : "N/A";
                EditorGUILayout.HelpBox(
                    $"INFO\n" +
                    $"brain_running: {brainRunningMessage}",
                    MessageType.Info
                );
            }
        }
    }

    private void drawGeneralGUI()
    {
        GUILayout.Label("General", EditorStyles.boldLabel);
        EditorUtility.GUIEnabled(!isSessionRunning && GUI.enabled, () =>
        {
            var prevBrain = _brain;
            _brain = EditorGUILayout.ObjectField("Brain", _brain, typeof(Brain), true) as Brain;

            if (prevBrain != _brain)
            {
                if (prevBrain != null)
                {
                    prevBrain.removeObserver(this);
                    prevBrain.setCoroutineDriver(null);
                }
                if (_brain != null)
                {
                    _brain.addObserver(this);
                    _brain.setCoroutineDriver(this);
                }
            }
        });

        EditorUtility.GUIEnabled(_brain && !isSessionRunning && GUI.enabled, () =>
        {
            if (!_brain || !_brain.isRunning)
            {
                if (GUILayout.Button("Start Brain Server"))
                {
                    _brain.start();
                }
            }
            else
            {
                if (GUILayout.Button("Stop Brain Server"))
                {
                    _brain.stop();
                }
            }
        });
    }

    private void drawFittingGUI()
    {
        GUILayout.Label("Training", EditorStyles.boldLabel);

        var globalGUIEnabled = GUI.enabled;
        EditorUtility.GUIEnabled((!isSessionRunning || _fitSession.isRunning) && GUI.enabled, () =>
        {
            GUI.enabled = !_fitSession.isRunning && !isSessionRunning && globalGUIEnabled;

            _fitRoDataFrameList.drawLayout(); 

            EditorGUILayout.Space();
            if (GUILayout.Button("Fit"))
            {
                var req = new Request.Fit();

                req.csvPaths = new string[_fitConfig.dataFrames.Count];
                for (int i = 0; i < req.csvPaths.Length; ++i)
                {
                    req.csvPaths[i] = _fitConfig.dataFrames[i].csvFullPath;
                }

                _brain.fit(req);
            }
        });
    }

    private void drawDataGatherGUI()
    {
        GUILayout.Label("Data Gathering", EditorStyles.boldLabel);

        EditorUtility.GUIEnabled(!isSessionRunning && GUI.enabled, () =>
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
        EditorUtility.GUIEnabled((!isSessionRunning || _dataGatherSession.isRunning) && GUI.enabled, () =>
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

        var dynamicThreshSnapshotInterval = 0.0f;

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

                if (input.elapsedTime > dynamicThreshSnapshotInterval)
                {
                    var request = new Request.AppendInstance();
                    request.csvPath = _dataGatherConfig.dataFrame.csvFullPath;
                    request.outputs = _dataGatherConfig.body.takeSnapshot().asArray();
                    _brain.appendInstance(request);

                    dynamicThreshSnapshotInterval = input.elapsedTime + _dataGatherConfig.snapshotInterval;
                }
            }

            return output;
        });

        SceneView.RepaintAll();
        EditorUtility.stopAnimationMode();
        gatherData_stop();
        Repaint();
    }

    void Brain.IObserver.onStart()
    {
        _logger.info("Server started");
    }

    void Brain.IObserver.onStop()
    {
        _logger.info("Server stopped");
    }

    void Brain.IObserver.onSaveModel(Response.SaveModel response)
    {
        _logger.info("Model saved");
    }

    void Brain.IObserver.onLoadModel(Response.LoadModel response)
    {
        _logger.info("Model loaded");
    }

    void Brain.IObserver.onAppendInstance(Response.AppendInstance response)
    {
        _logger.info($"Appended instance: {response.message}");
    }

    void Brain.IObserver.onFit(Response.Fit response)
    {
        _logger.info($"Fitted model: {response.message}");
        _fitSession.isRunning = false;
    }

    void Brain.IObserver.onPredict(Response.Predict response)
    {
        _logger.info("Predicted model");
    }

    void Brain.IObserver.onScore(Response.Score response)
    {
        _logger.info("Scored model");
    }

    void Brain.IObserver.onLog(Response.Log response)
    {
        _logger.info($"Received log: {response.message}");
    }

    void Brain.ICoroutineDriver.startSpinRoutine(IEnumerator routine)
    {
        _brainSpinRoutine = EditorCoroutineUtility.StartCoroutine(routine, this);
    }

    void Brain.ICoroutineDriver.stopSpinRoutine()
    {
        EditorCoroutineUtility.StopCoroutine(_brainSpinRoutine);
        _brainSpinRoutine = null;
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
        public List<BrainDataFrame> dataFrames = new List<BrainDataFrame>();
    }

    [Serializable]
    private class FitSession
    {
        public bool isRunning = false;
        public EditorCoroutine routine = null;
    }
}
