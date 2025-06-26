using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

public class GroqFilteredTTS : GroqTTS
{
    [SerializeField]
    // ExclusedReserved is a property to garantee that the narrator voice is never taken in TTS
    [ExcludeReserved]  // So clean!
    private PlayAIVoice filteredSelectedVoice = PlayAIVoice.Fritz_PlayAI;
    protected override PlayAIVoice SelectedVoice
    {
        get => filteredSelectedVoice;
        set
        {
            var registry = EnumReservationRegistry.Instance;
            if (registry.IsReserved(value))
            {
                string reservedBy = registry.GetReservedBy(value);
                Debug.LogWarning($"Cannot select {value} - it's reserved by {reservedBy}");

                var availableVoices = registry.GetAvailableValues<PlayAIVoice>();
                if (availableVoices.Length > 0)
                {
                    filteredSelectedVoice = availableVoices[0];
                    Debug.Log($"Auto-assigned voice: {filteredSelectedVoice}");
                }
                else
                {
                    filteredSelectedVoice = default(PlayAIVoice);
                    Debug.LogWarning("No available voices to assign. Assigned default.");
                }
            }
            else
            {
                filteredSelectedVoice = value;
            }
        }
    }

    void Start()
    {
        // Force validation of the initial value (from Inspector)
        SelectedVoice = SelectedVoice;
    }

    /// <summary>
    /// Example method for runtime voice selection UI
    /// </summary>
    public PlayAIVoice[] GetSelectableVoices()
    {
        return EnumReservationRegistry.Instance.GetAvailableValues<PlayAIVoice>();
    }
}

/*
#if UNITY_EDITOR

[CustomEditor(typeof(GroqFilteredTTS))]
public class GroqFilteredTTSEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all fields except the base selectedVoice
        DrawPropertiesExcluding(serializedObject, "selectedVoice");
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
*/