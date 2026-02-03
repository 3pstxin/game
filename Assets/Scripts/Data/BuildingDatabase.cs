using System.Collections.Generic;
using UnityEngine;

namespace IdleViking.Data
{
    /// <summary>
    /// Registry of all buildings in the game.
    /// Create one instance, assign all BuildingData assets.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingDatabase", menuName = "IdleViking/Building Database")]
    public class BuildingDatabase : ScriptableObject
    {
        public List<BuildingData> buildings = new List<BuildingData>();

        private Dictionary<string, BuildingData> _lookup;

        public BuildingData GetBuilding(string buildingId)
        {
            BuildLookup();
            _lookup.TryGetValue(buildingId, out BuildingData data);
            return data;
        }

        public List<BuildingData> GetAll() => buildings;

        private void BuildLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, BuildingData>();
            foreach (var b in buildings)
            {
                if (b != null && !string.IsNullOrEmpty(b.buildingId))
                    _lookup[b.buildingId] = b;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
