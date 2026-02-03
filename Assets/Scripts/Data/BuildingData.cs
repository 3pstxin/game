using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Static definition of a building.
    /// Create one asset per building type (e.g. "Lumber Camp", "Smithy", "Great Hall").
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuilding", menuName = "IdleViking/Building")]
    public class BuildingData : ScriptableObject
    {
        [Header("Identity")]
        public string buildingId;
        public string displayName;
        [TextArea] public string description;

        [Header("Levels")]
        [Tooltip("Maximum level this building can reach")]
        public int maxLevel = 50;

        [Header("Cost")]
        public ResourceCost[] buildCosts;

        [Tooltip("Cost multiplier per level: cost = base * costExponent^(level-1)")]
        public double costExponent = 1.15;

        [Header("Production")]
        [Tooltip("If set, building/upgrading this creates and levels the linked producer")]
        public ResourceProducerData linkedProducer;

        [Header("Bonus")]
        [Tooltip("If true, this building provides a global production bonus to a resource type")]
        public bool providesBonus;
        public ResourceType bonusResourceType;

        [Tooltip("Bonus % per level (0.1 = +10% per level)")]
        public double bonusPerLevel = 0.1;

        [Header("Prerequisites")]
        [Tooltip("Another building that must be at a minimum level before this one can be built")]
        public BuildingData prerequisiteBuilding;
        public int prerequisiteLevel = 1;

        /// <summary>
        /// Get the cost array scaled to a specific level.
        /// </summary>
        public ResourceCost[] GetCostsForLevel(int targetLevel)
        {
            if (buildCosts == null) return new ResourceCost[0];

            var scaled = new ResourceCost[buildCosts.Length];
            double multiplier = System.Math.Pow(costExponent, targetLevel - 1);

            for (int i = 0; i < buildCosts.Length; i++)
            {
                scaled[i] = new ResourceCost
                {
                    resourceType = buildCosts[i].resourceType,
                    amount = buildCosts[i].amount * multiplier
                };
            }
            return scaled;
        }

        /// <summary>
        /// Total bonus multiplier at a given level (e.g. level 5, 0.1/level = 0.5 = +50%).
        /// Returns 0 if this building doesn't provide a bonus.
        /// </summary>
        public double GetBonusAtLevel(int level)
        {
            if (!providesBonus || level <= 0) return 0;
            return bonusPerLevel * level;
        }
    }

    /// <summary>
    /// A single resource cost entry. Used in arrays for multi-resource costs.
    /// </summary>
    [System.Serializable]
    public struct ResourceCost
    {
        public ResourceType resourceType;
        public double amount;
    }
}
