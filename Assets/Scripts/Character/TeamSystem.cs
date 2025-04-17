using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace REcreationOfSpace.Character
{
    public class TeamSystem : MonoBehaviour
    {
        [Header("Team Settings")]
        [SerializeField] private int maxTeamSize = 4;
        [SerializeField] private float teamRadius = 5f;
        [SerializeField] private bool isTeamLeader = false;

        [Header("UI Events")]
        public UnityEvent<GameObject> onMemberJoined;
        public UnityEvent<GameObject> onMemberLeft;
        public UnityEvent<GameObject> onLeaderChanged;

        private List<GameObject> teamMembers = new List<GameObject>();
        private GameObject teamLeader;
        private bool hasTeam = false;

        private void Start()
        {
            if (isTeamLeader)
            {
                CreateTeam();
            }
        }

        private void Update()
        {
            if (hasTeam && teamLeader == gameObject)
            {
                // Update team member positions
                UpdateTeamFormation();
            }
        }

        public void CreateTeam()
        {
            if (hasTeam)
                return;

            teamLeader = gameObject;
            teamMembers.Clear();
            teamMembers.Add(gameObject);
            hasTeam = true;

            onLeaderChanged?.Invoke(teamLeader);
        }

        public void RequestJoinTeam()
        {
            if (!hasTeam)
            {
                // Create new team if not in one
                CreateTeam();
                return;
            }

            // Find nearby team leader
            Collider[] colliders = Physics.OverlapSphere(transform.position, teamRadius);
            foreach (var collider in colliders)
            {
                var otherTeam = collider.GetComponent<TeamSystem>();
                if (otherTeam != null && otherTeam.IsTeamLeader() && otherTeam.CanAcceptMember())
                {
                    otherTeam.AddMember(gameObject);
                    break;
                }
            }
        }

        public void LeaveTeam()
        {
            if (!hasTeam)
                return;

            if (teamLeader == gameObject)
            {
                // Disband team if leader leaves
                DisbandTeam();
            }
            else
            {
                // Remove self from team
                var leaderTeam = teamLeader.GetComponent<TeamSystem>();
                if (leaderTeam != null)
                {
                    leaderTeam.RemoveMember(gameObject);
                }
            }
        }

        public void AddMember(GameObject member)
        {
            if (!hasTeam || teamMembers.Count >= maxTeamSize || teamMembers.Contains(member))
                return;

            teamMembers.Add(member);

            // Set up member
            var memberTeam = member.GetComponent<TeamSystem>();
            if (memberTeam != null)
            {
                memberTeam.JoinExistingTeam(gameObject);
            }

            onMemberJoined?.Invoke(member);
        }

        public void RemoveMember(GameObject member)
        {
            if (!hasTeam || !teamMembers.Contains(member))
                return;

            teamMembers.Remove(member);

            // Clean up member
            var memberTeam = member.GetComponent<TeamSystem>();
            if (memberTeam != null)
            {
                memberTeam.ResetTeam();
            }

            onMemberLeft?.Invoke(member);
        }

        private void JoinExistingTeam(GameObject newLeader)
        {
            teamLeader = newLeader;
            hasTeam = true;
            isTeamLeader = false;
            teamMembers.Clear();
            teamMembers.Add(gameObject);

            onLeaderChanged?.Invoke(teamLeader);
        }

        private void DisbandTeam()
        {
            // Notify all members
            foreach (var member in new List<GameObject>(teamMembers))
            {
                if (member != gameObject)
                {
                    var memberTeam = member.GetComponent<TeamSystem>();
                    if (memberTeam != null)
                    {
                        memberTeam.ResetTeam();
                    }
                    onMemberLeft?.Invoke(member);
                }
            }

            ResetTeam();
        }

        private void ResetTeam()
        {
            teamLeader = null;
            hasTeam = false;
            isTeamLeader = false;
            teamMembers.Clear();

            onLeaderChanged?.Invoke(null);
        }

        private void UpdateTeamFormation()
        {
            // Simple circular formation
            float angleStep = 360f / teamMembers.Count;
            for (int i = 1; i < teamMembers.Count; i++) // Skip leader (index 0)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * teamRadius;
                Vector3 targetPosition = transform.position + offset;

                // Move member towards position
                var member = teamMembers[i];
                if (member != null)
                {
                    var agent = member.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.SetDestination(targetPosition);
                    }
                }
            }
        }

        public bool IsTeamLeader()
        {
            return hasTeam && teamLeader == gameObject;
        }

        public bool HasTeam()
        {
            return hasTeam;
        }

        public bool CanAcceptMember()
        {
            return hasTeam && IsTeamLeader() && teamMembers.Count < maxTeamSize;
        }

        public List<GameObject> GetTeamMembers()
        {
            return new List<GameObject>(teamMembers);
        }

        public GameObject GetTeamLeader()
        {
            return teamLeader;
        }
    }
}
