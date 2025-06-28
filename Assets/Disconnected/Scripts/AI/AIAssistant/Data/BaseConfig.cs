
using System;

public abstract class BaseConfig
{
    // TODO: needed if we were to use reasoning in making dialogues as well
    public abstract string AssistantResponse {get; set; }

    public abstract string UserIntent {get; set; }
}