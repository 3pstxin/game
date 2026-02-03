using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;

namespace IdleViking.UI
{
    /// <summary>
    /// List item showing a milestone/achievement.
    /// </summary>
    public class MilestoneListItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private ProgressBar progressBar;
        [SerializeField] private GameObject completedIndicator;
        [SerializeField] private Image background;

        [Header("Colors")]
        [SerializeField] private Color incompleteColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color completeColor = new Color(0.2f, 0.4f, 0.2f);

        public void Setup(MilestoneData milestone, bool isComplete, float progress)
        {
            if (nameText != null)
                nameText.text = milestone.DisplayName;

            if (descriptionText != null)
                descriptionText.text = milestone.Description;

            if (icon != null && milestone.Icon != null)
                icon.sprite = milestone.Icon;

            if (rewardText != null)
            {
                string rewards = "";
                foreach (var reward in milestone.Rewards)
                {
                    rewards += $"{reward.RewardType}: {reward.Value}\n";
                }
                rewardText.text = rewards.TrimEnd('\n');
            }

            if (completedIndicator != null)
                completedIndicator.SetActive(isComplete);

            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(!isComplete);
                if (!isComplete)
                {
                    progressBar.SetProgress(progress);
                }
            }

            if (background != null)
                background.color = isComplete ? completeColor : incompleteColor;

            // Dim completed milestones slightly
            if (icon != null)
                icon.color = isComplete ? Color.white : new Color(0.7f, 0.7f, 0.7f);
        }
    }
}
