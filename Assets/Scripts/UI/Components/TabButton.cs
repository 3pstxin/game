using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleViking.UI
{
    /// <summary>
    /// Individual tab button in the TabBar.
    /// </summary>
    public class TabButton : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private ScreenType screenType;

        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject notificationBadge;
        [SerializeField] private TextMeshProUGUI notificationCount;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.gray;
        [SerializeField] private Color selectedColor = Color.white;

        public ScreenType ScreenType => screenType;
        public event Action<TabButton> OnTabClicked;

        private bool _isSelected;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(HandleClick);

            if (notificationBadge != null)
                notificationBadge.SetActive(false);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            OnTabClicked?.Invoke(this);
        }

        /// <summary>
        /// Set the selected state.
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (selectedIndicator != null)
                selectedIndicator.SetActive(selected);

            Color targetColor = selected ? selectedColor : normalColor;

            if (icon != null)
                icon.color = targetColor;
            if (label != null)
                label.color = targetColor;
        }

        /// <summary>
        /// Show/hide notification badge.
        /// </summary>
        public void SetNotification(bool show, int count = 0)
        {
            if (notificationBadge != null)
                notificationBadge.SetActive(show);

            if (notificationCount != null)
            {
                if (count > 0)
                {
                    notificationCount.text = count > 99 ? "99+" : count.ToString();
                    notificationCount.gameObject.SetActive(true);
                }
                else
                {
                    notificationCount.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Set the icon sprite.
        /// </summary>
        public void SetIcon(Sprite sprite)
        {
            if (icon != null)
                icon.sprite = sprite;
        }

        /// <summary>
        /// Set the label text.
        /// </summary>
        public void SetLabel(string text)
        {
            if (label != null)
                label.text = text;
        }
    }
}
