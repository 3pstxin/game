using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IdleViking.UI
{
    /// <summary>
    /// Bottom tab bar for main navigation.
    /// </summary>
    public class TabBar : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private List<TabButton> tabs = new List<TabButton>();

        [Header("Settings")]
        [SerializeField] private ScreenType defaultTab = ScreenType.Home;

        private TabButton _activeTab;

        private void Start()
        {
            // Set up tab click handlers
            foreach (var tab in tabs)
            {
                if (tab != null)
                    tab.OnTabClicked += OnTabClicked;
            }

            // Select default tab
            SelectTab(defaultTab);
        }

        private void OnDestroy()
        {
            foreach (var tab in tabs)
            {
                if (tab != null)
                    tab.OnTabClicked -= OnTabClicked;
            }
        }

        private void OnTabClicked(TabButton tab)
        {
            if (tab == _activeTab) return;

            SelectTab(tab);
        }

        /// <summary>
        /// Select a tab by screen type.
        /// </summary>
        public void SelectTab(ScreenType screenType)
        {
            foreach (var tab in tabs)
            {
                if (tab != null && tab.ScreenType == screenType)
                {
                    SelectTab(tab);
                    return;
                }
            }
        }

        private void SelectTab(TabButton tab)
        {
            // Deselect current
            if (_activeTab != null)
                _activeTab.SetSelected(false);

            // Select new
            _activeTab = tab;
            if (_activeTab != null)
            {
                _activeTab.SetSelected(true);
                UIManager.Instance?.ShowScreen(_activeTab.ScreenType);
            }
        }

        /// <summary>
        /// Show/hide notification badge on a tab.
        /// </summary>
        public void SetNotification(ScreenType screenType, bool show, int count = 0)
        {
            foreach (var tab in tabs)
            {
                if (tab != null && tab.ScreenType == screenType)
                {
                    tab.SetNotification(show, count);
                    return;
                }
            }
        }
    }
}
