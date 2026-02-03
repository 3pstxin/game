using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// A single condition within a milestone. All conditions must be met.
    /// </summary>
    [System.Serializable]
    public struct MilestoneCondition
    {
        public MilestoneConditionType type;

        [Tooltip("ResourceType (for ResourceAmount), or unused")]
        public ResourceType resourceType;

        [Tooltip("Building/dungeon/milestone ID (for relevant condition types)")]
        public string targetId;

        [Tooltip("Required amount, level, or count")]
        public int requiredValue;
    }

    /// <summary>
    /// Reward granted when a milestone is completed.
    /// </summary>
    [System.Serializable]
    public struct MilestoneReward
    {
        public MilestoneRewardType rewardType;
        public ResourceType resourceType;
        public double amount;

        [Tooltip("For unlock rewards: the feature flag name to set")]
        public string unlockFlag;
    }

    public enum MilestoneRewardType
    {
        /// <summary>Grant a resource amount.</summary>
        Resource,

        /// <summary>Increase farm plot capacity.</summary>
        FarmSlot,

        /// <summary>Increase dungeon energy cap.</summary>
        EnergyCapIncrease,

        /// <summary>Increase inventory capacity.</summary>
        InventorySlot,

        /// <summary>Set a named unlock flag (checked by IsUnlocked).</summary>
        UnlockFlag
    }

    /// <summary>
    /// Static definition of a progression milestone.
    /// Milestones are the single driver for unlocking content.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMilestone", menuName = "IdleViking/Milestone")]
    public class MilestoneData : ScriptableObject
    {
        [Header("Identity")]
        public string milestoneId;
        public string displayName;
        [TextArea] public string description;

        [Header("Category")]
        [Tooltip("For UI grouping: Early, Mid, Late, Prestige")]
        public string category = "Early";

        [Tooltip("Sort order within category")]
        public int sortOrder;

        [Header("Conditions (ALL must be met)")]
        public MilestoneCondition[] conditions;

        [Header("Rewards")]
        public MilestoneReward[] rewards;

        [Header("Visibility")]
        [Tooltip("If set, this milestone is hidden until the prerequisite milestone is completed")]
        public MilestoneData hiddenUntil;
    }
}
