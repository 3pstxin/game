using System;
using UnityEngine;

namespace IdleViking.UI
{
    /// <summary>
    /// Base class for modal popup dialogs.
    /// Supports result callbacks and stacking via UIManager.
    /// </summary>
    public abstract class BasePopup : MonoBehaviour
    {
        [SerializeField] protected GameObject content;
        [SerializeField] protected GameObject backdrop;

        public bool IsVisible { get; private set; }

        protected Action onClose;
        protected Action<bool> onResult;

        protected virtual void Awake()
        {
            if (content == null)
                content = gameObject;
        }

        /// <summary>
        /// Show the popup with an optional close callback.
        /// </summary>
        public virtual void Show(Action onCloseCallback = null)
        {
            onClose = onCloseCallback;
            onResult = null;

            if (backdrop != null)
                backdrop.SetActive(true);
            content.SetActive(true);
            IsVisible = true;

            OnShow();
        }

        /// <summary>
        /// Show the popup with a result callback (for confirm dialogs).
        /// </summary>
        public virtual void ShowWithResult(Action<bool> resultCallback)
        {
            onResult = resultCallback;
            onClose = null;

            if (backdrop != null)
                backdrop.SetActive(true);
            content.SetActive(true);
            IsVisible = true;

            OnShow();
        }

        /// <summary>
        /// Close the popup.
        /// </summary>
        public virtual void Close()
        {
            if (backdrop != null)
                backdrop.SetActive(false);
            content.SetActive(false);
            IsVisible = false;

            OnClose();
            onClose?.Invoke();
        }

        /// <summary>
        /// Close with a result (for confirm dialogs).
        /// </summary>
        protected void CloseWithResult(bool result)
        {
            if (backdrop != null)
                backdrop.SetActive(false);
            content.SetActive(false);
            IsVisible = false;

            OnClose();
            onResult?.Invoke(result);
        }

        protected virtual void OnShow() { }
        protected virtual void OnClose() { }

        /// <summary>
        /// Called when backdrop is clicked. Default: close popup.
        /// </summary>
        public virtual void OnBackdropClick()
        {
            Close();
        }
    }
}
