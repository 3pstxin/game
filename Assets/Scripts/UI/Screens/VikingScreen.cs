using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

        [Header("Recruitment")]
        [SerializeField] private Transform recruitContainer;
        [SerializeField] private VikingRecruitItem recruitItemPrefab;

        [Header("Tabs")]
        [SerializeField] private Button ownedTabButton;
        [SerializeField] private Button recruitTabButton;
        [SerializeField] private GameObject ownedPanel;
        [SerializeField] private GameObject recruitPanel;

        [Header("Database")]
        [SerializeField] private VikingDatabase vikingDatabase;

        [Header("Detail Popup")]
        [SerializeField] private VikingDetailPopup detailPopupPrefab;

        private List<VikingListItem> _spawnedVikings = new List<VikingListItem>();
        private List<VikingRecruitItem> _spawnedRecruits = new List<VikingRecruitItem>();

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
            RefreshRecruitment();
        }

        protected override void SubscribeToEvents()
        {
            UIEvents.OnVikingsChanged += Refresh;
            UIEvents.OnResourcesChanged += RefreshAffordability;
        }

        protected override void UnsubscribeFromEvents()
        {
            UIEvents.OnVikingsChanged -= Refresh;
            UIEvents.OnResourcesChanged -= RefreshAffordability;
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
            if (state == null) return;

            foreach (var kvp in state.Vikings.OwnedVikings)
            {
                var vikingState = kvp.Value;
                var vikingData = vikingDatabase?.GetVikingById(vikingState.VikingId);
                if (vikingData == null) continue;

                var item = Instantiate(vikingItemPrefab, vikingContainer);
                item.Setup(vikingState, vikingData);
                item.OnVikingClicked += OnVikingSelected;
                _spawnedVikings.Add(item);
            }
        }

        private void RefreshRecruitment()
        {
            ClearRecruitItems();

            if (recruitContainer == null || recruitItemPrefab == null || vikingDatabase == null)
                return;

            var state = GameManager.Instance?.State;
            if (state == null) return;

            foreach (var viking in vikingDatabase.Vikings)
            {
                // Skip already owned
                bool owned = false;
                foreach (var kvp in state.Vikings.OwnedVikings)
                {
                    if (kvp.Value.VikingId == viking.VikingId)
                    {
                        owned = true;
                        break;
                    }
                }
                if (owned) continue;

                // Check if unlocked
                if (!VikingSystem.IsUnlocked(state, viking))
                    continue;

                var item = Instantiate(recruitItemPrefab, recruitContainer);
                item.Setup(viking, state);
                item.OnRecruitClicked += OnRecruitViking;
                _spawnedRecruits.Add(item);
            }
        }

        private void OnVikingSelected(VikingState vikingState)
        {
            if (detailPopupPrefab == null) return;

            var vikingData = vikingDatabase?.GetVikingById(vikingState.VikingId);
            if (vikingData == null) return;

            var popup = Instantiate(detailPopupPrefab, transform.parent);
            popup.Setup(vikingState, vikingData);
            UIManager.Instance?.ShowPopup(popup);
        }

        private void OnRecruitViking(VikingData vikingData)
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            var newViking = VikingSystem.TryRecruit(state, vikingData);
            if (newViking != null)
            {
                UIEvents.FireVikingsChanged();
                UIEvents.FireVikingRecruited(newViking.UniqueId);
                UIEvents.FireResourcesChanged();
                UIEvents.FireToast($"{vikingData.DisplayName} recruited!");
            }
            else
            {
                UIEvents.FireToast("Cannot recruit - check costs.");
            }
        }

        private void RefreshAffordability()
        {
            foreach (var item in _spawnedRecruits)
            {
                if (item != null)
                    item.RefreshAffordability();
            }
        }

        private void ClearOwnedItems()
        {
            foreach (var item in _spawnedVikings)
            {
                if (item != null)
                {
                    item.OnVikingClicked -= OnVikingSelected;
                    Destroy(item.gameObject);
                }
            }
            _spawnedVikings.Clear();
        }

        private void ClearRecruitItems()
        {
            foreach (var item in _spawnedRecruits)
            {
                if (item != null)
                {
                    item.OnRecruitClicked -= OnRecruitViking;
                    Destroy(item.gameObject);
                }
            }
            _spawnedRecruits.Clear();
        }

        protected override void OnHide()
        {
            ClearOwnedItems();
            ClearRecruitItems();
        }
    }
}
