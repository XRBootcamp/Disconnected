using System;
using System.Collections.Generic;
using System.Linq;

public static class PlayAIVoiceHelper
{
    private const string Suffix = "_PlayAI";

    // 1. From Enum → Clean String (e.g., "Aaliyah")
    public static string ToCharacterName(this PlayAIVoice voice)
    {
        string name = voice.ToString();
        return name.EndsWith(Suffix) ? name.Substring(0, name.Length - Suffix.Length) : name;
    }

    // 2. From Clean String → Enum (e.g., "Aaliyah" → PlayAIVoice.Aaliyah_PlayAI)
    public static bool TryParseCharacterName(string characterName, out PlayAIVoice voice)
    {
        string enumName = characterName + Suffix;
        return Enum.TryParse(enumName, out voice);
    }
}