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

    [SerializeField] private TMP_InputField characterTextInput;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown voiceDropdown;

    [SerializeField] private Button recordUserButton;
    [SerializeField] private Button createVoiceButton;

    public override void Bind(BaseAssistant assistant)
    {
        voiceAssistant = assistant as VoiceCharacterAssistant;
        volumeSlider.value = voiceAssistant.CharacterConfig.Volume;

        UpdateDropdownOptions();

        volumeSlider.onValueChanged.AddListener(v => voiceAssistant.SetCharacterVoiceVolume(v));
        voiceDropdown.onValueChanged.AddListener(TrySetNewCharacterVoice);
        recordUserButton.onClick.AddListener(voiceAssistant.ToggleRecording);
        createVoiceButton.onClick.AddListener(voiceAssistant.OverridePromptWithVoice);
        characterTextInput.onEndEdit.AddListener(t => voiceAssistant.SetUserIntent(t));
    }

    // use in inspector temporarily
    public void EnableCreateVoice(string newTextInput)
    {
        createVoiceButton.interactable = !string.IsNullOrWhiteSpace(newTextInput);
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
        if (voiceAssistant != null)
        {
            volumeSlider.onValueChanged.RemoveListener(v => voiceAssistant.SetCharacterVoiceVolume(v));
            voiceDropdown.onValueChanged.RemoveListener(TrySetNewCharacterVoice);
            recordUserButton.onClick.RemoveListener(voiceAssistant.ToggleRecording);
            createVoiceButton.onClick.RemoveListener(voiceAssistant.OverridePromptWithVoice);
        }
    }
}
