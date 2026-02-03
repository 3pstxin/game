using System;
using IdleViking.Data;

namespace IdleViking.Models
{
    /// <summary>
    /// Root container for all runtime game state. Serialized to JSON for saves.
    /// Each sub-system adds its own serializable section here.
    /// </summary>
    [Serializable]
    public class GameState
    {
        public string version = "0.1.0";
        public string lastSaveTimestamp;

        public ResourceState resources = new ResourceState();
        public ProducerState producers = new ProducerState();
        public BuildingState buildings = new BuildingState();
        public VikingState vikings = new VikingState();
        public InventoryState inventory = new InventoryState();
        public DungeonState dungeons = new DungeonState();
        public FarmState farm = new FarmState();
        public ProgressionState progression = new ProgressionState();

        public void MarkSaveTime()
        {
            lastSaveTimestamp = DateTime.UtcNow.ToString("o");
        }

        public DateTime GetLastSaveTime()
        {
            if (DateTime.TryParse(lastSaveTimestamp, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime result))
            {
                return result;
            }
            return DateTime.UtcNow;
        }
    }

    [Serializable]
    public class ResourceState
    {
        public SerializableDictionary<ResourceType, double> amounts = new SerializableDictionary<ResourceType, double>();

        public double Get(ResourceType type)
        {
            return amounts.TryGetValue(type, out double val) ? val : 0;
        }

        public void Add(ResourceType type, double amount)
        {
            if (!amounts.ContainsKey(type))
                amounts[type] = 0;
            amounts[type] += amount;
        }

        public bool Spend(ResourceType type, double amount)
        {
            if (Get(type) < amount)
                return false;
            amounts[type] -= amount;
            return true;
        }

        public bool CanAfford(ResourceType type, double amount)
        {
            return Get(type) >= amount;
        }

        /// <summary>
        /// Check if the player can afford a multi-resource cost array.
        /// </summary>
        public bool CanAfford(ResourceCost[] costs)
        {
            foreach (var cost in costs)
            {
                if (Get(cost.resourceType) < cost.amount)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Spend multiple resources at once. Returns false (and spends nothing) if any is insufficient.
        /// </summary>
        public bool Spend(ResourceCost[] costs)
        {
            if (!CanAfford(costs)) return false;

            foreach (var cost in costs)
                amounts[cost.resourceType] -= cost.amount;
            return true;
        }
    }
}
