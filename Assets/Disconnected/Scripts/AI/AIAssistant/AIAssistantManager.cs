using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class AIAssistantManager : MonoBehaviour
{
    [SerializeField] private AIGameSettings aiGameSettings;
    [SerializeField] private GameObject aiAssistantPrefab;

    [Header("Prompt Overrides")]
    [SerializeField] private ImageSessionPreferences sessionPreferences;


    [Header("Debug")]
    [SerializeField] private AIClientToggle aiClientToggle;

    // TODO: add documents and create our prompt file as reference for our assistants 
    // NOTE: when session stuff changes, need to change here and call other open assistants
    // unsure if need this manager to manage it - but might be important for other stuff

    private HashSet<AIAssistant> assistantsList;
    private AIAssistant currentAssistant;

    // Singleton instance - destroyed when scene ends
    private static AIAssistantManager instance;
    public static AIAssistantManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AIAssistantManager>();
                if (instance == null)
                {
                    Debug.LogError("[AIAssistantManager] AIAssistantManager not found in scene! Make sure to add it to your scene.");
                    throw new System.InvalidOperationException("AIAssistantManager not found in scene. Add it to your scene before using it.");
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
            Debug.LogError("[AIAssistantManager] Multiple AIAssistantManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        assistantsList = new();
        Debug.Log("[AIAssistantManager] Initialized successfully");
    }

    [Button]
    public void CreateNewChat()
    {
        // FIXME: assign position, rotation
        var obj = Instantiate(aiAssistantPrefab);
        AIAssistant newAssistant = obj.GetComponent<AIAssistant>();
        newAssistant.Initialize(aiGameSettings);

        assistantsList.Add(newAssistant);
        newAssistant.onClosing.AddListener(RemoveChat);
    }

    public AIAssistant.State SetStateAfterOnHold(AIAssistant aiAssistant)
    {
        return currentAssistant == aiAssistant ? AIAssistant.State.Selected : AIAssistant.State.None;
    } 

    public void SelectAssistant(AIAssistant aiAssistant)
    {
        TryUnselectAssistant(currentAssistant);
        currentAssistant = aiAssistant;
    }

    public void TryUnselectAssistant(AIAssistant aiAssistant)
    {
        if (currentAssistant == aiAssistant)
        {
            currentAssistant = null;
        }
    }

    public void RemoveChat(AIAssistant toDelete)
    {
        TryUnselectAssistant(toDelete);
        assistantsList.Remove(toDelete);
    }

    public void ClearAllChats()
    {
        foreach (var item in assistantsList)
        {
            item.onClosing.RemoveListener(RemoveChat);
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
}
