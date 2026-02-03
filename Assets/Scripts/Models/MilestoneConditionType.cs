namespace IdleViking.Models
{
    /// <summary>
    /// Types of conditions that can gate a milestone.
    /// </summary>
    public enum MilestoneConditionType
    {
        /// <summary>Have at least X of a resource (current amount, not lifetime).</summary>
        ResourceAmount,

        /// <summary>Building at minimum level.</summary>
        BuildingLevel,

        /// <summary>Dungeon cleared at least once.</summary>
        DungeonCleared,

        /// <summary>Own at least X vikings total.</summary>
        VikingCount,

        /// <summary>Any viking at minimum level.</summary>
        VikingLevel,

        /// <summary>Completed another milestone (for chaining).</summary>
        MilestoneCompleted,

        /// <summary>Prestige level at minimum value.</summary>
        PrestigeLevel
    }
}
