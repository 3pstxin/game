using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleViking.UI
{
    /// <summary>
    /// Brief notification that auto-dismisses.
    /// Slides in, stays briefly, then fades out.
    /// </summary>
    public class ToastNotification : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        [Header("Animation Settings")]
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float slideDistance = 50f;

        private Vector2 _targetPosition;
        private Vector2 _startPosition;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Show the toast with a message. Auto-destroys when done.
        /// </summary>
        public void Show(string message)
        {
            if (messageText != null)
                messageText.text = message;

            StartCoroutine(AnimateToast());
        }

        private IEnumerator AnimateToast()
        {
            // Setup initial state
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (rectTransform != null)
            {
                _targetPosition = rectTransform.anchoredPosition;
                _startPosition = _targetPosition + Vector2.down * slideDistance;
                rectTransform.anchoredPosition = _startPosition;
            }

            // Fade in + slide up
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                float eased = EaseOutCubic(t);

                if (canvasGroup != null)
                    canvasGroup.alpha = eased;
                if (rectTransform != null)
                    rectTransform.anchoredPosition = Vector2.Lerp(_startPosition, _targetPosition, eased);

                yield return null;
            }

            // Ensure final state
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
            if (rectTransform != null)
                rectTransform.anchoredPosition = _targetPosition;

            // Wait
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);

                if (canvasGroup != null)
                    canvasGroup.alpha = 1f - t;

                yield return null;
            }

            // Clean up
            Destroy(gameObject);
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
