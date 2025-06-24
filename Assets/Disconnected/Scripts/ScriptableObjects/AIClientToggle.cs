using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/AIClientToggle")]
public class AIClientToggle : ScriptableObject
{
    [Header("Recorder")]
    public bool IsRecorderOn = true;
    // TODO: fake audio recordings
    public List<AudioClip> fakeMicRecordings;


    [Header("Speech to Text")]
    public bool IsSTTOn = true;
    public List<TextAsset> sttFakeOutputs;


    [Header("Text to Speech")]
    public bool IsTTSOn = true;
    public List<AudioClip> ttsFakeOutputs;


    [Header("Text To Image")]
    public bool IsTTIOn = true;
    public List<Texture2D> ttiFakeOutputs;


    [Header("Image to 3D")]
    public bool IsImgTo3dOn = true;
    public List<GameObject> it3dFakeOutputs;


    [Header("AI Reasoning")]
    public bool IsReasoningOn = true;
    public List<TextAsset> reasoningFakeOutputs;
}