using System.Collections.Generic;
using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Registry of all farmable crops and animals.
    /// </summary>
    [CreateAssetMenu(fileName = "FarmDatabase", menuName = "IdleViking/Farm Database")]
    public class FarmDatabase : ScriptableObject
    {
        public List<FarmPlotData> farmPlots = new List<FarmPlotData>();

        private Dictionary<string, FarmPlotData> _lookup;

        public FarmPlotData GetFarmPlot(string farmPlotId)
        {
            BuildLookup();
            _lookup.TryGetValue(farmPlotId, out FarmPlotData data);
            return data;
        }

        public List<FarmPlotData> GetByType(FarmType type)
        {
            return farmPlots.FindAll(f => f.farmType == type);
        }

        public List<FarmPlotData> GetAll() => farmPlots;

        private void BuildLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, FarmPlotData>();
            foreach (var f in farmPlots)
            {
                if (f != null && !string.IsNullOrEmpty(f.farmPlotId))
                    _lookup[f.farmPlotId] = f;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
