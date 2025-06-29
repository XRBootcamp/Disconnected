using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class AssistantManager : MonoBehaviour
{
    [SerializeField] private AIGameSettings aiGameSettings;
    public AIGameSettings AISettings => aiGameSettings;
    [SerializeField] private GameObject aiSpeechToImage3dAssistant;
    [SerializeField] private GameObject voiceCharactersAssistant;

    [Header("Prompt Overrides")]
    [SerializeField] private Image3dSessionPreferences sessionPreferences;


    [Header("Debug")]
    [SerializeField] private AIClientToggle aiClientToggle;

    [SerializeField] private Transform debugSpawnImage3dLocation;
    [SerializeField] private Transform debugSpawnVoiceCharacterLocation;

    // TODO: add documents and create our prompt file as reference for our assistants 
    // NOTE: when session stuff changes, need to change here and call other open assistants
    // unsure if need this manager to manage it - but might be important for other stuff

    private Dictionary<string, BaseAssistant> assistantsList = new();
    private BaseAssistant currentAssistant;

    // Singleton instance - destroyed when scene ends
    private static AssistantManager instance;
    public static AssistantManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AssistantManager>();
                if (instance == null)
                {
                    Debug.LogError("[AssistantManager] AssistantManager not found in scene! Make sure to add it to your scene.");
                    throw new System.InvalidOperationException("AssistantManager not found in scene. Add it to your scene before using it.");
                }
            }
            return instance;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure only one instance exists
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogError("[AssistantManager] Multiple AssistantManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Debug.Log("[AssistantManager] Initialized successfully");
    }


    public void CreateNewImageTo3dChat(Vector3 position, Quaternion rotation, Transform parent)
    {
        // FIXME: assign position, rotation
        var obj = Instantiate(aiSpeechToImage3dAssistant, position, rotation, parent);
    }
    public void CreateNewVoiceCharacter(Vector3 position, Quaternion rotation, Transform parent)
    {
        // FIXME: assign position, rotation
        var obj = Instantiate(voiceCharactersAssistant, position, rotation, parent);
    }


    public BaseAssistant.State SetStateAfterOnHold(BaseAssistant aiAssistant)
    {
        return currentAssistant == aiAssistant ? BaseAssistant.State.Selected : BaseAssistant.State.None;
    }

    public void AddAssistant(string id, BaseAssistant newAssistant)
    {
        assistantsList.Add(id, newAssistant);
        newAssistant.onClosing.AddListener(RemoveAssistant);
    }

    public void SelectAssistant(string id)
    {
        TryUnselectAssistant(id);
        currentAssistant = assistantsList[id];
    }

    public void TryUnselectAssistant(string id)
    {
        if (currentAssistant == assistantsList[id])
        {
            currentAssistant = null;
        }
    }

    public void RemoveAssistant(string id)
    {
        TryUnselectAssistant(id);
        assistantsList.Remove(id);
    }

    public void ClearAllChats()
    {
        foreach (var item in assistantsList)
        {
            item.Value.onClosing.RemoveListener(RemoveAssistant);
        }
        currentAssistant = null;
        assistantsList.Clear();
    }

    void OnDestroy()
    {
        ClearAllChats();

        // Clear the singleton instance when destroyed
        if (instance == this)
        {
            instance = null;
        }
    }


    [Button]
    public void CreateNewImageTo3dChat()
    {
        Vector3 pos = debugSpawnImage3dLocation != null ? debugSpawnImage3dLocation.position : Vector3.zero;
        Quaternion rot = debugSpawnImage3dLocation != null ? debugSpawnImage3dLocation.rotation : Quaternion.identity;

        CreateNewImageTo3dChat(pos, rot, null);
    }

    [Button]
    public void CreateNewVoiceCharacter()
    {
        Vector3 pos = debugSpawnVoiceCharacterLocation != null ? debugSpawnVoiceCharacterLocation.position : Vector3.zero;
        Quaternion rot = debugSpawnVoiceCharacterLocation != null ? debugSpawnVoiceCharacterLocation.rotation : Quaternion.identity;
        CreateNewVoiceCharacter(pos, rot, null);
    }
}
