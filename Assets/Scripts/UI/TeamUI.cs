using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace REcreationOfSpace.UI
{
    public class TeamUI : MonoBehaviour
    {
        [System.Serializable]
        private class MemberDisplay
        {
            public GameObject displayObject;
            public Image portraitImage;
            public TextMeshProUGUI nameText;
            public Slider healthBar;
            public Image leaderIcon;
        }

        [Header("UI Elements")]
        [SerializeField] private GameObject memberDisplayPrefab;
        [SerializeField] private Transform memberContainer;
        [SerializeField] private GameObject noTeamMessage;
        [SerializeField] private GameObject leaderControls;

        [Header("Settings")]
        [SerializeField] private Color leaderColor = Color.yellow;
        [SerializeField] private Color memberColor = Color.white;

        private Dictionary<GameObject, MemberDisplay> memberDisplays = new Dictionary<GameObject, MemberDisplay>();
        private TeamSystem playerTeam;

        private void Start()
        {
            // Find player's team system
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTeam = player.GetComponent<TeamSystem>();
                if (playerTeam != null)
                {
                    // Subscribe to events
                    playerTeam.onMemberJoined.AddListener(OnMemberJoined);
                    playerTeam.onMemberLeft.AddListener(OnMemberLeft);
                    playerTeam.onLeaderChanged.AddListener(OnLeaderChanged);
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (playerTeam == null || !playerTeam.HasTeam())
            {
                // Show no team message
                if (noTeamMessage != null)
                    noTeamMessage.SetActive(true);
                if (leaderControls != null)
                    leaderControls.SetActive(false);
                return;
            }

            // Hide no team message
            if (noTeamMessage != null)
                noTeamMessage.SetActive(false);

            // Show/hide leader controls
            if (leaderControls != null)
                leaderControls.SetActive(playerTeam.IsTeamLeader());

            // Update member displays
            var members = playerTeam.GetTeamMembers();
            var leader = playerTeam.GetTeamLeader();

            foreach (var member in members)
            {
                if (!memberDisplays.ContainsKey(member))
                {
                    CreateMemberDisplay(member);
                }

                UpdateMemberDisplay(member, leader);
            }

            // Remove displays for members no longer in team
            var displayKeys = new List<GameObject>(memberDisplays.Keys);
            foreach (var key in displayKeys)
            {
                if (!members.Contains(key))
                {
                    RemoveMemberDisplay(key);
                }
            }
        }

        private void CreateMemberDisplay(GameObject member)
        {
            if (memberDisplayPrefab == null || memberContainer == null)
                return;

            var displayObj = Instantiate(memberDisplayPrefab, memberContainer);
            var display = new MemberDisplay
            {
                displayObject = displayObj,
                portraitImage = displayObj.GetComponentInChildren<Image>(),
                nameText = displayObj.GetComponentInChildren<TextMeshProUGUI>(),
                healthBar = displayObj.GetComponentInChildren<Slider>(),
                leaderIcon = displayObj.transform.Find("LeaderIcon")?.GetComponent<Image>()
            };

            memberDisplays[member] = display;
        }

        private void UpdateMemberDisplay(GameObject member, GameObject leader)
        {
            if (!memberDisplays.ContainsKey(member))
                return;

            var display = memberDisplays[member];
            bool isLeader = member == leader;

            // Update name
            if (display.nameText != null)
            {
                display.nameText.text = member.name;
                display.nameText.color = isLeader ? leaderColor : memberColor;
            }

            // Update health bar
            if (display.healthBar != null)
            {
                var health = member.GetComponent<Health>();
                if (health != null)
                {
                    display.healthBar.value = health.GetHealthPercentage();
                }
            }

            // Show/hide leader icon
            if (display.leaderIcon != null)
            {
                display.leaderIcon.gameObject.SetActive(isLeader);
            }
        }

        private void RemoveMemberDisplay(GameObject member)
        {
            if (!memberDisplays.ContainsKey(member))
                return;

            var display = memberDisplays[member];
            if (display.displayObject != null)
            {
                Destroy(display.displayObject);
            }

            memberDisplays.Remove(member);
        }

        private void OnMemberJoined(GameObject member)
        {
            UpdateUI();
        }

        private void OnMemberLeft(GameObject member)
        {
            UpdateUI();
        }

        private void OnLeaderChanged(GameObject newLeader)
        {
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (playerTeam != null)
            {
                playerTeam.onMemberJoined.RemoveListener(OnMemberJoined);
                playerTeam.onMemberLeft.RemoveListener(OnMemberLeft);
                playerTeam.onLeaderChanged.RemoveListener(OnLeaderChanged);
            }
        }
    }
}
