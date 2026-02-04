using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Data;
using IdleViking.Models;
using IdleViking.Systems;

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
            if (state == null || plotContainer == null || plotPrefab == null || farmDatabase == null)
                return;

            foreach (var plot in state.farm.plots)
            {
                var plotData = farmDatabase.GetFarmPlot(plot.farmPlotDataId);
                if (plotData == null) continue;

                var plotUI = Instantiate(plotPrefab, plotContainer);
                plotUI.Setup(plot, plotData);
                plotUI.OnPlotClicked += OnPlotClicked;
                _plotUIs.Add(plotUI);
            }
        }

        private void RefreshInfo()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            int active = state.farm.plots.Count;
            int max = state.farm.maxPlots;
            int ready = 0;

            foreach (var plot in state.farm.plots)
            {
                var plotData = farmDatabase?.GetFarmPlot(plot.farmPlotDataId);
                if (plotData != null && FarmSystem.IsReady(plot, plotData))
                    ready++;
            }

            if (totalPlotsText != null)
                totalPlotsText.text = $"Plots: {active}/{max}";

            if (activePlotsText != null)
                activePlotsText.text = $"Ready: {ready}";

            if (harvestAllButton != null)
                harvestAllButton.interactable = ready > 0;
        }

        private void OnPlotClicked(int plotId)
        {
            var state = GameManager.Instance?.State;
            if (state == null || farmDatabase == null) return;

            var plot = state.farm.GetPlot(plotId);
            if (plot == null) return;

            var plotData = farmDatabase.GetFarmPlot(plot.farmPlotDataId);
            if (plotData == null) return;

            if (FarmSystem.IsReady(plot, plotData))
            {
                double yield = FarmSystem.TryHarvest(state, farmDatabase, plotId);
                if (yield > 0)
                {
                    UIEvents.FireFarmChanged();
                    UIEvents.FireResourcesChanged();
                    UIEvents.FireToast($"Harvested {plotData.displayName}: +{yield} {plotData.yieldResource}");
                }
            }
            else
            {
                double remaining = FarmSystem.GetTimeRemaining(plot, plotData);
                UIEvents.FireToast($"Ready in {FormatTime((float)remaining)}");
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
            ClearSeedItems();
        }

        private void RefreshSeedList()
        {
            ClearSeedItems();

            var state = GameManager.Instance?.State;
            if (state == null || farmDatabase == null || seedListContainer == null || seedItemPrefab == null)
                return;

            foreach (var crop in farmDatabase.GetAll())
            {
                var item = Instantiate(seedItemPrefab, seedListContainer);
                item.Setup(crop, state);
                item.OnSeedSelected += OnSeedSelected;
                _seedItems.Add(item);
            }
        }

        private void OnSeedSelected(FarmPlotData cropData)
        {
            var state = GameManager.Instance?.State;
            if (state == null || farmDatabase == null) return;

            if (FarmSystem.TryPlant(state, farmDatabase, cropData.farmPlotId))
            {
                UIEvents.FireFarmChanged();
                UIEvents.FireResourcesChanged();
                HidePlantingPanel();
                UIEvents.FireToast($"Planted {cropData.displayName}!");
            }
            else
            {
                UIEvents.FireToast("Cannot plant - check costs or slots.");
            }
        }

        private void HarvestAll()
        {
            var state = GameManager.Instance?.State;
            if (state == null || farmDatabase == null) return;

            int harvested = 0;
            double totalYield = 0;

            // Iterate in reverse since harvesting crops removes them
            for (int i = state.farm.plots.Count - 1; i >= 0; i--)
            {
                var plot = state.farm.plots[i];
                var plotData = farmDatabase.GetFarmPlot(plot.farmPlotDataId);
                if (plotData == null) continue;

                if (FarmSystem.IsReady(plot, plotData))
                {
                    double yield = FarmSystem.TryHarvest(state, farmDatabase, plot.plotId);
                    if (yield > 0)
                    {
                        harvested++;
                        totalYield += yield;
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

        private void OnPlotReady(int plotId)
        {
            RefreshPlots();
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
    }
}
