using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// List item for dungeon selection.
    /// </summary>
    public class DungeonListItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private Button selectButton;

        public event Action<DungeonData> OnDungeonSelected;

        private DungeonData _dungeon;

        private void Awake()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(HandleSelect);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(HandleSelect);
        }

        public void Setup(DungeonData dungeon, GameState state)
        {
            _dungeon = dungeon;

            if (nameText != null)
                nameText.text = dungeon.DisplayName;

            if (descriptionText != null)
                descriptionText.text = dungeon.Description;

            if (icon != null && dungeon.Icon != null)
                icon.sprite = dungeon.Icon;

            if (energyText != null)
                energyText.text = $"Energy: {dungeon.EnergyCost}";

            if (progressText != null)
            {
                int highestFloor = state.Dungeon.GetHighestFloor(dungeon.DungeonId);
                progressText.text = $"Best: Floor {highestFloor}/{dungeon.FloorCount}";
            }
        }

        private void HandleSelect()
        {
            OnDungeonSelected?.Invoke(_dungeon);
        }
    }
}
