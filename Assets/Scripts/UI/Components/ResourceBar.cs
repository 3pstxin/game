using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Displays a resource with icon and amount.
    /// Optionally shows production rate.
    /// </summary>
    public class ResourceBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI rateText;

        [Header("Settings")]
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private bool showRate = false;

        public ResourceType ResourceType => resourceType;

        private void OnEnable()
        {
            UIEvents.OnResourcesChanged += Refresh;
            UIEvents.OnResourceChanged += OnResourceChanged;
        }

        private void OnDisable()
        {
            UIEvents.OnResourcesChanged -= Refresh;
            UIEvents.OnResourceChanged -= OnResourceChanged;
        }

        private void Start()
        {
            Refresh();
        }

        private void OnResourceChanged(ResourceType type, double amount)
        {
            if (type == resourceType)
                UpdateDisplay(amount);
        }

        public void Refresh()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            double amount = state.resources.Get(resourceType);
            UpdateDisplay(amount);
        }

        public void SetResourceType(ResourceType type)
        {
            resourceType = type;
            Refresh();
        }

        private void UpdateDisplay(double amount)
        {
            if (amountText != null)
                amountText.text = FormatNumber(amount);

            if (showRate && rateText != null)
            {
                double rate = CalculateRate();
                rateText.text = rate > 0 ? $"+{FormatNumber(rate)}/s" : "";
            }
        }

        private double CalculateRate()
        {
            // Rate calculation requires database lookup - simplified for now
            // Full implementation would iterate producers and match resourceType via ResourceProducerData
            return 0;
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
