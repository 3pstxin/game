using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI bonusText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        [SerializeField] private CostDisplay costDisplay;

        [Header("Colors")]
        [SerializeField] private Color maxLevelColor = Color.yellow;

        public event Action<BuildingData> OnUpgradeClicked;

        private BuildingData _building;
        private GameState _state;

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
            _state = state;

            int level = state.Buildings.GetBuildingLevel(building.BuildingId);
            bool isMaxLevel = level >= building.MaxLevel;

            if (nameText != null)
                nameText.text = building.DisplayName;

            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
                levelText.color = isMaxLevel ? maxLevelColor : Color.white;
            }

            if (descriptionText != null)
                descriptionText.text = building.Description;

            if (bonusText != null)
            {
                var bonuses = building.GetBonusesAtLevel(level);
                string bonusStr = "";
                foreach (var bonus in bonuses)
                {
                    bonusStr += $"{bonus.BonusType}: +{bonus.Value:F0}\n";
                }
                bonusText.text = bonusStr.TrimEnd('\n');
            }

            if (icon != null && building.Icon != null)
                icon.sprite = building.Icon;

            RefreshAffordability();
        }

        public void RefreshAffordability()
        {
            if (_building == null || _state == null) return;

            int level = _state.Buildings.GetBuildingLevel(_building.BuildingId);
            bool isMaxLevel = level >= _building.MaxLevel;

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
                var costs = _building.GetUpgradeCost(level + 1);

                if (costDisplay != null)
                {
                    costDisplay.gameObject.SetActive(true);
                    costDisplay.SetCosts(costs);
                }

                bool canAfford = BuildingSystem.CanAffordUpgrade(_state, _building);
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
