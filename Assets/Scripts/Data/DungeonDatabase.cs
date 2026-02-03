using System.Collections.Generic;
using UnityEngine;

namespace IdleViking.Data
{
    /// <summary>
    /// Registry of all dungeons.
    /// </summary>
    [CreateAssetMenu(fileName = "DungeonDatabase", menuName = "IdleViking/Dungeon Database")]
    public class DungeonDatabase : ScriptableObject
    {
        public List<DungeonData> dungeons = new List<DungeonData>();

        private Dictionary<string, DungeonData> _lookup;

        public DungeonData GetDungeon(string dungeonId)
        {
            BuildLookup();
            _lookup.TryGetValue(dungeonId, out DungeonData data);
            return data;
        }

        public List<DungeonData> GetAll() => dungeons;

        private void BuildLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, DungeonData>();
            foreach (var d in dungeons)
            {
                if (d != null && !string.IsNullOrEmpty(d.dungeonId))
                    _lookup[d.dungeonId] = d;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
