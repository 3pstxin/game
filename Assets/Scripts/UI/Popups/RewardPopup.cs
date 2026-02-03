using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleViking.UI
{
    /// <summary>
    /// Popup displaying rewards/loot after completing an action.
    /// </summary>
    public class RewardPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform rewardContainer;
        [SerializeField] private GameObject rewardItemPrefab;
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI continueButtonText;

        private List<GameObject> _spawnedItems = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnDestroy()
        {
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        /// <summary>
        /// Set up the popup with reward list.
        /// </summary>
        public void Setup(string title, List<string> rewards)
        {
            if (titleText != null)
                titleText.text = title;

            ClearRewardItems();

            if (rewardContainer != null && rewardItemPrefab != null)
            {
                foreach (var reward in rewards)
                {
                    var item = Instantiate(rewardItemPrefab, rewardContainer);
                    var text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                        text.text = reward;
                    _spawnedItems.Add(item);
                }
            }
        }

        /// <summary>
        /// Set up with resource rewards (type + amount).
        /// </summary>
        public void Setup(string title, Dictionary<string, double> rewards)
        {
            var rewardStrings = new List<string>();
            foreach (var kvp in rewards)
            {
                rewardStrings.Add($"{kvp.Key}: +{FormatNumber(kvp.Value)}");
            }
            Setup(title, rewardStrings);
        }

        private void ClearRewardItems()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Destroy(item);
            }
            _spawnedItems.Clear();
        }

        private void OnContinueClicked()
        {
            Close();
        }

        protected override void OnClose()
        {
            base.OnClose();
            ClearRewardItems();
        }

        public override void OnBackdropClick()
        {
            // Allow backdrop click to close
            Close();
        }

        private string FormatNumber(double value)
        {
            if (value >= 1_000_000_000)
                return $"{value / 1_000_000_000:F1}B";
            if (value >= 1_000_000)
                return $"{value / 1_000_000:F1}M";
            if (value >= 1_000)
                return $"{value / 1_000:F1}K";
            return value.ToString("F0");
        }
    }
}
