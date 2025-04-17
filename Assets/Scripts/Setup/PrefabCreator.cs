using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace REcreationOfSpace.Setup
{
    public class PrefabCreator : MonoBehaviour
    {
        // ... (keep existing methods) ...

        public static GameObject CreateCharacterMenuPrefab()
        {
            var menuObj = new GameObject("CharacterMenu");
            var menu = menuObj.AddComponent<CharacterMenu>();
            var canvas = menuObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            menuObj.AddComponent<CanvasScaler>();
            menuObj.AddComponent<GraphicRaycaster>();

            // Create panels
            var statsPanel = CreatePanel(menuObj, "StatsPanel");
            var skillsPanel = CreatePanel(menuObj, "SkillsPanel");
            var equipmentPanel = CreatePanel(menuObj, "EquipmentPanel");
            var classPanel = CreatePanel(menuObj, "ClassPanel");

            // Create stats display
            var statsContainer = CreateVerticalLayoutGroup(statsPanel, "StatsContainer");
            CreateStatText(statsContainer, "Level");
            CreateStatText(statsContainer, "Experience");
            CreateStatText(statsContainer, "Health");
            CreateStatText(statsContainer, "Damage");
            CreateStatText(statsContainer, "CraftingLevel");
            CreateStatText(statsContainer, "FarmingLevel");

            // Create equipment slots
            var equipmentContainer = CreateGridLayoutGroup(equipmentPanel, "EquipmentContainer");
            CreateEquipmentSlot(equipmentContainer, "WeaponSlot");
            CreateEquipmentSlot(equipmentContainer, "ArmorSlot");
            CreateEquipmentSlot(equipmentContainer, "AccessorySlot");
            CreateEquipmentSlot(equipmentContainer, "ToolSlot");

            // Create class selection
            var classContainer = CreateVerticalLayoutGroup(classPanel, "ClassContainer");

            return menuObj;
        }

        public static GameObject CreateGameMenuPrefab()
        {
            var menuObj = new GameObject("GameMenu");
            var menu = menuObj.AddComponent<GameMenu>();
            var canvas = menuObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            menuObj.AddComponent<CanvasScaler>();
            menuObj.AddComponent<GraphicRaycaster>();

            // Create panels
            var mainMenuPanel = CreatePanel(menuObj, "MainMenuPanel");
            var pauseMenuPanel = CreatePanel(menuObj, "PauseMenuPanel");
            var settingsPanel = CreatePanel(menuObj, "SettingsPanel");
            var saveLoadPanel = CreatePanel(menuObj, "SaveLoadPanel");

            // Main menu buttons
            var mainMenuContainer = CreateVerticalLayoutGroup(mainMenuPanel, "MainMenuContainer");
            CreateMenuButton(mainMenuContainer, "New Game", "StartNewGame");
            CreateMenuButton(mainMenuContainer, "Load Game", "ShowSaveLoad");
            CreateMenuButton(mainMenuContainer, "Settings", "ShowSettings");
            CreateMenuButton(mainMenuContainer, "Quit", "QuitGame");

            // Pause menu buttons
            var pauseMenuContainer = CreateVerticalLayoutGroup(pauseMenuPanel, "PauseMenuContainer");
            CreateMenuButton(pauseMenuContainer, "Resume", "HidePauseMenu");
            CreateMenuButton(pauseMenuContainer, "Save Game", "ShowSaveLoad");
            CreateMenuButton(pauseMenuContainer, "Settings", "ShowSettings");
            CreateMenuButton(pauseMenuContainer, "Quit to Menu", "QuitToMainMenu");

            // Settings controls
            var settingsContainer = CreateVerticalLayoutGroup(settingsPanel, "SettingsContainer");
            CreateSliderSetting(settingsContainer, "Music Volume");
            CreateSliderSetting(settingsContainer, "SFX Volume");
            CreateToggleSetting(settingsContainer, "Fullscreen");
            CreateDropdownSetting(settingsContainer, "Resolution");
            CreateDropdownSetting(settingsContainer, "Quality");

            // Save/Load slots
            var saveLoadContainer = CreateVerticalLayoutGroup(saveLoadPanel, "SaveLoadContainer");
            var saveSlotPrefab = CreateSaveSlotPrefab();

            return menuObj;
        }

        private static GameObject CreatePanel(GameObject parent, string name)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return panel;
        }

        private static GameObject CreateVerticalLayoutGroup(GameObject parent, string name)
        {
            var container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);
            var layout = container.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            return container;
        }

        private static GameObject CreateGridLayoutGroup(GameObject parent, string name)
        {
            var container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);
            var layout = container.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(80, 80);
            layout.spacing = new Vector2(10, 10);
            layout.padding = new RectOffset(10, 10, 10, 10);
            return container;
        }

        private static void CreateStatText(GameObject parent, string statName)
        {
            var textObj = new GameObject(statName + "Text");
            textObj.transform.SetParent(parent.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = statName + ": 0";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Left;
        }

        private static void CreateEquipmentSlot(GameObject parent, string slotName)
        {
            var slot = new GameObject(slotName);
            slot.transform.SetParent(parent.transform, false);
            var image = slot.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        private static void CreateMenuButton(GameObject parent, string text, string functionName)
        {
            var buttonObj = new GameObject(text + "Button");
            buttonObj.transform.SetParent(parent.transform, false);
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;

            var rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 50);
        }

        private static void CreateSliderSetting(GameObject parent, string label)
        {
            var container = new GameObject(label + "Container");
            container.transform.SetParent(parent.transform, false);

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;

            var sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(container.transform, false);
            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
        }

        private static void CreateToggleSetting(GameObject parent, string label)
        {
            var container = new GameObject(label + "Container");
            container.transform.SetParent(parent.transform, false);

            var toggle = container.AddComponent<Toggle>();
            var background = new GameObject("Background");
            background.transform.SetParent(container.transform, false);
            background.AddComponent<Image>();

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
        }

        private static void CreateDropdownSetting(GameObject parent, string label)
        {
            var container = new GameObject(label + "Container");
            container.transform.SetParent(parent.transform, false);

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;

            var dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(container.transform, false);
            var dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        }

        private static GameObject CreateSaveSlotPrefab()
        {
            var slot = new GameObject("SaveSlotPrefab");
            var container = CreateHorizontalLayoutGroup(slot, "Container");

            CreateMenuButton(container, "Save", "SaveGame");
            CreateMenuButton(container, "Load", "LoadGame");

            var infoText = new GameObject("InfoText");
            infoText.transform.SetParent(container.transform, false);
            var tmp = infoText.AddComponent<TextMeshProUGUI>();
            tmp.text = "Empty Slot";
            tmp.fontSize = 20;

            return slot;
        }

        public static GameObject CreateTimelineUIPrefab()
        {
            var timelineObj = new GameObject("TimelineUI");
            var timelineUI = timelineObj.AddComponent<TimelineUI>();
            var canvas = timelineObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            timelineObj.AddComponent<CanvasScaler>();
            timelineObj.AddComponent<GraphicRaycaster>();

            // Create main panel
            var panel = CreatePanel(timelineObj, "TimelinePanel");
            
            // Create header
            var header = CreateHorizontalLayoutGroup(panel, "Header");
            var eraText = CreateTextElement(header, "CurrentEra", "Current Era", 24);
            var yearText = CreateTextElement(header, "CurrentYear", "Year", 20);

            // Create navigation buttons
            var buttonContainer = CreateHorizontalLayoutGroup(panel, "Navigation");
            var prevButton = CreateUIButton(buttonContainer, "Previous Era");
            var nextButton = CreateUIButton(buttonContainer, "Next Era");

            // Create timeline container
            var scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(panel.transform, false);
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            viewport.AddComponent<RectMask2D>();
            
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentLayout = content.AddComponent<HorizontalLayoutGroup>();
            contentLayout.spacing = 100f;
            contentLayout.padding = new RectOffset(20, 20, 10, 10);

            // Set up scroll rect
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;

            // Create event marker prefab
            var markerPrefab = CreateTimelineMarkerPrefab();
            
            // Set references
            var timelineField = timelineUI.GetType().GetField("timelinePanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (timelineField != null)
                timelineField.SetValue(timelineUI, panel);

            var containerField = timelineUI.GetType().GetField("timelineContainer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (containerField != null)
                containerField.SetValue(timelineUI, content.GetComponent<RectTransform>());

            var eventPrefabField = timelineUI.GetType().GetField("eventPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (eventPrefabField != null)
                eventPrefabField.SetValue(timelineUI, markerPrefab);

            var eraTextField = timelineUI.GetType().GetField("currentEraText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (eraTextField != null)
                eraTextField.SetValue(timelineUI, eraText);

            var yearTextField = timelineUI.GetType().GetField("currentYearText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (yearTextField != null)
                yearTextField.SetValue(timelineUI, yearText);

            var prevButtonField = timelineUI.GetType().GetField("previousEraButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prevButtonField != null)
                prevButtonField.SetValue(timelineUI, prevButton.GetComponent<Button>());

            var nextButtonField = timelineUI.GetType().GetField("nextEraButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nextButtonField != null)
                nextButtonField.SetValue(timelineUI, nextButton.GetComponent<Button>());

            return timelineObj;
        }

        private static GameObject CreateTimelineMarkerPrefab()
        {
            var marker = new GameObject("TimelineMarker");
            var button = marker.AddComponent<Button>();
            var image = marker.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var layout = marker.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var text = CreateTextElement(marker, "EventText", "", 16);
            text.alignment = TextAlignmentOptions.Center;

            var rect = marker.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 80);

            return marker;
        }

        private static GameObject CreateHorizontalLayoutGroup(GameObject parent, string name)
        {
            var container = new GameObject(name);
            container.transform.SetParent(parent.transform, false);
            var layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            return container;
        }
    }
}
