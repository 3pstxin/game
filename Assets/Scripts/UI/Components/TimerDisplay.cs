using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleViking.UI
{
    /// <summary>
    /// Displays countdown or elapsed time.
    /// Can work with UTC timestamps or durations.
    /// </summary>
    public class TimerDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject readyIndicator;

        [Header("Settings")]
        [SerializeField] private bool countDown = true;
        [SerializeField] private bool autoHideWhenComplete = false;

        private long _startTimestamp;
        private long _endTimestamp;
        private float _durationSeconds;
        private bool _isActive;
        private Action _onComplete;

        public bool IsComplete => _isActive && GetRemainingSeconds() <= 0;

        private void Update()
        {
            if (!_isActive) return;

            float remaining = GetRemainingSeconds();

            if (remaining <= 0)
            {
                OnTimerComplete();
                return;
            }

            UpdateDisplay(remaining);
        }

        /// <summary>
        /// Start timer with a target UTC timestamp.
        /// </summary>
        public void StartTimer(long endTimestamp, Action onComplete = null)
        {
            _endTimestamp = endTimestamp;
            _startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _durationSeconds = _endTimestamp - _startTimestamp;
            _onComplete = onComplete;
            _isActive = true;

            if (readyIndicator != null)
                readyIndicator.SetActive(false);
        }

        /// <summary>
        /// Start timer with a duration in seconds.
        /// </summary>
        public void StartTimer(float durationSeconds, Action onComplete = null)
        {
            _startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _endTimestamp = _startTimestamp + (long)durationSeconds;
            _durationSeconds = durationSeconds;
            _onComplete = onComplete;
            _isActive = true;

            if (readyIndicator != null)
                readyIndicator.SetActive(false);
        }

        /// <summary>
        /// Set timer from existing state (e.g., after loading).
        /// </summary>
        public void SetFromState(long startTimestamp, float durationSeconds, Action onComplete = null)
        {
            _startTimestamp = startTimestamp;
            _durationSeconds = durationSeconds;
            _endTimestamp = _startTimestamp + (long)durationSeconds;
            _onComplete = onComplete;
            _isActive = true;

            // Check if already complete
            if (GetRemainingSeconds() <= 0)
            {
                OnTimerComplete();
            }
            else if (readyIndicator != null)
            {
                readyIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void StopTimer()
        {
            _isActive = false;
        }

        /// <summary>
        /// Get remaining time in seconds.
        /// </summary>
        public float GetRemainingSeconds()
        {
            if (!_isActive) return 0;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Mathf.Max(0, _endTimestamp - now);
        }

        /// <summary>
        /// Get elapsed time in seconds.
        /// </summary>
        public float GetElapsedSeconds()
        {
            if (!_isActive) return 0;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Mathf.Max(0, now - _startTimestamp);
        }

        /// <summary>
        /// Get progress as 0-1 value.
        /// </summary>
        public float GetProgress()
        {
            if (_durationSeconds <= 0) return 1f;
            return Mathf.Clamp01(GetElapsedSeconds() / _durationSeconds);
        }

        private void UpdateDisplay(float remainingSeconds)
        {
            if (timerText != null)
            {
                if (countDown)
                    timerText.text = FormatTime(remainingSeconds);
                else
                    timerText.text = FormatTime(GetElapsedSeconds());
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = GetProgress();
            }
        }

        private void OnTimerComplete()
        {
            _isActive = false;

            if (timerText != null)
                timerText.text = countDown ? "Ready!" : FormatTime(_durationSeconds);

            if (fillImage != null)
                fillImage.fillAmount = 1f;

            if (readyIndicator != null)
                readyIndicator.SetActive(true);

            if (autoHideWhenComplete)
                gameObject.SetActive(false);

            _onComplete?.Invoke();
        }

        /// <summary>
        /// Display a static time value (no countdown).
        /// </summary>
        public void SetStaticTime(float seconds)
        {
            _isActive = false;
            if (timerText != null)
                timerText.text = FormatTime(seconds);
        }

        private string FormatTime(float totalSeconds)
        {
            if (totalSeconds <= 0) return "0:00";

            int hours = (int)(totalSeconds / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            int seconds = (int)(totalSeconds % 60);

            if (hours > 0)
                return $"{hours}:{minutes:D2}:{seconds:D2}";
            else
                return $"{minutes}:{seconds:D2}";
        }
    }
}
