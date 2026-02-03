using System;
using IdleViking.Models;
using IdleViking.Data;

namespace IdleViking.UI
{
    /// <summary>
    /// Static event bus for UI state change notifications.
    /// Systems fire events, UI components subscribe.
    /// </summary>
    public static class UIEvents
    {
        // Resource changes
        public static event Action OnResourcesChanged;
        public static event Action<ResourceType, double> OnResourceChanged;

        // Building changes
        public static event Action OnBuildingsChanged;
        public static event Action<string, int> OnBuildingLevelChanged; // buildingId, newLevel

        // Viking changes
        public static event Action OnVikingsChanged;
        public static event Action<int> OnVikingLevelUp; // vikingUniqueId
        public static event Action<int> OnVikingRecruited; // vikingUniqueId

        // Equipment/Inventory changes
        public static event Action OnInventoryChanged;
        public static event Action<int> OnItemEquipped; // itemId
        public static event Action<int> OnItemCrafted; // itemId

        // Combat/Dungeon
        public static event Action<CombatLog> OnCombatComplete;
        public static event Action<DungeonRun> OnDungeonFloorComplete;
        public static event Action<DungeonRun> OnDungeonComplete;

        // Farm
        public static event Action OnFarmChanged;
        public static event Action<int> OnPlotReady; // plotId

        // Progression
        public static event Action<MilestoneData> OnMilestoneCompleted;
        public static event Action<int> OnPrestigeComplete; // newPrestigeLevel

        // General UI
        public static event Action OnGameStateLoaded;
        public static event Action<string> OnToastMessage;

        // Fire methods - called by systems
        public static void FireResourcesChanged() => OnResourcesChanged?.Invoke();
        public static void FireResourceChanged(ResourceType type, double newAmount) =>
            OnResourceChanged?.Invoke(type, newAmount);

        public static void FireBuildingsChanged() => OnBuildingsChanged?.Invoke();
        public static void FireBuildingLevelChanged(string buildingId, int newLevel) =>
            OnBuildingLevelChanged?.Invoke(buildingId, newLevel);

        public static void FireVikingsChanged() => OnVikingsChanged?.Invoke();
        public static void FireVikingLevelUp(int vikingId) => OnVikingLevelUp?.Invoke(vikingId);
        public static void FireVikingRecruited(int vikingId) => OnVikingRecruited?.Invoke(vikingId);

        public static void FireInventoryChanged() => OnInventoryChanged?.Invoke();
        public static void FireItemEquipped(int itemId) => OnItemEquipped?.Invoke(itemId);
        public static void FireItemCrafted(int itemId) => OnItemCrafted?.Invoke(itemId);

        public static void FireCombatComplete(CombatLog log) => OnCombatComplete?.Invoke(log);
        public static void FireDungeonFloorComplete(DungeonRun run) => OnDungeonFloorComplete?.Invoke(run);
        public static void FireDungeonComplete(DungeonRun run) => OnDungeonComplete?.Invoke(run);

        public static void FireFarmChanged() => OnFarmChanged?.Invoke();
        public static void FirePlotReady(int plotId) => OnPlotReady?.Invoke(plotId);

        public static void FireMilestoneCompleted(MilestoneData milestone) =>
            OnMilestoneCompleted?.Invoke(milestone);
        public static void FirePrestigeComplete(int newLevel) => OnPrestigeComplete?.Invoke(newLevel);

        public static void FireGameStateLoaded() => OnGameStateLoaded?.Invoke();
        public static void FireToast(string message) => OnToastMessage?.Invoke(message);
    }
}
