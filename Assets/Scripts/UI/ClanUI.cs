using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Social;
using System.Collections.Generic;
using System.Linq;

namespace REcreationOfSpace.UI
{
    public class ClanUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private KeyCode toggleKey = KeyCode.C;
        [SerializeField] private TabGroup tabGroup;

        [Header("Clan List")]
        [SerializeField] private RectTransform clanListContainer;
        [SerializeField] private GameObject clanListItemPrefab;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button createClanButton;

        [Header("Clan Details")]
        [SerializeField] private GameObject clanDetailsPanel;
        [SerializeField] private Image clanBanner;
        [SerializeField] private TextMeshProUGUI clanNameText;
        [SerializeField] private TextMeshProUGUI clanDescriptionText;
        [SerializeField] private TextMeshProUGUI clanFocusText;
        [SerializeField] private TextMeshProUGUI memberCountText;
        [SerializeField] private TextMeshProUGUI influenceText;
        [SerializeField] private Button joinLeaveButton;
        [SerializeField] private Button manageButton;

        [Header("Member List")]
        [SerializeField] private RectTransform memberListContainer;
        [SerializeField] private GameObject memberListItemPrefab;
        [SerializeField] private TMP_Dropdown memberFilterDropdown;
        [SerializeField] private Button promoteButton;
        [SerializeField] private Button demoteButton;
        [SerializeField] private Button kickButton;

        [Header("Alliance Panel")]
        [SerializeField] private RectTransform allianceContainer;
        [SerializeField] private GameObject allianceItemPrefab;
        [SerializeField] private Button formAllianceButton;
        [SerializeField] private Button declareRivalButton;
        [SerializeField] private TextMeshProUGUI allianceCountText;

        [Header("Traditions Panel")]
        [SerializeField] private RectTransform traditionsContainer;
        [SerializeField] private GameObject traditionItemPrefab;
        [SerializeField] private Button addTraditionButton;
        [SerializeField] private TextMeshProUGUI traditionCountText;

        [Header("Honor Panel")]
        [SerializeField] private TextMeshProUGUI totalHonorText;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private Slider honorProgressBar;
        [SerializeField] private RectTransform honorHistoryContainer;
        [SerializeField] private GameObject honorHistoryItemPrefab;

        [Header("Create Clan Panel")]
        [SerializeField] private GameObject createClanPanel;
        [SerializeField] private TMP_InputField clanNameInput;
        [SerializeField] private TMP_InputField clanDescriptionInput;
        [SerializeField] private TMP_Dropdown clanFocusDropdown;
        [SerializeField] private Button confirmCreateButton;
        [SerializeField] private Button cancelCreateButton;

        private ClanSystem clanSystem;
        private Dictionary<string, GameObject> clanListItems = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> memberListItems = new Dictionary<string, GameObject>();
        private ClanSystem.Clan selectedClan;
        private string selectedMemberID;
        private float updateTimer;

        private void Start()
        {
            clanSystem = FindObjectOfType<ClanSystem>();
            if (clanSystem == null)
            {
                Debug.LogError("No ClanSystem found!");
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
                    "All Clans",
                    "My Clan",
                    "Open Clans",
                    "Warrior Clans",
                    "Mystic Clans",
                    "Merchant Clans",
                    "Scholar Clans",
                    "Guardian Clans",
                    "Artisan Clans",
                    "Explorer Clans",
                    "Diplomat Clans"
                });
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            if (memberFilterDropdown != null)
            {
                memberFilterDropdown.ClearOptions();
                memberFilterDropdown.AddOptions(new List<string> {
                    "All Members",
                    "Elders",
                    "Warriors",
                    "Initiates"
                });
                memberFilterDropdown.onValueChanged.AddListener(OnMemberFilterChanged);
            }

            if (clanFocusDropdown != null)
            {
                clanFocusDropdown.ClearOptions();
                clanFocusDropdown.AddOptions(System.Enum.GetNames(typeof(ClanSystem.ClanFocus)).ToList());
            }

            if (createClanButton != null)
                createClanButton.onClick.AddListener(ShowCreateClanPanel);

            if (confirmCreateButton != null)
                confirmCreateButton.onClick.AddListener(OnCreateClanConfirmed);

            if (cancelCreateButton != null)
                cancelCreateButton.onClick.AddListener(() => createClanPanel.SetActive(false));

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

            if (formAllianceButton != null)
                formAllianceButton.onClick.AddListener(ShowAllianceFormation);

            if (declareRivalButton != null)
                declareRivalButton.onClick.AddListener(ShowRivalDeclaration);

            if (addTraditionButton != null)
                addTraditionButton.onClick.AddListener(ShowTraditionCreation);

            if (searchInput != null)
                searchInput.onValueChanged.AddListener(OnSearchChanged);
        }

        private void UpdateUI()
        {
            UpdateClanList();
            if (selectedClan != null)
            {
                UpdateClanDetails(selectedClan);
                UpdateMemberList(selectedClan);
                UpdateAlliances(selectedClan);
                UpdateTraditions(selectedClan);
                UpdateHonorPanel(selectedClan);
            }
        }

        private void UpdateClanList()
        {
            // Clear existing items
            foreach (var item in clanListItems.Values)
            {
                Destroy(item);
            }
            clanListItems.Clear();

            // Get filtered clans
            var clans = GetFilteredClans();

            // Apply search filter if needed
            if (!string.IsNullOrEmpty(searchInput?.text))
            {
                string search = searchInput.text.ToLower();
                clans = clans.Where(c => 
                    c.name.ToLower().Contains(search) || 
                    c.description.ToLower().Contains(search)).ToList();
            }

            // Create new items
            foreach (var clan in clans)
            {
                CreateClanListItem(clan);
            }
        }

        private List<ClanSystem.Clan> GetFilteredClans()
        {
            if (filterDropdown == null)
                return clanSystem.GetOpenClans();

            switch (filterDropdown.value)
            {
                case 0: // All Clans
                    return clanSystem.GetOpenClans();
                case 1: // My Clan
                    return clanSystem.GetPlayerClans("player"); // Replace with actual player ID
                case 2: // Open Clans
                    return clanSystem.GetOpenClans();
                default: // Clan Focus Types
                    var focus = (ClanSystem.ClanFocus)(filterDropdown.value - 3);
                    return clanSystem.GetClansByFocus(focus);
            }
        }

        private void CreateClanListItem(ClanSystem.Clan clan)
        {
            GameObject item = Instantiate(clanListItemPrefab, clanListContainer);
            clanListItems[clan.id] = item;

            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var focusText = item.transform.Find("FocusText")?.GetComponent<TextMeshProUGUI>();
            var memberText = item.transform.Find("MemberText")?.GetComponent<TextMeshProUGUI>();
            var statusText = item.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            var viewButton = item.GetComponentInChildren<Button>();

            if (nameText != null) nameText.text = clan.name;
            if (focusText != null) focusText.text = clan.focus.ToString();
            if (memberText != null) memberText.text = $"Members: {clan.memberIDs.Count}/{clan.maxMembers}";
            if (statusText != null) statusText.text = clan.isOpen ? "Open" : "Closed";

            if (viewButton != null)
            {
                viewButton.onClick.AddListener(() => SelectClan(clan));
            }
        }

        private void SelectClan(ClanSystem.Clan clan)
        {
            selectedClan = clan;
            clanDetailsPanel.SetActive(true);
            UpdateClanDetails(clan);
        }

        private void UpdateClanDetails(ClanSystem.Clan clan)
        {
            clanNameText.text = clan.name;
            clanDescriptionText.text = clan.description;
            clanFocusText.text = clan.focus.ToString();
            memberCountText.text = $"Members: {clan.memberIDs.Count}/{clan.maxMembers}";
            influenceText.text = $"Influence: {clan.influence:F0}";

            bool isMember = clan.memberIDs.Contains("player"); // Replace with actual player ID
            joinLeaveButton.GetComponentInChildren<TextMeshProUGUI>().text = isMember ? "Leave Clan" : "Join Clan";
            manageButton.gameObject.SetActive(isMember && clan.memberRoles["player"].tier >= 3); // Elders and above
        }

        private void UpdateMemberList(ClanSystem.Clan clan)
        {
            // Clear existing items
            foreach (var item in memberListItems.Values)
            {
                Destroy(item);
            }
            memberListItems.Clear();

            foreach (var memberId in clan.memberIDs)
            {
                if (ShouldShowMember(clan, memberId))
                {
                    CreateMemberListItem(clan, memberId);
                }
            }

            // Update button states
            bool canManageMembers = clan.memberRoles["player"].canPromote; // Replace with actual player ID
            promoteButton.interactable = canManageMembers && selectedMemberID != null;
            demoteButton.interactable = canManageMembers && selectedMemberID != null;
            kickButton.interactable = canManageMembers && selectedMemberID != null;
        }

        private bool ShouldShowMember(ClanSystem.Clan clan, string memberId)
        {
            if (memberFilterDropdown == null) return true;

            var role = clan.memberRoles[memberId];
            switch (memberFilterDropdown.value)
            {
                case 1: // Elders
                    return role.tier >= 3;
                case 2: // Warriors
                    return role.tier == 2;
                case 3: // Initiates
                    return role.tier == 1;
                default:
                    return true;
            }
        }

        private void CreateMemberListItem(ClanSystem.Clan clan, string memberId)
        {
            GameObject item = Instantiate(memberListItemPrefab, memberListContainer);
            memberListItems[memberId] = item;

            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var roleText = item.transform.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
            var honorText = item.transform.Find("HonorText")?.GetComponent<TextMeshProUGUI>();
            var selectButton = item.GetComponent<Button>();

            var role = clan.memberRoles[memberId];
            if (nameText != null) nameText.text = memberId; // Replace with actual player name
            if (roleText != null) roleText.text = role.name;
            if (honorText != null)
            {
                float honor = clan.memberHonor.ContainsKey(memberId) ? 
                    clan.memberHonor[memberId] : 0f;
                honorText.text = $"Honor: {honor:F0}";
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => SelectMember(memberId));
            }
        }

        private void UpdateAlliances(ClanSystem.Clan clan)
        {
            foreach (Transform child in allianceContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var alliance in clan.alliances.Where(a => a.isActive))
            {
                GameObject allianceItem = Instantiate(allianceItemPrefab, allianceContainer);
                
                var nameText = allianceItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var typeText = allianceItem.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
                var strengthText = allianceItem.transform.Find("StrengthText")?.GetComponent<TextMeshProUGUI>();
                var breakButton = allianceItem.GetComponentInChildren<Button>();

                var alliedClan = clanSystem.GetClan(alliance.clanID);
                if (nameText != null) nameText.text = alliedClan?.name ?? "Unknown Clan";
                if (typeText != null) typeText.text = alliance.type.ToString();
                if (strengthText != null) strengthText.text = $"Strength: {alliance.strength:F0}";

                if (breakButton != null && clan.memberRoles["player"].canManageAlliances) // Replace with actual player ID
                {
                    breakButton.gameObject.SetActive(true);
                    breakButton.onClick.AddListener(() => BreakAlliance(alliance));
                }
            }

            allianceCountText.text = $"Alliances: {clan.alliances.Count(a => a.isActive)}/{clanSystem.maxAlliances}";
            formAllianceButton.gameObject.SetActive(
                clan.memberRoles["player"].canManageAlliances); // Replace with actual player ID
        }

        private void UpdateTraditions(ClanSystem.Clan clan)
        {
            foreach (Transform child in traditionsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var tradition in clan.traditions.Where(t => t.isActive))
            {
                GameObject traditionItem = Instantiate(traditionItemPrefab, traditionsContainer);
                
                var nameText = traditionItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var descText = traditionItem.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
                var typeText = traditionItem.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
                var bonusText = traditionItem.transform.Find("BonusText")?.GetComponent<TextMeshProUGUI>();

                if (nameText != null) nameText.text = tradition.name;
                if (descText != null) descText.text = tradition.description;
                if (typeText != null) typeText.text = tradition.type.ToString();
                if (bonusText != null) bonusText.text = $"Honor Bonus: +{tradition.honorBonus:F1}";
            }

            traditionCountText.text = $"Active Traditions: {clan.traditions.Count(t => t.isActive)}";
            addTraditionButton.gameObject.SetActive(
                clan.memberRoles["player"].tier >= 3); // Elders and above
        }

        private void UpdateHonorPanel(ClanSystem.Clan clan)
        {
            float honor = clan.memberHonor.ContainsKey("player") ? // Replace with actual player ID
                clan.memberHonor["player"] : 0f;

            totalHonorText.text = $"Total Honor: {honor:F0}";
            
            string rank = "Initiate";
            if (honor >= 500f) rank = "Elder";
            else if (honor >= 100f) rank = "Warrior";
            rankText.text = $"Rank: {rank}";

            float nextRankHonor = rank == "Initiate" ? 100f : (rank == "Warrior" ? 500f : 1000f);
            honorProgressBar.value = honor / nextRankHonor;
        }

        private void SelectMember(string memberId)
        {
            selectedMemberID = memberId;
            UpdateMemberList(selectedClan);
        }

        private void ShowCreateClanPanel()
        {
            createClanPanel.SetActive(true);
            clanNameInput.text = "";
            clanDescriptionInput.text = "";
            clanFocusDropdown.value = 0;
        }

        private void OnCreateClanConfirmed()
        {
            if (string.IsNullOrEmpty(clanNameInput.text))
            {
                Debug.LogWarning("Clan name cannot be empty");
                return;
            }

            var focus = (ClanSystem.ClanFocus)clanFocusDropdown.value;
            if (clanSystem.CreateClan("player", clanNameInput.text, clanDescriptionInput.text, focus)) // Replace with actual player ID
            {
                createClanPanel.SetActive(false);
                UpdateUI();
            }
        }

        private void OnJoinLeaveClicked()
        {
            if (selectedClan == null) return;

            bool isMember = selectedClan.memberIDs.Contains("player"); // Replace with actual player ID
            if (isMember)
            {
                if (clanSystem.LeaveClan("player", selectedClan.id))
                {
                    UpdateUI();
                }
            }
            else
            {
                if (clanSystem.JoinClan("player", selectedClan.id))
                {
                    UpdateUI();
                }
            }
        }

        private void OnManageClicked()
        {
            // Implement clan management panel
        }

        private void OnPromoteClicked()
        {
            // Implement promotion logic
        }

        private void OnDemoteClicked()
        {
            // Implement demotion logic
        }

        private void OnKickClicked()
        {
            if (selectedClan == null || selectedMemberID == null) return;

            if (clanSystem.LeaveClan(selectedMemberID, selectedClan.id))
            {
                selectedMemberID = null;
                UpdateUI();
            }
        }

        private void ShowAllianceFormation()
        {
            // Implement alliance formation panel
        }

        private void ShowRivalDeclaration()
        {
            // Implement rival declaration panel
        }

        private void ShowTraditionCreation()
        {
            // Implement tradition creation panel
        }

        private void BreakAlliance(ClanSystem.ClanAlliance alliance)
        {
            alliance.isActive = false;
            UpdateUI();
        }

        private void OnFilterChanged(int index)
        {
            UpdateClanList();
        }

        private void OnMemberFilterChanged(int index)
        {
            if (selectedClan != null)
            {
                UpdateMemberList(selectedClan);
            }
        }

        private void OnSearchChanged(string search)
        {
            UpdateClanList();
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

            if (createClanButton != null)
                createClanButton.onClick.RemoveListener(ShowCreateClanPanel);

            if (confirmCreateButton != null)
                confirmCreateButton.onClick.RemoveListener(OnCreateClanConfirmed);

            if (cancelCreateButton != null)
                cancelCreateButton.onClick.RemoveListener(() => createClanPanel.SetActive(false));

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

            if (formAllianceButton != null)
                formAllianceButton.onClick.RemoveListener(ShowAllianceFormation);

            if (declareRivalButton != null)
                declareRivalButton.onClick.RemoveListener(ShowRivalDeclaration);

            if (addTraditionButton != null)
                addTraditionButton.onClick.RemoveListener(ShowTraditionCreation);
        }
    }
}
