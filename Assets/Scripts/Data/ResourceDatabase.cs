using System.Collections.Generic;
using UnityEngine;

namespace IdleViking.Data
{
    /// <summary>
    /// Registry of all resource producers in the game.
    /// Create one instance and assign all ResourceProducerData assets to it.
    /// Referenced by GameManager and passed to ResourceSystem.
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceDatabase", menuName = "IdleViking/Resource Database")]
    public class ResourceDatabase : ScriptableObject
    {
        public List<ResourceProducerData> producers = new List<ResourceProducerData>();

        private Dictionary<string, ResourceProducerData> _lookup;

        public ResourceProducerData GetProducer(string producerId)
        {
            BuildLookup();
            _lookup.TryGetValue(producerId, out ResourceProducerData data);
            return data;
        }

        private void BuildLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, ResourceProducerData>();
            foreach (var p in producers)
            {
                if (p != null && !string.IsNullOrEmpty(p.producerId))
                    _lookup[p.producerId] = p;
            }
        }

        /// <summary>
        /// Force rebuild after hot-reload in editor.
        /// </summary>
        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
