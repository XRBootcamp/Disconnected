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
    private Image3dAssistant image3dAssistant;

    [SerializeField] private TMP_InputField characterTextInput;

    [SerializeField] private Button recordUserButton;
    [SerializeField] private Button sendRequestButton;

    public override void Bind(BaseAssistant assistant)
    {
        image3dAssistant = assistant as Image3dAssistant;

        recordUserButton.onClick.AddListener(image3dAssistant.ToggleRecording);
        // TODO: later on
        //sendRequestButton.onClick.AddListener(image3dAssistant.OverridePromptWithVoice);
        characterTextInput.onEndEdit.AddListener(t => image3dAssistant.SetUserIntent(t));
    }

    // use in inspector temporarily
    public void EnableSendRequest(string newTextInput)
    {
        sendRequestButton.interactable = !string.IsNullOrWhiteSpace(newTextInput);
    }

    public override void Unbind()
    {
        if (image3dAssistant != null)
        {
            recordUserButton.onClick.RemoveListener(image3dAssistant.ToggleRecording);
            // TODO: later on
            //sendRequestButton.onClick.RemoveListener(image3dAssistant.OverridePromptWithVoice);
            characterTextInput.onEndEdit.RemoveListener(t => image3dAssistant.SetUserIntent(t));
            image3dAssistant = null;
        }

    }

    private void OnDestroy()
    {
        Unbind(); // for precaution
    }
}
