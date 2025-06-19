using UnityEngine;
using NaughtyAttributes;
using Unity.Collections;
using System;
using System.Threading.Tasks;

public class AIPipeline : MonoBehaviour
{
    [SerializeField] private WhisperTranscriber text2SpeechAI;

    [SerializeField] private GameObject speech2TextAIPrefab;
    [SerializeField] private FileEnumPath storeSpeech2TextWavFiles = FileEnumPath.Persistent;


    [Header("Debug")]
    [SerializeField] private AIClientToggle aiClientToggle;
    [SerializeField] private PlayAIVoice debugAIVoice;

    [Space]
    [SerializeField] private AudioClip micRecording;
    [SerializeField, TextArea(5,20)] private string currentSpeech;

    
    private GroqTTS currentGroqTTS;

    #region SpeechToText

    [Button]
    private void FirstStartRecording()
    {
        // NOTE: Fake Mic Recording
        if (AIClientFakes.TryHandleFakeRecorder(aiClientToggle, SetFakeMicRecording))
        {
            return;
        }

        text2SpeechAI.recorder.StartRecording();
    }

    [Button]
    private void SecondStopRecordingAndSave()
    {
        // NOTE: Fake Mic Recording
        if (AIClientFakes.TryHandleFakeRecorder(aiClientToggle, SetFakeMicRecording))
        {
            return;
        }

        text2SpeechAI.recorder.StopAndSave();
        SetMicRecording(text2SpeechAI.recorder.GetLastAudioClip());
    }

    private void SetFakeMicRecording(AudioClip newClip)
    {
#if UNITY_EDITOR
        text2SpeechAI.recorder.OverrideAudioClipAndPath(newClip);
#endif
        micRecording = newClip;
    }

    private void SetMicRecording(AudioClip newClip)
    {
        micRecording = newClip;
    }

    [Button]
    private void ThirdTranscribeRecording()
    {
        // NOTE: Fake STT
        if (AIClientFakes.TryHandleFakeSTT(aiClientToggle, SetCurrentSpeech))
        {
            return;
        }

        StartCoroutine(text2SpeechAI.Transcribe(
            text2SpeechAI.recorder.GetLastFilePath(), 
            SetCurrentSpeech 
        ));
    }
    private void SetCurrentSpeech(string text)
    {
        currentSpeech = text;
    }

    #endregion

    #region TextToSpeech
    [Button]
    private async Task ConvertTextToSpeech()
    {
        await CreateTextToSpeech(debugAIVoice, currentSpeech, null);
    }

    public async Task CreateTextToSpeech(PlayAIVoice aiVoice, string prompt, Transform parent)
    {
        // TODO: will we need to assign a position, parent?
        GameObject obj = Instantiate(speech2TextAIPrefab, parent);
        GroqTTS groqTTS = obj.GetComponent<GroqTTS>();

        // NOTE: Fake TTS
        if (AIClientFakes.TryHandleFakeTTS(aiClientToggle, clip => {
            groqTTS.SetClip(clip);
            groqTTS.PlayAudio();
        }))
        {
            return;
        }

        groqTTS.SetVoice(aiVoice);
        groqTTS.SetStoreGeneratedWavFiles(storeSpeech2TextWavFiles);
        await groqTTS.GenerateAndPlaySpeech(prompt);
    }
    #endregion

    #region TextToImage
    #endregion

    #region ImageTo3D
    #endregion

    #region AI_Reasoning
    #endregion

}
