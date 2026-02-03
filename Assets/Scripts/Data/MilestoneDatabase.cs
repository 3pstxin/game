using System.Collections.Generic;
using UnityEngine;

namespace IdleViking.Data
{
    /// <summary>
    /// Registry of all milestones and prestige configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "MilestoneDatabase", menuName = "IdleViking/Milestone Database")]
    public class MilestoneDatabase : ScriptableObject
    {
        public List<MilestoneData> milestones = new List<MilestoneData>();
        public PrestigeData prestigeConfig;

        private Dictionary<string, MilestoneData> _lookup;

        public MilestoneData GetMilestone(string milestoneId)
        {
            BuildLookup();
            _lookup.TryGetValue(milestoneId, out MilestoneData data);
            return data;
        }

        /// <summary>
        /// Get milestones by category, sorted by sortOrder.
        /// </summary>
        public List<MilestoneData> GetByCategory(string category)
        {
            var result = milestones.FindAll(m => m.category == category);
            result.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
            return result;
        }

        public List<MilestoneData> GetAll() => milestones;

        private void BuildLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, MilestoneData>();
            foreach (var m in milestones)
            {
                if (m != null && !string.IsNullOrEmpty(m.milestoneId))
                    _lookup[m.milestoneId] = m;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
