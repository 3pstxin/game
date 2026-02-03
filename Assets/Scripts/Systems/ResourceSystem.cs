using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Systems
{
    /// <summary>
    /// Handles resource production from all active producers.
    /// Applies building bonuses, workforce bonuses, and prestige multiplier.
    /// </summary>
    public static class ResourceSystem
    {
        /// <summary>
        /// Called every frame.
        /// </summary>
        public static void Tick(GameState state, ResourceDatabase resDB,
            BuildingDatabase bldDB, VikingDatabase vikDB,
            double prestigeMultiplier, float deltaTime)
        {
            foreach (var instance in state.producers.producers)
            {
                var data = resDB.GetProducer(instance.producerId);
                if (data == null) continue;

                double finalRate = CalculateRate(state, data, instance, bldDB, vikDB) * prestigeMultiplier;
                state.resources.Add(data.resourceType, finalRate * deltaTime);
            }
        }

        /// <summary>
        /// Called once at startup for offline gains.
        /// </summary>
        public static void ApplyOffline(GameState state, ResourceDatabase resDB,
            BuildingDatabase bldDB, VikingDatabase vikDB,
            double prestigeMultiplier, double offlineSeconds)
        {
            if (offlineSeconds <= 0) return;

            foreach (var instance in state.producers.producers)
            {
                var data = resDB.GetProducer(instance.producerId);
                if (data == null) continue;

                double finalRate = CalculateRate(state, data, instance, bldDB, vikDB) * prestigeMultiplier;
                state.resources.Add(data.resourceType, finalRate * offlineSeconds);
            }

            Debug.Log($"[ResourceSystem] Offline gains applied for {offlineSeconds:F1}s (prestige: {prestigeMultiplier:F2}x)");
        }

        /// <summary>
        /// Get the total production rate per second for a resource type.
        /// Includes all bonuses and prestige multiplier.
        /// </summary>
        public static double GetTotalRate(GameState state, ResourceDatabase resDB,
            BuildingDatabase bldDB, VikingDatabase vikDB,
            double prestigeMultiplier, ResourceType type)
        {
            double total = 0;
            foreach (var instance in state.producers.producers)
            {
                var data = resDB.GetProducer(instance.producerId);
                if (data == null || data.resourceType != type) continue;
                total += CalculateRate(state, data, instance, bldDB, vikDB);
            }
            return total * prestigeMultiplier;
        }

        /// <summary>
        /// Calculate production rate for a single producer before prestige.
        /// Formula: baseRate * (1 + buildingBonus + workforceBonus)
        /// Prestige multiplier is applied on top by the caller.
        /// </summary>
        private static double CalculateRate(GameState state, ResourceProducerData data,
            ProducerInstance instance, BuildingDatabase bldDB, VikingDatabase vikDB)
        {
            double baseRate = data.GetProductionRate(instance.level);

            double buildingBonus = 0;
            if (bldDB != null)
                buildingBonus = BuildingSystem.GetTotalBonus(state, bldDB, data.resourceType);

            double workforceBonus = 0;
            if (bldDB != null && vikDB != null)
                workforceBonus = GetWorkforceBonusForProducer(state, bldDB, vikDB, data.producerId);

            return baseRate * (1 + buildingBonus + workforceBonus);
        }

        private static double GetWorkforceBonusForProducer(GameState state,
            BuildingDatabase bldDB, VikingDatabase vikDB, string producerId)
        {
            foreach (var buildingData in bldDB.GetAll())
            {
                if (buildingData.linkedProducer == null) continue;
                if (buildingData.linkedProducer.producerId != producerId) continue;
                return VikingSystem.GetWorkforceBonus(state, vikDB, buildingData.buildingId);
            }
            return 0;
        }
    }
}
