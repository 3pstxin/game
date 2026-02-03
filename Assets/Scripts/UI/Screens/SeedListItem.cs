using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
                nameText.text = crop.DisplayName;

            if (icon != null && crop.Icon != null)
                icon.sprite = crop.Icon;

            if (timeText != null)
                timeText.text = $"Time: {FormatTime(crop.GrowthTime)}";

            if (yieldText != null)
            {
                string yields = "";
                foreach (var yield in crop.Yields)
                {
                    yields += $"{yield.ResourceType}: {yield.MinAmount}-{yield.MaxAmount}\n";
                }
                yieldText.text = yields.TrimEnd('\n');
            }

            RefreshAffordability();
        }

        public void RefreshAffordability()
        {
            if (_cropData == null || _state == null) return;

            if (costDisplay != null)
                costDisplay.SetCosts(_cropData.PlantCost);

            bool canAfford = FarmSystem.CanAffordPlant(_state, _cropData);
            if (selectButton != null)
                selectButton.interactable = canAfford;
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
