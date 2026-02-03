using System;
using System.Collections.Generic;
using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Systems
{
    /// <summary>
    /// Status snapshot for a single farm plot. Used by UI.
    /// </summary>
    public struct FarmPlotStatus
    {
        public int plotId;
        public string farmPlotDataId;
        public string displayName;
        public FarmType farmType;
        public bool isReady;
        public double secondsRemaining;
        public double growTimeTotal;
        public int totalHarvests;
    }

    /// <summary>
    /// Handles planting, harvesting, and offline catch-up for crops and animals.
    /// </summary>
    public static class FarmSystem
    {
        /// <summary>
        /// Plant a crop or place an animal in an empty farm slot.
        /// </summary>
        public static bool TryPlant(GameState state, FarmDatabase db, string farmPlotDataId)
        {
            var data = db.GetFarmPlot(farmPlotDataId);
            if (data == null)
            {
                Debug.LogWarning($"[FarmSystem] Unknown farm plot: {farmPlotDataId}");
                return false;
            }

            if (!state.farm.HasEmptySlot)
            {
                Debug.Log("[FarmSystem] No empty farm slots.");
                return false;
            }

            // Check prerequisite
            if (data.prerequisiteBuilding != null)
            {
                int bldLevel = state.buildings.GetLevel(data.prerequisiteBuilding.buildingId);
                if (bldLevel < data.prerequisiteLevel)
                {
                    Debug.Log($"[FarmSystem] Requires {data.prerequisiteBuilding.displayName} level {data.prerequisiteLevel}.");
                    return false;
                }
            }

            // Spend cost
            if (data.plantCosts != null && data.plantCosts.Length > 0)
            {
                if (!state.resources.Spend(data.plantCosts))
                {
                    Debug.Log($"[FarmSystem] Can't afford to plant {data.displayName}.");
                    return false;
                }
            }

            var plot = new FarmPlotInstance
            {
                plotId = state.farm.nextPlotId++,
                farmPlotDataId = farmPlotDataId
            };
            plot.SetPlantedNow();
            state.farm.plots.Add(plot);

            Debug.Log($"[FarmSystem] Planted {data.displayName} (plot #{plot.plotId})");
            return true;
        }

        /// <summary>
        /// Harvest a ready crop (removes it) or collect from a ready animal (resets timer).
        /// Returns the yield amount or 0 if not ready.
        /// </summary>
        public static double TryHarvest(GameState state, FarmDatabase db, int plotId)
        {
            var plot = state.farm.GetPlot(plotId);
            if (plot == null) return 0;

            var data = db.GetFarmPlot(plot.farmPlotDataId);
            if (data == null) return 0;

            if (!IsReady(plot, data))
            {
                Debug.Log($"[FarmSystem] Plot #{plotId} not ready yet.");
                return 0;
            }

            state.resources.Add(data.yieldResource, data.yieldAmount);
            plot.totalHarvests++;

            if (data.farmType == FarmType.Crop)
            {
                // Crops are consumed on harvest
                state.farm.plots.RemoveAll(p => p.plotId == plotId);
                Debug.Log($"[FarmSystem] Harvested {data.displayName}: +{data.yieldAmount} {data.yieldResource}");
            }
            else
            {
                // Animals reset their timer for the next cycle
                plot.ResetTimer();
                Debug.Log($"[FarmSystem] Collected from {data.displayName}: +{data.yieldAmount} {data.yieldResource}");
            }

            return data.yieldAmount;
        }

        /// <summary>
        /// Remove a plot (sell/release animal, or clear an unfinished crop).
        /// No resources returned.
        /// </summary>
        public static bool RemovePlot(GameState state, int plotId)
        {
            int removed = state.farm.plots.RemoveAll(p => p.plotId == plotId);
            return removed > 0;
        }

        /// <summary>
        /// Check if a plot's growth timer has completed.
        /// </summary>
        public static bool IsReady(FarmPlotInstance plot, FarmPlotData data)
        {
            return plot.GetElapsedSeconds() >= data.growTimeSeconds;
        }

        /// <summary>
        /// Get time remaining in seconds. Returns 0 if ready.
        /// </summary>
        public static double GetTimeRemaining(FarmPlotInstance plot, FarmPlotData data)
        {
            double remaining = data.growTimeSeconds - plot.GetElapsedSeconds();
            return Math.Max(0, remaining);
        }

        /// <summary>
        /// Get a status snapshot of all active plots. Useful for UI.
        /// </summary>
        public static List<FarmPlotStatus> GetAllStatuses(GameState state, FarmDatabase db)
        {
            var statuses = new List<FarmPlotStatus>();
            foreach (var plot in state.farm.plots)
            {
                var data = db.GetFarmPlot(plot.farmPlotDataId);
                if (data == null) continue;

                statuses.Add(new FarmPlotStatus
                {
                    plotId = plot.plotId,
                    farmPlotDataId = plot.farmPlotDataId,
                    displayName = data.displayName,
                    farmType = data.farmType,
                    isReady = IsReady(plot, data),
                    secondsRemaining = GetTimeRemaining(plot, data),
                    growTimeTotal = data.growTimeSeconds,
                    totalHarvests = plot.totalHarvests
                });
            }
            return statuses;
        }

        /// <summary>
        /// Process offline time for all farm plots.
        /// - Ready crops: if autoHarvest is on, harvest them.
        /// - Animals: calculate how many full cycles completed while offline, grant yields.
        /// Called once at startup.
        /// </summary>
        public static void ProcessOffline(GameState state, FarmDatabase db)
        {
            // Process in reverse so we can safely remove crops
            for (int i = state.farm.plots.Count - 1; i >= 0; i--)
            {
                var plot = state.farm.plots[i];
                var data = db.GetFarmPlot(plot.farmPlotDataId);
                if (data == null) continue;

                if (data.farmType == FarmType.Animal)
                {
                    ProcessOfflineAnimal(state, plot, data);
                }
                else if (data.farmType == FarmType.Crop && state.farm.autoHarvest)
                {
                    if (IsReady(plot, data))
                    {
                        state.resources.Add(data.yieldResource, data.yieldAmount);
                        plot.totalHarvests++;
                        state.farm.plots.RemoveAt(i);
                        Debug.Log($"[FarmSystem] Auto-harvested {data.displayName} (offline)");
                    }
                }
                // If autoHarvest is off, crops just sit there ready for manual harvest
            }
        }

        /// <summary>
        /// For animals: calculate completed production cycles since last harvest
        /// and grant all accumulated yields.
        /// </summary>
        private static void ProcessOfflineAnimal(GameState state, FarmPlotInstance plot, FarmPlotData data)
        {
            double elapsed = plot.GetElapsedSeconds();
            int completedCycles = (int)(elapsed / data.growTimeSeconds);

            if (completedCycles <= 0) return;

            double totalYield = data.yieldAmount * completedCycles;
            state.resources.Add(data.yieldResource, totalYield);
            plot.totalHarvests += completedCycles;

            // Advance the timer past completed cycles so the remainder carries over
            double consumedSeconds = completedCycles * data.growTimeSeconds;
            DateTime newBase = plot.GetPlantedTime().AddSeconds(consumedSeconds);
            plot.plantedTimestamp = newBase.ToString("o");

            Debug.Log($"[FarmSystem] {data.displayName} produced {completedCycles}x offline: +{totalYield} {data.yieldResource}");
        }
    }
}
