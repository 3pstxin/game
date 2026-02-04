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
    /// Screen showing owned vikings and recruitment options.
    /// </summary>
    public class VikingScreen : BaseScreen
    {
        [Header("Viking List")]
        [SerializeField] private Transform vikingContainer;
        [SerializeField] private VikingListItem vikingItemPrefab;

        [Header("Tabs")]
        [SerializeField] private Button ownedTabButton;
        [SerializeField] private Button recruitTabButton;
        [SerializeField] private GameObject ownedPanel;
        [SerializeField] private GameObject recruitPanel;

        private List<VikingListItem> _spawnedVikings = new List<VikingListItem>();

        protected override void Awake()
        {
            base.Awake();

            if (ownedTabButton != null)
                ownedTabButton.onClick.AddListener(() => ShowTab(true));
            if (recruitTabButton != null)
                recruitTabButton.onClick.AddListener(() => ShowTab(false));
        }

        public override void Refresh()
        {
            RefreshOwnedVikings();
        }

        protected override void SubscribeToEvents()
        {
            UIEvents.OnVikingsChanged += Refresh;
        }

        protected override void UnsubscribeFromEvents()
        {
            UIEvents.OnVikingsChanged -= Refresh;
        }

        private void ShowTab(bool showOwned)
        {
            if (ownedPanel != null)
                ownedPanel.SetActive(showOwned);
            if (recruitPanel != null)
                recruitPanel.SetActive(!showOwned);
        }

        private void RefreshOwnedVikings()
        {
            ClearOwnedItems();

            if (vikingContainer == null || vikingItemPrefab == null)
                return;

            var state = GameManager.Instance?.State;
            var vikingDB = GameManager.Instance?.VikingDB;
            if (state == null || vikingDB == null) return;

            foreach (var vikingInstance in state.vikings.vikings)
            {
                var vikingData = vikingDB.GetViking(vikingInstance.vikingDataId);
                if (vikingData == null) continue;

                var item = Instantiate(vikingItemPrefab, vikingContainer);
                item.Setup(vikingInstance, vikingData);
                _spawnedVikings.Add(item);
            }
        }

        private void ClearOwnedItems()
        {
            foreach (var item in _spawnedVikings)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _spawnedVikings.Clear();
        }

        protected override void OnHide()
        {
            ClearOwnedItems();
        }
    }
}
