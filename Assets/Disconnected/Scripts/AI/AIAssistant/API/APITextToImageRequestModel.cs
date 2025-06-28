using System;
using System.Text.Json.Serialization;
using Runware;
using UnityEngine;


[Serializable]
public class APITextToImageRequestModel
{
    /// Constants - Invariable user prompt regardless of funcion (to deliver best results)
    public string UserRequestIntro { get; set; }

    /// Most important - this is what changes on every interaction - what the user wants now
    public string UserIntent { get; set; }


    /// Guidelines for prompt
    // name of Text 2 Image Model to help deliver the finest results 
    public string Text2ImageModel { get; set; }
    
    // Previous assistant response - not sure if it is needed - it may provide some additional context (although I am unsure)
    public string PreviousAssistantResponse { get; set; } = null; 

    // Previous positive prompt - when building text to image prompt - used to provide context: of things to include
    public string PreviousPositivePrompt { get; set; } = null;
    // Previous negative prompt - when building text to image prompt - used to provide context: of things to exclude
    public string PreviousNegativePrompt { get; set; } = null;
    
    // NOTE: details that come from session and chat preferences for additional awareness
    // CustomBackground - if null - then background should be transparent/white
    public string CustomBackground { get; set; } = null;
    // ArtStyle - image art style intended by user
    public string ArtStyle { get; set; } = null;
    // ImageShape - enum that mentions what type image dimension we should return: Square | Horizontal | Vertical
    public ImageShape ImageShape {get; set;} = ImageShape.Square;
    // AllowNSFW - boolean indicating if image may contain content NSFW 
    public bool AllowNSFW {get; set; } = false;
    // ImageTarget - generating this image for: ImageTo3D (image to 3d AI generator), WallArt (to be displayed as wall art)
    public ImageTarget ImageTargetGoal {get; set;} = ImageTarget.ImageTo3D; 


    public override string ToString()
    {
        string result = ""; 
        result += $"{UserRequestIntro}\n**{UserIntent}**\n";
        result += $"\nData to be considered:\n";
        result += $"Previous Assistant Response to be aware of (if any): {PreviousAssistantResponse}\n";
        result += $"Previous Positive Prompt to build upon (if any): {PreviousPositivePrompt}\n";
        result += $"Previous Negative Prompt to build upon (if any): {PreviousNegativePrompt}\n";
        // TODO: in the future text-to-image models may be swapped based on request for specific styles and stuff
        result += $"Text-to-Image AI Model: {Text2ImageModel}\n";
        result += $"\nSession/Chat Info that should be considered if user does not request to override it:\n";
        result += $"Output's End Goal: Generate an image for {ImageTargetGoal}\n";
        result += $"Allow Images NSFW: {AllowNSFW}\n";
        result += $"Image Shape: {ImageShape}\n";
        result += $"Art Style (if any): {ArtStyle}\n";
        result += $"Custom Background (if any): {CustomBackground}\n";

        return result;
    }
}