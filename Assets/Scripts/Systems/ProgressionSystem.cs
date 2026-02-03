using System.Collections.Generic;
using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Systems
{
    /// <summary>
    /// Result of a milestone check pass.
    /// </summary>
    public class MilestoneCheckResult
    {
        public List<MilestoneData> newlyCompleted = new List<MilestoneData>();
        public int totalRewardsGranted;
    }

    /// <summary>
    /// Handles milestone evaluation, reward granting, unlock checks, and prestige.
    /// </summary>
    public static class ProgressionSystem
    {
        /// <summary>
        /// Evaluate all pending milestones. Completes any whose conditions are met,
        /// grants rewards, and returns a list of newly completed milestones.
        /// Safe to call periodically (every few seconds, not every frame).
        /// </summary>
        public static MilestoneCheckResult CheckAllMilestones(GameState state, MilestoneDatabase db)
        {
            var result = new MilestoneCheckResult();

            // May need multiple passes since completing one milestone can unlock another
            bool anyCompleted;
            do
            {
                anyCompleted = false;
                foreach (var milestone in db.GetAll())
                {
                    if (state.progression.IsMilestoneCompleted(milestone.milestoneId))
                        continue;

                    if (!AreConditionsMet(state, milestone))
                        continue;

                    // Complete it
                    state.progression.CompleteMilestone(milestone.milestoneId);
                    int rewardCount = GrantRewards(state, milestone);
                    result.newlyCompleted.Add(milestone);
                    result.totalRewardsGranted += rewardCount;
                    anyCompleted = true;

                    Debug.Log($"[ProgressionSystem] Milestone completed: {milestone.displayName}");
                }
            }
            while (anyCompleted);

            return result;
        }

        /// <summary>
        /// Check if all conditions for a milestone are met.
        /// </summary>
        public static bool AreConditionsMet(GameState state, MilestoneData milestone)
        {
            if (milestone.conditions == null || milestone.conditions.Length == 0)
                return true;

            foreach (var condition in milestone.conditions)
            {
                if (!EvaluateCondition(state, condition))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Evaluate a single milestone condition against current game state.
        /// </summary>
        public static bool EvaluateCondition(GameState state, MilestoneCondition condition)
        {
            switch (condition.type)
            {
                case MilestoneConditionType.ResourceAmount:
                    return state.resources.Get(condition.resourceType) >= condition.requiredValue;

                case MilestoneConditionType.BuildingLevel:
                    return state.buildings.GetLevel(condition.targetId) >= condition.requiredValue;

                case MilestoneConditionType.DungeonCleared:
                {
                    var progress = state.dungeons.GetProgress(condition.targetId);
                    return progress != null && progress.timesCompleted >= condition.requiredValue;
                }

                case MilestoneConditionType.VikingCount:
                    return state.vikings.vikings.Count >= condition.requiredValue;

                case MilestoneConditionType.VikingLevel:
                {
                    foreach (var viking in state.vikings.vikings)
                    {
                        if (viking.level >= condition.requiredValue)
                            return true;
                    }
                    return false;
                }

                case MilestoneConditionType.MilestoneCompleted:
                    return state.progression.IsMilestoneCompleted(condition.targetId);

                case MilestoneConditionType.PrestigeLevel:
                    return state.progression.prestigeLevel >= condition.requiredValue;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Grant all rewards for a completed milestone.
        /// </summary>
        private static int GrantRewards(GameState state, MilestoneData milestone)
        {
            if (milestone.rewards == null) return 0;

            int count = 0;
            foreach (var reward in milestone.rewards)
            {
                ApplyReward(state, reward);
                count++;
            }
            return count;
        }

        private static void ApplyReward(GameState state, MilestoneReward reward)
        {
            switch (reward.rewardType)
            {
                case MilestoneRewardType.Resource:
                    state.resources.Add(reward.resourceType, reward.amount);
                    break;

                case MilestoneRewardType.FarmSlot:
                    state.farm.maxPlots += (int)reward.amount;
                    Debug.Log($"[ProgressionSystem] Farm slots increased to {state.farm.maxPlots}");
                    break;

                case MilestoneRewardType.EnergyCapIncrease:
                    state.dungeons.maxEnergy += (float)reward.amount;
                    Debug.Log($"[ProgressionSystem] Energy cap increased to {state.dungeons.maxEnergy}");
                    break;

                case MilestoneRewardType.InventorySlot:
                    state.inventory.maxCapacity += (int)reward.amount;
                    Debug.Log($"[ProgressionSystem] Inventory capacity increased to {state.inventory.maxCapacity}");
                    break;

                case MilestoneRewardType.UnlockFlag:
                    if (!string.IsNullOrEmpty(reward.unlockFlag))
                        state.progression.SetFlag(reward.unlockFlag);
                    Debug.Log($"[ProgressionSystem] Unlocked: {reward.unlockFlag}");
                    break;
            }
        }

        /// <summary>
        /// Check if a named feature is unlocked via milestone flags.
        /// Use this as the single gating check for UI and systems.
        /// </summary>
        public static bool IsUnlocked(GameState state, string flag)
        {
            return state.progression.HasFlag(flag);
        }

        /// <summary>
        /// Check if a milestone is visible to the player.
        /// Hidden milestones require their hiddenUntil milestone to be completed.
        /// </summary>
        public static bool IsVisible(GameState state, MilestoneData milestone)
        {
            if (milestone.hiddenUntil == null) return true;
            return state.progression.IsMilestoneCompleted(milestone.hiddenUntil.milestoneId);
        }

        /// <summary>
        /// Get the current prestige production multiplier.
        /// </summary>
        public static double GetPrestigeMultiplier(GameState state, PrestigeData config)
        {
            if (config == null) return 1.0;
            return config.GetMultiplier(state.progression.prestigeLevel);
        }

        /// <summary>
        /// Check if the player meets the requirements to prestige.
        /// </summary>
        public static bool CanPrestige(GameState state, PrestigeData config)
        {
            if (config == null) return false;

            if (config.maxPrestigeLevel > 0 && state.progression.prestigeLevel >= config.maxPrestigeLevel)
                return false;

            // Check building requirement
            if (config.requireBuilding != null)
            {
                int level = state.buildings.GetLevel(config.requireBuilding.buildingId);
                if (level < config.requireBuildingLevel)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Perform a prestige reset. Increments prestige level, resets specified state,
        /// grants prestige rewards. Returns false if requirements not met.
        /// </summary>
        public static bool TryPrestige(GameState state, PrestigeData config)
        {
            if (!CanPrestige(state, config))
            {
                Debug.Log("[ProgressionSystem] Prestige requirements not met.");
                return false;
            }

            state.progression.prestigeLevel++;
            state.progression.totalPrestiges++;

            Debug.Log($"[ProgressionSystem] Prestige! Level {state.progression.prestigeLevel} " +
                      $"(multiplier: {config.GetMultiplier(state.progression.prestigeLevel):F2}x)");

            // Apply resets
            if (config.resetResources)
                state.resources = new ResourceState();

            if (config.resetBuildings)
            {
                state.buildings = new BuildingState();
                state.producers = new ProducerState();
            }

            if (config.resetFarm)
                state.farm = new FarmState();

            if (config.resetDungeonProgress)
            {
                // Reset progress but keep energy full
                float savedEnergy = state.dungeons.maxEnergy;
                state.dungeons = new DungeonState();
                state.dungeons.energy = savedEnergy;
                state.dungeons.maxEnergy = savedEnergy;
            }

            // Grant prestige bonus rewards
            if (config.prestigeRewards != null)
            {
                foreach (var reward in config.prestigeRewards)
                    state.resources.Add(reward.resourceType, reward.amount);
            }

            return true;
        }

        /// <summary>
        /// Get a progress summary for UI: total milestones, completed count,
        /// next incomplete milestone in each category.
        /// </summary>
        public static ProgressionSummary GetSummary(GameState state, MilestoneDatabase db)
        {
            var summary = new ProgressionSummary();
            summary.totalMilestones = db.GetAll().Count;
            summary.completedCount = state.progression.completedMilestones.Count;
            summary.prestigeLevel = state.progression.prestigeLevel;

            foreach (var milestone in db.GetAll())
            {
                if (state.progression.IsMilestoneCompleted(milestone.milestoneId))
                    continue;

                if (!IsVisible(state, milestone))
                    continue;

                // Track the next incomplete per category
                if (!summary.nextPerCategory.ContainsKey(milestone.category))
                    summary.nextPerCategory[milestone.category] = milestone;
            }

            return summary;
        }
    }

    public class ProgressionSummary
    {
        public int totalMilestones;
        public int completedCount;
        public int prestigeLevel;
        public Dictionary<string, MilestoneData> nextPerCategory = new Dictionary<string, MilestoneData>();

        public float CompletionPercent => totalMilestones > 0
            ? (float)completedCount / totalMilestones * 100f
            : 0f;
    }
}
