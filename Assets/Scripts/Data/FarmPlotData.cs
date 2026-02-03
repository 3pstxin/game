using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Static definition of a farmable crop or animal.
    /// e.g. "Wheat" (crop), "Chicken" (animal), "Barley" (crop).
    /// </summary>
    [CreateAssetMenu(fileName = "NewFarmPlot", menuName = "IdleViking/Farm Plot")]
    public class FarmPlotData : ScriptableObject
    {
        [Header("Identity")]
        public string farmPlotId;
        public string displayName;
        [TextArea] public string description;

        [Header("Type")]
        public FarmType farmType = FarmType.Crop;

        [Header("Growth")]
        [Tooltip("Time in seconds to grow/produce one cycle")]
        public float growTimeSeconds = 300f;

        [Header("Yield")]
        public ResourceType yieldResource = ResourceType.Food;
        public double yieldAmount = 10;

        [Header("Cost")]
        [Tooltip("Resources spent to plant/acquire")]
        public ResourceCost[] plantCosts;

        [Header("Unlock")]
        [Tooltip("Building required to unlock this farmable")]
        public BuildingData prerequisiteBuilding;
        public int prerequisiteLevel;
    }
}
