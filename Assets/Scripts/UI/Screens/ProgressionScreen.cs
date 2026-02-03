using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

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
                milestonesTabButton.onClick.RemoveListener(() => ShowTab(true));
            if (prestigeTabButton != null)
                prestigeTabButton.onClick.RemoveListener(() => ShowTab(false));
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

            foreach (var milestone in milestoneDatabase.Milestones)
            {
                var item = Instantiate(milestoneItemPrefab, milestoneContainer);
                bool isComplete = state.Progression.IsMilestoneComplete(milestone.MilestoneId);
                float progress = ProgressionSystem.GetMilestoneProgress(state, milestone);
                item.Setup(milestone, isComplete, progress);
                _milestoneItems.Add(item);
            }
        }

        private void RefreshPrestige()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            int currentLevel = state.Progression.PrestigeLevel;
            float currentMult = state.Progression.GetPrestigeMultiplier();
            float nextMult = 1f + (currentLevel + 1) * 0.1f; // Next level multiplier

            if (currentPrestigeText != null)
                currentPrestigeText.text = $"Prestige Level: {currentLevel}";

            if (currentMultiplierText != null)
                currentMultiplierText.text = $"Current Bonus: x{currentMult:F1}";

            if (nextMultiplierText != null)
                nextMultiplierText.text = $"Next Level Bonus: x{nextMult:F1}";

            // Check prestige requirements
            bool canPrestige = ProgressionSystem.CanPrestige(state, milestoneDatabase);

            if (prestigeRequirementsText != null)
            {
                if (canPrestige)
                {
                    prestigeRequirementsText.text = "All requirements met!";
                    prestigeRequirementsText.color = Color.green;
                }
                else
                {
                    // Show what's needed
                    int completedMilestones = state.Progression.CompletedMilestones.Count;
                    int requiredMilestones = milestoneDatabase != null ? milestoneDatabase.Milestones.Count / 2 : 5;
                    prestigeRequirementsText.text = $"Complete {requiredMilestones} milestones ({completedMilestones} done)";
                    prestigeRequirementsText.color = Color.yellow;
                }
            }

            if (prestigeButton != null)
                prestigeButton.interactable = canPrestige;

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
                // Calculate total play time (rough estimate from timestamps)
                long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long playTime = now - state.CreatedTimestamp;
                totalPlayTimeText.text = $"Total Play Time: {FormatPlayTime(playTime)}";
            }

            if (milestonesCompletedText != null)
            {
                int completed = state.Progression.CompletedMilestones.Count;
                int total = milestoneDatabase?.Milestones.Count ?? 0;
                milestonesCompletedText.text = $"Milestones: {completed}/{total}";
            }
        }

        private void OnPrestigeClicked()
        {
            UIManager.Instance?.ShowConfirm(
                "Confirm Prestige",
                "Are you sure you want to prestige? This will reset most progress but grant permanent bonuses.",
                confirmed =>
                {
                    if (confirmed)
                    {
                        var state = GameManager.Instance?.State;
                        if (state != null && ProgressionSystem.CanPrestige(state, milestoneDatabase))
                        {
                            ProgressionSystem.PerformPrestige(state, milestoneDatabase);
                            UIEvents.FirePrestigeComplete(state.Progression.PrestigeLevel);
                            UIEvents.FireGameStateLoaded();
                            UIEvents.FireToast($"Prestiged! Now level {state.Progression.PrestigeLevel}");
                        }
                    }
                }
            );
        }

        private void OnMilestoneCompleted(MilestoneData milestone)
        {
            Refresh();
            UIManager.Instance?.ShowReward(
                "Milestone Complete!",
                new List<string> { milestone.DisplayName, milestone.Description }
            );
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

        private string FormatPlayTime(long seconds)
        {
            int days = (int)(seconds / 86400);
            int hours = (int)((seconds % 86400) / 3600);
            int minutes = (int)((seconds % 3600) / 60);

            if (days > 0)
                return $"{days}d {hours}h";
            else if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }
    }
}
