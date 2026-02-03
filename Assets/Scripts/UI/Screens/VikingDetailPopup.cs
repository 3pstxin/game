using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Popup showing viking details with assignment and equipment options.
    /// </summary>
    public class VikingDetailPopup : BasePopup
    {
        [Header("Viking Info")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private ProgressBar expBar;

        [Header("Stats")]
        [SerializeField] private StatDisplay hpStat;
        [SerializeField] private StatDisplay attackStat;
        [SerializeField] private StatDisplay defenseStat;
        [SerializeField] private StatDisplay speedStat;

        [Header("Assignment")]
        [SerializeField] private TMP_Dropdown assignmentDropdown;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private CostDisplay levelUpCost;

        [Header("Equipment Slots")]
        [SerializeField] private Button weaponSlot;
        [SerializeField] private Button armorSlot;
        [SerializeField] private Button accessorySlot;
        [SerializeField] private TextMeshProUGUI weaponText;
        [SerializeField] private TextMeshProUGUI armorText;
        [SerializeField] private TextMeshProUGUI accessoryText;

        private VikingState _vikingState;
        private VikingData _vikingData;

        protected override void Awake()
        {
            base.Awake();

            if (assignmentDropdown != null)
                assignmentDropdown.onValueChanged.AddListener(OnAssignmentChanged);
            if (levelUpButton != null)
                levelUpButton.onClick.AddListener(OnLevelUpClicked);
        }

        private void OnDestroy()
        {
            if (assignmentDropdown != null)
                assignmentDropdown.onValueChanged.RemoveListener(OnAssignmentChanged);
            if (levelUpButton != null)
                levelUpButton.onClick.RemoveListener(OnLevelUpClicked);
        }

        public void Setup(VikingState state, VikingData data)
        {
            _vikingState = state;
            _vikingData = data;

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_vikingState == null || _vikingData == null) return;

            if (nameText != null)
                nameText.text = _vikingData.DisplayName;

            if (levelText != null)
                levelText.text = $"Level {_vikingState.Level}";

            if (rarityText != null)
                rarityText.text = _vikingData.Rarity.ToString();

            if (portrait != null && _vikingData.Portrait != null)
                portrait.sprite = _vikingData.Portrait;

            if (expBar != null)
            {
                int expForNext = VikingSystem.GetExpForLevel(_vikingState.Level + 1);
                expBar.SetProgress(_vikingState.Experience, expForNext);
            }

            // Stats
            var totalStats = _vikingState.GetTotalStats(_vikingData);
            var baseStats = _vikingData.GetStatsAtLevel(_vikingState.Level);

            if (hpStat != null)
                hpStat.SetStatWithBonus("HP", baseStats.MaxHP, totalStats.MaxHP - baseStats.MaxHP);
            if (attackStat != null)
                attackStat.SetStatWithBonus("ATK", baseStats.Attack, totalStats.Attack - baseStats.Attack);
            if (defenseStat != null)
                defenseStat.SetStatWithBonus("DEF", baseStats.Defense, totalStats.Defense - baseStats.Defense);
            if (speedStat != null)
                speedStat.SetStatWithBonus("SPD", baseStats.Speed, totalStats.Speed - baseStats.Speed);

            // Assignment dropdown
            if (assignmentDropdown != null)
            {
                assignmentDropdown.ClearOptions();
                var options = new List<string>();
                foreach (VikingAssignment assignment in Enum.GetValues(typeof(VikingAssignment)))
                {
                    options.Add(assignment.ToString());
                }
                assignmentDropdown.AddOptions(options);
                assignmentDropdown.value = (int)_vikingState.Assignment;
            }

            // Equipment display
            RefreshEquipmentSlots();
        }

        private void RefreshEquipmentSlots()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            // Weapon
            if (weaponText != null)
            {
                if (_vikingState.EquippedWeaponId > 0)
                {
                    var item = state.Inventory.GetItem(_vikingState.EquippedWeaponId);
                    weaponText.text = item?.ItemName ?? "Unknown";
                }
                else
                {
                    weaponText.text = "(Empty)";
                }
            }

            // Armor
            if (armorText != null)
            {
                if (_vikingState.EquippedArmorId > 0)
                {
                    var item = state.Inventory.GetItem(_vikingState.EquippedArmorId);
                    armorText.text = item?.ItemName ?? "Unknown";
                }
                else
                {
                    armorText.text = "(Empty)";
                }
            }

            // Accessory
            if (accessoryText != null)
            {
                if (_vikingState.EquippedAccessoryId > 0)
                {
                    var item = state.Inventory.GetItem(_vikingState.EquippedAccessoryId);
                    accessoryText.text = item?.ItemName ?? "Unknown";
                }
                else
                {
                    accessoryText.text = "(Empty)";
                }
            }
        }

        private void OnAssignmentChanged(int index)
        {
            var newAssignment = (VikingAssignment)index;
            var state = GameManager.Instance?.State;
            if (state == null) return;

            VikingSystem.AssignViking(state, _vikingState.UniqueId, newAssignment);
            UIEvents.FireVikingsChanged();
        }

        private void OnLevelUpClicked()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            // For now, simple level up via XP
            // In full implementation, might have level-up costs
            int expNeeded = VikingSystem.GetExpForLevel(_vikingState.Level + 1) - _vikingState.Experience;
            if (expNeeded <= 0)
            {
                VikingSystem.AddExperience(state, _vikingState.UniqueId, 0); // Triggers level up check
                RefreshDisplay();
                UIEvents.FireVikingsChanged();
            }
            else
            {
                UIEvents.FireToast($"Need {expNeeded} more XP to level up.");
            }
        }
    }
}
