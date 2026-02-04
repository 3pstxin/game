using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Displays resource costs with affordability coloring.
    /// </summary>
    public class CostDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform costContainer;
        [SerializeField] private GameObject costItemPrefab;

        [Header("Colors")]
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = Color.red;

        private List<GameObject> _spawnedItems = new List<GameObject>();

        /// <summary>
        /// Display costs with affordability check.
        /// </summary>
        public void SetCosts(Dictionary<ResourceType, double> costs)
        {
            ClearItems();

            if (costContainer == null || costItemPrefab == null)
                return;

            var state = GameManager.Instance?.State;

            foreach (var kvp in costs)
            {
                var item = Instantiate(costItemPrefab, costContainer);
                _spawnedItems.Add(item);

                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{kvp.Key}: {FormatNumber(kvp.Value)}";

                    // Check affordability
                    bool canAfford = state != null && state.resources.Get(kvp.Key) >= kvp.Value;
                    text.color = canAfford ? affordableColor : unaffordableColor;
                }
            }
        }

        /// <summary>
        /// Display costs from a SerializableDictionary.
        /// </summary>
        public void SetCosts(SerializableDictionary<ResourceType, double> costs)
        {
            var dict = new Dictionary<ResourceType, double>();
            foreach (var kvp in costs)
            {
                dict[kvp.Key] = kvp.Value;
            }
            SetCosts(dict);
        }

        /// <summary>
        /// Check if all costs can be afforded.
        /// </summary>
        public bool CanAfford(Dictionary<ResourceType, double> costs)
        {
            var state = GameManager.Instance?.State;
            if (state == null) return false;

            foreach (var kvp in costs)
            {
                if (state.resources.Get(kvp.Key) < kvp.Value)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Refresh affordability colors without rebuilding.
        /// </summary>
        public void RefreshAffordability(Dictionary<ResourceType, double> costs)
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            int index = 0;
            foreach (var kvp in costs)
            {
                if (index >= _spawnedItems.Count) break;

                var text = _spawnedItems[index].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    bool canAfford = state.resources.Get(kvp.Key) >= kvp.Value;
                    text.color = canAfford ? affordableColor : unaffordableColor;
                }
                index++;
            }
        }

        private void ClearItems()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Destroy(item);
            }
            _spawnedItems.Clear();
        }

        private void OnDestroy()
        {
            ClearItems();
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
