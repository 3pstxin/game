using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleViking.UI
{
    /// <summary>
    /// Modal confirmation dialog with Yes/No buttons.
    /// </summary>
    public class ConfirmPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        protected override void Awake()
        {
            base.Awake();

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmClicked);
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        /// <summary>
        /// Set up the popup content.
        /// </summary>
        public void Setup(string title, string message, string confirmText = "Yes", string cancelText = "No")
        {
            if (titleText != null)
                titleText.text = title;
            if (messageText != null)
                messageText.text = message;
            if (confirmButtonText != null)
                confirmButtonText.text = confirmText;
            if (cancelButtonText != null)
                cancelButtonText.text = cancelText;
        }

        private void OnConfirmClicked()
        {
            CloseWithResult(true);
        }

        private void OnCancelClicked()
        {
            CloseWithResult(false);
        }

        public override void OnBackdropClick()
        {
            // Treat backdrop click as cancel
            CloseWithResult(false);
        }
    }
}
