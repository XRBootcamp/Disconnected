using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Early stage Voice Character UI Assistant with main buttons
/// The UI may change but the logic should stay similar
/// </summary>
public class VoiceCharacterUIAssistant : BaseUIAssistant
{
    private VoiceCharacterAssistant voiceAssistant;

    [SerializeField] private TMP_InputField voiceTextInput;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown voiceDropdown;

    [SerializeField] private Button recordUserButton;
    [SerializeField] private Button createVoiceButton;
    [SerializeField] private Button playVoiceButton;

    public override void Bind(BaseAssistant assistant)
    {
        voiceAssistant = assistant as VoiceCharacterAssistant;
        volumeSlider.value = voiceAssistant.CharacterConfig.Volume;

        UpdateDropdownOptions();

        // NOTE: Important
        volumeSlider.onValueChanged.AddListener(v => voiceAssistant.SetCharacterVoiceVolume(v));
        voiceDropdown.onValueChanged.AddListener(TrySetNewCharacterVoice);
        voiceTextInput.onEndEdit.AddListener(SetNewUserIntent);

        recordUserButton.onClick.AddListener(voiceAssistant.ToggleRecording);
        createVoiceButton.onClick.AddListener(voiceAssistant.OverridePromptWithVoice);
        playVoiceButton.onClick.AddListener(voiceAssistant.PlayCharacterVoice);
    }

    private void SetNewUserIntent(string newTextInput)
    {
        // NOTE: this part always

        // TODO: write now I can convert user intent to a new dialogue, it just copies the stuff and goes
        voiceAssistant.SetUserIntent(newTextInput);
        // NOTE: remove this line once user intent goes through a LLM and creates a dialogue
        voiceAssistant.SetCharacterTextPrompt(newTextInput);
    }

    // use in inspector temporarily
    public void EnableCreateVoice(string newTextInput)
    {
        bool isTextNull = string.IsNullOrWhiteSpace(newTextInput);

        createVoiceButton.interactable = !isTextNull;

        /*
        // reset play button if null
        if (isTextNull)
        {
            playVoiceButton.interactable = false;
        }
        */
    }

    private void TrySetNewCharacterVoice(int i)
    {
        if (PlayAIVoiceHelper.TryParseCharacterName(voiceDropdown.options[i].text, out PlayAIVoice newVoice))
        {
            voiceAssistant.SetCharacterVoice(newVoice);
        }
        // TODO: if fails?
    }

    // FIXME: can't handle Assistant Voice changes at the moment
    public void UpdateDropdownOptions()
    {
        voiceDropdown.ClearOptions();
        voiceDropdown.AddOptions(voiceAssistant.GetSelectableVoices().Select(v => v.ToCharacterName()).ToList());
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public override void Unbind()
    {
        if (voiceAssistant != null)
        {
            volumeSlider.onValueChanged.RemoveListener(v => voiceAssistant.SetCharacterVoiceVolume(v));
            voiceDropdown.onValueChanged.RemoveListener(TrySetNewCharacterVoice);
            voiceTextInput.onEndEdit.RemoveListener(SetNewUserIntent);

            recordUserButton.onClick.RemoveListener(voiceAssistant.ToggleRecording);
            createVoiceButton.onClick.RemoveListener(voiceAssistant.OverridePromptWithVoice);
            playVoiceButton.onClick.RemoveListener(voiceAssistant.PlayCharacterVoice);
            voiceAssistant = null;
        }
    }
}
