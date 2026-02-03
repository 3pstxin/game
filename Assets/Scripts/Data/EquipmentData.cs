using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Static definition of an equipment item template.
    /// e.g. "Iron Sword", "Leather Tunic", "Wolf Pelt Hood".
    /// Actual instances get random bonus rolls on top of base stats.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "IdleViking/Equipment")]
    public class EquipmentData : ScriptableObject
    {
        [Header("Identity")]
        public string equipmentId;
        public string displayName;
        [TextArea] public string description;

        [Header("Classification")]
        public EquipmentSlot slot;
        public Rarity rarity = Rarity.Common;

        [Header("Base Stats (guaranteed)")]
        public int bonusHP;
        public int bonusATK;
        public int bonusDEF;
        public int bonusSPD;

        [Header("Random Bonus Rolls")]
        [Tooltip("Max extra stat points that can be randomly distributed on creation")]
        public int maxRandomBonus = 5;

        [Header("Crafting")]
        [Tooltip("Leave empty if this item is loot-only (not craftable)")]
        public ResourceCost[] craftCosts;

        [Header("Sell")]
        [Tooltip("Resource returned when selling this item")]
        public ResourceType sellResourceType = ResourceType.Gold;
        public double sellValue = 5;

        public bool IsCraftable => craftCosts != null && craftCosts.Length > 0;

        public int GetBaseStat(StatType stat)
        {
            switch (stat)
            {
                case StatType.HP: return bonusHP;
                case StatType.ATK: return bonusATK;
                case StatType.DEF: return bonusDEF;
                case StatType.SPD: return bonusSPD;
                default: return 0;
            }
        }

        /// <summary>
        /// Number of random bonus rolls based on rarity.
        /// Higher rarity = more rolls, each roll adds 1 point to a random stat.
        /// </summary>
        public int GetRollCount()
        {
            switch (rarity)
            {
                case Rarity.Common: return Mathf.Min(maxRandomBonus, 1);
                case Rarity.Uncommon: return Mathf.Min(maxRandomBonus, 3);
                case Rarity.Rare: return Mathf.Min(maxRandomBonus, 5);
                case Rarity.Epic: return Mathf.Min(maxRandomBonus, 8);
                case Rarity.Legendary: return Mathf.Min(maxRandomBonus, 12);
                default: return 0;
            }
        }
    }
}
