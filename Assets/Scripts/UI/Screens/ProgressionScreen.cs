using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Data;
using IdleViking.Models;
using IdleViking.Systems;

namespace IdleViking.UI
{
    /// <summary>
    /// Screen showing milestones, achievements, and prestige.
    /// </summary>
    public class ProgressionScreen : BaseScreen
    {
        [Header("Tabs")]
        [SerializeField] private Button milestonesTabButton;
        [SerializeField] private Button prestigeTabButton;
        [SerializeField] private GameObject milestonesPanel;
        [SerializeField] private GameObject prestigePanel;

        [Header("Milestones")]
        [SerializeField] private Transform milestoneContainer;
        [SerializeField] private MilestoneListItem milestoneItemPrefab;

        [Header("Prestige")]
        [SerializeField] private TextMeshProUGUI currentPrestigeText;
        [SerializeField] private TextMeshProUGUI currentMultiplierText;
        [SerializeField] private TextMeshProUGUI nextMultiplierText;
        [SerializeField] private TextMeshProUGUI prestigeRequirementsText;
        [SerializeField] private Button prestigeButton;
        [SerializeField] private TextMeshProUGUI prestigeWarningText;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI totalPlayTimeText;
        [SerializeField] private TextMeshProUGUI milestonesCompletedText;

        [Header("Database")]
        [SerializeField] private MilestoneDatabase milestoneDatabase;

        private List<MilestoneListItem> _milestoneItems = new List<MilestoneListItem>();

        protected override void Awake()
        {
            base.Awake();

            if (milestonesTabButton != null)
                milestonesTabButton.onClick.AddListener(() => ShowTab(true));
            if (prestigeTabButton != null)
                prestigeTabButton.onClick.AddListener(() => ShowTab(false));
            if (prestigeButton != null)
                prestigeButton.onClick.AddListener(OnPrestigeClicked);
        }

        private void OnDestroy()
        {
            if (milestonesTabButton != null)
                milestonesTabButton.onClick.RemoveAllListeners();
            if (prestigeTabButton != null)
                prestigeTabButton.onClick.RemoveAllListeners();
            if (prestigeButton != null)
                prestigeButton.onClick.RemoveListener(OnPrestigeClicked);
        }

        protected override void SubscribeToEvents()
        {
            UIEvents.OnMilestoneCompleted += OnMilestoneCompleted;
            UIEvents.OnPrestigeComplete += OnPrestigeComplete;
        }

        protected override void UnsubscribeFromEvents()
        {
            UIEvents.OnMilestoneCompleted -= OnMilestoneCompleted;
            UIEvents.OnPrestigeComplete -= OnPrestigeComplete;
        }

        public override void Refresh()
        {
            RefreshMilestones();
            RefreshPrestige();
            RefreshStats();
        }

        private void ShowTab(bool showMilestones)
        {
            if (milestonesPanel != null)
                milestonesPanel.SetActive(showMilestones);
            if (prestigePanel != null)
                prestigePanel.SetActive(!showMilestones);
        }

        private void RefreshMilestones()
        {
            ClearMilestoneItems();

            if (milestoneDatabase == null || milestoneContainer == null || milestoneItemPrefab == null)
                return;

            var state = GameManager.Instance?.State;
            if (state == null) return;

            foreach (var milestone in milestoneDatabase.GetAll())
            {
                // Skip hidden milestones
                if (!ProgressionSystem.IsVisible(state, milestone))
                    continue;

                var item = Instantiate(milestoneItemPrefab, milestoneContainer);
                bool isComplete = state.progression.IsMilestoneCompleted(milestone.milestoneId);
                float progress = CalculateMilestoneProgress(state, milestone);
                item.Setup(milestone, isComplete, progress);
                _milestoneItems.Add(item);
            }
        }

        private float CalculateMilestoneProgress(GameState state, MilestoneData milestone)
        {
            if (milestone.conditions == null || milestone.conditions.Length == 0)
                return 1f;

            // Simple progress: percentage of conditions met
            int metCount = 0;
            foreach (var condition in milestone.conditions)
            {
                if (ProgressionSystem.EvaluateCondition(state, condition))
                    metCount++;
            }
            return (float)metCount / milestone.conditions.Length;
        }

        private void RefreshPrestige()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            int currentLevel = state.progression.prestigeLevel;
            float currentMult = 1f + currentLevel * 0.1f;
            float nextMult = 1f + (currentLevel + 1) * 0.1f;

            if (currentPrestigeText != null)
                currentPrestigeText.text = $"Prestige Level: {currentLevel}";

            if (currentMultiplierText != null)
                currentMultiplierText.text = $"Current Bonus: x{currentMult:F1}";

            if (nextMultiplierText != null)
                nextMultiplierText.text = $"Next Level Bonus: x{nextMult:F1}";

            // Prestige requirements info
            if (prestigeRequirementsText != null)
            {
                int completedMilestones = state.progression.completedMilestones.Count;
                int totalMilestones = milestoneDatabase?.GetAll().Count ?? 0;
                prestigeRequirementsText.text = $"Milestones: {completedMilestones}/{totalMilestones}";
            }

            // Prestige button (disabled for now - would need PrestigeData)
            if (prestigeButton != null)
                prestigeButton.interactable = false;

            if (prestigeWarningText != null)
            {
                prestigeWarningText.text = "Warning: Prestige will reset your resources, buildings, and vikings.\n" +
                                           "You will keep: Prestige level, equipment, and permanent unlocks.";
            }
        }

        private void RefreshStats()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (totalPlayTimeText != null)
            {
                totalPlayTimeText.text = "Total Play Time: --";
            }

            if (milestonesCompletedText != null)
            {
                int completed = state.progression.completedMilestones.Count;
                int total = milestoneDatabase?.GetAll().Count ?? 0;
                milestonesCompletedText.text = $"Milestones: {completed}/{total}";
            }
        }

        private void OnPrestigeClicked()
        {
            UIEvents.FireToast("Prestige not available yet.");
        }

        private void OnMilestoneCompleted(MilestoneData milestone)
        {
            Refresh();
        }

        private void OnPrestigeComplete(int newLevel)
        {
            Refresh();
        }

        private void ClearMilestoneItems()
        {
            foreach (var item in _milestoneItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _milestoneItems.Clear();
        }

        protected override void OnHide()
        {
            ClearMilestoneItems();
        }
    }
}
