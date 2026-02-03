using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Screen showing farm plots and crop/animal management.
    /// </summary>
    public class FarmScreen : BaseScreen
    {
        [Header("Plot Grid")]
        [SerializeField] private Transform plotContainer;
        [SerializeField] private FarmPlotUI plotPrefab;

        [Header("Planting Selection")]
        [SerializeField] private GameObject plantingPanel;
        [SerializeField] private Transform seedListContainer;
        [SerializeField] private SeedListItem seedItemPrefab;
        [SerializeField] private Button cancelPlantingButton;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI totalPlotsText;
        [SerializeField] private TextMeshProUGUI activePlotsText;
        [SerializeField] private Button harvestAllButton;

        [Header("Database")]
        [SerializeField] private FarmDatabase farmDatabase;

        private List<FarmPlotUI> _plotUIs = new List<FarmPlotUI>();
        private List<SeedListItem> _seedItems = new List<SeedListItem>();
        private int _selectedPlotId = -1;

        protected override void Awake()
        {
            base.Awake();

            if (cancelPlantingButton != null)
                cancelPlantingButton.onClick.AddListener(HidePlantingPanel);
            if (harvestAllButton != null)
                harvestAllButton.onClick.AddListener(HarvestAll);
        }

        private void OnDestroy()
        {
            if (cancelPlantingButton != null)
                cancelPlantingButton.onClick.RemoveListener(HidePlantingPanel);
            if (harvestAllButton != null)
                harvestAllButton.onClick.RemoveListener(HarvestAll);
        }

        protected override void SubscribeToEvents()
        {
            UIEvents.OnFarmChanged += Refresh;
            UIEvents.OnPlotReady += OnPlotReady;
        }

        protected override void UnsubscribeFromEvents()
        {
            UIEvents.OnFarmChanged -= Refresh;
            UIEvents.OnPlotReady -= OnPlotReady;
        }

        public override void Refresh()
        {
            RefreshPlots();
            RefreshInfo();
            HidePlantingPanel();
        }

        private void RefreshPlots()
        {
            ClearPlots();

            var state = GameManager.Instance?.State;
            if (state == null || plotContainer == null || plotPrefab == null) return;

            foreach (var kvp in state.Farm.Plots)
            {
                var plotState = kvp.Value;
                var plotData = farmDatabase?.GetPlotById(plotState.PlotDataId);

                var plotUI = Instantiate(plotPrefab, plotContainer);
                plotUI.Setup(kvp.Key, plotState, plotData);
                plotUI.OnPlotClicked += OnPlotClicked;
                _plotUIs.Add(plotUI);
            }
        }

        private void RefreshInfo()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            int total = state.Farm.Plots.Count;
            int active = 0;
            int ready = 0;

            foreach (var kvp in state.Farm.Plots)
            {
                if (kvp.Value.IsPlanted)
                {
                    active++;
                    if (kvp.Value.IsReady())
                        ready++;
                }
            }

            if (totalPlotsText != null)
                totalPlotsText.text = $"Plots: {total}";

            if (activePlotsText != null)
                activePlotsText.text = $"Growing: {active} | Ready: {ready}";

            if (harvestAllButton != null)
                harvestAllButton.interactable = ready > 0;
        }

        private void OnPlotClicked(int plotId)
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (!state.Farm.Plots.TryGetValue(plotId, out var plotState))
                return;

            if (plotState.IsPlanted)
            {
                // Try to harvest
                if (plotState.IsReady())
                {
                    var rewards = FarmSystem.Harvest(state, plotId);
                    if (rewards != null)
                    {
                        UIEvents.FireFarmChanged();
                        UIEvents.FireResourcesChanged();
                        ShowHarvestRewards(rewards);
                    }
                }
                else
                {
                    // Show time remaining
                    float remaining = plotState.GetRemainingTime();
                    UIEvents.FireToast($"Ready in {FormatTime(remaining)}");
                }
            }
            else
            {
                // Show planting options
                _selectedPlotId = plotId;
                ShowPlantingPanel();
            }
        }

        private void ShowPlantingPanel()
        {
            if (plantingPanel != null)
                plantingPanel.SetActive(true);

            RefreshSeedList();
        }

        private void HidePlantingPanel()
        {
            if (plantingPanel != null)
                plantingPanel.SetActive(false);
            _selectedPlotId = -1;
            ClearSeedItems();
        }

        private void RefreshSeedList()
        {
            ClearSeedItems();

            var state = GameManager.Instance?.State;
            if (state == null || farmDatabase == null || seedListContainer == null || seedItemPrefab == null)
                return;

            // Show crops that can be planted
            foreach (var crop in farmDatabase.Crops)
            {
                var item = Instantiate(seedItemPrefab, seedListContainer);
                item.Setup(crop, state);
                item.OnSeedSelected += OnSeedSelected;
                _seedItems.Add(item);
            }
        }

        private void OnSeedSelected(FarmPlotData cropData)
        {
            if (_selectedPlotId < 0) return;

            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (FarmSystem.Plant(state, _selectedPlotId, cropData))
            {
                UIEvents.FireFarmChanged();
                UIEvents.FireResourcesChanged();
                HidePlantingPanel();
                UIEvents.FireToast($"Planted {cropData.DisplayName}!");
            }
            else
            {
                UIEvents.FireToast("Cannot plant - check costs.");
            }
        }

        private void HarvestAll()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            int harvested = 0;
            var totalRewards = new Dictionary<ResourceType, double>();

            foreach (var kvp in state.Farm.Plots)
            {
                if (kvp.Value.IsPlanted && kvp.Value.IsReady())
                {
                    var rewards = FarmSystem.Harvest(state, kvp.Key);
                    if (rewards != null)
                    {
                        harvested++;
                        foreach (var r in rewards)
                        {
                            if (totalRewards.ContainsKey(r.Key))
                                totalRewards[r.Key] += r.Value;
                            else
                                totalRewards[r.Key] = r.Value;
                        }
                    }
                }
            }

            if (harvested > 0)
            {
                UIEvents.FireFarmChanged();
                UIEvents.FireResourcesChanged();
                UIEvents.FireToast($"Harvested {harvested} plots!");
            }
        }

        private void ShowHarvestRewards(Dictionary<ResourceType, double> rewards)
        {
            var rewardStrings = new List<string>();
            foreach (var kvp in rewards)
            {
                rewardStrings.Add($"{kvp.Key}: +{FormatNumber(kvp.Value)}");
            }

            UIManager.Instance?.ShowReward("Harvest Complete!", rewardStrings);
        }

        private void OnPlotReady(int plotId)
        {
            // Refresh the specific plot
            foreach (var plotUI in _plotUIs)
            {
                if (plotUI.PlotId == plotId)
                {
                    var state = GameManager.Instance?.State;
                    if (state != null && state.Farm.Plots.TryGetValue(plotId, out var plotState))
                    {
                        var plotData = farmDatabase?.GetPlotById(plotState.PlotDataId);
                        plotUI.Setup(plotId, plotState, plotData);
                    }
                    break;
                }
            }
            RefreshInfo();
        }

        private void ClearPlots()
        {
            foreach (var plot in _plotUIs)
            {
                if (plot != null)
                {
                    plot.OnPlotClicked -= OnPlotClicked;
                    Destroy(plot.gameObject);
                }
            }
            _plotUIs.Clear();
        }

        private void ClearSeedItems()
        {
            foreach (var item in _seedItems)
            {
                if (item != null)
                {
                    item.OnSeedSelected -= OnSeedSelected;
                    Destroy(item.gameObject);
                }
            }
            _seedItems.Clear();
        }

        protected override void OnHide()
        {
            ClearPlots();
            ClearSeedItems();
        }

        private string FormatTime(float totalSeconds)
        {
            int hours = (int)(totalSeconds / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            int seconds = (int)(totalSeconds % 60);

            if (hours > 0)
                return $"{hours}h {minutes}m";
            else if (minutes > 0)
                return $"{minutes}m {seconds}s";
            else
                return $"{seconds}s";
        }

        private string FormatNumber(double value)
        {
            if (value >= 1_000_000)
                return $"{value / 1_000_000:F1}M";
            if (value >= 1_000)
                return $"{value / 1_000:F1}K";
            return value.ToString("F0");
        }
    }
}
