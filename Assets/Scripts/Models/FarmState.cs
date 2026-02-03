using System;
using System.Collections.Generic;

namespace IdleViking.Models
{
    /// <summary>
    /// Runtime state for the farming system.
    /// </summary>
    [Serializable]
    public class FarmState
    {
        public int maxPlots = 4;
        public int nextPlotId = 1;
        public bool autoHarvest;

        public List<FarmPlotInstance> plots = new List<FarmPlotInstance>();

        public FarmPlotInstance GetPlot(int plotId)
        {
            return plots.Find(p => p.plotId == plotId);
        }

        public int ActivePlotCount => plots.Count;
        public bool HasEmptySlot => plots.Count < maxPlots;
    }

    /// <summary>
    /// A single active farm plot with a planted crop or housed animal.
    /// </summary>
    [Serializable]
    public class FarmPlotInstance
    {
        public int plotId;
        public string farmPlotDataId;

        /// <summary>
        /// UTC timestamp when this was planted or last harvested (for animals).
        /// </summary>
        public string plantedTimestamp;

        /// <summary>
        /// Number of completed cycles harvested (animals only, for stats/UI).
        /// </summary>
        public int totalHarvests;

        public void SetPlantedNow()
        {
            plantedTimestamp = DateTime.UtcNow.ToString("o");
        }

        public void ResetTimer()
        {
            SetPlantedNow();
        }

        public DateTime GetPlantedTime()
        {
            if (DateTime.TryParse(plantedTimestamp, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime result))
            {
                return result;
            }
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Seconds elapsed since planted/last harvested.
        /// </summary>
        public double GetElapsedSeconds()
        {
            return (DateTime.UtcNow - GetPlantedTime()).TotalSeconds;
        }
    }
}
