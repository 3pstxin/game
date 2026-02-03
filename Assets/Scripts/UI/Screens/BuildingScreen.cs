using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

        [Header("Database")]
        [SerializeField] private BuildingDatabase buildingDatabase;

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
            if (buildingDatabase == null || buildingContainer == null || buildingItemPrefab == null)
                return;

            var state = GameManager.Instance?.State;
            if (state == null) return;

            foreach (var building in buildingDatabase.Buildings)
            {
                // Check if unlocked
                if (!BuildingSystem.IsUnlocked(state, building))
                    continue;

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

            if (BuildingSystem.TryUpgrade(state, building))
            {
                UIEvents.FireBuildingsChanged();
                UIEvents.FireResourcesChanged();

                int newLevel = state.Buildings.GetBuildingLevel(building.BuildingId);
                UIEvents.FireToast($"{building.DisplayName} upgraded to Lv.{newLevel}!");
            }
            else
            {
                UIEvents.FireToast("Cannot upgrade - check requirements.");
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
