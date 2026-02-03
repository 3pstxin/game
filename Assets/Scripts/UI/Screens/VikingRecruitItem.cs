using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// List item for recruiting a new viking.
    /// </summary>
    public class VikingRecruitItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button recruitButton;
        [SerializeField] private CostDisplay costDisplay;

        [Header("Rarity")]
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TextMeshProUGUI rarityText;

        public event Action<VikingData> OnRecruitClicked;

        private VikingData _vikingData;
        private GameState _state;

        private void Awake()
        {
            if (recruitButton != null)
                recruitButton.onClick.AddListener(HandleRecruit);
        }

        private void OnDestroy()
        {
            if (recruitButton != null)
                recruitButton.onClick.RemoveListener(HandleRecruit);
        }

        public void Setup(VikingData data, GameState state)
        {
            _vikingData = data;
            _state = state;

            if (nameText != null)
                nameText.text = data.DisplayName;

            if (descriptionText != null)
                descriptionText.text = data.Description;

            if (portrait != null && data.Portrait != null)
                portrait.sprite = data.Portrait;

            if (rarityText != null)
                rarityText.text = data.Rarity.ToString();

            if (statsText != null)
            {
                var stats = data.BaseStats;
                statsText.text = $"HP:{stats.MaxHP} ATK:{stats.Attack} DEF:{stats.Defense}";
            }

            RefreshAffordability();
        }

        public void RefreshAffordability()
        {
            if (_vikingData == null || _state == null) return;

            if (costDisplay != null)
                costDisplay.SetCosts(_vikingData.RecruitCost);

            bool canAfford = VikingSystem.CanAffordRecruit(_state, _vikingData);
            if (recruitButton != null)
                recruitButton.interactable = canAfford;
        }

        private void HandleRecruit()
        {
            OnRecruitClicked?.Invoke(_vikingData);
        }
    }
}
