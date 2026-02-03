using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// List item showing an owned viking.
    /// </summary>
    public class VikingListItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI assignmentText;
        [SerializeField] private ProgressBar expBar;
        [SerializeField] private Button selectButton;

        [Header("Rarity Colors")]
        [SerializeField] private Image rarityBorder;
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.5f, 0f);

        public event Action<VikingState> OnVikingClicked;

        private VikingState _vikingState;
        private VikingData _vikingData;

        private void Awake()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(HandleClick);
        }

        public void Setup(VikingState state, VikingData data)
        {
            _vikingState = state;
            _vikingData = data;

            if (nameText != null)
                nameText.text = data.DisplayName;

            if (levelText != null)
                levelText.text = $"Lv.{state.Level}";

            if (assignmentText != null)
                assignmentText.text = GetAssignmentText(state.Assignment);

            if (portrait != null && data.Portrait != null)
                portrait.sprite = data.Portrait;

            if (expBar != null)
            {
                int expForNext = VikingSystem.GetExpForLevel(state.Level + 1);
                expBar.SetProgress(state.Experience, expForNext);
            }

            if (rarityBorder != null)
                rarityBorder.color = GetRarityColor(data.Rarity);
        }

        private string GetAssignmentText(VikingAssignment assignment)
        {
            return assignment switch
            {
                VikingAssignment.Idle => "Idle",
                VikingAssignment.Production => "Working",
                VikingAssignment.Combat => "Combat",
                VikingAssignment.Farming => "Farming",
                _ => assignment.ToString()
            };
        }

        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => commonColor,
                Rarity.Uncommon => uncommonColor,
                Rarity.Rare => rareColor,
                Rarity.Epic => epicColor,
                Rarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }

        private void HandleClick()
        {
            OnVikingClicked?.Invoke(_vikingState);
        }
    }
}
