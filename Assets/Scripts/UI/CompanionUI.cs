using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Companion;
using System.Collections.Generic;
using System.Linq;

namespace REcreationOfSpace.UI
{
    public class CompanionUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private KeyCode toggleKey = KeyCode.P;
        [SerializeField] private TabGroup tabGroup;

        [Header("Companion List")]
        [SerializeField] private RectTransform companionListContainer;
        [SerializeField] private GameObject companionListItemPrefab;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TMP_InputField searchInput;

        [Header("Companion Details")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Image companionIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider loyaltyBar;
        [SerializeField] private Slider happinessBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Slider experienceBar;
        [SerializeField] private Button activateButton;
        [SerializeField] private Button mountButton;

        [Header("Stats Panel")]
        [SerializeField] private RectTransform statsContainer;
        [SerializeField] private GameObject statItemPrefab;

        [Header("Abilities Panel")]
        [SerializeField] private RectTransform abilitiesContainer;
        [SerializeField] private GameObject abilityItemPrefab;

        [Header("Training Panel")]
        [SerializeField] private Slider trainingProgressBar;
        [SerializeField] private Button trainButton;
        [SerializeField] private TextMeshProUGUI trainingTimeText;
        [SerializeField] private Slider trainingDurationSlider;

        [Header("Care Panel")]
        [SerializeField] private Button feedButton;
        [SerializeField] private TextMeshProUGUI lastFedText;
        [SerializeField] private TextMeshProUGUI lastRestText;
        [SerializeField] private Slider feedAmountSlider;

        private CompanionSystem companionSystem;
        private Dictionary<string, GameObject> companionListItems = new Dictionary<string, GameObject>();
        private CompanionSystem.Companion selectedCompanion;
        private float updateTimer;

        private void Start()
        {
            companionSystem = FindObjectOfType<CompanionSystem>();
            if (companionSystem == null)
            {
                Debug.LogError("No CompanionSystem found!");
                return;
            }

            InitializeUI();
            UpdateUI();
            mainPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }

            if (mainPanel.activeSelf)
            {
                updateTimer += Time.deltaTime;
                if (updateTimer >= 1f)
                {
                    UpdateUI();
                    updateTimer = 0f;
                }
            }
        }

        private void InitializeUI()
        {
            if (filterDropdown != null)
            {
                filterDropdown.ClearOptions();
                filterDropdown.AddOptions(new List<string> {
                    "All Companions",
                    "Active Companions",
                    "Mounts",
                    "Combat Pets",
                    "Utility Pets",
                    "Special Companions"
                });
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            if (activateButton != null)
                activateButton.onClick.AddListener(OnActivateClicked);

            if (mountButton != null)
                mountButton.onClick.AddListener(OnMountClicked);

            if (trainButton != null)
                trainButton.onClick.AddListener(OnTrainClicked);

            if (feedButton != null)
                feedButton.onClick.AddListener(OnFeedClicked);

            if (searchInput != null)
                searchInput.onValueChanged.AddListener(OnSearchChanged);

            if (trainingDurationSlider != null)
                trainingDurationSlider.onValueChanged.AddListener(OnTrainingDurationChanged);

            if (feedAmountSlider != null)
                feedAmountSlider.onValueChanged.AddListener(OnFeedAmountChanged);
        }

        private void UpdateUI()
        {
            UpdateCompanionList();
            if (selectedCompanion != null)
            {
                UpdateCompanionDetails(selectedCompanion);
                UpdateStatsPanel(selectedCompanion);
                UpdateAbilitiesPanel(selectedCompanion);
                UpdateTrainingPanel(selectedCompanion);
                UpdateCarePanel(selectedCompanion);
            }
        }

        private void UpdateCompanionList()
        {
            // Clear existing items
            foreach (var item in companionListItems.Values)
            {
                Destroy(item);
            }
            companionListItems.Clear();

            // Get filtered companions
            var companions = GetFilteredCompanions();

            // Apply search filter if needed
            if (!string.IsNullOrEmpty(searchInput?.text))
            {
                string search = searchInput.text.ToLower();
                companions = companions.Where(c => c.name.ToLower().Contains(search)).ToList();
            }

            // Create new items
            foreach (var companion in companions)
            {
                CreateCompanionListItem(companion);
            }
        }

        private List<CompanionSystem.Companion> GetFilteredCompanions()
        {
            if (filterDropdown == null)
                return companionSystem.GetActiveCompanions();

            switch (filterDropdown.value)
            {
                case 0: // All Companions
                    return companionSystem.GetActiveCompanions();
                case 1: // Active Companions
                    return companionSystem.GetActiveCompanions();
                case 2: // Mounts
                    return companionSystem.GetCompanionsByCategory(CompanionSystem.CompanionCategory.Mount);
                case 3: // Combat Pets
                    return companionSystem.GetCompanionsByCategory(CompanionSystem.CompanionCategory.Combat);
                case 4: // Utility Pets
                    return companionSystem.GetCompanionsByCategory(CompanionSystem.CompanionCategory.Utility);
                case 5: // Special Companions
                    return companionSystem.GetCompanionsByCategory(CompanionSystem.CompanionCategory.Special);
                default:
                    return companionSystem.GetActiveCompanions();
            }
        }

        private void CreateCompanionListItem(CompanionSystem.Companion companion)
        {
            GameObject item = Instantiate(companionListItemPrefab, companionListContainer);
            companionListItems[companion.id] = item;

            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var typeText = item.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
            var levelText = item.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            var statusText = item.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            var selectButton = item.GetComponent<Button>();

            if (nameText != null) nameText.text = companion.name;
            if (typeText != null) typeText.text = companion.type.ToString();
            if (levelText != null) levelText.text = $"Level {companion.level}";
            if (statusText != null)
            {
                string status = companion.isActive ? (companion.isMounted ? "Mounted" : "Active") : "Resting";
                statusText.text = status;
                statusText.color = companion.isActive ? Color.green : Color.white;
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => SelectCompanion(companion));
            }
        }

        private void SelectCompanion(CompanionSystem.Companion companion)
        {
            selectedCompanion = companion;
            detailsPanel.SetActive(true);
            UpdateCompanionDetails(companion);
        }

        private void UpdateCompanionDetails(CompanionSystem.Companion companion)
        {
            nameText.text = companion.name;
            typeText.text = $"Type: {companion.type} ({companion.category})";
            levelText.text = $"Level {companion.level}";

            loyaltyBar.value = companion.loyalty / 100f;
            happinessBar.value = companion.happiness / 100f;
            energyBar.value = companion.energy / 100f;
            experienceBar.value = companion.experience / (100f * companion.level);

            activateButton.GetComponentInChildren<TextMeshProUGUI>().text = 
                companion.isActive ? "Deactivate" : "Activate";

            bool canMount = companion.category == CompanionSystem.CompanionCategory.Mount;
            mountButton.gameObject.SetActive(canMount);
            if (canMount)
            {
                mountButton.GetComponentInChildren<TextMeshProUGUI>().text = 
                    companion.isMounted ? "Dismount" : "Mount";
                mountButton.interactable = companion.energy >= 20f;
            }
        }

        private void UpdateStatsPanel(CompanionSystem.Companion companion)
        {
            foreach (Transform child in statsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var stat in companion.stats)
            {
                GameObject item = Instantiate(statItemPrefab, statsContainer);
                var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var valueText = item.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();

                if (nameText != null) nameText.text = stat.Key;
                if (valueText != null) valueText.text = $"{stat.Value:F1}";
            }
        }

        private void UpdateAbilitiesPanel(CompanionSystem.Companion companion)
        {
            foreach (Transform child in abilitiesContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var ability in companion.abilities)
            {
                GameObject item = Instantiate(abilityItemPrefab, abilitiesContainer);
                var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                
                if (nameText != null) nameText.text = ability;
            }
        }

        private void UpdateTrainingPanel(CompanionSystem.Companion companion)
        {
            trainingProgressBar.value = companion.trainingProgress / 100f;
            trainButton.interactable = companion.energy >= 30f;
            
            float remainingTrainingTime = 4f - trainingDurationSlider.value;
            trainingTimeText.text = $"Training Time: {trainingDurationSlider.value:F1} hours\nRemaining Today: {remainingTrainingTime:F1} hours";
        }

        private void UpdateCarePanel(CompanionSystem.Companion companion)
        {
            var timeSinceLastFed = System.DateTime.Now - companion.lastFed;
            var timeSinceLastRest = System.DateTime.Now - companion.lastRest;

            lastFedText.text = $"Last Fed: {(timeSinceLastFed.TotalHours < 1 ? 
                $"{timeSinceLastFed.TotalMinutes:F0} minutes ago" : 
                $"{timeSinceLastFed.TotalHours:F1} hours ago")}";

            lastRestText.text = $"Last Rest: {(timeSinceLastRest.TotalHours < 1 ? 
                $"{timeSinceLastRest.TotalMinutes:F0} minutes ago" : 
                $"{timeSinceLastRest.TotalHours:F1} hours ago")}";

            feedButton.interactable = timeSinceLastFed.TotalHours >= 4;
        }

        private void OnActivateClicked()
        {
            if (selectedCompanion == null) return;

            if (selectedCompanion.isActive)
            {
                companionSystem.DeactivateCompanion(selectedCompanion.id);
            }
            else
            {
                companionSystem.ActivateCompanion(selectedCompanion.id);
            }
            UpdateUI();
        }

        private void OnMountClicked()
        {
            if (selectedCompanion == null) return;

            if (selectedCompanion.isMounted)
            {
                companionSystem.DismountCompanion(selectedCompanion.id);
            }
            else
            {
                companionSystem.MountCompanion(selectedCompanion.id);
            }
            UpdateUI();
        }

        private void OnTrainClicked()
        {
            if (selectedCompanion == null) return;

            if (companionSystem.TrainCompanion(selectedCompanion.id, trainingDurationSlider.value))
            {
                UpdateUI();
            }
        }

        private void OnFeedClicked()
        {
            if (selectedCompanion == null) return;

            if (companionSystem.FeedCompanion(selectedCompanion.id, feedAmountSlider.value))
            {
                UpdateUI();
            }
        }

        private void OnFilterChanged(int index)
        {
            UpdateCompanionList();
        }

        private void OnSearchChanged(string search)
        {
            UpdateCompanionList();
        }

        private void OnTrainingDurationChanged(float value)
        {
            UpdateTrainingPanel(selectedCompanion);
        }

        private void OnFeedAmountChanged(float value)
        {
            // Update UI elements related to feeding amount if needed
        }

        public void TogglePanel()
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            if (mainPanel.activeSelf)
            {
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            if (filterDropdown != null)
                filterDropdown.onValueChanged.RemoveListener(OnFilterChanged);

            if (searchInput != null)
                searchInput.onValueChanged.RemoveListener(OnSearchChanged);

            if (activateButton != null)
                activateButton.onClick.RemoveListener(OnActivateClicked);

            if (mountButton != null)
                mountButton.onClick.RemoveListener(OnMountClicked);

            if (trainButton != null)
                trainButton.onClick.RemoveListener(OnTrainClicked);

            if (feedButton != null)
                feedButton.onClick.RemoveListener(OnFeedClicked);

            if (trainingDurationSlider != null)
                trainingDurationSlider.onValueChanged.RemoveListener(OnTrainingDurationChanged);

            if (feedAmountSlider != null)
                feedAmountSlider.onValueChanged.RemoveListener(OnFeedAmountChanged);
        }
    }
}
