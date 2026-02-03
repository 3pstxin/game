using System;
using UnityEngine;

namespace IdleViking.Core
{
    /// <summary>
    /// Tracks game time and calculates offline elapsed time.
    /// Used by idle systems to compute resource gains while the app was closed.
    /// </summary>
    public class TimeManager
    {
        // Cap offline gains to prevent absurd accumulation (8 hours default)
        public double MaxOfflineSeconds { get; set; } = 28800;

        private DateTime _sessionStartTime;
        private double _offlineSeconds;

        /// <summary>
        /// Seconds elapsed since the player last saved. Capped by MaxOfflineSeconds.
        /// Only valid after calling CalculateOfflineTime.
        /// </summary>
        public double OfflineSeconds => _offlineSeconds;

        /// <summary>
        /// Call once at game start with the timestamp from the last save.
        /// </summary>
        public void CalculateOfflineTime(DateTime lastSaveTime)
        {
            _sessionStartTime = DateTime.UtcNow;
            double elapsed = (_sessionStartTime - lastSaveTime).TotalSeconds;

            // Clamp to 0 in case of clock manipulation
            elapsed = Math.Max(0, elapsed);
            _offlineSeconds = Math.Min(elapsed, MaxOfflineSeconds);

            Debug.Log($"[TimeManager] Offline for {_offlineSeconds:F1}s (raw: {elapsed:F1}s, cap: {MaxOfflineSeconds}s)");
        }

        /// <summary>
        /// Returns the game tick delta. Uses Time.deltaTime for normal updates.
        /// For idle systems, multiply production rates by this value each frame.
        /// </summary>
        public float GetDeltaTime()
        {
            return Time.deltaTime;
        }
    }
}
