using UnityEngine;
using System.Collections.Generic;

public class TeamSystem : MonoBehaviour
{
    [System.Serializable]
    public class TeamMember
    {
        public GameObject character;
        public float contributionScore;
        public float trustLevel;
        public List<string> sharedKnowledge;

        public TeamMember(GameObject chr)
        {
            character = chr;
            contributionScore = 0f;
            trustLevel = 0.1f;
            sharedKnowledge = new List<string>();
        }
    }

    [Header("Team Settings")]
    public float maxTeamSize = 4f;
    public float teamRadius = 15f;
    public float trustGainRate = 0.1f;
    public float knowledgeSharingInterval = 5f;
    public LayerMask teamMemberMask;

    private List<TeamMember> teamMembers = new List<TeamMember>();
    private float lastKnowledgeShare;
    private bool isTeamLeader;

    private NeuralNetwork neuralNetwork;
    private TradingSystem tradingSystem;

    void Start()
    {
        neuralNetwork = GetComponent<NeuralNetwork>();
        tradingSystem = GetComponent<TradingSystem>();
        lastKnowledgeShare = Time.time;
    }

    void Update()
    {
        if (isTeamLeader)
        {
            ManageTeam();
        }

        // Knowledge sharing timer
        if (Time.time - lastKnowledgeShare >= knowledgeSharingInterval)
        {
            ShareKnowledgeWithTeam();
            lastKnowledgeShare = Time.time;
        }

        // Team formation input
        if (Input.GetKeyDown(KeyCode.G))
        {
            TryFormTeam();
        }
    }

    private void TryFormTeam()
    {
        if (teamMembers.Count > 0) return; // Already in a team

        // Find nearby potential team members
        Collider[] nearbyCharacters = Physics.OverlapSphere(transform.position, teamRadius, teamMemberMask);
        
        foreach (var character in nearbyCharacters)
        {
            TeamSystem otherTeam = character.GetComponent<TeamSystem>();
            if (otherTeam != null && otherTeam != this && !otherTeam.IsInTeam())
            {
                // Check if both are Sinai characters
                if (IsSinaiCharacter(gameObject) && IsSinaiCharacter(character.gameObject))
                {
                    InitiateTeam(otherTeam);
                    break;
                }
            }
        }
    }

    private bool IsSinaiCharacter(GameObject character)
    {
        return character.GetComponent<SinaiCharacter>() != null;
    }

    private void InitiateTeam(TeamSystem other)
    {
        isTeamLeader = true;
        AddTeamMember(other.gameObject);
        other.JoinTeam(this);

        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Team formed! Press G near others to invite them.");
        }
    }

    public void JoinTeam(TeamSystem leader)
    {
        isTeamLeader = false;
        teamMembers.Clear(); // Clear any existing team data
        AddTeamMember(leader.gameObject);
    }

    private void AddTeamMember(GameObject member)
    {
        if (teamMembers.Count < maxTeamSize)
        {
            teamMembers.Add(new TeamMember(member));

            if (GuiderMessageUI.Instance != null)
            {
                GuiderMessageUI.Instance.ShowMessage("New team member joined!");
            }
        }
    }

    private void ManageTeam()
    {
        // Remove disconnected or distant members
        teamMembers.RemoveAll(member => 
            member.character == null || 
            Vector3.Distance(transform.position, member.character.transform.position) > teamRadius * 1.5f
        );

        // Update trust levels
        foreach (var member in teamMembers)
        {
            if (member.character != null)
            {
                // Increase trust based on proximity and time
                float distance = Vector3.Distance(transform.position, member.character.transform.position);
                if (distance <= teamRadius)
                {
                    member.trustLevel = Mathf.Min(1f, member.trustLevel + trustGainRate * Time.deltaTime);
                }

                // Update contribution score
                UpdateMemberContribution(member);
            }
        }
    }

    private void UpdateMemberContribution(TeamMember member)
    {
        if (member.character == null) return;

        NeuralNetwork memberNetwork = member.character.GetComponent<NeuralNetwork>();
        if (memberNetwork != null)
        {
            // Calculate contribution based on neural network development
            float development = memberNetwork.GetTotalDevelopment();
            member.contributionScore = development * member.trustLevel;
        }
    }

    private void ShareKnowledgeWithTeam()
    {
        if (teamMembers.Count == 0 || neuralNetwork == null) return;

        foreach (var member in teamMembers)
        {
            if (member.character != null && member.trustLevel >= 0.5f)
            {
                NeuralNetwork memberNetwork = member.character.GetComponent<NeuralNetwork>();
                if (memberNetwork != null)
                {
                    // Share neural network insights
                    float sharedExperience = neuralNetwork.GetTotalDevelopment() * 0.1f * member.trustLevel;
                    memberNetwork.GainExperience(sharedExperience);

                    // Share any tradeable items if trust is high
                    if (member.trustLevel >= 0.8f && tradingSystem != null)
                    {
                        TradingSystem memberTrading = member.character.GetComponent<TradingSystem>();
                        if (memberTrading != null && !tradingSystem.IsTrading() && !memberTrading.IsTrading())
                        {
                            // Automatic safe trading between trusted team members
                            tradingSystem.InitiateTrade(memberTrading);
                        }
                    }
                }
            }
        }
    }

    public bool IsInTeam()
    {
        return teamMembers.Count > 0;
    }

    public bool IsTeamLeader()
    {
        return isTeamLeader;
    }

    public List<TeamMember> GetTeamMembers()
    {
        return teamMembers;
    }

    void OnDrawGizmosSelected()
    {
        // Draw team radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, teamRadius);

        // Draw lines to team members
        if (teamMembers != null)
        {
            foreach (var member in teamMembers)
            {
                if (member.character != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, member.character.transform.position);
                }
            }
        }
    }
}
