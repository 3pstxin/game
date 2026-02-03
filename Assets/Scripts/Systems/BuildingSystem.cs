using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Systems
{
    /// <summary>
    /// Handles building construction, upgrades, prerequisite checks, and bonus calculation.
    /// When a building has a linked producer, the producer is auto-created/synced.
    /// </summary>
    public static class BuildingSystem
    {
        /// <summary>
        /// Attempt to build a new building (goes to level 1).
        /// Checks prerequisites and costs. Auto-creates linked producer if defined.
        /// </summary>
        public static BuildResult TryBuild(GameState state, BuildingDatabase db, string buildingId)
        {
            var data = db.GetBuilding(buildingId);
            if (data == null)
                return BuildResult.Fail("Unknown building.");

            if (state.buildings.GetBuilding(buildingId) != null)
                return BuildResult.Fail("Already built.");

            // Check prerequisite
            if (!MeetsPrerequisite(state, data))
            {
                return BuildResult.Fail(
                    $"Requires {data.prerequisiteBuilding.displayName} level {data.prerequisiteLevel}.");
            }

            // Check cost (level 1)
            var costs = data.GetCostsForLevel(1);
            if (!state.resources.Spend(costs))
                return BuildResult.Fail("Not enough resources.");

            // Create the building
            state.buildings.AddBuilding(buildingId);

            // Sync linked producer
            SyncProducer(state, data, 1);

            Debug.Log($"[BuildingSystem] Built {data.displayName}");
            return BuildResult.Success(1);
        }

        /// <summary>
        /// Attempt to upgrade an existing building by one level.
        /// </summary>
        public static BuildResult TryUpgrade(GameState state, BuildingDatabase db, string buildingId)
        {
            var data = db.GetBuilding(buildingId);
            if (data == null)
                return BuildResult.Fail("Unknown building.");

            var instance = state.buildings.GetBuilding(buildingId);
            if (instance == null)
                return BuildResult.Fail("Not built yet.");

            if (instance.level >= data.maxLevel)
                return BuildResult.Fail("Already at max level.");

            int targetLevel = instance.level + 1;
            var costs = data.GetCostsForLevel(targetLevel);
            if (!state.resources.Spend(costs))
                return BuildResult.Fail("Not enough resources.");

            instance.level = targetLevel;

            // Sync linked producer
            SyncProducer(state, data, targetLevel);

            Debug.Log($"[BuildingSystem] Upgraded {data.displayName} to level {targetLevel}");
            return BuildResult.Success(targetLevel);
        }

        /// <summary>
        /// Check if the player meets the prerequisite for a building.
        /// </summary>
        public static bool MeetsPrerequisite(GameState state, BuildingData data)
        {
            if (data.prerequisiteBuilding == null)
                return true;

            int currentLevel = state.buildings.GetLevel(data.prerequisiteBuilding.buildingId);
            return currentLevel >= data.prerequisiteLevel;
        }

        /// <summary>
        /// Check if the player can afford to build or upgrade a building.
        /// </summary>
        public static bool CanAfford(GameState state, BuildingData data, int targetLevel)
        {
            var costs = data.GetCostsForLevel(targetLevel);
            return state.resources.CanAfford(costs);
        }

        /// <summary>
        /// Get the total production bonus multiplier for a resource type from all buildings.
        /// Returns the sum of all bonuses (e.g. 0.3 means +30% total).
        /// The caller should apply this as: finalRate = baseRate * (1 + bonus).
        /// </summary>
        public static double GetTotalBonus(GameState state, BuildingDatabase db, ResourceType type)
        {
            double total = 0;
            foreach (var instance in state.buildings.buildings)
            {
                var data = db.GetBuilding(instance.buildingId);
                if (data == null || !data.providesBonus) continue;
                if (data.bonusResourceType != type) continue;

                total += data.GetBonusAtLevel(instance.level);
            }
            return total;
        }

        /// <summary>
        /// If the building has a linked producer, create it or set its level.
        /// </summary>
        private static void SyncProducer(GameState state, BuildingData data, int level)
        {
            if (data.linkedProducer == null) return;

            string producerId = data.linkedProducer.producerId;
            var producer = state.producers.GetProducer(producerId);

            if (producer == null)
                producer = state.producers.AddProducer(producerId);

            producer.level = level;
        }
    }

    /// <summary>
    /// Result of a build/upgrade attempt. Carries success state, new level, or failure reason.
    /// </summary>
    public struct BuildResult
    {
        public bool IsSuccess;
        public int NewLevel;
        public string FailReason;

        public static BuildResult Success(int level) =>
            new BuildResult { IsSuccess = true, NewLevel = level };

        public static BuildResult Fail(string reason) =>
            new BuildResult { IsSuccess = false, FailReason = reason };
    }
}
