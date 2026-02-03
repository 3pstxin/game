using System.Collections.Generic;
using UnityEngine;

namespace IdleViking.UI
{
    /// <summary>
    /// Singleton that manages screens, popups, and navigation.
    /// Attach to a root UI GameObject with all screens as children.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Screens")]
        [SerializeField] private List<BaseScreen> screens = new List<BaseScreen>();

        [Header("Popup Container")]
        [SerializeField] private Transform popupContainer;

        [Header("Prefabs")]
        [SerializeField] private ConfirmPopup confirmPopupPrefab;
        [SerializeField] private RewardPopup rewardPopupPrefab;
        [SerializeField] private ToastNotification toastPrefab;

        private BaseScreen _currentScreen;
        private Stack<BasePopup> _popupStack = new Stack<BasePopup>();
        private Dictionary<ScreenType, BaseScreen> _screenLookup;

        public BaseScreen CurrentScreen => _currentScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildScreenLookup();
            HideAllScreens();
        }

        private void Start()
        {
            // Show default screen
            ShowScreen(ScreenType.Home);

            // Subscribe to toast events
            UIEvents.OnToastMessage += ShowToast;
        }

        private void OnDestroy()
        {
            UIEvents.OnToastMessage -= ShowToast;
        }

        private void BuildScreenLookup()
        {
            _screenLookup = new Dictionary<ScreenType, BaseScreen>();
            foreach (var screen in screens)
            {
                if (screen != null)
                    _screenLookup[screen.ScreenType] = screen;
            }
        }

        private void HideAllScreens()
        {
            foreach (var screen in screens)
            {
                if (screen != null)
                    screen.Hide();
            }
        }

        /// <summary>
        /// Navigate to a screen by type.
        /// </summary>
        public void ShowScreen(ScreenType type)
        {
            if (!_screenLookup.TryGetValue(type, out BaseScreen screen))
            {
                Debug.LogWarning($"[UIManager] Screen not found: {type}");
                return;
            }

            if (_currentScreen != null)
                _currentScreen.Hide();

            _currentScreen = screen;
            _currentScreen.Show();
        }

        /// <summary>
        /// Refresh the current screen (e.g. after state change).
        /// </summary>
        public void RefreshCurrentScreen()
        {
            _currentScreen?.Refresh();
        }

        /// <summary>
        /// Show a popup. Pushes onto stack.
        /// </summary>
        public void ShowPopup(BasePopup popup)
        {
            _popupStack.Push(popup);
            popup.Show(() => OnPopupClosed(popup));
        }

        /// <summary>
        /// Close the topmost popup.
        /// </summary>
        public void CloseTopPopup()
        {
            if (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                popup.Close();
            }
        }

        /// <summary>
        /// Close all open popups.
        /// </summary>
        public void CloseAllPopups()
        {
            while (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                popup.Close();
            }
        }

        private void OnPopupClosed(BasePopup popup)
        {
            // Remove from stack if it's there (may have been closed by something else)
            // Stack doesn't support Remove, so we rebuild
            var temp = new Stack<BasePopup>();
            while (_popupStack.Count > 0)
            {
                var p = _popupStack.Pop();
                if (p != popup)
                    temp.Push(p);
            }
            while (temp.Count > 0)
                _popupStack.Push(temp.Pop());
        }

        /// <summary>
        /// Show a confirmation dialog.
        /// </summary>
        public void ShowConfirm(string title, string message, System.Action<bool> onResult)
        {
            if (confirmPopupPrefab == null)
            {
                Debug.LogWarning("[UIManager] ConfirmPopup prefab not assigned.");
                onResult?.Invoke(false);
                return;
            }

            var popup = Instantiate(confirmPopupPrefab, popupContainer);
            popup.Setup(title, message);
            popup.ShowWithResult(result =>
            {
                onResult?.Invoke(result);
                Destroy(popup.gameObject);
            });
            _popupStack.Push(popup);
        }

        /// <summary>
        /// Show a reward/loot popup.
        /// </summary>
        public void ShowReward(string title, List<string> rewards, System.Action onContinue = null)
        {
            if (rewardPopupPrefab == null)
            {
                Debug.LogWarning("[UIManager] RewardPopup prefab not assigned.");
                onContinue?.Invoke();
                return;
            }

            var popup = Instantiate(rewardPopupPrefab, popupContainer);
            popup.Setup(title, rewards);
            popup.Show(() =>
            {
                onContinue?.Invoke();
                Destroy(popup.gameObject);
            });
            _popupStack.Push(popup);
        }

        /// <summary>
        /// Show a brief toast notification.
        /// </summary>
        public void ShowToast(string message)
        {
            if (toastPrefab == null)
            {
                Debug.Log($"[Toast] {message}");
                return;
            }

            var toast = Instantiate(toastPrefab, popupContainer);
            toast.Show(message);
        }
    }
}
