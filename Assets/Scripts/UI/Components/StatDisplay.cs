using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Models;

namespace IdleViking.UI
{
    /// <summary>
    /// Displays a single stat with label, value, and optional comparison.
    /// </summary>
    public class StatDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private TextMeshProUGUI compareText;
        [SerializeField] private Image icon;

        [Header("Colors")]
        [SerializeField] private Color positiveColor = Color.green;
        [SerializeField] private Color negativeColor = Color.red;
        [SerializeField] private Color neutralColor = Color.white;

        /// <summary>
        /// Set stat display.
        /// </summary>
        public void SetStat(string label, double value, string format = "F0")
        {
            if (labelText != null)
                labelText.text = label;
            if (valueText != null)
                valueText.text = value.ToString(format);
            if (compareText != null)
                compareText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Set stat display with StatType.
        /// </summary>
        public void SetStat(StatType statType, double value)
        {
            string label = GetStatLabel(statType);
            SetStat(label, value);
        }

        /// <summary>
        /// Set stat with comparison value (shows diff).
        /// </summary>
        public void SetStatWithCompare(string label, double currentValue, double compareValue, string format = "F0")
        {
            if (labelText != null)
                labelText.text = label;
            if (valueText != null)
                valueText.text = currentValue.ToString(format);

            if (compareText != null)
            {
                double diff = compareValue - currentValue;
                if (Mathf.Approximately((float)diff, 0f))
                {
                    compareText.gameObject.SetActive(false);
                }
                else
                {
                    compareText.gameObject.SetActive(true);
                    string sign = diff > 0 ? "+" : "";
                    compareText.text = $"({sign}{diff.ToString(format)})";
                    compareText.color = diff > 0 ? positiveColor : negativeColor;
                }
            }
        }

        /// <summary>
        /// Set stat with bonus display.
        /// </summary>
        public void SetStatWithBonus(string label, double baseValue, double bonusValue, string format = "F0")
        {
            if (labelText != null)
                labelText.text = label;

            double total = baseValue + bonusValue;

            if (valueText != null)
                valueText.text = total.ToString(format);

            if (compareText != null)
            {
                if (bonusValue > 0)
                {
                    compareText.gameObject.SetActive(true);
                    compareText.text = $"(+{bonusValue.ToString(format)})";
                    compareText.color = positiveColor;
                }
                else if (bonusValue < 0)
                {
                    compareText.gameObject.SetActive(true);
                    compareText.text = $"({bonusValue.ToString(format)})";
                    compareText.color = negativeColor;
                }
                else
                {
                    compareText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Set icon sprite.
        /// </summary>
        public void SetIcon(Sprite sprite)
        {
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.gameObject.SetActive(sprite != null);
            }
        }

        /// <summary>
        /// Set value color.
        /// </summary>
        public void SetValueColor(Color color)
        {
            if (valueText != null)
                valueText.color = color;
        }

        private string GetStatLabel(StatType statType)
        {
            return statType switch
            {
                StatType.MaxHP => "HP",
                StatType.Attack => "ATK",
                StatType.Defense => "DEF",
                StatType.Speed => "SPD",
                StatType.CritChance => "CRIT%",
                StatType.CritDamage => "CDMG",
                _ => statType.ToString()
            };
        }
    }
}
