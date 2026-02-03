using System;
using System.Collections.Generic;

namespace IdleViking.Models
{
    /// <summary>
    /// Runtime state for progression, milestones, and prestige.
    /// Persists across prestige resets.
    /// </summary>
    [Serializable]
    public class ProgressionState
    {
        /// <summary>
        /// IDs of completed milestones.
        /// </summary>
        public List<string> completedMilestones = new List<string>();

        /// <summary>
        /// Named unlock flags set by milestone rewards.
        /// e.g. "dungeon_dark_forest", "craft_epic", "auto_harvest"
        /// </summary>
        public List<string> unlockFlags = new List<string>();

        /// <summary>
        /// Current prestige level. Affects production multiplier.
        /// </summary>
        public int prestigeLevel;

        /// <summary>
        /// Total number of prestige resets performed (for stats/UI).
        /// </summary>
        public int totalPrestiges;

        public bool IsMilestoneCompleted(string milestoneId)
        {
            return completedMilestones.Contains(milestoneId);
        }

        public void CompleteMilestone(string milestoneId)
        {
            if (!IsMilestoneCompleted(milestoneId))
                completedMilestones.Add(milestoneId);
        }

        public bool HasFlag(string flag)
        {
            return unlockFlags.Contains(flag);
        }

        public void SetFlag(string flag)
        {
            if (!HasFlag(flag))
                unlockFlags.Add(flag);
        }
    }
}
