using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Early stage Voice Character UI Assistant with main buttons
/// The UI may change but the logic should stay similar
/// </summary>
public class Image3dUIAssistant : BaseUIAssistant
{
    private VoiceCharacterAssistant image3dAssistant;

    [SerializeField] private TMP_InputField characterTextInput;

    [SerializeField] private Button recordUserButton;
    [SerializeField] private Button sendRequestButton;

    public override void Bind(BaseAssistant assistant)
    {
        image3dAssistant = assistant as VoiceCharacterAssistant;
        
        recordUserButton.onClick.AddListener(image3dAssistant.ToggleRecording);
        // TODO: later on
        //sendRequestButton.onClick.AddListener(image3dAssistant.OverridePromptWithVoice);
        characterTextInput.onEndEdit.AddListener(t => image3dAssistant.SetUserIntent(t));
    }

    // use in inspector temporarily
    public void EnableCreateVoice(string newTextInput)
    {
        sendRequestButton.interactable = !string.IsNullOrWhiteSpace(newTextInput);
    }

    private void OnDestroy()
    {
        if (image3dAssistant != null)
        {
            // TODO: later on
            recordUserButton.onClick.RemoveListener(image3dAssistant.ToggleRecording);
            //sendRequestButton.onClick.RemoveListener(image3dAssistant.OverridePromptWithVoice);
        }
    }
}
