using System;
using UnityEngine;

/// <summary>
/// For every Assistant Prefab - place this component in the root object
/// It links Assistant and UI and avoids the logic of going through the AssistantManager
/// And have the same logic encoded twice
/// </summary>
public class BaseAssistantAssembler : MonoBehaviour
{
    void Awake()
    {
        AssistantManager manager = FindFirstObjectByType<AssistantManager>();

        if (manager == null)
        {
            Debug.LogError($"[{nameof(BaseAssistantAssembler)}] - no {nameof(AssistantManager)} in scene. Please add the prefab");
            return;
        }

        if (manager.AISettings == null)
        {
            Debug.LogError($"[{nameof(BaseAssistantAssembler)}] - {nameof(AssistantManager)} without {nameof(AIGameSettings)} scriptable object added. It is mandatory");
            return;
        }

        var assistant = GetComponentInChildren<BaseAssistant>();
        if (assistant == null)
        {
            Debug.LogError("[SelfAssemblingAssistant] Missing Assistant component.");
            return;
        }

        var ui = GetComponentInChildren<BaseUIAssistant>();
        if (ui == null)
        {
            Debug.LogWarning("[SelfAssemblingAssistant] Missing UI component - no UI binding possible but assistant still works.");
        }

        var id = Guid.NewGuid().ToString();
        var config = GenerateConfigFor(assistant);

        assistant.Initialize(id, config, manager.AISettings);

        if (ui != null)
        {
            ui.Bind(assistant);
        }

        manager.AddAssistant(id, assistant);
    }

    private BaseConfig GenerateConfigFor(BaseAssistant assistant)
    {
        if (assistant is Image3dAssistant) return new Image3dConfig();
        if (assistant is VoiceCharacterAssistant) return new VoiceCharacterConfig();
        Debug.LogError("Unknown assistant type");
        return null;
    }
}