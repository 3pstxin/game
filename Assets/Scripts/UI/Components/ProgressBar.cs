using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleViking.UI
{
    /// <summary>
    /// Generic progress bar with optional label.
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI valueText;

        [Header("Settings")]
        [SerializeField] private bool showPercentage = true;
        [SerializeField] private bool animateFill = true;
        [SerializeField] private float animationSpeed = 5f;

        private float _targetFill;
        private float _currentFill;

        private void Update()
        {
            if (animateFill && fillImage != null)
            {
                _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.deltaTime * animationSpeed);
                fillImage.fillAmount = _currentFill;
            }
        }

        /// <summary>
        /// Set progress as a 0-1 value.
        /// </summary>
        public void SetProgress(float progress)
        {
            _targetFill = Mathf.Clamp01(progress);

            if (!animateFill && fillImage != null)
            {
                fillImage.fillAmount = _targetFill;
                _currentFill = _targetFill;
            }

            if (showPercentage && valueText != null)
            {
                valueText.text = $"{_targetFill * 100:F0}%";
            }
        }

        /// <summary>
        /// Set progress with current and max values.
        /// </summary>
        public void SetProgress(double current, double max)
        {
            float progress = max > 0 ? (float)(current / max) : 0f;
            SetProgress(progress);

            if (valueText != null && !showPercentage)
            {
                valueText.text = $"{FormatNumber(current)}/{FormatNumber(max)}";
            }
        }

        /// <summary>
        /// Set the label text.
        /// </summary>
        public void SetLabel(string label)
        {
            if (labelText != null)
                labelText.text = label;
        }

        /// <summary>
        /// Set fill color.
        /// </summary>
        public void SetColor(Color color)
        {
            if (fillImage != null)
                fillImage.color = color;
        }

        /// <summary>
        /// Instantly set progress without animation.
        /// </summary>
        public void SetProgressImmediate(float progress)
        {
            _targetFill = Mathf.Clamp01(progress);
            _currentFill = _targetFill;
            if (fillImage != null)
                fillImage.fillAmount = _currentFill;
        }

        private string FormatNumber(double value)
        {
            if (value >= 1_000_000_000)
                return $"{value / 1_000_000_000:F1}B";
            if (value >= 1_000_000)
                return $"{value / 1_000_000:F1}M";
            if (value >= 1_000)
                return $"{value / 1_000:F1}K";
            return value.ToString("F0");
        }
    }
}
