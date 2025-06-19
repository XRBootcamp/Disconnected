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

    /// <summary>
    /// Save the recorded audio as a WAV file in the temporary cache path.
    /// </summary>
    /// <param name="filename">The base filename (without extension).</param>
    /// <param name="clip">The AudioClip to save.</param>
    protected override void SaveWav(string filename, AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("No audio clip to save.");
            return;
        }

        string newFilePath = FileManagementExtensions.GenerateFilePath(FileEnumPath.Temporary, FilePaths.MIC_RECORDINGS, filename, FileExtensions.WAV, true);
        try
        {
            if (SavWav.Save(newFilePath, clip, false))
            {
                Debug.Log($"Saved WAV to temp: {newFilePath}");
                base.filePath = newFilePath; // Use protected field from base
            }
            else
            {
                Debug.LogError("Failed to save WAV to temp.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving WAV to temp: {ex.Message}");
        }
    }
    /*
    /// <summary>
    /// Move the last saved WAV file from temp to persistentDataPath/Recordings.
    /// </summary>
    public void MoveWavToPersistent()
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogError("No temp WAV file to move.");
            return;
        }
        persistentFilePath = FileManagementExtensions.MoveFileToPersistentDataPath(filePath, null);
    }
    

    /// <summary>
    /// Get the path to the last WAV file moved to persistent storage.
    /// </summary>
    /// <returns>Path to the last persistent WAV file.</returns>
    public string GetLastPersistentFile() => persistentFilePath;
    */
}
