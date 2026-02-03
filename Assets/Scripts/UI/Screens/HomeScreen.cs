using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
                int count = state.Vikings.OwnedVikings.Count;
                vikingCountText.text = $"Vikings: {count}";
            }
        }

        private void RefreshBuildingCount()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (buildingCountText != null)
            {
                int count = state.Buildings.OwnedBuildings.Count;
                int totalLevels = 0;
                foreach (var kvp in state.Buildings.OwnedBuildings)
                {
                    totalLevels += kvp.Value.Level;
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
                foreach (var kvp in state.Dungeon.DungeonProgress)
                {
                    if (kvp.Value > highestFloor)
                        highestFloor = kvp.Value;
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
                prestigeLevelText.text = $"Prestige: {state.Progression.PrestigeLevel}";
            }

            if (prestigeMultiplierText != null)
            {
                float mult = state.Progression.GetPrestigeMultiplier();
                prestigeMultiplierText.text = $"Bonus: x{mult:F1}";
            }
        }

        private void OnPrestigeComplete(int newLevel)
        {
            RefreshPrestige();
        }

        private void OnCollectAllClicked()
        {
            // Collect all ready farm plots
            var state = GameManager.Instance?.State;
            if (state == null) return;

            bool collected = false;
            foreach (var kvp in state.Farm.Plots)
            {
                var plot = kvp.Value;
                if (plot.IsPlanted && plot.IsReady())
                {
                    FarmSystem.Harvest(state, kvp.Key);
                    collected = true;
                }
            }

            if (collected)
            {
                UIEvents.FireFarmChanged();
                UIEvents.FireResourcesChanged();
                UIEvents.FireToast("Collected all ready harvests!");
            }
            else
            {
                UIEvents.FireToast("Nothing to collect.");
            }
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
