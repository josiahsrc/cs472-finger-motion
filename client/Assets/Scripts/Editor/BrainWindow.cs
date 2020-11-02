using UnityEngine;
using UnityEditor;
using System;

public class BrainWindow : EditorWindow
{
    [SerializeField] private TrainingConfig _trainingConfig = new TrainingConfig();

    private TrainingSession _trainingSession = new TrainingSession();
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
        _trainingConfig.body = EditorGUILayout.ObjectField("Body", _trainingConfig.body, typeof(Body), true) as Body;
        _trainingConfig.clip = EditorGUILayout.ObjectField("Clip", _trainingConfig.clip, typeof(AnimationClip), false) as AnimationClip;
        _trainingConfig.duration = EditorGUILayout.FloatField("Duration", _trainingConfig.duration);
        _trainingConfig.startDelay = EditorGUILayout.FloatField("Start Delay", _trainingConfig.startDelay);
        _trainingConfig.speed = EditorGUILayout.FloatField("Speed", _trainingConfig.speed);

        EditorGUILayout.Space();
        {
            EditorGUILayout.HelpBox(
                $"CURRENT SESSION INFO\n" +
                $"running:\t\t{_trainingSession.isRunning}\n" +
                $"time:\t\t{TimeSpan.FromSeconds(_trainingSession.t).ToString("mm':'ss':'ff")}\n" +
                $"percent complete:\t{Mathf.Clamp01(_trainingSession.t / _trainingConfig.duration) * 100}%",
                MessageType.Info
            );
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Fit (Override)"))
        {

        }

        if (GUILayout.Button("Fit (Append)"))
        {

        }
    }

    [Serializable]
    private class TrainingConfig
    {
        public Body body = null;
        public AnimationClip clip = null;
        public float duration = 10;
        public float startDelay = 3;
        public float speed = 1;
    }

    [Serializable]
    private class TrainingSession
    {
        public bool isRunning = false;
        public float t = 0.0f;
    }
}
