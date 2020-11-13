using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Data Frame", menuName = "App/Brain Data Frame")]
public class BrainDataFrame : ScriptableObject
{
    [SerializeField] private string _csvPath = "data/file.csv";

    private Logger _logger = new Logger("Brain Data Frame");

    public string csvPath => _csvPath;
    public string csvFullPath => $"{Application.persistentDataPath}/{_csvPath}";

    public void wipe()
    {
        try
        {
            File.Create(csvFullPath);
        }
        catch (IOException e)
        {
            _logger.error(e);
        }
    }
}
