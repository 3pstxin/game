using System;
using System.Collections.Generic;

namespace IdleViking.Models
{
    /// <summary>
    /// Runtime state for all vikings the player owns.
    /// </summary>
    [Serializable]
    public class VikingState
    {
        public List<VikingInstance> vikings = new List<VikingInstance>();
        public int nextUniqueId = 1;

        /// <summary>
        /// Find a viking by its unique runtime ID.
        /// </summary>
        public VikingInstance GetById(int uniqueId)
        {
            return vikings.Find(v => v.uniqueId == uniqueId);
        }

        /// <summary>
        /// Get all vikings assigned to a specific building.
        /// </summary>
        public List<VikingInstance> GetAssignedTo(string buildingId)
        {
            return vikings.FindAll(v =>
                v.assignment == VikingAssignment.Building &&
                v.assignedBuildingId == buildingId);
        }

        /// <summary>
        /// Get all vikings currently in the combat party.
        /// </summary>
        public List<VikingInstance> GetParty()
        {
            return vikings.FindAll(v => v.assignment == VikingAssignment.Party);
        }

        /// <summary>
        /// Get all idle (unassigned) vikings.
        /// </summary>
        public List<VikingInstance> GetIdle()
        {
            return vikings.FindAll(v => v.assignment == VikingAssignment.Idle);
        }

        /// <summary>
        /// Create a new viking instance with a unique ID.
        /// </summary>
        public VikingInstance Add(string vikingDataId)
        {
            var instance = new VikingInstance
            {
                uniqueId = nextUniqueId++,
                vikingDataId = vikingDataId,
                level = 1,
                xp = 0,
                assignment = VikingAssignment.Idle
            };
            vikings.Add(instance);
            return instance;
        }
    }

    [Serializable]
    public class VikingInstance
    {
        public int uniqueId;
        public string vikingDataId;
        public int level = 1;
        public int xp;

        public VikingAssignment assignment = VikingAssignment.Idle;
        public string assignedBuildingId;
    }
}
