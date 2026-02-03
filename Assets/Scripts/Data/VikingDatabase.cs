using System.Collections.Generic;
using UnityEngine;

namespace IdleViking.Data
{
    /// <summary>
    /// Registry of all viking archetypes.
    /// Create one instance, assign all VikingData assets.
    /// </summary>
    [CreateAssetMenu(fileName = "VikingDatabase", menuName = "IdleViking/Viking Database")]
    public class VikingDatabase : ScriptableObject
    {
        public List<VikingData> vikings = new List<VikingData>();

        private Dictionary<string, VikingData> _lookup;

        public VikingData GetViking(string vikingId)
        {
            BuildLookup();
            _lookup.TryGetValue(vikingId, out VikingData data);
            return data;
        }

        public List<VikingData> GetAll() => vikings;

        private void BuildLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, VikingData>();
            foreach (var v in vikings)
            {
                if (v != null && !string.IsNullOrEmpty(v.vikingId))
                    _lookup[v.vikingId] = v;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
