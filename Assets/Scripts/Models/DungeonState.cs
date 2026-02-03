using System;
using System.Collections.Generic;
using IdleViking.Data;
using IdleViking.Models;
using IdleViking.Systems;

namespace IdleViking.Models
{
    /// <summary>
    /// Persistent dungeon progress and energy.
    /// </summary>
    [Serializable]
    public class DungeonState
    {
        [Serializable]
        public class DungeonProgress
        {
            public string dungeonId;
            public int highestFloorCleared;
            public int timesCompleted;
        }

        public List<DungeonProgress> progress = new List<DungeonProgress>();

        // Energy system
        public float energy = 5f;
        public float maxEnergy = 5f;
        public float energyRegenPerSecond = 0.001389f; // ~1 energy per 12 minutes

        public DungeonProgress GetProgress(string dungeonId)
        {
            return progress.Find(p => p.dungeonId == dungeonId);
        }

        public DungeonProgress GetOrCreateProgress(string dungeonId)
        {
            var existing = GetProgress(dungeonId);
            if (existing != null) return existing;

            var p = new DungeonProgress { dungeonId = dungeonId };
            progress.Add(p);
            return p;
        }

        public void RegenEnergy(float seconds)
        {
            energy += energyRegenPerSecond * seconds;
            if (energy > maxEnergy)
                energy = maxEnergy;
        }

        public bool SpendEnergy(int cost)
        {
            if (energy < cost)
                return false;
            energy -= cost;
            return true;
        }
    }

    /// <summary>
    /// Transient state for an active dungeon run.
    /// Not saved — a run must be completed or abandoned in one session.
    /// </summary>
    public class DungeonRun
    {
        public string dungeonId;
        public int currentFloor;
        public int totalFloors;

        /// <summary>
        /// Tracked HP for each party viking across floors. Key = vikingUniqueId.
        /// </summary>
        public Dictionary<int, int> partyHP = new Dictionary<int, int>();

        /// <summary>
        /// Loot banked from cleared floors (applied on retreat or completion).
        /// </summary>
        public List<ResourceReward> bankedResources = new List<ResourceReward>();
        public List<string> bankedEquipmentNames = new List<string>();
        public int bankedXP;

        /// <summary>
        /// Combat logs from each completed floor for UI replay.
        /// </summary>
        public List<CombatLog> floorLogs = new List<CombatLog>();

        public bool IsComplete => currentFloor >= totalFloors;
        public bool IsBossFloor => currentFloor == totalFloors - 1;
    }
}
