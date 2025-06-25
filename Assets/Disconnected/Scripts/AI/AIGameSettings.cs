
using Runware;
using UnityEngine;

/// <summary>
/// Game settings that reserves the narrator voice on startup
/// </summary>
[CreateAssetMenu(fileName = "AIGameSettings", menuName = "Game/AISettings")]
public class AIGameSettings : ScriptableObject
{
    [Header("AI Assistant Settings")]
    public PlayAIVoice aiAssistantVoice = PlayAIVoice.Angelo_PlayAI;
    public TextToImageAIModel textToImageAIModelName;
    public string aiReasoningModel = "qwen/qwen3-32b";

    
    [Header("Registry Reference")]
    public EnumReservationRegistry reservationRegistry;
    
    /// <summary>
    /// Call this to set up initial reservations (e.g., on game start)
    /// </summary>
    public void ApplyReservations()
    {
        if (reservationRegistry != null)
        {
            reservationRegistry.ReserveValue("LockedAIVoices", aiAssistantVoice);
        }
    }
    
    void OnValidate()
    {
        // Automatically apply reservations when values change in editor
        if (reservationRegistry != null)
        {
            // FIXME: be careful with this method - if used elsewhere
            reservationRegistry.ClearAllReservations();
            ApplyReservations();
        }
    }
}