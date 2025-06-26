
using System.Collections.Generic;

public static class AssistantSpeechSnippets
{

    /// <summary>
    /// Signals creativity, thinking, or AI “working”
    /// </summary>
    public static readonly List<string> EffortBasedInterjections = new()
    {
        "Let me see what I can do.",
        "Ah, interesting... let me try something.",
        "Alright, let’s process that.",
        "Let me dig that up.",
        "Got it. I’m on it.",
        "Okay, thinking it through...",
        "Okay, let me find that.",
        "Let me work some magic.",
        "Hmm... let's see...",
        "Alright, computing the answer.",
        "Let me try to piece that together.",
        "Let’s build something for that.",
        "Okay, give me a second to figure this out.",
        "Let me cook something up.",
        "Alright, crafting a reply...",
        "I’ve got an idea... one sec.",
        "Let’s engineer a response.",
        "Alright, running some numbers.",
        "Let me spin up something clever.",
        "Time to do some thinking...",
        "Let me wire that together.",
        "I’ll see what I can invent here.",
        "Cooking up something new...",
        "Challenge accepted. Working on it.",
        "Okay, this’ll take some brainpower.",
        "Let me figure out a clever path.",
        "Rolling up my sleeves for this one...",
        "This one’s fun — let’s build something.",
        "Alright, tuning my thoughts...",
        "Let me improvise something helpful..."
    };

    /// <summary>
    /// Signals time, delay, or loading
    /// </summary>
    public static readonly List<string> TimeBasedInterjections = new()
    {
        "One moment...",
        "Just a second...",
        "Hang on...",
        "Hold tight...",
        "Almost there...",
        "Just a tick.",
        "Give me a moment.",
        "Still working on it...",
        "Processing... almost done.",
        "Alright, give me a sec.",
        "It won’t be long.",
        "Just pulling that up now.",
        "This’ll just take a second.",
        "Bear with me a little longer.",
        "Just loading things up...",
        "Fetching that right now...",
        "Working on it — won't be long.",
        "A little patience, please.",
        "Getting closer...",
        "Still spinning some gears...",
        "Just wrapping that up...",
        "Nearly there...",
        "Wait for it...",
        "Won’t keep you waiting too long.",
        "Almost finished...",
        "Let me just get that sorted.",
        "Downloading mental data...",
        "Give me a heartbeat.",
        "Just syncing things up...",
        "Almost set — give it a breath."
    };

    /// <summary>
    /// Signals in AI Speech To Speech Assistant that voices are ready to go.
    /// </summary>
    public static readonly List<string> CharacterVoiceReadyPhrases = new()
    {
        "All set! The voice is ready.",
        "The voice is good to go. Ready when you are.",
        "Voice loaded — just hit play to hear it.",
        "Done! The character’s voice is now live.",
        "All done! You can try it out now.",
        "Voice prepared. Want to give it a listen?",
        "Setup complete. Feel free to press ▶️.",
        "Voice’s in place. Let’s bring the scene to life.",
        "Ready! Hit the button when you're ready to hear it.",
        "The voice is warmed up and standing by.",
        "Good to go. The voice is ready for action.",
        "It’s alive! The character’s voice is ready.",
        "The voice is cooked and ready to serve.",
        "Voice completed. You can preview it now.",
        "Done! Tap the play button when you're ready.",
        "Voice generation successful. Try it out!",
        "Everything's in place. Ready to go.",
        "Character voice online. You may proceed.",
        "Voice is finalized. Press play if you're curious.",
        "All clear — voice is locked and loaded.",
        "Voice finished. Want to hear how it sounds?",
        "Completed. Just tap the play icon.",
        "It's ready! Hit the button to test it.",
        "Processing complete. Character voice is live.",
        "Voice loaded. Feel free to trigger playback.",
        "Mission accomplished — voice ready for use.",
        "Voice successfully prepared. Cue it up when ready.",
        "Character voice ready. Take it for a spin!",
        "It’s done. You can launch playback anytime.",
        "Voice ready. If you’re curious, give it a listen."
    };


    /// <summary>
    /// Voice lines when something went wrong
    /// </summary>
    public static readonly List<string> ErrorInterjections = new List<string>
{
    // Mild and neutral
    "Hmm... something didn’t work.",
    "That didn’t go as planned.",
    "Oops, looks like I hit a snag.",
    "Well, that’s awkward — it didn’t work.",
    "Hmm, I couldn't finish that.",
    "Uh-oh, something broke.",
    "Looks like something went wrong.",
    "Sorry, I couldn’t get that done.",
    "I ran into a little problem.",
    "That didn’t quite go through.",
    
    // With retry encouragement
    "Let me try that again.",
    "Give me a moment, I’ll retry.",
    "Hang tight — I’m trying again.",
    "Trying that once more...",
    "Let's give it another shot.",
    "One more try coming up.",
    "I’ll have another go at it.",
    "Okay, retrying now.",
    "Taking another stab at it.",
    "That failed — but I’m not giving up yet.",
    
    // With delay / ask for patience
    "That didn’t work. Maybe try again in a bit?",
    "Hmm, this might work better later.",
    "Let’s wait a little and try again.",
    "It’s not cooperating — try again soon.",
    "That failed — maybe give it another go later.",
    "This might be a temporary glitch.",
    "Not this time. Let’s try again shortly.",
    "Let’s pause and give it another go later.",
    "Something didn’t click. Retry in a few?",
    
    // Friendly / creative responses
    "My circuits got confused. Let’s try again.",
    "I got a little scrambled — retrying.",
    "Gremlins in the system, maybe?",
    "That didn’t land right. Let’s redo it.",
    "That one didn’t pass the vibe check.",
    "I think I fumbled that — trying again.",
    "Uh-oh, a hiccup. Let me reset.",
    "Even I make mistakes sometimes — trying again!",
    "No luck... but I'm still optimistic.",
    "Well, that backfired. Attempting again."
};

}