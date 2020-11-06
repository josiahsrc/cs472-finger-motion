using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Logger
{
    public bool debug = false;
    public string label = "Logger";

    public Logger() { }

    public Logger(bool debug)
    {
        this.debug = debug;
    }

    public Logger(string label)
    {
        this.label = label;
    }

    public Logger(string label, bool debug)
    {
        this.debug = debug;
        this.label = label;
    }

    public Logger(bool debug, string label)
    {
        this.debug = debug;
        this.label = label;
    }

    public void info(object msg)
    {
        if (debug)
        {
            Debug.Log($"[{label}] {msg}");
        }
    }

    public void warning(object msg)
    {
        if (debug)
        {
            Debug.LogWarning($"[{label}] {msg}");
        }
    }

    public void error(object msg)
    {
        if (debug)
        {
            Debug.LogError($"[{label}] {msg}");
        }
    }
}
