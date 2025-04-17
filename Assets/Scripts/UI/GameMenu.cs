using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace REcreationOfSpace.UI
{
    public class GameMenu : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject saveLoadPanel;

        [Header("Settings")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;

        [Header("Save Slots")]
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private Transform saveSlotContainer;
        [SerializeField] private GameObject saveSlotPrefab;

        private PlayerController playerController;
        private CharacterMenu characterMenu;
        private Resolution[] resolutions;
        private bool isPaused = false;

        private void Start()
        {
            playerController = FindObjectOfType<PlayerController>();
            characterMenu = FindObjectOfType<CharacterMenu>();

            // Initialize settings
            InitializeSettings();
            CreateSaveSlots();

            // Show appropriate menu
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                ShowMainMenu();
            }
            else
            {
                HideAllMenus();
            }
        }

        private void Update()
        {
            // Handle pause menu
            if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name != "MainMenu")
            {
                TogglePauseMenu();
            }
        }

        private void InitializeSettings()
        {
            // Initialize resolution dropdown
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            
            int currentResolutionIndex = 0;
            var options = new System.Collections.Generic.List<string>();
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = $"{resolutions[i].width}x{resolutions[i].height}";
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();

            // Initialize quality dropdown
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();

            // Initialize other settings
            fullscreenToggle.isOn = Screen.fullScreen;
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

            // Add listeners
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            qualityDropdown.onValueChanged.AddListener(SetQuality);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        private void CreateSaveSlots()
        {
            if (saveSlotContainer == null || saveSlotPrefab == null)
                return;

            for (int i = 0; i < maxSaveSlots; i++)
            {
                var slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
                var slotNumber = i + 1;

                // Set up save button
                var saveButton = slotObj.transform.Find("SaveButton")?.GetComponent<Button>();
                if (saveButton != null)
                {
                    saveButton.onClick.AddListener(() => SaveGame(slotNumber));
                }

                // Set up load button
                var loadButton = slotObj.transform.Find("LoadButton")?.GetComponent<Button>();
                if (loadButton != null)
                {
                    loadButton.onClick.AddListener(() => LoadGame(slotNumber));
                    loadButton.interactable = DoesSaveExist(slotNumber);
                }
            }
        }

        public void ShowMainMenu()
        {
            HideAllMenus();
            mainMenuPanel.SetActive(true);
            Time.timeScale = 1f;
            isPaused = false;
        }

        public void ShowPauseMenu()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
                return;

            HideAllMenus();
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;

            if (playerController != null)
                playerController.ToggleCursor(true);
        }

        public void HidePauseMenu()
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;

            if (playerController != null)
                playerController.ToggleCursor(false);
        }

        public void TogglePauseMenu()
        {
            if (isPaused)
                HidePauseMenu();
            else
                ShowPauseMenu();
        }

        public void ShowSettings()
        {
            HideAllMenus();
            settingsPanel.SetActive(true);
        }

        public void ShowSaveLoad()
        {
            HideAllMenus();
            saveLoadPanel.SetActive(true);
        }

        private void HideAllMenus()
        {
            mainMenuPanel.SetActive(false);
            pauseMenuPanel.SetActive(false);
            settingsPanel.SetActive(false);
            saveLoadPanel.SetActive(false);

            if (characterMenu != null)
                characterMenu.Hide();
        }

        public void StartNewGame()
        {
            SceneManager.LoadScene("OutdoorsScene");
        }

        public void QuitToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void SaveGame(int slot)
        {
            // Save game state to PlayerPrefs or a file
            PlayerPrefs.SetInt($"SaveSlot_{slot}_Exists", 1);
            // Add more save data here
            PlayerPrefs.Save();
        }

        private void LoadGame(int slot)
        {
            if (!DoesSaveExist(slot))
                return;

            // Load game state
            SceneManager.LoadScene("OutdoorsScene");
            // Add more load logic here
        }

        private bool DoesSaveExist(int slot)
        {
            return PlayerPrefs.GetInt($"SaveSlot_{slot}_Exists", 0) == 1;
        }

        private void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        private void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }

        private void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        private void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat("MusicVolume", volume);
            // Update audio mixer here
        }

        private void SetSFXVolume(float volume)
        {
            PlayerPrefs.SetFloat("SFXVolume", volume);
            // Update audio mixer here
        }
    }
}
