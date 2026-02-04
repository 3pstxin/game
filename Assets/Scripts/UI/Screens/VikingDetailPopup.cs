using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Core;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Popup showing viking details with assignment options.
    /// </summary>
    public class VikingDetailPopup : BasePopup
    {
        [Header("Viking Info")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Assignment")]
        [SerializeField] private TMP_Dropdown assignmentDropdown;
        [SerializeField] private Button closeButton;

        private VikingInstance _vikingInstance;
        private VikingData _vikingData;

        protected override void Awake()
        {
            base.Awake();

            if (assignmentDropdown != null)
                assignmentDropdown.onValueChanged.AddListener(OnAssignmentChanged);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            if (assignmentDropdown != null)
                assignmentDropdown.onValueChanged.RemoveListener(OnAssignmentChanged);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
        }

        public void Setup(VikingInstance instance, VikingData data)
        {
            _vikingInstance = instance;
            _vikingData = data;

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_vikingInstance == null || _vikingData == null) return;

            if (nameText != null)
                nameText.text = _vikingData.displayName;

            if (levelText != null)
                levelText.text = $"Level {_vikingInstance.level}";

            if (rarityText != null)
                rarityText.text = _vikingData.rarity.ToString();

            if (statsText != null)
            {
                int hp = _vikingData.GetStat(StatType.HP, _vikingInstance.level);
                int atk = _vikingData.GetStat(StatType.ATK, _vikingInstance.level);
                int def = _vikingData.GetStat(StatType.DEF, _vikingInstance.level);
                int spd = _vikingData.GetStat(StatType.SPD, _vikingInstance.level);
                statsText.text = $"HP: {hp}\nATK: {atk}\nDEF: {def}\nSPD: {spd}";
            }

            // Assignment dropdown
            if (assignmentDropdown != null)
            {
                assignmentDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string> { "Idle", "Building", "Party" };
                assignmentDropdown.AddOptions(options);
                assignmentDropdown.value = (int)_vikingInstance.assignment;
            }
        }

        private void OnAssignmentChanged(int index)
        {
            if (_vikingInstance == null) return;

            _vikingInstance.assignment = (VikingAssignment)index;
            UIEvents.FireVikingsChanged();
        }
    }
}
