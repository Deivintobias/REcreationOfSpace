using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TeamUI : MonoBehaviour
{
    [System.Serializable]
    public class TeamMemberUI
    {
        public GameObject panel;
        public Text nameText;
        public Image trustBar;
        public Image contributionBar;
        public GameObject tradingIcon;
    }

    [Header("UI References")]
    public GameObject teamPanel;
    public GameObject memberPrefab;
    public Transform membersContainer;
    public Text teamStatusText;
    public Image teamBondBar;

    [Header("Visual Settings")]
    public Color lowTrustColor = Color.yellow;
    public Color highTrustColor = Color.green;
    public float updateInterval = 0.5f;

    private TeamSystem teamSystem;
    private TradingSystem tradingSystem;
    private Dictionary<GameObject, TeamMemberUI> memberUIs = new Dictionary<GameObject, TeamMemberUI>();
    private float nextUpdate;

    void Start()
    {
        teamSystem = GetComponent<TeamSystem>();
        tradingSystem = GetComponent<TradingSystem>();
        nextUpdate = Time.time;

        // Initially hide panel
        if (teamPanel != null)
        {
            teamPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Toggle UI visibility
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (teamPanel != null)
            {
                teamPanel.SetActive(!teamPanel.activeSelf);
            }
        }

        // Update UI at intervals
        if (Time.time >= nextUpdate)
        {
            UpdateTeamUI();
            nextUpdate = Time.time + updateInterval;
        }
    }

    void UpdateTeamUI()
    {
        if (teamSystem == null || teamPanel == null) return;

        bool isInTeam = teamSystem.IsInTeam();
        teamPanel.SetActive(isInTeam && teamPanel.activeSelf);

        if (!isInTeam) return;

        // Update team status
        if (teamStatusText != null)
        {
            string status = teamSystem.IsTeamLeader() ? "Team Leader" : "Team Member";
            teamStatusText.text = $"Status: {status}";
        }

        // Update member displays
        UpdateMemberDisplays();

        // Update team bond bar
        UpdateTeamBond();
    }

    void UpdateMemberDisplays()
    {
        var teamMembers = teamSystem.GetTeamMembers();
        
        // Remove displays for members no longer in team
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in memberUIs)
        {
            bool stillInTeam = false;
            foreach (var member in teamMembers)
            {
                if (member.character == kvp.Key)
                {
                    stillInTeam = true;
                    break;
                }
            }
            if (!stillInTeam)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            if (memberUIs[key].panel != null)
            {
                Destroy(memberUIs[key].panel);
            }
            memberUIs.Remove(key);
        }

        // Update or create displays for current members
        foreach (var member in teamMembers)
        {
            if (member.character == null) continue;

            TeamMemberUI memberUI;
            if (!memberUIs.TryGetValue(member.character, out memberUI))
            {
                memberUI = CreateMemberUI(member);
                memberUIs[member.character] = memberUI;
            }

            UpdateMemberUI(memberUI, member);
        }
    }

    TeamMemberUI CreateMemberUI(TeamSystem.TeamMember member)
    {
        if (memberPrefab == null || membersContainer == null) return null;

        GameObject panel = Instantiate(memberPrefab, membersContainer);
        TeamMemberUI ui = new TeamMemberUI();
        ui.panel = panel;

        // Get references to UI elements
        ui.nameText = panel.transform.Find("NameText")?.GetComponent<Text>();
        ui.trustBar = panel.transform.Find("TrustBar")?.GetComponent<Image>();
        ui.contributionBar = panel.transform.Find("ContributionBar")?.GetComponent<Image>();
        ui.tradingIcon = panel.transform.Find("TradingIcon")?.gameObject;

        return ui;
    }

    void UpdateMemberUI(TeamMemberUI ui, TeamSystem.TeamMember member)
    {
        if (ui == null) return;

        // Update name
        if (ui.nameText != null)
        {
            string memberName = member.character.name;
            ui.nameText.text = memberName;
        }

        // Update trust bar
        if (ui.trustBar != null)
        {
            ui.trustBar.fillAmount = member.trustLevel;
            ui.trustBar.color = Color.Lerp(lowTrustColor, highTrustColor, member.trustLevel);
        }

        // Update contribution bar
        if (ui.contributionBar != null)
        {
            ui.contributionBar.fillAmount = member.contributionScore / 100f;
        }

        // Update trading icon
        if (ui.tradingIcon != null)
        {
            TradingSystem memberTrading = member.character.GetComponent<TradingSystem>();
            ui.tradingIcon.SetActive(memberTrading != null && memberTrading.IsTrading());
        }
    }

    void UpdateTeamBond()
    {
        if (teamBondBar == null) return;

        // Calculate average trust level
        float totalTrust = 0f;
        int memberCount = 0;
        
        foreach (var member in teamSystem.GetTeamMembers())
        {
            if (member.character != null)
            {
                totalTrust += member.trustLevel;
                memberCount++;
            }
        }

        float averageTrust = memberCount > 0 ? totalTrust / memberCount : 0f;
        teamBondBar.fillAmount = averageTrust;
        teamBondBar.color = Color.Lerp(lowTrustColor, highTrustColor, averageTrust);
    }

    void OnDestroy()
    {
        // Clean up instantiated UI elements
        foreach (var ui in memberUIs.Values)
        {
            if (ui.panel != null)
            {
                Destroy(ui.panel);
            }
        }
        memberUIs.Clear();
    }
}
