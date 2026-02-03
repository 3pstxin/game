using UnityEngine;

namespace IdleViking.UI
{
    /// <summary>
    /// Base class for all full-screen UI panels.
    /// Handles show/hide lifecycle and event subscription.
    /// </summary>
    public abstract class BaseScreen : MonoBehaviour
    {
        [SerializeField] protected ScreenType screenType;
        [SerializeField] protected GameObject content;

        public ScreenType ScreenType => screenType;
        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            if (content == null)
                content = gameObject;
        }

        protected virtual void OnEnable()
        {
            SubscribeToEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Show this screen. Called by UIManager.
        /// </summary>
        public virtual void Show()
        {
            content.SetActive(true);
            IsVisible = true;
            Refresh();
            OnShow();
        }

        /// <summary>
        /// Hide this screen. Called by UIManager.
        /// </summary>
        public virtual void Hide()
        {
            content.SetActive(false);
            IsVisible = false;
            OnHide();
        }

        /// <summary>
        /// Refresh all displayed data. Call when state changes.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Override to run logic when screen becomes visible.
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// Override to run logic when screen is hidden.
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// Override to subscribe to UIEvents.
        /// </summary>
        protected virtual void SubscribeToEvents() { }

        /// <summary>
        /// Override to unsubscribe from UIEvents.
        /// </summary>
        protected virtual void UnsubscribeFromEvents() { }
    }
}
