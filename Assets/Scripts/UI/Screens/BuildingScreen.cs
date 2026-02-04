using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Screen showing all available buildings with upgrade options.
    /// </summary>
    public class BuildingScreen : BaseScreen
    {
        [Header("Building List")]
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private BuildingListItem buildingItemPrefab;

        private List<BuildingListItem> _spawnedItems = new List<BuildingListItem>();

        public override void Refresh()
        {
            ClearItems();
            PopulateBuildings();
        }

        protected override void SubscribeToEvents()
        {
            UIEvents.OnBuildingsChanged += Refresh;
            UIEvents.OnResourcesChanged += RefreshAffordability;
        }

        protected override void UnsubscribeFromEvents()
        {
            UIEvents.OnBuildingsChanged -= Refresh;
            UIEvents.OnResourcesChanged -= RefreshAffordability;
        }

        private void PopulateBuildings()
        {
            if (buildingContainer == null || buildingItemPrefab == null)
                return;

            var state = GameManager.Instance?.State;
            var buildingDB = GameManager.Instance?.BuildingDB;
            if (state == null || buildingDB == null) return;

            foreach (var building in buildingDB.buildings)
            {
                var item = Instantiate(buildingItemPrefab, buildingContainer);
                item.Setup(building, state);
                item.OnUpgradeClicked += OnBuildingUpgrade;
                _spawnedItems.Add(item);
            }
        }

        private void OnBuildingUpgrade(BuildingData building)
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            // Try to upgrade using state directly
            int currentLevel = state.buildings.GetLevel(building.buildingId);
            var costs = building.GetCostsForLevel(currentLevel + 1);

            if (state.resources.CanAfford(costs))
            {
                state.resources.Spend(costs);
                if (currentLevel == 0)
                {
                    state.buildings.AddBuilding(building.buildingId);
                }
                else
                {
                    var instance = state.buildings.GetBuilding(building.buildingId);
                    if (instance != null)
                        instance.level++;
                }

                UIEvents.FireBuildingsChanged();
                UIEvents.FireResourcesChanged();
                UIEvents.FireToast($"{building.displayName} upgraded!");
            }
            else
            {
                UIEvents.FireToast("Cannot afford upgrade.");
            }
        }

        private void RefreshAffordability()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    item.RefreshAffordability();
            }
        }

        private void ClearItems()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                {
                    item.OnUpgradeClicked -= OnBuildingUpgrade;
                    Destroy(item.gameObject);
                }
            }
            _spawnedItems.Clear();
        }

        protected override void OnHide()
        {
            ClearItems();
        }
    }
}
