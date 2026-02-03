using System;
using System.Collections.Generic;

namespace IdleViking.Models
{
    /// <summary>
    /// Runtime state for all buildings the player owns.
    /// </summary>
    [Serializable]
    public class BuildingState
    {
        public List<BuildingInstance> buildings = new List<BuildingInstance>();

        public BuildingInstance GetBuilding(string buildingId)
        {
            return buildings.Find(b => b.buildingId == buildingId);
        }

        public int GetLevel(string buildingId)
        {
            var b = GetBuilding(buildingId);
            return b != null ? b.level : 0;
        }

        public BuildingInstance AddBuilding(string buildingId)
        {
            var existing = GetBuilding(buildingId);
            if (existing != null) return existing;

            var instance = new BuildingInstance { buildingId = buildingId, level = 1 };
            buildings.Add(instance);
            return instance;
        }
    }

    [Serializable]
    public class BuildingInstance
    {
        public string buildingId;
        public int level = 1;
    }
}
