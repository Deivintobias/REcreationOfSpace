using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Social;
using System.Collections.Generic;
using System.Linq;

namespace REcreationOfSpace.UI
{
    public class SocialUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private KeyCode toggleKey = KeyCode.R;
        [SerializeField] private TabGroup tabGroup;

        [Header("Relationships List")]
        [SerializeField] private RectTransform relationshipsContainer;
        [SerializeField] private GameObject relationshipPrefab;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Button refreshButton;

        [Header("Relationship Details")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Image relationshipIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI occupationText;
        [SerializeField] private TextMeshProUGUI personalityText;
        [SerializeField] private TextMeshProUGUI interestsText;
        [SerializeField] private Slider friendshipSlider;
        [SerializeField] private Slider trustSlider;
        [SerializeField] private Slider respectSlider;

        [Header("Interactions")]
        [SerializeField] private RectTransform interactionsContainer;
        [SerializeField] private GameObject interactionButtonPrefab;
        [SerializeField] private TextMeshProUGUI interactionCooldownText;
        [SerializeField] private TextMeshProUGUI lastInteractionText;

        [Header("Recent Events")]
        [SerializeField] private RectTransform eventsContainer;
        [SerializeField] private GameObject eventPrefab;
        [SerializeField] private int maxDisplayedEvents = 5;

        [Header("Icons")]
        [SerializeField] private Sprite strangerIcon;
        [SerializeField] private Sprite acquaintanceIcon;
        [SerializeField] private Sprite friendIcon;
        [SerializeField] private Sprite closeFriendIcon;
        [SerializeField] private Sprite bestFriendIcon;
        [SerializeField] private Sprite familyIcon;
        [SerializeField] private Sprite spouseIcon;
        [SerializeField] private Sprite professionalIcon;

        private SocialSystem socialSystem;
        private Dictionary<string, GameObject> relationshipItems = new Dictionary<string, GameObject>();
        private SocialSystem.Relationship selectedRelationship;
        private float updateTimer;

        private void Start()
        {
            socialSystem = FindObjectOfType<SocialSystem>();
            if (socialSystem == null)
            {
                Debug.LogError("No SocialSystem found!");
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
                    "All Relationships",
                    "Family Only",
                    "Friends Only",
                    "Coworkers Only",
                    "Neighbors Only",
                    "Professional Only"
                });
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(new List<string> {
                    "Name (A-Z)",
                    "Relationship Level (High-Low)",
                    "Recent Interaction",
                    "Type"
                });
                sortDropdown.onValueChanged.AddListener(OnSortChanged);
            }

            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshList);
        }

        private void UpdateUI()
        {
            UpdateRelationshipsList();
            if (selectedRelationship != null)
            {
                UpdateRelationshipDetails(selectedRelationship);
            }
        }

        private void UpdateRelationshipsList()
        {
            // Clear existing items
            foreach (var item in relationshipItems.Values)
            {
                Destroy(item);
            }
            relationshipItems.Clear();

            // Get and sort relationships
            var relationships = socialSystem.GetAllRelationships().Values.ToList();
            SortRelationships(relationships);

            // Create new items
            foreach (var relationship in relationships)
            {
                if (ShouldShowRelationship(relationship))
                {
                    CreateRelationshipItem(relationship);
                }
            }
        }

        private void SortRelationships(List<SocialSystem.Relationship> relationships)
        {
            switch (sortDropdown.value)
            {
                case 0: // Name
                    relationships.Sort((a, b) => a.name.CompareTo(b.name));
                    break;
                case 1: // Level
                    relationships.Sort((a, b) => 
                        ((b.friendshipLevel + b.trustLevel + b.respectLevel) / 3f)
                        .CompareTo((a.friendshipLevel + a.trustLevel + a.respectLevel) / 3f));
                    break;
                case 2: // Recent Interaction
                    relationships.Sort((a, b) => b.lastInteractionTime.CompareTo(a.lastInteractionTime));
                    break;
                case 3: // Type
                    relationships.Sort((a, b) => a.type.CompareTo(b.type));
                    break;
            }
        }

        private bool ShouldShowRelationship(SocialSystem.Relationship relationship)
        {
            switch (filterDropdown.value)
            {
                case 1: return relationship.isFamily;
                case 2: return relationship.type == SocialSystem.RelationType.Friend ||
                       relationship.type == SocialSystem.RelationType.CloseFriend ||
                       relationship.type == SocialSystem.RelationType.BestFriend;
                case 3: return relationship.isCoworker;
                case 4: return relationship.isNeighbor;
                case 5: return relationship.type == SocialSystem.RelationType.Professional;
                default: return true;
            }
        }

        private void CreateRelationshipItem(SocialSystem.Relationship relationship)
        {
            GameObject item = Instantiate(relationshipPrefab, relationshipsContainer);
            relationshipItems[relationship.id] = item;

            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var typeText = item.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
            var levelText = item.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            var icon = item.transform.Find("Icon")?.GetComponent<Image>();
            var viewButton = item.GetComponentInChildren<Button>();

            if (nameText != null) nameText.text = relationship.name;
            if (typeText != null) typeText.text = relationship.type.ToString();
            if (levelText != null)
            {
                float level = (relationship.friendshipLevel + relationship.trustLevel + relationship.respectLevel) / 3f;
                levelText.text = $"Level: {level:F0}";
            }
            if (icon != null) icon.sprite = GetRelationshipIcon(relationship.type);

            if (viewButton != null)
            {
                viewButton.onClick.AddListener(() => SelectRelationship(relationship));
            }
        }

        private void SelectRelationship(SocialSystem.Relationship relationship)
        {
            selectedRelationship = relationship;
            UpdateRelationshipDetails(relationship);
            detailsPanel.SetActive(true);
        }

        private void UpdateRelationshipDetails(SocialSystem.Relationship relationship)
        {
            nameText.text = relationship.name;
            typeText.text = relationship.type.ToString();
            occupationText.text = $"Occupation: {relationship.occupation}";
            personalityText.text = $"Personality: {relationship.personality}";
            interestsText.text = $"Interests: {string.Join(", ", relationship.sharedInterests)}";

            relationshipIcon.sprite = GetRelationshipIcon(relationship.type);

            friendshipSlider.value = relationship.friendshipLevel;
            trustSlider.value = relationship.trustLevel;
            respectSlider.value = relationship.respectLevel;

            UpdateInteractionButtons(relationship);
            UpdateRecentEvents(relationship);
        }

        private void UpdateInteractionButtons(SocialSystem.Relationship relationship)
        {
            // Clear existing buttons
            foreach (Transform child in interactionsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create interaction buttons
            foreach (string interaction in socialSystem.GetAvailableInteractions())
            {
                GameObject buttonObj = Instantiate(interactionButtonPrefab, interactionsContainer);
                var button = buttonObj.GetComponent<Button>();
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null) text.text = interaction;
                if (button != null)
                {
                    button.onClick.AddListener(() => OnInteractionClicked(relationship.id, interaction));
                }
            }

            // Update last interaction time
            float timeSinceLastInteraction = Time.time - relationship.lastInteractionTime;
            lastInteractionText.text = $"Last Interaction: {FormatTime(timeSinceLastInteraction)} ago";
        }

        private void UpdateRecentEvents(SocialSystem.Relationship relationship)
        {
            // Clear existing events
            foreach (Transform child in eventsContainer)
            {
                Destroy(child.gameObject);
            }

            // Display recent interactions
            var recentInteractions = relationship.recentInteractions
                .OrderByDescending(i => i.time)
                .Take(maxDisplayedEvents);

            foreach (var interaction in recentInteractions)
            {
                GameObject eventObj = Instantiate(eventPrefab, eventsContainer);
                var text = eventObj.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{interaction.description} ({FormatTime(Time.time - interaction.time)} ago)";
                    text.color = interaction.wasPositive ? Color.green : Color.red;
                }
            }
        }

        private string FormatTime(float seconds)
        {
            if (seconds < 60) return "Just now";
            if (seconds < 3600) return $"{Mathf.Floor(seconds / 60)}m";
            if (seconds < 86400) return $"{Mathf.Floor(seconds / 3600)}h";
            return $"{Mathf.Floor(seconds / 86400)}d";
        }

        private Sprite GetRelationshipIcon(SocialSystem.RelationType type)
        {
            switch (type)
            {
                case SocialSystem.RelationType.Stranger: return strangerIcon;
                case SocialSystem.RelationType.Acquaintance: return acquaintanceIcon;
                case SocialSystem.RelationType.Friend: return friendIcon;
                case SocialSystem.RelationType.CloseFriend: return closeFriendIcon;
                case SocialSystem.RelationType.BestFriend: return bestFriendIcon;
                case SocialSystem.RelationType.Family: return familyIcon;
                case SocialSystem.RelationType.Spouse: return spouseIcon;
                case SocialSystem.RelationType.Professional: return professionalIcon;
                default: return strangerIcon;
            }
        }

        private void OnInteractionClicked(string relationshipId, string interactionType)
        {
            if (socialSystem.TryInteract(relationshipId, interactionType))
            {
                UpdateUI();
            }
        }

        private void OnFilterChanged(int index)
        {
            RefreshList();
        }

        private void OnSortChanged(int index)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            UpdateRelationshipsList();
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

            if (sortDropdown != null)
                sortDropdown.onValueChanged.RemoveListener(OnSortChanged);

            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(RefreshList);
        }
    }
}
