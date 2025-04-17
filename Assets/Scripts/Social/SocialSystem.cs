using UnityEngine;
using System.Collections.Generic;
using REcreationOfSpace.Character;
using REcreationOfSpace.UI;

namespace REcreationOfSpace.Social
{
    public class SocialSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Relationship
        {
            public string id;
            public string name;
            public RelationType type;
            public float friendshipLevel;
            public float trustLevel;
            public float respectLevel;
            public bool isFamily;
            public bool isCoworker;
            public bool isNeighbor;
            public string[] sharedInterests;
            public string occupation;
            public int age;
            public string personality;
            public List<Interaction> recentInteractions;
            public float lastInteractionTime;
        }

        [System.Serializable]
        public class Interaction
        {
            public string type;
            public float impact;
            public float time;
            public string description;
            public bool wasPositive;
        }

        public enum RelationType
        {
            Stranger,
            Acquaintance,
            Friend,
            CloseFriend,
            BestFriend,
            Family,
            Spouse,
            Professional
        }

        [Header("Relationship Settings")]
        [SerializeField] private float maxFriendshipLevel = 100f;
        [SerializeField] private float maxTrustLevel = 100f;
        [SerializeField] private float maxRespectLevel = 100f;
        [SerializeField] private float relationshipDecayRate = 0.1f;
        [SerializeField] private float interactionCooldown = 1f;
        [SerializeField] private int maxRecentInteractions = 10;

        [Header("Social Network")]
        [SerializeField] private int maxRelationships = 50;
        [SerializeField] private float networkingChance = 0.2f;
        [SerializeField] private float jobReferralBonus = 0.3f;
        [SerializeField] private float businessPartnershipBonus = 0.5f;

        [Header("Events")]
        [SerializeField] private float socialEventChance = 0.1f;
        [SerializeField] private float eventSuccessBonus = 0.2f;
        [SerializeField] private float eventFailPenalty = 0.3f;

        [Header("UI References")]
        [SerializeField] private GuiderMessageUI guiderMessage;

        private Dictionary<string, Relationship> relationships = new Dictionary<string, Relationship>();
        private List<string> availableInteractions = new List<string>();
        private float lastInteractionTime;

        private void Start()
        {
            InitializeInteractions();
        }

        private void Update()
        {
            UpdateRelationships();
            CheckForSocialEvents();
        }

        private void InitializeInteractions()
        {
            availableInteractions.AddRange(new string[] {
                "Chat",
                "Deep Conversation",
                "Share Meal",
                "Give Gift",
                "Ask for Advice",
                "Offer Help",
                "Business Discussion",
                "Social Activity",
                "Professional Meeting",
                "Family Gathering"
            });
        }

        private void UpdateRelationships()
        {
            foreach (var relationship in relationships.Values)
            {
                // Natural relationship decay over time
                float timeSinceLastInteraction = Time.time - relationship.lastInteractionTime;
                if (timeSinceLastInteraction > 24f * 3600f) // One day in seconds
                {
                    relationship.friendshipLevel = Mathf.Max(0f, relationship.friendshipLevel - relationshipDecayRate * Time.deltaTime);
                    relationship.trustLevel = Mathf.Max(0f, relationship.trustLevel - relationshipDecayRate * 0.5f * Time.deltaTime);
                }

                // Update relationship type based on levels
                UpdateRelationType(relationship);
            }
        }

        private void UpdateRelationType(Relationship relationship)
        {
            if (relationship.isFamily)
            {
                relationship.type = RelationType.Family;
                return;
            }

            float totalLevel = (relationship.friendshipLevel + relationship.trustLevel + relationship.respectLevel) / 3f;

            if (totalLevel < 20f)
                relationship.type = RelationType.Stranger;
            else if (totalLevel < 40f)
                relationship.type = RelationType.Acquaintance;
            else if (totalLevel < 60f)
                relationship.type = RelationType.Friend;
            else if (totalLevel < 80f)
                relationship.type = RelationType.CloseFriend;
            else
                relationship.type = RelationType.BestFriend;
        }

        private void CheckForSocialEvents()
        {
            if (Random.value < socialEventChance * Time.deltaTime)
            {
                TriggerSocialEvent();
            }
        }

        private void TriggerSocialEvent()
        {
            if (relationships.Count == 0) return;

            // Select random relationship
            var relationshipArray = new Relationship[relationships.Count];
            relationships.Values.CopyTo(relationshipArray, 0);
            var relationship = relationshipArray[Random.Range(0, relationshipArray.Length)];

            // Generate event
            string eventType = GetRandomSocialEvent(relationship);
            bool success = Random.value < 0.7f; // 70% success chance

            if (success)
            {
                float bonus = eventSuccessBonus;
                if (relationship.isCoworker) bonus *= 1.5f;
                if (relationship.isNeighbor) bonus *= 1.2f;

                relationship.friendshipLevel = Mathf.Min(maxFriendshipLevel, relationship.friendshipLevel + bonus);
                relationship.trustLevel = Mathf.Min(maxTrustLevel, relationship.trustLevel + bonus * 0.5f);

                guiderMessage.ShowMessage($"Successful {eventType} with {relationship.name}!", Color.green);
            }
            else
            {
                relationship.friendshipLevel = Mathf.Max(0f, relationship.friendshipLevel - eventFailPenalty);
                relationship.trustLevel = Mathf.Max(0f, relationship.trustLevel - eventFailPenalty * 0.5f);

                guiderMessage.ShowMessage($"Awkward {eventType} with {relationship.name}...", Color.yellow);
            }

            AddInteraction(relationship, eventType, success);
        }

        private string GetRandomSocialEvent(Relationship relationship)
        {
            List<string> possibleEvents = new List<string>();

            if (relationship.isCoworker)
            {
                possibleEvents.Add("Work Lunch");
                possibleEvents.Add("Project Meeting");
                possibleEvents.Add("Office Party");
            }

            if (relationship.isNeighbor)
            {
                possibleEvents.Add("Neighborhood BBQ");
                possibleEvents.Add("Community Meeting");
                possibleEvents.Add("Local Event");
            }

            if (relationship.isFamily)
            {
                possibleEvents.Add("Family Dinner");
                possibleEvents.Add("Family Gathering");
                possibleEvents.Add("Holiday Celebration");
            }

            // General events
            possibleEvents.Add("Coffee Meeting");
            possibleEvents.Add("Social Gathering");
            possibleEvents.Add("Casual Conversation");

            return possibleEvents[Random.Range(0, possibleEvents.Count)];
        }

        public bool TryInteract(string relationshipId, string interactionType)
        {
            if (Time.time - lastInteractionTime < interactionCooldown)
                return false;

            if (!relationships.TryGetValue(relationshipId, out Relationship relationship))
                return false;

            if (!availableInteractions.Contains(interactionType))
                return false;

            float impact = CalculateInteractionImpact(relationship, interactionType);
            bool wasPositive = impact > 0;

            relationship.friendshipLevel = Mathf.Clamp(relationship.friendshipLevel + impact, 0f, maxFriendshipLevel);
            relationship.trustLevel = Mathf.Clamp(relationship.trustLevel + impact * 0.5f, 0f, maxTrustLevel);
            relationship.respectLevel = Mathf.Clamp(relationship.respectLevel + impact * 0.3f, 0f, maxRespectLevel);

            AddInteraction(relationship, interactionType, wasPositive);
            lastInteractionTime = Time.time;

            return true;
        }

        private float CalculateInteractionImpact(Relationship relationship, string interactionType)
        {
            float baseImpact = 5f;
            float multiplier = 1f;

            // Adjust based on relationship type
            switch (relationship.type)
            {
                case RelationType.Stranger: multiplier *= 0.5f; break;
                case RelationType.Acquaintance: multiplier *= 0.8f; break;
                case RelationType.Friend: multiplier *= 1.2f; break;
                case RelationType.CloseFriend: multiplier *= 1.5f; break;
                case RelationType.BestFriend: multiplier *= 2f; break;
                case RelationType.Family: multiplier *= 1.8f; break;
                case RelationType.Spouse: multiplier *= 2.5f; break;
            }

            // Adjust based on interaction type
            switch (interactionType)
            {
                case "Deep Conversation": baseImpact *= 2f; break;
                case "Give Gift": baseImpact *= 1.5f; break;
                case "Offer Help": baseImpact *= 1.8f; break;
                case "Share Meal": baseImpact *= 1.3f; break;
                case "Business Discussion": 
                    if (relationship.isCoworker) baseImpact *= 1.5f;
                    break;
                case "Family Gathering":
                    if (relationship.isFamily) baseImpact *= 2f;
                    break;
            }

            // Random variation
            multiplier *= Random.Range(0.8f, 1.2f);

            return baseImpact * multiplier;
        }

        private void AddInteraction(Relationship relationship, string type, bool wasPositive)
        {
            var interaction = new Interaction
            {
                type = type,
                impact = wasPositive ? 1f : -1f,
                time = Time.time,
                description = $"{(wasPositive ? "Positive" : "Negative")} {type} interaction",
                wasPositive = wasPositive
            };

            relationship.recentInteractions.Add(interaction);
            if (relationship.recentInteractions.Count > maxRecentInteractions)
            {
                relationship.recentInteractions.RemoveAt(0);
            }

            relationship.lastInteractionTime = Time.time;
        }

        public bool AddRelationship(string id, string name, bool isFamily = false, bool isCoworker = false, bool isNeighbor = false)
        {
            if (relationships.Count >= maxRelationships || relationships.ContainsKey(id))
                return false;

            var relationship = new Relationship
            {
                id = id,
                name = name,
                type = isFamily ? RelationType.Family : RelationType.Stranger,
                friendshipLevel = isFamily ? 50f : 0f,
                trustLevel = isFamily ? 50f : 0f,
                respectLevel = isFamily ? 50f : 0f,
                isFamily = isFamily,
                isCoworker = isCoworker,
                isNeighbor = isNeighbor,
                sharedInterests = new string[0],
                recentInteractions = new List<Interaction>(),
                lastInteractionTime = Time.time
            };

            relationships.Add(id, relationship);
            return true;
        }

        public void RemoveRelationship(string id)
        {
            relationships.Remove(id);
        }

        public float GetRelationshipLevel(string id)
        {
            if (relationships.TryGetValue(id, out Relationship relationship))
            {
                return (relationship.friendshipLevel + relationship.trustLevel + relationship.respectLevel) / 3f;
            }
            return 0f;
        }

        public RelationType GetRelationType(string id)
        {
            if (relationships.TryGetValue(id, out Relationship relationship))
            {
                return relationship.type;
            }
            return RelationType.Stranger;
        }

        public Dictionary<string, Relationship> GetAllRelationships()
        {
            return relationships;
        }

        public List<string> GetAvailableInteractions()
        {
            return availableInteractions;
        }

        public float GetNetworkingBonus(string id)
        {
            if (!relationships.TryGetValue(id, out Relationship relationship))
                return 0f;

            float bonus = 0f;
            
            if (relationship.isCoworker)
                bonus += jobReferralBonus;
            
            if (relationship.type == RelationType.Professional)
                bonus += businessPartnershipBonus;
            
            bonus *= relationship.trustLevel / maxTrustLevel;
            
            return bonus;
        }
    }
}
