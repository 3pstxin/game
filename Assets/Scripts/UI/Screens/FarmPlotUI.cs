using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleViking.Data;
using IdleViking.Models;

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
        [SerializeField] private TimerDisplay timer;
        [SerializeField] private Button plotButton;
        [SerializeField] private GameObject readyIndicator;

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.4f, 0.3f, 0.2f);
        [SerializeField] private Color plantedColor = new Color(0.3f, 0.5f, 0.2f);
        [SerializeField] private Color readyColor = new Color(0.8f, 0.7f, 0.2f);

        public event Action<int> OnPlotClicked;

        public int PlotId { get; private set; }

        private PlotState _plotState;
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
            if (_plotState != null && _plotState.IsPlanted && !_plotState.IsReady())
            {
                UpdateGrowthProgress();
            }
        }

        public void Setup(int plotId, PlotState state, FarmPlotData data)
        {
            PlotId = plotId;
            _plotState = state;
            _plotData = data;

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_plotState == null) return;

            bool isPlanted = _plotState.IsPlanted;
            bool isReady = _plotState.IsReady();

            // Set color based on state
            if (plotImage != null)
            {
                if (isReady)
                    plotImage.color = readyColor;
                else if (isPlanted)
                    plotImage.color = plantedColor;
                else
                    plotImage.color = emptyColor;
            }

            // Show/hide crop icon
            if (cropIcon != null)
            {
                cropIcon.gameObject.SetActive(isPlanted);
            }

            // Status text
            if (statusText != null)
            {
                if (isReady)
                    statusText.text = "Ready!";
                else if (isPlanted)
                    statusText.text = _plotState.PlantedCropName ?? "Growing";
                else
                    statusText.text = "Empty";
            }

            // Ready indicator
            if (readyIndicator != null)
                readyIndicator.SetActive(isReady);

            // Growth bar
            if (growthBar != null)
            {
                growthBar.gameObject.SetActive(isPlanted && !isReady);
                if (isPlanted && !isReady)
                {
                    UpdateGrowthProgress();
                }
            }

            // Timer
            if (timer != null)
            {
                timer.gameObject.SetActive(isPlanted && !isReady);
                if (isPlanted && !isReady)
                {
                    long endTime = _plotState.PlantedTime + (long)_plotState.GrowthDuration;
                    timer.StartTimer(endTime);
                }
            }
        }

        private void UpdateGrowthProgress()
        {
            if (_plotState == null || !_plotState.IsPlanted) return;

            float progress = _plotState.GetGrowthProgress();

            if (growthBar != null)
                growthBar.SetProgress(progress);

            // Check if just became ready
            if (_plotState.IsReady())
            {
                RefreshDisplay();
            }
        }

        private void HandleClick()
        {
            OnPlotClicked?.Invoke(PlotId);
        }
    }
}
