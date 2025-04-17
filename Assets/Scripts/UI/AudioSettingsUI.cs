using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Audio;

namespace REcreationOfSpace.UI
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Volume Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider ambienceVolumeSlider;
        [SerializeField] private Slider effectsVolumeSlider;
        [SerializeField] private Slider uiVolumeSlider;
        [SerializeField] private Slider characterVolumeSlider;
        [SerializeField] private Slider divineVolumeSlider;

        [Header("Volume Text")]
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private TextMeshProUGUI ambienceVolumeText;
        [SerializeField] private TextMeshProUGUI effectsVolumeText;
        [SerializeField] private TextMeshProUGUI uiVolumeText;
        [SerializeField] private TextMeshProUGUI characterVolumeText;
        [SerializeField] private TextMeshProUGUI divineVolumeText;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        [Header("Additional Settings")]
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private Toggle autoMuteOnFocusLostToggle;
        [SerializeField] private Slider reverbSlider;
        [SerializeField] private TextMeshProUGUI reverbText;

        private AudioManager audioManager;
        private float[] previousVolumes = new float[7]; // Store volumes for mute/unmute
        private bool wasMuted = false;

        private void Start()
        {
            audioManager = AudioManager.Instance;
            if (audioManager == null)
            {
                Debug.LogError("No AudioManager found!");
                return;
            }

            InitializeUI();
            LoadSavedSettings();
            settingsPanel.SetActive(false);
        }

        private void InitializeUI()
        {
            // Setup volume sliders
            SetupVolumeSlider(masterVolumeSlider, masterVolumeText, "MasterVolume");
            SetupVolumeSlider(musicVolumeSlider, musicVolumeText, "MusicVolume");
            SetupVolumeSlider(ambienceVolumeSlider, ambienceVolumeText, "AmbienceVolume");
            SetupVolumeSlider(effectsVolumeSlider, effectsVolumeText, "EffectsVolume");
            SetupVolumeSlider(uiVolumeSlider, uiVolumeText, "UIVolume");
            SetupVolumeSlider(characterVolumeSlider, characterVolumeText, "CharacterVolume");
            SetupVolumeSlider(divineVolumeSlider, divineVolumeText, "DivineVolume");

            // Setup reverb slider
            if (reverbSlider != null)
            {
                reverbSlider.onValueChanged.AddListener(OnReverbChanged);
            }

            // Setup toggles
            if (muteToggle != null)
            {
                muteToggle.onValueChanged.AddListener(OnMuteToggled);
            }

            if (autoMuteOnFocusLostToggle != null)
            {
                autoMuteOnFocusLostToggle.onValueChanged.AddListener(OnAutoMuteToggled);
            }

            // Setup close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => settingsPanel.SetActive(false));
            }
        }

        private void SetupVolumeSlider(Slider slider, TextMeshProUGUI text, string parameter)
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener((value) => OnVolumeChanged(value, parameter, text));
            }
        }

        private void OnVolumeChanged(float value, string parameter, TextMeshProUGUI text)
        {
            audioManager.SetVolume(parameter, value);
            if (text != null)
            {
                text.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
            SaveSettings();
        }

        private void OnReverbChanged(float value)
        {
            audioManager.SetEnvironmentReverb(value > 0.5f);
            if (reverbText != null)
            {
                reverbText.text = $"Reverb: {(value > 0.5f ? "Indoor" : "Outdoor")}";
            }
            SaveSettings();
        }

        private void OnMuteToggled(bool isMuted)
        {
            if (isMuted && !wasMuted)
            {
                // Store current volumes
                StoreCurrentVolumes();
                // Mute all channels
                SetAllVolumes(0f);
                wasMuted = true;
            }
            else if (!isMuted && wasMuted)
            {
                // Restore previous volumes
                RestorePreviousVolumes();
                wasMuted = false;
            }
            SaveSettings();
        }

        private void OnAutoMuteToggled(bool enabled)
        {
            PlayerPrefs.SetInt("AutoMuteOnFocusLost", enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void StoreCurrentVolumes()
        {
            previousVolumes[0] = masterVolumeSlider.value;
            previousVolumes[1] = musicVolumeSlider.value;
            previousVolumes[2] = ambienceVolumeSlider.value;
            previousVolumes[3] = effectsVolumeSlider.value;
            previousVolumes[4] = uiVolumeSlider.value;
            previousVolumes[5] = characterVolumeSlider.value;
            previousVolumes[6] = divineVolumeSlider.value;
        }

        private void SetAllVolumes(float volume)
        {
            masterVolumeSlider.value = volume;
            musicVolumeSlider.value = volume;
            ambienceVolumeSlider.value = volume;
            effectsVolumeSlider.value = volume;
            uiVolumeSlider.value = volume;
            characterVolumeSlider.value = volume;
            divineVolumeSlider.value = volume;
        }

        private void RestorePreviousVolumes()
        {
            masterVolumeSlider.value = previousVolumes[0];
            musicVolumeSlider.value = previousVolumes[1];
            ambienceVolumeSlider.value = previousVolumes[2];
            effectsVolumeSlider.value = previousVolumes[3];
            uiVolumeSlider.value = previousVolumes[4];
            characterVolumeSlider.value = previousVolumes[5];
            divineVolumeSlider.value = previousVolumes[6];
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            PlayerPrefs.SetFloat("AmbienceVolume", ambienceVolumeSlider.value);
            PlayerPrefs.SetFloat("EffectsVolume", effectsVolumeSlider.value);
            PlayerPrefs.SetFloat("UIVolume", uiVolumeSlider.value);
            PlayerPrefs.SetFloat("CharacterVolume", characterVolumeSlider.value);
            PlayerPrefs.SetFloat("DivineVolume", divineVolumeSlider.value);
            PlayerPrefs.SetFloat("ReverbAmount", reverbSlider.value);
            PlayerPrefs.SetInt("Muted", muteToggle.isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadSavedSettings()
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            ambienceVolumeSlider.value = PlayerPrefs.GetFloat("AmbienceVolume", 1f);
            effectsVolumeSlider.value = PlayerPrefs.GetFloat("EffectsVolume", 1f);
            uiVolumeSlider.value = PlayerPrefs.GetFloat("UIVolume", 1f);
            characterVolumeSlider.value = PlayerPrefs.GetFloat("CharacterVolume", 1f);
            divineVolumeSlider.value = PlayerPrefs.GetFloat("DivineVolume", 1f);
            reverbSlider.value = PlayerPrefs.GetFloat("ReverbAmount", 0f);
            muteToggle.isOn = PlayerPrefs.GetInt("Muted", 0) == 1;
            autoMuteOnFocusLostToggle.isOn = PlayerPrefs.GetInt("AutoMuteOnFocusLost", 1) == 1;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
        }

        public void TogglePanel()
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (autoMuteOnFocusLostToggle.isOn)
            {
                if (!hasFocus && !wasMuted)
                {
                    StoreCurrentVolumes();
                    SetAllVolumes(0f);
                    wasMuted = true;
                }
                else if (hasFocus && wasMuted)
                {
                    RestorePreviousVolumes();
                    wasMuted = false;
                }
            }
        }
    }
}
