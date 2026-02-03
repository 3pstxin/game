using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Slot for displaying a viking in team selection.
    /// </summary>
    public class VikingSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private ProgressBar hpBar;
        [SerializeField] private Button slotButton;
        [SerializeField] private GameObject selectedIndicator;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.green;

        public event Action<VikingSlot> OnSlotClicked;

        public bool IsSelected { get; private set; }
        public VikingState VikingState { get; private set; }

        private void Awake()
        {
            if (slotButton != null)
                slotButton.onClick.AddListener(HandleClick);

            if (selectedIndicator != null)
                selectedIndicator.SetActive(false);
        }

        private void OnDestroy()
        {
            if (slotButton != null)
                slotButton.onClick.RemoveListener(HandleClick);
        }

        public void Setup(VikingState state, VikingData data)
        {
            VikingState = state;

            if (nameText != null)
                nameText.text = data?.DisplayName ?? "Viking";

            if (levelText != null)
                levelText.text = $"Lv.{state.Level}";

            if (portrait != null && data?.Portrait != null)
                portrait.sprite = data.Portrait;

            if (hpBar != null)
            {
                var stats = data != null ? state.GetTotalStats(data) : new CombatStats();
                hpBar.SetProgress(state.CurrentHP, stats.MaxHP);
            }
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (selectedIndicator != null)
                selectedIndicator.SetActive(selected);

            if (portrait != null)
                portrait.color = selected ? selectedColor : normalColor;
        }

        public void UpdateHP(int currentHP, int maxHP)
        {
            if (hpBar != null)
                hpBar.SetProgress(currentHP, maxHP);
        }

        private void HandleClick()
        {
            OnSlotClicked?.Invoke(this);
        }
    }
}
