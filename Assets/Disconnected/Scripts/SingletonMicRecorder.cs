using UnityEngine;
using System;
using System.IO;
using UnityEditor;

/// <summary>
/// Singleton version of MicRecorder that saves audio to a temp folder first, then moves to persistent storage.
/// </summary>
public class SingletonMicRecorder : MicRecorder
{
    public static SingletonMicRecorder Instance { get; private set; }
    // NOTE: I assume always that persistentFiles are never the current filePath
    //private string persistentFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

}
