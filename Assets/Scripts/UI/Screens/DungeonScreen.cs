using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Screen for selecting and running dungeons.
    /// </summary>
    public class DungeonScreen : BaseScreen
    {
        [Header("Dungeon Selection")]
        [SerializeField] private Transform dungeonListContainer;
        [SerializeField] private DungeonListItem dungeonItemPrefab;

        [Header("Energy Display")]
        [SerializeField] private TextMeshProUGUI energyText;

        private List<DungeonListItem> _dungeonItems = new List<DungeonListItem>();

        public override void Refresh()
        {
            RefreshEnergy();
            RefreshDungeonList();
        }

        protected override void SubscribeToEvents()
        {
            // Subscribe to dungeon events if needed
        }

        protected override void UnsubscribeFromEvents()
        {
            // Unsubscribe
        }

        private void RefreshEnergy()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (energyText != null)
            {
                int current = Mathf.FloorToInt(state.dungeons.energy);
                int max = Mathf.FloorToInt(state.dungeons.maxEnergy);
                energyText.text = $"Energy: {current}/{max}";
            }
        }

        private void RefreshDungeonList()
        {
            ClearDungeonItems();

            if (dungeonListContainer == null || dungeonItemPrefab == null)
                return;

            var state = GameManager.Instance?.State;
            var dungeonDB = GameManager.Instance?.DungeonDB;
            if (state == null || dungeonDB == null) return;

            foreach (var dungeon in dungeonDB.dungeons)
            {
                var item = Instantiate(dungeonItemPrefab, dungeonListContainer);
                item.Setup(dungeon, state);
                item.OnDungeonSelected += OnDungeonSelected;
                _dungeonItems.Add(item);
            }
        }

        private void OnDungeonSelected(DungeonData dungeon)
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (state.dungeons.energy < dungeon.energyCost)
            {
                UIEvents.FireToast("Not enough energy!");
                return;
            }

            // For now, just show a message - full dungeon run would be more complex
            UIEvents.FireToast($"Starting {dungeon.displayName}...");
        }

        private void ClearDungeonItems()
        {
            foreach (var item in _dungeonItems)
            {
                if (item != null)
                {
                    item.OnDungeonSelected -= OnDungeonSelected;
                    Destroy(item.gameObject);
                }
            }
            _dungeonItems.Clear();
        }

        protected override void OnHide()
        {
            ClearDungeonItems();
        }
    }
}
