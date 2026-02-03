using System.Collections.Generic;
using IdleViking.Systems;

namespace IdleViking.Models
{
    public enum CombatResult
    {
        Victory,
        Defeat
    }

    public enum CombatantSide
    {
        Player,
        Enemy
    }

    /// <summary>
    /// Tracks a single combatant during battle. Created at battle start,
    /// mutated as damage is dealt.
    /// </summary>
    public class Combatant
    {
        public string name;
        public CombatantSide side;
        public int index;

        public StatBlock stats;
        public int currentHP;

        public bool IsAlive => currentHP > 0;

        // For player combatants: links back to viking for XP distribution
        public int vikingUniqueId;

        // For enemy combatants: links back to EnemyData for loot
        public string enemyDataId;
    }

    /// <summary>
    /// A single action in the combat log.
    /// </summary>
    public class CombatTurn
    {
        public int roundNumber;
        public string attackerName;
        public string defenderName;
        public int damage;
        public int defenderHPAfter;
        public bool defenderDied;
    }

    /// <summary>
    /// Full result of a completed battle.
    /// UI can replay this turn by turn for presentation.
    /// </summary>
    public class CombatLog
    {
        public CombatResult result;
        public List<CombatTurn> turns = new List<CombatTurn>();
        public int totalRounds;

        // Rewards (populated on victory)
        public int totalXP;
        public List<string> lootedEquipmentNames = new List<string>();
        public List<ResourceReward> resourceRewards = new List<ResourceReward>();

        // Surviving party members (for XP distribution)
        public List<int> survivingVikingIds = new List<int>();
    }

    public struct ResourceReward
    {
        public ResourceType type;
        public double amount;
    }
}
