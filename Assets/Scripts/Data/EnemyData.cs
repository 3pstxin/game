using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Static definition of an enemy type.
    /// e.g. "Wolf", "Draugr", "Troll Chieftain".
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "IdleViking/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyId;
        public string displayName;

        [Header("Stats")]
        public int hp = 50;
        public int atk = 8;
        public int def = 3;
        public int spd = 4;

        [Header("Rewards")]
        [Tooltip("XP awarded to each surviving party member on kill")]
        public int xpReward = 20;

        [Header("Loot")]
        [Tooltip("Equipment that can drop from this enemy (null = no drop)")]
        public EquipmentData lootDrop;

        [Tooltip("Drop chance 0-1 (0.2 = 20%)")]
        [Range(0f, 1f)]
        public float dropChance = 0.1f;

        [Header("Resource Drops")]
        public ResourceCost[] resourceDrops;
    }
}
