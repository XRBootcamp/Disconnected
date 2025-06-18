using System;
using System.Collections.Generic;

[Serializable]
public class ClipData
{
    public float startTime;
    public float duration;
    public string animationClipName; // name of the animation
}

[Serializable]
public class TrackData
{
    /// <summary>
    /// Name of the GUID of the object that this track is animating
    /// </summary>
    public string targetObjectGuid;
    public string trackType; // "AnimationTrack", "ActivationTrack", etc.
    public List<ClipData> clips = new List<ClipData>();
}

[Serializable]
public class TimelineData
{
    public string timelineId;
    public List<TrackData> tracks = new List<TrackData>();
}