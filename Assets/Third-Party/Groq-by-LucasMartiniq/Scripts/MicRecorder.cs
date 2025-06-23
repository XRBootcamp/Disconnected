using UnityEngine;
using System.IO;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.Events;

public class MicRecorder : MonoBehaviour
{
    protected AudioClip recordedClip;
    protected string micDevice;
    protected string filePath;

    [Header("Mic Settings")]
    private int duration = 30; // seconds
    public int sampleRate = 44100;

    // TODO: unsure if i should separate onRecordedAudio from TimePassed
    public UnityEvent<AudioClip> onRecordedAudio;
    public UnityEvent<AudioClip> onDurationPassed;

    // Recording state tracking
    private bool isRecording = false;
    private float recordingStartTime = 0f;
    private bool hasTriggeredOverRecord = false;

    protected void Start()
    {
        micDevice = Microphone.devices[0];
        
        // Register with the MicRecorderManager
        try
        {
            MicRecorderManager.Instance.RegisterRecorder(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MicRecorder] Failed to register with MicRecorderManager: {ex.Message}");
        }
    }

    [Button]
    public void StartRecording()
    {
        // Request permission from MicRecorderManager
        if (!MicRecorderManager.Instance.RequestStartRecording(this))
        {
            Debug.LogWarning($"[MicRecorder] Recording blocked - another recorder is already active");
            return;
        }

        recordedClip = Microphone.Start(micDevice, false, duration, sampleRate);
        isRecording = true;
        recordingStartTime = Time.time;
        hasTriggeredOverRecord = false;
        Debug.Log("Recording...");
    }

    [Button]
    public virtual void StopAndSave(bool invokeOnRecordedAudio = true)
    {
        if (!isRecording)
        {
            Debug.LogWarning("[MicRecorder] Attempted to stop recording when not recording");
            return;
        }

        Microphone.End(micDevice);
        isRecording = false;
        hasTriggeredOverRecord = false;
        
        // Notify the manager that recording has stopped
        MicRecorderManager.Instance.NotifyRecordingStopped(this);
        
        Debug.Log("Recording stopped.");

        SaveWav("recorded_audio", recordedClip);

        if (invokeOnRecordedAudio)
            onRecordedAudio.Invoke(recordedClip);
    }

    protected virtual void SaveWav(string filename, AudioClip clip)
    {
        filePath = Path.Combine(Application.persistentDataPath, filename + ".wav");
        if(SavWav.Save(filename, clip, true))
        {
            Debug.Log("Saved WAV to: " + filePath);
        }
    }

    void Update()
    {
        // Check if recording has exceeded the duration limit
        if (isRecording && !hasTriggeredOverRecord)
        {
            float currentRecordingTime = Time.time - recordingStartTime;
            
            if (currentRecordingTime >= duration)
            {
                hasTriggeredOverRecord = true;
                StopAndSave(false);
                onDurationPassed.Invoke(recordedClip);
                Debug.LogWarning($"[MicRecorder] Recording exceeded duration limit of {duration} seconds!");
            }
        }
    }

    /// <summary>
    /// Gets whether the microphone is currently recording.
    /// </summary>
    /// <returns>True if recording, false otherwise.</returns>
    public bool IsRecording()
    {
        return isRecording;
    }

    /// <summary>
    /// Gets the current recording time in seconds.
    /// </summary>
    /// <returns>Current recording time, or 0 if not recording.</returns>
    public float GetCurrentRecordingTime()
    {
        if (!isRecording)
        {
            return 0f;
        }
        
        return Time.time - recordingStartTime;
    }

    /// <summary>
    /// Gets the remaining recording time in seconds.
    /// </summary>
    /// <returns>Remaining recording time, or 0 if not recording or exceeded limit.</returns>
    public float GetRemainingRecordingTime()
    {
        if (!isRecording)
        {
            return 0f;
        }
        
        float remaining = duration - GetCurrentRecordingTime();
        return Mathf.Max(0f, remaining);
    }

    /// <summary>
    /// Gets whether the recording has exceeded the duration limit.
    /// </summary>
    /// <returns>True if recording has exceeded duration, false otherwise.</returns>
    public bool HasExceededDuration()
    {
        return hasTriggeredOverRecord;
    }

    /// <summary>
    /// Sets the recording duration. This method is called by MicRecorderManager.
    /// </summary>
    /// <param name="newDuration">The new duration in seconds.</param>
    public void SetDuration(int newDuration)
    {
        if (newDuration <= 0)
        {
            Debug.LogWarning("[MicRecorder] Duration must be greater than 0 seconds");
            return;
        }

        if (newDuration == duration)
        {
            return; // No change needed
        }

        int oldDuration = duration;
        duration = newDuration;

        Debug.Log($"[MicRecorder] Duration updated from {oldDuration}s to {duration}s");
    }

    /// <summary>
    /// Checks if this recorder is the one currently recording.
    /// </summary>
    /// <returns>True if this recorder is the active one, false otherwise.</returns>
    public bool IsActiveRecorder()
    {
        return MicRecorderManager.Instance.IsRecorderRecording(this);
    }

#if UNITY_EDITOR
    // NOTE: Only works in UNITY_EDITOR - for test mode usage only
    public void OverrideAudioClipAndPath(AudioClip clip)
    {
        recordedClip = clip;
        filePath = AssetDatabase.GetAssetPath(clip);
        Debug.Log($"[{nameof(MicRecorder)}] - TEST MODE: OverrideAudioClipAndPath by {clip.name}");
    }
#endif

    public string GetLastFilePath() => filePath;

    public AudioClip GetLastAudioClip() => recordedClip;

    void OnDestroy()
    {
        // Unregister from the MicRecorderManager
        try
        {
            MicRecorderManager.Instance.UnregisterRecorder(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MicRecorder] Failed to unregister from MicRecorderManager: {ex.Message}");
        }

        // Clean up events
        onRecordedAudio?.RemoveAllListeners();
        onDurationPassed?.RemoveAllListeners();
    }
}
