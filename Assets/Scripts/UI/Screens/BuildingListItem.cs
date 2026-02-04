using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// List item displaying a building with upgrade button.
    /// </summary>
    public class BuildingListItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        [SerializeField] private CostDisplay costDisplay;

        [Header("Colors")]
        [SerializeField] private Color maxLevelColor = Color.yellow;

        public event Action<BuildingData> OnUpgradeClicked;

        private BuildingData _building;

        private void Awake()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(HandleUpgradeClick);
        }

        private void OnDestroy()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.RemoveListener(HandleUpgradeClick);
        }

        public void Setup(BuildingData building, GameState state)
        {
            _building = building;

            int level = state.buildings.GetLevel(building.buildingId);
            bool isMaxLevel = level >= building.maxLevel;

            if (nameText != null)
                nameText.text = building.displayName;

            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
                levelText.color = isMaxLevel ? maxLevelColor : Color.white;
            }

            if (descriptionText != null)
                descriptionText.text = building.description;

            RefreshAffordability();
        }

        public void RefreshAffordability()
        {
            if (_building == null) return;

            var state = GameManager.Instance?.State;
            if (state == null) return;

            int level = state.buildings.GetLevel(_building.buildingId);
            bool isMaxLevel = level >= _building.maxLevel;

            if (isMaxLevel)
            {
                if (upgradeButton != null)
                    upgradeButton.interactable = false;
                if (upgradeButtonText != null)
                    upgradeButtonText.text = "MAX";
                if (costDisplay != null)
                    costDisplay.gameObject.SetActive(false);
            }
            else
            {
                var costs = _building.GetCostsForLevel(level + 1);

                if (costDisplay != null)
                {
                    costDisplay.gameObject.SetActive(true);
                    // Convert ResourceCost[] to Dictionary for CostDisplay
                    var costDict = new System.Collections.Generic.Dictionary<ResourceType, double>();
                    foreach (var c in costs)
                    {
                        costDict[c.resourceType] = c.amount;
                    }
                    costDisplay.SetCosts(costDict);
                }

                bool canAfford = state.resources.CanAfford(costs);
                if (upgradeButton != null)
                    upgradeButton.interactable = canAfford;
                if (upgradeButtonText != null)
                    upgradeButtonText.text = level == 0 ? "Build" : "Upgrade";
            }
        }

        private void HandleUpgradeClick()
        {
            OnUpgradeClicked?.Invoke(_building);
        }
    }
}
