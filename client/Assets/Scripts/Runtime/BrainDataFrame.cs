using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data Frame", menuName = "App/Brain Data Frame")]
public class BrainDataFrame : ScriptableObject
{
    [SerializeField] private string _csvPath = "data/file.csv";

    public string csvPath => _csvPath;
    public string csvFullPath => $"{Application.streamingAssetsPath}/{_csvPath}";
}
