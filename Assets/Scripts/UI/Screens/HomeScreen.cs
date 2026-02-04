using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Main home screen showing resource overview and quick actions.
    /// </summary>
    public class HomeScreen : BaseScreen
    {
        [Header("Resource Display")]
        [SerializeField] private ResourceBar goldBar;
        [SerializeField] private ResourceBar woodBar;
        [SerializeField] private ResourceBar stoneBar;
        [SerializeField] private ResourceBar foodBar;
        [SerializeField] private ResourceBar ironBar;

        [Header("Quick Stats")]
        [SerializeField] private TextMeshProUGUI vikingCountText;
        [SerializeField] private TextMeshProUGUI buildingCountText;
        [SerializeField] private TextMeshProUGUI dungeonProgressText;

        [Header("Quick Actions")]
        [SerializeField] private Button collectAllButton;
        [SerializeField] private Button dungeonButton;
        [SerializeField] private Button farmButton;

        [Header("Prestige")]
        [SerializeField] private TextMeshProUGUI prestigeLevelText;
        [SerializeField] private TextMeshProUGUI prestigeMultiplierText;

        protected override void Awake()
        {
            base.Awake();

            if (collectAllButton != null)
                collectAllButton.onClick.AddListener(OnCollectAllClicked);
            if (dungeonButton != null)
                dungeonButton.onClick.AddListener(OnDungeonClicked);
            if (farmButton != null)
                farmButton.onClick.AddListener(OnFarmClicked);
        }

        private void OnDestroy()
        {
            if (collectAllButton != null)
                collectAllButton.onClick.RemoveListener(OnCollectAllClicked);
            if (dungeonButton != null)
                dungeonButton.onClick.RemoveListener(OnDungeonClicked);
            if (farmButton != null)
                farmButton.onClick.RemoveListener(OnFarmClicked);
        }

        protected override void SubscribeToEvents()
        {
            UIEvents.OnResourcesChanged += Refresh;
            UIEvents.OnVikingsChanged += RefreshVikingCount;
            UIEvents.OnBuildingsChanged += RefreshBuildingCount;
            UIEvents.OnPrestigeComplete += OnPrestigeComplete;
        }

        protected override void UnsubscribeFromEvents()
        {
            UIEvents.OnResourcesChanged -= Refresh;
            UIEvents.OnVikingsChanged -= RefreshVikingCount;
            UIEvents.OnBuildingsChanged -= RefreshBuildingCount;
            UIEvents.OnPrestigeComplete -= OnPrestigeComplete;
        }

        public override void Refresh()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            RefreshVikingCount();
            RefreshBuildingCount();
            RefreshDungeonProgress();
            RefreshPrestige();
        }

        private void RefreshVikingCount()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (vikingCountText != null)
            {
                int count = state.vikings.vikings.Count;
                vikingCountText.text = $"Vikings: {count}";
            }
        }

        private void RefreshBuildingCount()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (buildingCountText != null)
            {
                int count = state.buildings.buildings.Count;
                int totalLevels = 0;
                foreach (var building in state.buildings.buildings)
                {
                    totalLevels += building.level;
                }
                buildingCountText.text = $"Buildings: {count} (Lvl {totalLevels})";
            }
        }

        private void RefreshDungeonProgress()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (dungeonProgressText != null)
            {
                int highestFloor = 0;
                foreach (var dp in state.dungeons.progress)
                {
                    if (dp.highestFloorCleared > highestFloor)
                        highestFloor = dp.highestFloorCleared;
                }
                dungeonProgressText.text = $"Dungeon: Floor {highestFloor}";
            }
        }

        private void RefreshPrestige()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (prestigeLevelText != null)
            {
                prestigeLevelText.text = $"Prestige: {state.progression.prestigeLevel}";
            }

            if (prestigeMultiplierText != null)
            {
                // Prestige multiplier: 1 + 0.1 per level
                float mult = 1f + state.progression.prestigeLevel * 0.1f;
                prestigeMultiplierText.text = $"Bonus: x{mult:F1}";
            }
        }

        private void OnPrestigeComplete(int newLevel)
        {
            RefreshPrestige();
        }

        private void OnCollectAllClicked()
        {
            // Simplified - just show toast for now
            // Full implementation would iterate farm plots and harvest ready ones
            UIEvents.FireToast("Collect All not yet implemented.");
        }

        private void OnDungeonClicked()
        {
            UIManager.Instance?.ShowScreen(ScreenType.Dungeon);
        }

        private void OnFarmClicked()
        {
            UIManager.Instance?.ShowScreen(ScreenType.Farm);
        }
    }
}
