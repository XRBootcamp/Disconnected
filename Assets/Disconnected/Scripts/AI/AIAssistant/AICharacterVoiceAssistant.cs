using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Runware;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEditor;
using TMPro;
using System.Linq;
using UnityEditor.Overlays;
using System.Text.Json;
using Assets.Disconnected.Scripts.AI.AIAssistant.API;
using System.Linq.Expressions;
using System.IO;
using System.Diagnostics;


[RequireComponent(typeof(GroqFilteredTTS))]
public class AICharacterVoiceAssistant : BaseAIAssistant
{
    [SerializeField] private GameObject characterVoicePrefab;
    [SerializeField] private GroqFilteredTTS characterTextToSpeechAI;


    // starts as null - it is the one being created by the prefab (but only once)
    // then it is forever referenced.
    public AudioSource CharacterVoiceSource { get; private set; }

    /// <summary>
    /// To populate everything I can right away
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
        characterTextToSpeechAI = GetComponent<GroqFilteredTTS>();
    }

    protected override void Start()
    {
        base.Start();

        characterTextToSpeechAI.onCompletedTTS.AddListener(AssignNewCharacterClipToAudioSource);
        ////characterTextToSpeechAI.onErrorTTS.AddListener(RemoveAudioClipFromCharacterAudioSource);
    }

    public void OverrideCharacterTextPrompt(string prompt)
    {
        characterTextToSpeechAI.SetPrompt(prompt);
    }

    public override void Initialize(AIGameSettings gameSettings)
    {
        base.Initialize(gameSettings);
        // TODO: if using reasoning AI to create dialogues then I need to initialize it
    }

    /// <summary>
    /// Get list of possible voices - to include in UI
    /// Technically these methods might not be needed if you get the GroqFilteredTTS
    /// </summary>
    /// <returns></returns>
    public PlayAIVoice[] GetSelectableVoices()
    {
        return characterTextToSpeechAI.GetSelectableVoices();
    }

    /// <summary>
    /// Set character voice - for UI
    /// Technically these methods might not be needed if you get the GroqFilteredTTS
    /// </summary>
    /// <param name="newVoice"></param>
    public void SetCharacterVoice(PlayAIVoice newVoice)
    {
        characterTextToSpeechAI.SelectedVoice = newVoice;
    }

    protected override async Task MakeRequestToAssistant()
    {
        try
        {
            // NOTE: this method does all the Heavy Lifting
            await characterTextToSpeechAI.GenerateTTS(
                text: userIntent,
                saveClipInRootPath: FileEnumPath.Persistent,
                relativePath: FilePaths.CHARACTER_SPEECH,
                filename: characterTextToSpeechAI.SelectedVoice.ToString(),
                appendDateTimeToFileName: true
            );
        }
        // TODO: play closer attention my try catch spread in the functions
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
            errorOutput = e.ToString();
            AssistantOnErrorOccurred(e.ToString());
            state = AIAssistantManager.Instance.SetStateAfterOnHold(this);

        }


    }

    private async void AssignNewCharacterClipToAudioSource(AudioClip newCharacterClip)
    {

        // assistant notifying the user that the voice is ready
        await AssistantAnswer(AssistantSpeechSnippets.CharacterVoiceReadyPhrases.GetRandomEntry());

        // starts as null - it is the one being created by the prefab (but only once)
        // then it is forever referenced.
        if (CharacterVoiceSource == null)
        {
            // prefab must include AudioSource otherwise big mistake
            GameObject obj = Instantiate(characterVoicePrefab);
            if (obj.TryGetComponent(out AudioSource source))
            {
                CharacterVoiceSource = source;
            }
            else
            {
                UnityEngine.Debug.LogError($"[{nameof(AICharacterVoiceAssistant)}] {characterVoicePrefab.name} prefab MUST HAVE AN AUDIOSOURCE HAS COMPONENT!");
            }
        }

        CharacterVoiceSource.clip = newCharacterClip;
    }

    private void RemoveAudioClipFromCharacterAudioSource()
    {
        if (CharacterVoiceSource == null || CharacterVoiceSource.clip == null) return;

        CharacterVoiceSource.Stop();
        CharacterVoiceSource.clip = null;
    }

    [Button]
    public void PlayCharacterVoice()
    {
        if (CharacterVoiceSource == null || CharacterVoiceSource.clip == null) return;
        CharacterVoiceSource.Stop();
        CharacterVoiceSource.Play();
    }

    [Button]
    public void StopCharacterVoice()
    {
        if (CharacterVoiceSource == null || CharacterVoiceSource.clip == null) return;
        CharacterVoiceSource.Stop();
    }

    [Button]
    public async void OverridePromptWithVoice()
    {
        await characterTextToSpeechAI.GenerateTTS(
            text: characterTextToSpeechAI.Prompt,
            saveClipInRootPath: FileEnumPath.Persistent,
            relativePath: FilePaths.CHARACTER_SPEECH,
            filename: characterTextToSpeechAI.SelectedVoice.ToString(),
            appendDateTimeToFileName: true
        );
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        characterTextToSpeechAI.onCompletedTTS.RemoveListener(AssignNewCharacterClipToAudioSource);
        //characterTextToSpeechAI.onErrorTTS.RemoveListener(RemoveAudioClipFromCharacterAudioSource);
    }

}