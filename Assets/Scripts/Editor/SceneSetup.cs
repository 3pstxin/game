#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using IdleViking.UI;

namespace IdleViking.Editor
{
    /// <summary>
    /// Editor tool to automatically set up the game scene.
    /// Access via menu: IdleViking → Setup Scene
    /// </summary>
    public class SceneSetup : EditorWindow
    {
        [MenuItem("IdleViking/Setup Scene")]
        public static void SetupGameScene()
        {
            // Create GameManager
            var gameManagerObj = new GameObject("GameManager");
            var gameManager = gameManagerObj.AddComponent<GameManager>();
            Debug.Log("[Setup] Created GameManager");

            // Create EventSystem if none exists
            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Debug.Log("[Setup] Created EventSystem");
            }

            // Create Main Canvas
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasObj.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create UIManager
            var uiManagerObj = new GameObject("UIManager");
            uiManagerObj.transform.SetParent(canvasObj.transform, false);
            var uiManager = uiManagerObj.AddComponent<UIManager>();
            var uiRect = uiManagerObj.AddComponent<RectTransform>();
            uiRect.anchorMin = Vector2.zero;
            uiRect.anchorMax = Vector2.one;
            uiRect.sizeDelta = Vector2.zero;

            // Create Screens Container
            var screensObj = new GameObject("Screens");
            screensObj.transform.SetParent(uiManagerObj.transform, false);
            var screensRect = screensObj.AddComponent<RectTransform>();
            screensRect.anchorMin = Vector2.zero;
            screensRect.anchorMax = Vector2.one;
            screensRect.sizeDelta = Vector2.zero;

            // Create each screen
            CreateScreen<HomeScreen>(screensObj.transform, "HomeScreen", ScreenType.Home);
            CreateScreen<BuildingScreen>(screensObj.transform, "BuildingScreen", ScreenType.Buildings);
            CreateScreen<VikingScreen>(screensObj.transform, "VikingScreen", ScreenType.Vikings);
            CreateScreen<DungeonScreen>(screensObj.transform, "DungeonScreen", ScreenType.Dungeon);
            CreateScreen<FarmScreen>(screensObj.transform, "FarmScreen", ScreenType.Farm);
            CreateScreen<ProgressionScreen>(screensObj.transform, "ProgressionScreen", ScreenType.Progression);

            // Create Popup Container
            var popupContainer = new GameObject("PopupContainer");
            popupContainer.transform.SetParent(uiManagerObj.transform, false);
            var popupRect = popupContainer.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.sizeDelta = Vector2.zero;

            // Create TabBar
            var tabBarObj = new GameObject("TabBar");
            tabBarObj.transform.SetParent(uiManagerObj.transform, false);
            var tabBar = tabBarObj.AddComponent<TabBar>();
            var tabBarRect = tabBarObj.AddComponent<RectTransform>();
            tabBarRect.anchorMin = new Vector2(0, 0);
            tabBarRect.anchorMax = new Vector2(1, 0);
            tabBarRect.pivot = new Vector2(0.5f, 0);
            tabBarRect.sizeDelta = new Vector2(0, 150);
            var tabBarLayout = tabBarObj.AddComponent<HorizontalLayoutGroup>();
            tabBarLayout.childAlignment = TextAnchor.MiddleCenter;
            tabBarLayout.childForceExpandWidth = true;
            tabBarLayout.childForceExpandHeight = true;
            tabBarObj.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Create Tab Buttons
            CreateTabButton(tabBarObj.transform, "Home", ScreenType.Home);
            CreateTabButton(tabBarObj.transform, "Build", ScreenType.Buildings);
            CreateTabButton(tabBarObj.transform, "Vikings", ScreenType.Vikings);
            CreateTabButton(tabBarObj.transform, "Dungeon", ScreenType.Dungeon);
            CreateTabButton(tabBarObj.transform, "Farm", ScreenType.Farm);

            Debug.Log("[Setup] Scene setup complete! Now assign databases to GameManager.");

            Selection.activeGameObject = gameManagerObj;
            EditorUtility.DisplayDialog("Setup Complete",
                "Scene created successfully!\n\n" +
                "Next steps:\n" +
                "1. Run 'IdleViking → Create Sample Data'\n" +
                "2. Assign databases to GameManager\n" +
                "3. Hit Play!", "OK");
        }

        private static void CreateScreen<T>(Transform parent, string name, ScreenType type) where T : BaseScreen
        {
            var screenObj = new GameObject(name);
            screenObj.transform.SetParent(parent, false);
            var screen = screenObj.AddComponent<T>();
            var rect = screenObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // Add background
            var bg = screenObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.12f, 1f);

            // Add placeholder text
            var textObj = new GameObject("Placeholder");
            textObj.transform.SetParent(screenObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = name;
            text.fontSize = 48;
            text.alignment = TextAlignmentOptions.Center;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Set screen type via serialized field
            var so = new SerializedObject(screen);
            so.FindProperty("screenType").enumValueIndex = (int)type;
            so.FindProperty("content").objectReferenceValue = screenObj;
            so.ApplyModifiedProperties();

            Debug.Log($"[Setup] Created {name}");
        }

        private static void CreateTabButton(Transform parent, string label, ScreenType type)
        {
            var tabObj = new GameObject($"Tab_{label}");
            tabObj.transform.SetParent(parent, false);
            var tabButton = tabObj.AddComponent<TabButton>();
            tabObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var button = tabObj.AddComponent<Button>();

            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(tabObj.transform, false);
            var text = labelObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            // Set properties
            var so = new SerializedObject(tabButton);
            so.FindProperty("screenType").enumValueIndex = (int)type;
            so.FindProperty("button").objectReferenceValue = button;
            so.FindProperty("label").objectReferenceValue = text;
            so.ApplyModifiedProperties();
        }
    }
}
#endif
