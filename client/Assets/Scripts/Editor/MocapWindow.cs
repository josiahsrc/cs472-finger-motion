using UnityEngine;
using UnityEditor;

public class MocapWindow : EditorWindow
{
    [SerializeField] private Body _target = null;

    private Vector2 _windowScrollPos = Vector2.zero;

    [MenuItem("Window/App/Mocap")]
    private static void init()
    {
        MocapWindow window = EditorWindow.GetWindow<MocapWindow>();
        window.titleContent = new GUIContent("Mocap");
        window.Show();
    }

    private void OnGUI()
    {
        _windowScrollPos = GUILayout.BeginScrollView(_windowScrollPos);

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        _target = EditorGUILayout.ObjectField("Target", _target, typeof(Body), true) as Body;

        GUILayout.EndScrollView();
    }
}
