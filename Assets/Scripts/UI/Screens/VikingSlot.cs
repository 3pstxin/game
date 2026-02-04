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
        [SerializeField] private Button slotButton;
        [SerializeField] private GameObject selectedIndicator;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.green;

        public event Action<VikingSlot> OnSlotClicked;

        public bool IsSelected { get; private set; }
        public VikingInstance VikingInstance { get; private set; }

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

        public void Setup(VikingInstance instance, VikingData data)
        {
            VikingInstance = instance;

            if (nameText != null)
                nameText.text = data?.displayName ?? "Viking";

            if (levelText != null)
                levelText.text = $"Lv.{instance.level}";
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (selectedIndicator != null)
                selectedIndicator.SetActive(selected);

            if (portrait != null)
                portrait.color = selected ? selectedColor : normalColor;
        }

        private void HandleClick()
        {
            OnSlotClicked?.Invoke(this);
        }
    }
}
