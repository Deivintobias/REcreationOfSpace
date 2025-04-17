using UnityEngine;
using UnityEngine.SceneManagement;

namespace REcreationOfSpace.Setup
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Game Setup")]
        [SerializeField] private GameObject gameSetupPrefab;
        [SerializeField] private GameObject topDownGameSetupPrefab;
        [SerializeField] private GameObject safeModeManagerPrefab;
        [SerializeField] private GameObject debugManagerPrefab;

        private SafeModeManager safeModeManager;
        private DebugManager debugManager;

        [Header("UI Setup")]
        [SerializeField] private GameObject characterMenuPrefab;
        [SerializeField] private GameObject gameMenuPrefab;
        [SerializeField] private GameObject timelineUIPrefab;

        private void Awake()
        {
            // Initialize debug and safe mode systems first
            InitializeDebugSystems();

            // Only proceed with game setup if not in safe mode diagnostic screen
            if (safeModeManager == null || !safeModeManager.IsInSafeMode())
            {
                InitializeGame();
            }
        }

        private void InitializeDebugSystems()
        {
            // Create SafeModeManager
            if (safeModeManagerPrefab != null)
            {
                var safeMode = Instantiate(safeModeManagerPrefab);
                safeModeManager = safeMode.GetComponent<SafeModeManager>();
            }
            else
            {
                var safeMode = new GameObject("SafeModeManager");
                safeModeManager = safeMode.AddComponent<SafeModeManager>();
            }

            // Create DebugManager
            if (debugManagerPrefab != null)
            {
                var debug = Instantiate(debugManagerPrefab);
                debugManager = debug.GetComponent<DebugManager>();
            }
            else
            {
                var debug = new GameObject("DebugManager");
                debugManager = debug.AddComponent<DebugManager>();
                debug.AddComponent<DebugCommands>();
            }

            // Don't destroy these objects when loading new scenes
            DontDestroyOnLoad(safeModeManager.gameObject);
            DontDestroyOnLoad(debugManager.gameObject);
        }

        private void InitializeGame()
        {
            // Create game setup
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                if (gameSetupPrefab == null)
                {
                    var setupObj = new GameObject("GameSetup");
                    setupObj.AddComponent<GameSetup>();
                }
                else
                {
                    Instantiate(gameSetupPrefab);
                }

                if (topDownGameSetupPrefab == null)
                {
                    var setupObj = new GameObject("TopDownGameSetup");
                    setupObj.AddComponent<TopDownGameSetup>();
                }
                else
                {
                    Instantiate(topDownGameSetupPrefab);
                }
            }

            // Create UI
            CreateMenus();
            CreateTimelineUI();
        }

        private void CreateMenus()
        {
            // Create character menu
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                GameObject characterMenu;
                if (characterMenuPrefab != null)
                {
                    characterMenu = Instantiate(characterMenuPrefab);
                }
                else
                {
                    characterMenu = PrefabCreator.CreateCharacterMenuPrefab();
                }
                characterMenu.name = "CharacterMenu";
                characterMenu.SetActive(false); // Hide initially
            }

            // Create game menu
            GameObject gameMenu;
            if (gameMenuPrefab != null)
            {
                gameMenu = Instantiate(gameMenuPrefab);
            }
            else
            {
                gameMenu = PrefabCreator.CreateGameMenuPrefab();
            }
            gameMenu.name = "GameMenu";

            // Set up menu references
            var menus = FindObjectsOfType<Canvas>();
            foreach (var menu in menus)
            {
                // Ensure proper sorting order
                switch (menu.gameObject.name)
                {
                    case "CharacterMenu":
                        menu.sortingOrder = 1;
                        break;
                    case "GameMenu":
                        menu.sortingOrder = 2;
                        break;
                    default:
                        menu.sortingOrder = 0;
                        break;
                }
            }

            // Initialize default menu state
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                var gameMenuComponent = gameMenu.GetComponent<GameMenu>();
                if (gameMenuComponent != null)
                {
                    gameMenuComponent.ShowMainMenu();
                }
            }
            else
            {
                var gameMenuComponent = gameMenu.GetComponent<GameMenu>();
                if (gameMenuComponent != null)
                {
                    gameMenuComponent.HidePauseMenu();
                }
            }
        }

        private void CreateTimelineUI()
        {
            GameObject timelineUI;
            if (timelineUIPrefab != null)
            {
                timelineUI = Instantiate(timelineUIPrefab);
            }
            else
            {
                timelineUI = PrefabCreator.CreateTimelineUIPrefab();
            }
            timelineUI.name = "TimelineUI";

            // Set sorting order
            var canvas = timelineUI.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 3; // Above other menus
            }

            timelineUI.SetActive(false); // Hide initially
        }

        private void OnDestroy()
        {
            // Clean up any temporary objects or references if needed
        }
    }
}
