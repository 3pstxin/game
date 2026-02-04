using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;
using IdleViking.Systems;

namespace IdleViking.UI
{
    /// <summary>
    /// UI representation of a single farm plot.
    /// </summary>
    public class FarmPlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image plotImage;
        [SerializeField] private Image cropIcon;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private ProgressBar growthBar;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button plotButton;
        [SerializeField] private GameObject readyIndicator;

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.4f, 0.3f, 0.2f);
        [SerializeField] private Color plantedColor = new Color(0.3f, 0.5f, 0.2f);
        [SerializeField] private Color readyColor = new Color(0.8f, 0.7f, 0.2f);

        public event Action<int> OnPlotClicked;

        public int PlotId { get; private set; }

        private FarmPlotInstance _plot;
        private FarmPlotData _plotData;

        private void Awake()
        {
            if (plotButton != null)
                plotButton.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (plotButton != null)
                plotButton.onClick.RemoveListener(HandleClick);
        }

        private void Update()
        {
            if (_plot != null && _plotData != null && !FarmSystem.IsReady(_plot, _plotData))
            {
                UpdateGrowthProgress();
            }
        }

        public void Setup(FarmPlotInstance plot, FarmPlotData data)
        {
            PlotId = plot.plotId;
            _plot = plot;
            _plotData = data;

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_plot == null || _plotData == null) return;

            bool isReady = FarmSystem.IsReady(_plot, _plotData);

            // Set color based on state
            if (plotImage != null)
            {
                plotImage.color = isReady ? readyColor : plantedColor;
            }

            // Show crop icon
            if (cropIcon != null)
            {
                cropIcon.gameObject.SetActive(true);
            }

            // Status text
            if (statusText != null)
            {
                if (isReady)
                    statusText.text = "Ready!";
                else
                    statusText.text = _plotData.displayName;
            }

            // Ready indicator
            if (readyIndicator != null)
                readyIndicator.SetActive(isReady);

            // Growth bar
            if (growthBar != null)
            {
                growthBar.gameObject.SetActive(!isReady);
                if (!isReady)
                {
                    UpdateGrowthProgress();
                }
            }
        }

        private void UpdateGrowthProgress()
        {
            if (_plot == null || _plotData == null) return;

            double elapsed = _plot.GetElapsedSeconds();
            double total = _plotData.growTimeSeconds;
            float progress = Mathf.Clamp01((float)(elapsed / total));

            if (growthBar != null)
                growthBar.SetProgress(progress);

            // Update timer text
            if (timerText != null)
            {
                double remaining = FarmSystem.GetTimeRemaining(_plot, _plotData);
                timerText.text = FormatTime((float)remaining);
            }

            // Check if just became ready
            if (FarmSystem.IsReady(_plot, _plotData))
            {
                RefreshDisplay();
            }
        }

        private string FormatTime(float totalSeconds)
        {
            int minutes = (int)(totalSeconds / 60);
            int seconds = (int)(totalSeconds % 60);
            return $"{minutes}:{seconds:D2}";
        }

        private void HandleClick()
        {
            OnPlotClicked?.Invoke(PlotId);
        }
    }
}
