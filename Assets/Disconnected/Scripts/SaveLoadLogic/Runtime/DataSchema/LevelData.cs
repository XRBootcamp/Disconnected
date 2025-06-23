using System;
using System.Collections.Generic;

namespace Disconnected.Scripts.DataSchema 
{
    [Serializable]
    public class LevelData
    {
        public string levelGuid;
        public string levelName;
        public string authorId;

        // List of objects in scene
        public List<SceneObjectData> objectsInScene;

        // List of timelines and events
        public List<TimelineData> timelines;
        public List<EventData> events;

        // Builder to initalizate tasks
        public LevelData()
        {
            objectsInScene = new List<SceneObjectData>();
            timelines = new List<TimelineData>();
            events = new List<EventData>();
        }
    }
}