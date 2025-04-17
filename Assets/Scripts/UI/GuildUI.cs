using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Social;
using System.Collections.Generic;
using System.Linq;

namespace REcreationOfSpace.UI
{
    public class GuildUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private KeyCode toggleKey = KeyCode.G;
        [SerializeField] private TabGroup tabGroup;

        [Header("Guild List")]
        [SerializeField] private RectTransform guildListContainer;
        [SerializeField] private GameObject guildListItemPrefab;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button createGuildButton;

        [Header("Guild Details")]
        [SerializeField] private GameObject guildDetailsPanel;
        [SerializeField] private Image guildEmblem;
        [SerializeField] private TextMeshProUGUI guildNameText;
        [SerializeField] private TextMeshProUGUI guildDescriptionText;
        [SerializeField] private TextMeshProUGUI guildTypeText;
        [SerializeField] private TextMeshProUGUI memberCountText;
        [SerializeField] private TextMeshProUGUI treasuryText;
        [SerializeField] private Button joinLeaveButton;
        [SerializeField] private Button manageButton;

        [Header("Member List")]
        [SerializeField] private RectTransform memberListContainer;
        [SerializeField] private GameObject memberListItemPrefab;
        [SerializeField] private TMP_Dropdown memberFilterDropdown;
        [SerializeField] private Button promoteButton;
        [SerializeField] private Button demoteButton;
        [SerializeField] private Button kickButton;

        [Header("Events Panel")]
        [SerializeField] private RectTransform eventsContainer;
        [SerializeField] private GameObject eventItemPrefab;
        [SerializeField] private Button createEventButton;
        [SerializeField] private GameObject eventCreationPanel;

        [Header("Treasury Panel")]
        [SerializeField] private TextMeshProUGUI totalFundsText;
        [SerializeField] private TextMeshProUGUI dailyUpkeepText;
        [SerializeField] private TMP_InputField contributionInput;
        [SerializeField] private Button contributeButton;
        [SerializeField] private RectTransform contributionHistoryContainer;
        [SerializeField] private GameObject contributionItemPrefab;

        [Header("Achievement Panel")]
        [SerializeField] private RectTransform achievementsContainer;
        [SerializeField] private GameObject achievementItemPrefab;
        [SerializeField] private TextMeshProUGUI totalAchievementsText;

        [Header("Create Guild Panel")]
        [SerializeField] private GameObject createGuildPanel;
        [SerializeField] private TMP_InputField guildNameInput;
        [SerializeField] private TMP_InputField guildDescriptionInput;
        [SerializeField] private TMP_Dropdown guildTypeDropdown;
        [SerializeField] private Button confirmCreateButton;
        [SerializeField] private Button cancelCreateButton;

        private GuildSystem guildSystem;
        private Dictionary<string, GameObject> guildListItems = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> memberListItems = new Dictionary<string, GameObject>();
        private GuildSystem.Guild selectedGuild;
        private string selectedMemberID;
        private float updateTimer;

        private void Start()
        {
            guildSystem = FindObjectOfType<GuildSystem>();
            if (guildSystem == null)
            {
                Debug.LogError("No GuildSystem found!");
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
                    "All Guilds",
                    "My Guilds",
                    "Recruiting",
                    "Combat Guilds",
                    "Crafting Guilds",
                    "Trading Guilds",
                    "Farming Guilds",
                    "Social Guilds",
                    "Adventure Guilds",
                    "Research Guilds",
                    "Spiritual Guilds"
                });
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            if (memberFilterDropdown != null)
            {
                memberFilterDropdown.ClearOptions();
                memberFilterDropdown.AddOptions(new List<string> {
                    "All Members",
                    "Online Members",
                    "Officers",
                    "Regular Members",
                    "Initiates"
                });
                memberFilterDropdown.onValueChanged.AddListener(OnMemberFilterChanged);
            }

            if (guildTypeDropdown != null)
            {
                guildTypeDropdown.ClearOptions();
                guildTypeDropdown.AddOptions(System.Enum.GetNames(typeof(GuildSystem.GuildType)).ToList());
            }

            if (createGuildButton != null)
                createGuildButton.onClick.AddListener(ShowCreateGuildPanel);

            if (confirmCreateButton != null)
                confirmCreateButton.onClick.AddListener(OnCreateGuildConfirmed);

            if (cancelCreateButton != null)
                cancelCreateButton.onClick.AddListener(() => createGuildPanel.SetActive(false));

            if (joinLeaveButton != null)
                joinLeaveButton.onClick.AddListener(OnJoinLeaveClicked);

            if (manageButton != null)
                manageButton.onClick.AddListener(OnManageClicked);

            if (promoteButton != null)
                promoteButton.onClick.AddListener(OnPromoteClicked);

            if (demoteButton != null)
                demoteButton.onClick.AddListener(OnDemoteClicked);

            if (kickButton != null)
                kickButton.onClick.AddListener(OnKickClicked);

            if (createEventButton != null)
                createEventButton.onClick.AddListener(ShowEventCreationPanel);

            if (contributeButton != null)
                contributeButton.onClick.AddListener(OnContributeClicked);

            if (searchInput != null)
                searchInput.onValueChanged.AddListener(OnSearchChanged);
        }

        private void UpdateUI()
        {
            UpdateGuildList();
            if (selectedGuild != null)
            {
                UpdateGuildDetails(selectedGuild);
                UpdateMemberList(selectedGuild);
                UpdateEventsList(selectedGuild);
                UpdateTreasuryPanel(selectedGuild);
                UpdateAchievements(selectedGuild);
            }
        }

        private void UpdateGuildList()
        {
            // Clear existing items
            foreach (var item in guildListItems.Values)
            {
                Destroy(item);
            }
            guildListItems.Clear();

            // Get filtered guilds
            var guilds = GetFilteredGuilds();

            // Apply search filter if needed
            if (!string.IsNullOrEmpty(searchInput?.text))
            {
                string search = searchInput.text.ToLower();
                guilds = guilds.Where(g => 
                    g.name.ToLower().Contains(search) || 
                    g.description.ToLower().Contains(search)).ToList();
            }

            // Create new items
            foreach (var guild in guilds)
            {
                CreateGuildListItem(guild);
            }
        }

        private List<GuildSystem.Guild> GetFilteredGuilds()
        {
            if (filterDropdown == null)
                return guildSystem.GetRecruitingGuilds();

            switch (filterDropdown.value)
            {
                case 0: // All Guilds
                    return guildSystem.GetRecruitingGuilds();
                case 1: // My Guilds
                    return guildSystem.GetPlayerGuilds("player"); // Replace with actual player ID
                case 2: // Recruiting
                    return guildSystem.GetRecruitingGuilds();
                default: // Guild Types
                    var type = (GuildSystem.GuildType)(filterDropdown.value - 3);
                    return guildSystem.GetGuildsByType(type);
            }
        }

        private void CreateGuildListItem(GuildSystem.Guild guild)
        {
            GameObject item = Instantiate(guildListItemPrefab, guildListContainer);
            guildListItems[guild.id] = item;

            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var typeText = item.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
            var memberText = item.transform.Find("MemberText")?.GetComponent<TextMeshProUGUI>();
            var statusText = item.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            var viewButton = item.GetComponentInChildren<Button>();

            if (nameText != null) nameText.text = guild.name;
            if (typeText != null) typeText.text = guild.type.ToString();
            if (memberText != null) memberText.text = $"Members: {guild.memberIDs.Count}/{guild.maxMembers}";
            if (statusText != null) statusText.text = guild.isRecruiting ? "Recruiting" : "Closed";

            if (viewButton != null)
            {
                viewButton.onClick.AddListener(() => SelectGuild(guild));
            }
        }

        private void SelectGuild(GuildSystem.Guild guild)
        {
            selectedGuild = guild;
            guildDetailsPanel.SetActive(true);
            UpdateGuildDetails(guild);
        }

        private void UpdateGuildDetails(GuildSystem.Guild guild)
        {
            guildNameText.text = guild.name;
            guildDescriptionText.text = guild.description;
            guildTypeText.text = guild.type.ToString();
            memberCountText.text = $"Members: {guild.memberIDs.Count}/{guild.maxMembers}";
            treasuryText.text = $"Treasury: ${guild.treasury:N0}";

            bool isMember = guild.memberIDs.Contains("player"); // Replace with actual player ID
            joinLeaveButton.GetComponentInChildren<TextMeshProUGUI>().text = isMember ? "Leave Guild" : "Join Guild";
            manageButton.gameObject.SetActive(isMember && guild.memberRanks["player"].level >= 3); // Officers and above
        }

        private void UpdateMemberList(GuildSystem.Guild guild)
        {
            // Clear existing items
            foreach (var item in memberListItems.Values)
            {
                Destroy(item);
            }
            memberListItems.Clear();

            foreach (var memberId in guild.memberIDs)
            {
                if (ShouldShowMember(guild, memberId))
                {
                    CreateMemberListItem(guild, memberId);
                }
            }

            // Update button states
            bool canManageMembers = guild.memberRanks["player"].canPromote; // Replace with actual player ID
            promoteButton.interactable = canManageMembers && selectedMemberID != null;
            demoteButton.interactable = canManageMembers && selectedMemberID != null;
            kickButton.interactable = canManageMembers && selectedMemberID != null;
        }

        private bool ShouldShowMember(GuildSystem.Guild guild, string memberId)
        {
            if (memberFilterDropdown == null) return true;

            var rank = guild.memberRanks[memberId];
            switch (memberFilterDropdown.value)
            {
                case 1: // Online Members
                    return true; // Implement online check
                case 2: // Officers
                    return rank.level >= 3;
                case 3: // Regular Members
                    return rank.level == 2;
                case 4: // Initiates
                    return rank.level == 1;
                default:
                    return true;
            }
        }

        private void CreateMemberListItem(GuildSystem.Guild guild, string memberId)
        {
            GameObject item = Instantiate(memberListItemPrefab, memberListContainer);
            memberListItems[memberId] = item;

            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var rankText = item.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            var contributionText = item.transform.Find("ContributionText")?.GetComponent<TextMeshProUGUI>();
            var selectButton = item.GetComponent<Button>();

            var rank = guild.memberRanks[memberId];
            if (nameText != null) nameText.text = memberId; // Replace with actual player name
            if (rankText != null) rankText.text = rank.name;
            if (contributionText != null)
            {
                float contribution = guild.memberContributions.ContainsKey(memberId) ? 
                    guild.memberContributions[memberId] : 0f;
                contributionText.text = $"${contribution:N0}";
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => SelectMember(memberId));
            }
        }

        private void SelectMember(string memberId)
        {
            selectedMemberID = memberId;
            UpdateMemberList(selectedGuild);
        }

        private void UpdateEventsList(GuildSystem.Guild guild)
        {
            foreach (Transform child in eventsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var guildEvent in guild.events.Where(e => e.isActive))
            {
                GameObject eventItem = Instantiate(eventItemPrefab, eventsContainer);
                
                var nameText = eventItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var timeText = eventItem.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
                var participantsText = eventItem.transform.Find("ParticipantsText")?.GetComponent<TextMeshProUGUI>();
                var joinButton = eventItem.GetComponentInChildren<Button>();

                if (nameText != null) nameText.text = guildEvent.name;
                if (timeText != null) timeText.text = $"{guildEvent.startTime:g} - {guildEvent.endTime:g}";
                if (participantsText != null) 
                    participantsText.text = $"Participants: {guildEvent.participants.Count}";

                if (joinButton != null)
                {
                    bool isParticipating = guildEvent.participants.Contains("player"); // Replace with actual player ID
                    joinButton.GetComponentInChildren<TextMeshProUGUI>().text = 
                        isParticipating ? "Leave Event" : "Join Event";
                    joinButton.onClick.AddListener(() => ToggleEventParticipation(guildEvent));
                }
            }

            createEventButton.gameObject.SetActive(
                guild.memberRanks["player"].canOrganizeEvents); // Replace with actual player ID
        }

        private void UpdateTreasuryPanel(GuildSystem.Guild guild)
        {
            totalFundsText.text = $"Total Funds: ${guild.treasury:N0}";
            dailyUpkeepText.text = $"Daily Upkeep: ${guild.treasury * 0.01f:N0}";

            foreach (Transform child in contributionHistoryContainer)
            {
                Destroy(child.gameObject);
            }

            var topContributors = guild.memberContributions
                .OrderByDescending(kvp => kvp.value)
                .Take(5);

            foreach (var contributor in topContributors)
            {
                GameObject item = Instantiate(contributionItemPrefab, contributionHistoryContainer);
                var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var amountText = item.transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();

                if (nameText != null) nameText.text = contributor.Key; // Replace with actual player name
                if (amountText != null) amountText.text = $"${contributor.Value:N0}";
            }
        }

        private void UpdateAchievements(GuildSystem.Guild guild)
        {
            foreach (Transform child in achievementsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var achievement in guild.achievements)
            {
                GameObject item = Instantiate(achievementItemPrefab, achievementsContainer);
                
                var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var descText = item.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
                var progressBar = item.GetComponentInChildren<Slider>();
                var progressText = item.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();

                if (nameText != null) nameText.text = achievement.name;
                if (descText != null) descText.text = achievement.description;
                if (progressBar != null)
                {
                    progressBar.value = achievement.progress / achievement.goal;
                    progressBar.interactable = false;
                }
                if (progressText != null)
                    progressText.text = $"{achievement.progress:N0}/{achievement.goal:N0}";
            }

            if (totalAchievementsText != null)
            {
                int completed = guild.achievements.Count(a => a.isCompleted);
                totalAchievementsText.text = $"Achievements: {completed}/{guild.achievements.Count}";
            }
        }

        private void ShowCreateGuildPanel()
        {
            createGuildPanel.SetActive(true);
            guildNameInput.text = "";
            guildDescriptionInput.text = "";
            guildTypeDropdown.value = 0;
        }

        private void OnCreateGuildConfirmed()
        {
            if (string.IsNullOrEmpty(guildNameInput.text))
            {
                Debug.LogWarning("Guild name cannot be empty");
                return;
            }

            var type = (GuildSystem.GuildType)guildTypeDropdown.value;
            if (guildSystem.CreateGuild("player", guildNameInput.text, guildDescriptionInput.text, type)) // Replace with actual player ID
            {
                createGuildPanel.SetActive(false);
                UpdateUI();
            }
        }

        private void OnJoinLeaveClicked()
        {
            if (selectedGuild == null) return;

            bool isMember = selectedGuild.memberIDs.Contains("player"); // Replace with actual player ID
            if (isMember)
            {
                if (guildSystem.LeaveGuild("player", selectedGuild.id))
                {
                    UpdateUI();
                }
            }
            else
            {
                if (guildSystem.JoinGuild("player", selectedGuild.id))
                {
                    UpdateUI();
                }
            }
        }

        private void OnManageClicked()
        {
            // Implement guild management panel
        }

        private void OnPromoteClicked()
        {
            if (selectedGuild == null || selectedMemberID == null) return;

            var currentRank = selectedGuild.memberRanks[selectedMemberID];
            string newRank = "";

            switch (currentRank.name)
            {
                case "Initiate": newRank = "Member"; break;
                case "Member": newRank = "Officer"; break;
            }

            if (!string.IsNullOrEmpty(newRank))
            {
                if (guildSystem.PromoteMember("player", selectedMemberID, selectedGuild.id, newRank)) // Replace with actual player ID
                {
                    UpdateUI();
                }
            }
        }

        private void OnDemoteClicked()
        {
            // Implement demotion logic
        }

        private void OnKickClicked()
        {
            if (selectedGuild == null || selectedMemberID == null) return;

            if (guildSystem.LeaveGuild(selectedMemberID, selectedGuild.id))
            {
                selectedMemberID = null;
                UpdateUI();
            }
        }

        private void ShowEventCreationPanel()
        {
            eventCreationPanel.SetActive(true);
            // Initialize event creation form
        }

        private void OnContributeClicked()
        {
            if (selectedGuild == null) return;

            float amount;
            if (!float.TryParse(contributionInput.text, out amount))
            {
                Debug.LogWarning("Invalid contribution amount");
                return;
            }

            if (guildSystem.ContributeToTreasury("player", selectedGuild.id, amount)) // Replace with actual player ID
            {
                contributionInput.text = "";
                UpdateUI();
            }
        }

        private void ToggleEventParticipation(GuildSystem.GuildEvent guildEvent)
        {
            if (guildEvent.participants.Contains("player")) // Replace with actual player ID
            {
                guildEvent.participants.Remove("player");
            }
            else
            {
                guildEvent.participants.Add("player");
            }
            UpdateEventsList(selectedGuild);
        }

        private void OnFilterChanged(int index)
        {
            UpdateGuildList();
        }

        private void OnMemberFilterChanged(int index)
        {
            if (selectedGuild != null)
            {
                UpdateMemberList(selectedGuild);
            }
        }

        private void OnSearchChanged(string search)
        {
            UpdateGuildList();
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

            if (memberFilterDropdown != null)
                memberFilterDropdown.onValueChanged.RemoveListener(OnMemberFilterChanged);

            if (searchInput != null)
                searchInput.onValueChanged.RemoveListener(OnSearchChanged);

            if (createGuildButton != null)
                createGuildButton.onClick.RemoveListener(ShowCreateGuildPanel);

            if (confirmCreateButton != null)
                confirmCreateButton.onClick.RemoveListener(OnCreateGuildConfirmed);

            if (cancelCreateButton != null)
                cancelCreateButton.onClick.RemoveListener(() => createGuildPanel.SetActive(false));

            if (joinLeaveButton != null)
                joinLeaveButton.onClick.RemoveListener(OnJoinLeaveClicked);

            if (manageButton != null)
                manageButton.onClick.RemoveListener(OnManageClicked);

            if (promoteButton != null)
                promoteButton.onClick.RemoveListener(OnPromoteClicked);

            if (demoteButton != null)
                demoteButton.onClick.RemoveListener(OnDemoteClicked);

            if (kickButton != null)
                kickButton.onClick.RemoveListener(OnKickClicked);

            if (createEventButton != null)
                createEventButton.onClick.RemoveListener(ShowEventCreationPanel);

            if (contributeButton != null)
                contributeButton.onClick.RemoveListener(OnContributeClicked);
        }
    }
}
