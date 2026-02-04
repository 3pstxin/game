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
        [SerializeField] private TextMeshProUGUI rarityText;

        public event Action<VikingData> OnRecruitClicked;

        private VikingData _vikingData;

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

            if (nameText != null)
                nameText.text = data.displayName;

            if (descriptionText != null)
                descriptionText.text = data.description;

            if (rarityText != null)
                rarityText.text = data.rarity.ToString();

            if (statsText != null)
            {
                statsText.text = $"HP:{data.baseHP} ATK:{data.baseATK} DEF:{data.baseDEF}";
            }

            RefreshAffordability();
        }

        public void RefreshAffordability()
        {
            if (_vikingData == null) return;

            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (costDisplay != null && _vikingData.recruitCosts != null)
            {
                var costDict = new System.Collections.Generic.Dictionary<ResourceType, double>();
                foreach (var c in _vikingData.recruitCosts)
                {
                    costDict[c.resourceType] = c.amount;
                }
                costDisplay.SetCosts(costDict);
            }

            bool canAfford = _vikingData.recruitCosts != null && state.resources.CanAfford(_vikingData.recruitCosts);
            if (recruitButton != null)
                recruitButton.interactable = canAfford;
        }

        private void HandleRecruit()
        {
            OnRecruitClicked?.Invoke(_vikingData);
        }
    }
}
