using UnityEngine;
using System.IO;
using NaughtyAttributes;
using UnityEditor;

public class MicRecorder : MonoBehaviour
{
    protected AudioClip recordedClip;
    protected string micDevice;
    protected string filePath;

    [Header("Mic Settings")]
    public int duration = 10; // seconds
    public int sampleRate = 44100;

    protected void Start()
    {
        micDevice = Microphone.devices[0];
    }

    [Button]
    public void StartRecording()
    {
        recordedClip = Microphone.Start(micDevice, false, duration, sampleRate);
        Debug.Log("Recording...");
    }

    [Button]
    public virtual void StopAndSave()
    {
        Microphone.End(micDevice);
        Debug.Log("Recording stopped.");

        SaveWav("recorded_audio", recordedClip);
    }

    protected virtual void SaveWav(string filename, AudioClip clip)
    {
        filePath = Path.Combine(Application.persistentDataPath, filename + ".wav");
        if(SavWav.Save(filename, clip, true))
        {
            Debug.Log("Saved WAV to: " + filePath);
        }
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
}
