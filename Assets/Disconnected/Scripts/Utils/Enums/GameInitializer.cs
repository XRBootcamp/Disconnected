using UnityEngine;

/// <summary>
/// Runtime manager to initialize reservations - add this to a GameObject in your first scene
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Settings Reference")]
    public AIGameSettings aigameSettings;
    
    void Awake()
    {
        // Make sure this persists across scenes
        DontDestroyOnLoad(gameObject);
        
        // Apply initial reservations
        if (aigameSettings != null)
        {
            aigameSettings.ApplyReservations();
            Debug.Log("Enum reservations initialized!");
        }
        else
        {
            Debug.LogWarning($"{nameof(AIGameSettings)} not assigned to {nameof(GameInitializer)}!");
        }
    }
}