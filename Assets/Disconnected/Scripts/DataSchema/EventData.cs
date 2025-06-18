using System;

namespace Disconnected.Scripts.DataSchema
{
    [Serializable]
    public class EventData
    {
        /// <summary>
        /// (ej. "OnObjectClick", "OnLevelStart").
        /// </summary>
        public string triggerType;

        /// <summary>
        /// GUID of the object that origin the trigger.
        /// </summary>
        public string triggerSourceGuid;

        /// <summary>
        /// Kind of action (ej. "PlayAnimation", "PlaySound", "PlayTimeline").
        /// </summary>
        public string actionType;

        /// <summary>
        /// Identifier of the target of the action (ej. el nombre de un clip de animación o el ID de un timeline).
        /// </summary>
        public string actionTargetId;
    }
}