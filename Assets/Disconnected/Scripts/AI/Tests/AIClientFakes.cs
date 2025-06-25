using System;
using System.IO;
using Runware;
using UnityEngine;

/// <summary>
/// Utility class to handle fake/test-mode AI responses from a central toggle config.
/// NOTE: These methods only work in UNITY_EDITOR - not build
/// </summary>
public static class AIClientFakes
{
    public static bool TryHandleFakeRecorder(AIClientToggle toggle, Action<AudioClip> onFake)
    {
#if !UNITY_EDITOR
        return false;
#endif
        if (toggle == null || toggle.IsRecorderOn)
            return false;

        if (toggle.fakeMicRecordings == null || toggle.fakeMicRecordings.Count == 0)
        {
            Debug.LogError("Recorder in TEST Mode ERROR - list of examples null or empty");
            return false;
        }

        var fake = ListExtension.GetRandomEntry(toggle.fakeMicRecordings);
        if (fake != null)
        {
            Debug.Log("Recorder in TEST Mode (using fake microphone recording)");
            onFake?.Invoke(fake);
            return true;
        }

        return false;
    }

    public static bool TryHandleFakeSTT(AIClientToggle toggle, Action<string> onFake)
    {
#if !UNITY_EDITOR
        return false;
#endif
        if (toggle == null || toggle.IsSTTOn)
            return false;

        if (toggle.sttFakeOutputs == null || toggle.sttFakeOutputs.Count == 0)
        {
            Debug.LogError("Speech-to-Text in TEST Mode ERROR - list of examples null or empty");
            onFake?.Invoke(null);
            return true;
        }

        var fake = ListExtension.GetRandomEntry(toggle.sttFakeOutputs);
        if (fake != null)
        {
            Debug.Log("Speech-to-Text in TEST Mode (using fake text asset)");
            onFake?.Invoke(fake.text);
            return true;
        }

        return false;
    }

    public static bool TryHandleFakeTTS(AIClientToggle toggle, Action<AudioClip> onFake)
    {
#if !UNITY_EDITOR
        return false;
#endif

        if (toggle == null || toggle.IsTTSOn)
            return false;

        if (toggle.ttsFakeOutputs == null || toggle.ttsFakeOutputs.Count == 0)
        {
            Debug.LogError("Text-to-Speech in TEST Mode ERROR - list of examples null or empty");
            onFake?.Invoke(null);
            return true;
        }

        var fake = ListExtension.GetRandomEntry(toggle.ttsFakeOutputs);
        if (fake != null)
        {
            Debug.Log("Text-to-Speech in TEST Mode (using fake audio clip)");
            onFake?.Invoke(fake);
            return true;
        }

        return false;
    }

    public static bool TryHandleFakeTTI(AIClientToggle toggle, Action<GenerateTextToImageOutputModel> onFake)
    {
#if !UNITY_EDITOR
        return false;
#endif

        if (toggle == null || toggle.IsTTIOn)
            return false;

        if (toggle.ttiFakeOutputs == null || toggle.ttiFakeOutputs.Count == 0)
        {
            Debug.LogError("Text-to-Image in TEST Mode ERROR - list of examples null or empty");
            onFake?.Invoke(null);
            return true;
        }

        var fake = ListExtension.GetRandomEntry(toggle.ttiFakeOutputs);
        if (fake != null)
        {
            GenerateTextToImageOutputModel outputFake = new(imagePath: $"Assets/ImaginaryPlace/{fake.name}", texture: fake );
            Debug.Log("Text-to-Image in TEST Mode (using fake texture)");
            onFake?.Invoke(outputFake);
            return true;
        }

        return false;
    }

    public static bool TryHandleFakeImageTo3D(AIClientToggle toggle, Action<GameObject> onFake)
    {
#if !UNITY_EDITOR
        return false;
#endif

        if (toggle == null || toggle.IsImgTo3dOn)
            return false;

        if (toggle.it3dFakeOutputs == null || toggle.it3dFakeOutputs.Count == 0)
        {
            Debug.LogError("Image-to-3D in TEST Mode ERROR - list of examples null or empty");
            onFake?.Invoke(null);
            return true;
        }

        var fake = ListExtension.GetRandomEntry(toggle.it3dFakeOutputs);
        if (fake != null)
        {
            Debug.Log("Image-to-3D in TEST Mode (using fake GameObject)");
            onFake?.Invoke(fake);
            return true;
        }

        return false;
    }

    public static bool TryHandleFakeReasoning(AIClientToggle toggle, Action<string> onFake)
    {
#if !UNITY_EDITOR
        return false;
#endif

        if (toggle == null || toggle.IsReasoningOn)
            return false;

        if (toggle.reasoningFakeOutputs == null || toggle.reasoningFakeOutputs.Count == 0)
        {
            Debug.LogError("AI Reasoning in TEST Mode ERROR - list of examples null or empty");
            onFake?.Invoke(null);
            return true;
        }

        var fake = ListExtension.GetRandomEntry(toggle.reasoningFakeOutputs);
        if (fake != null)
        {
            Debug.Log("AI Reasoning in TEST Mode (using fake reasoning text)");
            onFake?.Invoke(fake.text);
            return true;
        }

        return false;
    }
}
