using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Configuration for the prestige (rebirth) system.
    /// Create one instance for your game's prestige rules.
    /// </summary>
    [CreateAssetMenu(fileName = "PrestigeConfig", menuName = "IdleViking/Prestige Config")]
    public class PrestigeData : ScriptableObject
    {
        [Header("Requirement")]
        [Tooltip("Minimum building level on a specific building to prestige")]
        public BuildingData requireBuilding;
        public int requireBuildingLevel = 10;

        [Tooltip("Alternative: minimum total resource ever earned (Gold)")]
        public double requireGoldLifetime = 10000;

        [Header("Multiplier")]
        [Tooltip("Production multiplier per prestige level: total = 1 + prestigeLevel * bonusPerLevel")]
        public double bonusPerLevel = 0.25;

        [Tooltip("Maximum prestige level (0 = unlimited)")]
        public int maxPrestigeLevel;

        [Header("What Resets")]
        [Tooltip("Resources reset to zero")]
        public bool resetResources = true;

        [Tooltip("Buildings and producers reset")]
        public bool resetBuildings = true;

        [Tooltip("Farm plots cleared")]
        public bool resetFarm = true;

        [Tooltip("Dungeon progress reset (but energy stays full)")]
        public bool resetDungeonProgress = true;

        [Header("What Persists")]
        // Vikings, equipment/inventory, milestones, prestige level, and unlock flags
        // always persist. These bools control the optional resets above.

        [Header("Prestige Bonus Reward")]
        [Tooltip("Bonus resources granted on prestige")]
        public ResourceCost[] prestigeRewards;

        /// <summary>
        /// Total production multiplier at a given prestige level.
        /// Applied globally to all resource production.
        /// </summary>
        public double GetMultiplier(int prestigeLevel)
        {
            return 1.0 + prestigeLevel * bonusPerLevel;
        }
    }
}
