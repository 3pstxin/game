using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Static definition of a dungeon.
    /// Each dungeon has a sequence of floors with escalating enemies and a boss.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDungeon", menuName = "IdleViking/Dungeon")]
    public class DungeonData : ScriptableObject
    {
        [Header("Identity")]
        public string dungeonId;
        public string displayName;
        [TextArea] public string description;

        [Header("Structure")]
        [Tooltip("Total number of floors including boss")]
        public int floorCount = 5;

        [Tooltip("Enemies that appear on regular floors (picked randomly)")]
        public EnemyData[] enemyPool;

        [Tooltip("Number of enemies per regular floor")]
        public int enemiesPerFloor = 2;

        [Tooltip("Boss enemy on the final floor")]
        public EnemyData bossEnemy;

        [Header("Scaling")]
        [Tooltip("Enemy stat multiplier per floor: stats = base * (1 + floor * scalingPerFloor)")]
        public float scalingPerFloor = 0.15f;

        [Header("Cost")]
        [Tooltip("Energy cost to start a run")]
        public int energyCost = 1;

        [Header("Prerequisites")]
        [Tooltip("Required building and its minimum level to unlock this dungeon")]
        public BuildingData prerequisiteBuilding;
        public int prerequisiteLevel;

        [Header("Completion Rewards")]
        [Tooltip("Bonus resources for clearing all floors")]
        public ResourceCost[] completionRewards;

        [Tooltip("Bonus XP for clearing the dungeon (split among survivors)")]
        public int completionBonusXP = 100;

        /// <summary>
        /// Get the stat multiplier for a given floor (0-indexed).
        /// </summary>
        public float GetFloorMultiplier(int floorIndex)
        {
            return 1f + floorIndex * scalingPerFloor;
        }

        /// <summary>
        /// Pick random enemies for a regular floor from the pool.
        /// </summary>
        public EnemyData[] GetFloorEnemies(int floorIndex)
        {
            if (enemyPool == null || enemyPool.Length == 0)
                return new EnemyData[0];

            // Last floor is boss
            if (floorIndex >= floorCount - 1 && bossEnemy != null)
                return new EnemyData[] { bossEnemy };

            var enemies = new EnemyData[enemiesPerFloor];
            for (int i = 0; i < enemiesPerFloor; i++)
                enemies[i] = enemyPool[Random.Range(0, enemyPool.Length)];

            return enemies;
        }
    }
}
