using System;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class VoiceCharacterConfig : BaseConfig
{
    // TODO: needed if we were to use reasoning in making dialogues as well
    public override string AssistantResponse { get; set; }
    public override string UserIntent { get; set; }
    
    [Min(0f), MaxValue(1f)]
    public float Volume {get; set; }

    [ExcludeReserved]
    public PlayAIVoice CharacterVoice {get; set; }

    // TODO: improvement - probably in session preferences not here
    public bool HearAssistantReplies {get; set; } = true;

}