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
    /// List item for seed/crop selection when planting.
    /// </summary>
    public class SeedListItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI yieldText;
        [SerializeField] private Button selectButton;
        [SerializeField] private CostDisplay costDisplay;

        public event Action<FarmPlotData> OnSeedSelected;

        private FarmPlotData _cropData;
        private GameState _state;

        private void Awake()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(HandleSelect);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(HandleSelect);
        }

        public void Setup(FarmPlotData crop, GameState state)
        {
            _cropData = crop;
            _state = state;

            if (nameText != null)
                nameText.text = crop.displayName;

            if (timeText != null)
                timeText.text = $"Time: {FormatTime(crop.growTimeSeconds)}";

            if (yieldText != null)
            {
                yieldText.text = $"{crop.yieldResource}: {crop.yieldAmount}";
            }

            RefreshAffordability();
        }

        public void RefreshAffordability()
        {
            if (_cropData == null || _state == null) return;

            if (costDisplay != null && _cropData.plantCosts != null)
            {
                var costDict = new System.Collections.Generic.Dictionary<ResourceType, double>();
                foreach (var c in _cropData.plantCosts)
                {
                    costDict[c.resourceType] = c.amount;
                }
                costDisplay.SetCosts(costDict);
            }

            bool canAfford = _cropData.plantCosts == null || _state.resources.CanAfford(_cropData.plantCosts);
            bool hasSlot = _state.farm.HasEmptySlot;

            if (selectButton != null)
                selectButton.interactable = canAfford && hasSlot;
        }

        private void HandleSelect()
        {
            OnSeedSelected?.Invoke(_cropData);
        }

        private string FormatTime(float totalSeconds)
        {
            int hours = (int)(totalSeconds / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);

            if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }
    }
}
