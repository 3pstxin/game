using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Static definition of a viking archetype / class.
    /// e.g. "Warrior", "Archer", "Gatherer", "Blacksmith".
    /// Each recruited viking references one of these as its template.
    /// </summary>
    [CreateAssetMenu(fileName = "NewViking", menuName = "IdleViking/Viking")]
    public class VikingData : ScriptableObject
    {
        [Header("Identity")]
        public string vikingId;
        public string displayName;
        [TextArea] public string description;
        public Rarity rarity = Rarity.Common;

        [Header("Base Stats (Level 1)")]
        public int baseHP = 100;
        public int baseATK = 10;
        public int baseDEF = 5;
        public int baseSPD = 5;

        [Header("Growth Per Level")]
        [Tooltip("Flat stat gain per level, scaled by rarity multiplier")]
        public float growthHP = 20f;
        public float growthATK = 3f;
        public float growthDEF = 2f;
        public float growthSPD = 1f;

        [Header("Recruitment")]
        public ResourceCost[] recruitCosts;

        [Header("Workforce")]
        [Tooltip("Production bonus when assigned to a building (e.g. 0.05 = +5%)")]
        public double workforceBonus = 0.05;

        /// <summary>
        /// Get a stat at a given level, applying rarity growth scaling.
        /// </summary>
        public int GetStat(StatType stat, int level)
        {
            float rarityScale = GetRarityMultiplier();
            int baseStat = GetBaseStat(stat);
            float growth = GetGrowth(stat);
            return baseStat + Mathf.FloorToInt(growth * (level - 1) * rarityScale);
        }

        /// <summary>
        /// XP required to reach a given level.
        /// Formula: 50 * level^1.8 — gives a smooth curve that accelerates.
        /// </summary>
        public int GetXPForLevel(int level)
        {
            if (level <= 1) return 0;
            return Mathf.FloorToInt(50f * Mathf.Pow(level, 1.8f));
        }

        private int GetBaseStat(StatType stat)
        {
            switch (stat)
            {
                case StatType.HP: return baseHP;
                case StatType.ATK: return baseATK;
                case StatType.DEF: return baseDEF;
                case StatType.SPD: return baseSPD;
                default: return 0;
            }
        }

        private float GetGrowth(StatType stat)
        {
            switch (stat)
            {
                case StatType.HP: return growthHP;
                case StatType.ATK: return growthATK;
                case StatType.DEF: return growthDEF;
                case StatType.SPD: return growthSPD;
                default: return 0;
            }
        }

        private float GetRarityMultiplier()
        {
            switch (rarity)
            {
                case Rarity.Common: return 1.0f;
                case Rarity.Uncommon: return 1.2f;
                case Rarity.Rare: return 1.5f;
                case Rarity.Epic: return 1.8f;
                case Rarity.Legendary: return 2.2f;
                default: return 1.0f;
            }
        }
    }
}
