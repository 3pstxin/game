using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Static definition of a resource producer.
    /// Create one asset per producer type (e.g. "Lumber Camp", "Quarry", "Farm").
    /// </summary>
    [CreateAssetMenu(fileName = "NewProducer", menuName = "IdleViking/Resource Producer")]
    public class ResourceProducerData : ScriptableObject
    {
        [Header("Identity")]
        public string producerId;
        public string displayName;

        [Header("Production")]
        public ResourceType resourceType;

        [Tooltip("Units produced per second at level 1")]
        public double baseProductionRate = 1.0;

        [Tooltip("Production multiplier per level: rate = base * (1 + (level-1) * upgradeMultiplier)")]
        public double upgradeMultiplier = 0.5;

        [Header("Cost")]
        public ResourceType costResource = ResourceType.Gold;

        [Tooltip("Cost to build or buy the first one")]
        public double baseCost = 10;

        [Tooltip("Cost scaling per level: cost = baseCost * costExponent^(level-1)")]
        public double costExponent = 1.15;

        /// <summary>
        /// Production rate at a given level.
        /// </summary>
        public double GetProductionRate(int level)
        {
            if (level <= 0) return 0;
            return baseProductionRate * (1 + (level - 1) * upgradeMultiplier);
        }

        /// <summary>
        /// Cost to upgrade from current level to next.
        /// </summary>
        public double GetUpgradeCost(int currentLevel)
        {
            int targetLevel = Mathf.Max(1, currentLevel + 1);
            return baseCost * System.Math.Pow(costExponent, targetLevel - 1);
        }
    }
}
