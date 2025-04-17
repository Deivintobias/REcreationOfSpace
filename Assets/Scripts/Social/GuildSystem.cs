using UnityEngine;
using System.Collections.Generic;
using REcreationOfSpace.Character;
using REcreationOfSpace.UI;

namespace REcreationOfSpace.Social
{
    public class GuildSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Guild
        {
            public string id;
            public string name;
            public string description;
            public string leaderID;
            public List<string> officerIDs = new List<string>();
            public List<string> memberIDs = new List<string>();
            public Dictionary<string, GuildRank> memberRanks = new Dictionary<string, GuildRank>();
            public string motto;
            public string emblem;
            public GuildType type;
            public float treasury;
            public List<GuildAchievement> achievements = new List<GuildAchievement>();
            public List<GuildEvent> events = new List<GuildEvent>();
            public Dictionary<string, float> memberContributions = new Dictionary<string, float>();
            public bool isRecruiting;
            public int maxMembers = 100;
            public Dictionary<string, List<string>> permissions = new Dictionary<string, List<string>>();
        }

        [System.Serializable]
        public class GuildRank
        {
            public string name;
            public int level;
            public List<string> permissions;
            public float contributionRequirement;
            public bool canPromote;
            public bool canRecruit;
            public bool canManageTreasury;
            public bool canOrganizeEvents;
        }

        [System.Serializable]
        public class GuildAchievement
        {
            public string name;
            public string description;
            public float progress;
            public float goal;
            public string reward;
            public bool isCompleted;
            public System.DateTime completionDate;
        }

        [System.Serializable]
        public class GuildEvent
        {
            public string name;
            public string description;
            public System.DateTime startTime;
            public System.DateTime endTime;
            public List<string> participants = new List<string>();
            public string location;
            public string eventType;
            public Dictionary<string, string> roles = new Dictionary<string, string>();
            public bool isActive;
        }

        public enum GuildType
        {
            Combat,
            Crafting,
            Trading,
            Farming,
            Social,
            Adventure,
            Research,
            Spiritual
        }

        [Header("Guild Settings")]
        [SerializeField] private float minGuildCreationCost = 10000f;
        [SerializeField] private int minMembersForGuild = 5;
        [SerializeField] private float dailyUpkeepCost = 100f;
        [SerializeField] private float maxTreasuryContributionPerDay = 10000f;
        [SerializeField] private int maxActiveEvents = 3;
        [SerializeField] private int maxGuildsPerPlayer = 3;

        [Header("UI References")]
        [SerializeField] private GuiderMessageUI guiderMessage;

        private Dictionary<string, Guild> guilds = new Dictionary<string, Guild>();
        private Dictionary<string, List<string>> playerGuilds = new Dictionary<string, List<string>>();

        public bool CreateGuild(string playerID, string guildName, string description, GuildType type)
        {
            if (playerGuilds.ContainsKey(playerID) && playerGuilds[playerID].Count >= maxGuildsPerPlayer)
            {
                guiderMessage.ShowMessage("You have reached the maximum number of guilds.", Color.red);
                return false;
            }

            string guildID = System.Guid.NewGuid().ToString();
            var guild = new Guild
            {
                id = guildID,
                name = guildName,
                description = description,
                leaderID = playerID,
                type = type,
                treasury = 0f,
                isRecruiting = true
            };

            // Set up default ranks
            var ranks = new Dictionary<string, GuildRank>
            {
                ["Leader"] = new GuildRank
                {
                    name = "Leader",
                    level = 4,
                    permissions = new List<string> { "all" },
                    canPromote = true,
                    canRecruit = true,
                    canManageTreasury = true,
                    canOrganizeEvents = true
                },
                ["Officer"] = new GuildRank
                {
                    name = "Officer",
                    level = 3,
                    permissions = new List<string> { "invite", "kick", "event", "treasury_view" },
                    canPromote = false,
                    canRecruit = true,
                    canManageTreasury = false,
                    canOrganizeEvents = true
                },
                ["Member"] = new GuildRank
                {
                    name = "Member",
                    level = 2,
                    permissions = new List<string> { "event_participate", "treasury_view" },
                    canPromote = false,
                    canRecruit = false,
                    canManageTreasury = false,
                    canOrganizeEvents = false
                },
                ["Initiate"] = new GuildRank
                {
                    name = "Initiate",
                    level = 1,
                    permissions = new List<string> { "event_participate" },
                    canPromote = false,
                    canRecruit = false,
                    canManageTreasury = false,
                    canOrganizeEvents = false
                }
            };

            guild.memberRanks = ranks;
            guild.memberRanks[playerID] = ranks["Leader"];
            guild.memberIDs.Add(playerID);

            guilds[guildID] = guild;

            if (!playerGuilds.ContainsKey(playerID))
            {
                playerGuilds[playerID] = new List<string>();
            }
            playerGuilds[playerID].Add(guildID);

            guiderMessage.ShowMessage($"Guild '{guildName}' created successfully!", Color.green);
            return true;
        }

        public bool JoinGuild(string playerID, string guildID)
        {
            if (!guilds.ContainsKey(guildID))
                return false;

            var guild = guilds[guildID];

            if (guild.memberIDs.Contains(playerID))
            {
                guiderMessage.ShowMessage("You are already a member of this guild.", Color.yellow);
                return false;
            }

            if (guild.memberIDs.Count >= guild.maxMembers)
            {
                guiderMessage.ShowMessage("This guild has reached its maximum member capacity.", Color.red);
                return false;
            }

            if (!guild.isRecruiting)
            {
                guiderMessage.ShowMessage("This guild is not currently recruiting.", Color.red);
                return false;
            }

            guild.memberIDs.Add(playerID);
            guild.memberRanks[playerID] = guild.memberRanks["Initiate"];

            if (!playerGuilds.ContainsKey(playerID))
            {
                playerGuilds[playerID] = new List<string>();
            }
            playerGuilds[playerID].Add(guildID);

            guiderMessage.ShowMessage($"Successfully joined {guild.name}!", Color.green);
            return true;
        }

        public bool LeaveGuild(string playerID, string guildID)
        {
            if (!guilds.ContainsKey(guildID))
                return false;

            var guild = guilds[guildID];

            if (!guild.memberIDs.Contains(playerID))
                return false;

            if (playerID == guild.leaderID)
            {
                if (guild.memberIDs.Count > 1)
                {
                    guiderMessage.ShowMessage("You must transfer leadership before leaving the guild.", Color.red);
                    return false;
                }
                else
                {
                    // Last member leaving, dissolve the guild
                    DissolveGuild(guildID);
                }
            }

            guild.memberIDs.Remove(playerID);
            guild.memberRanks.Remove(playerID);
            playerGuilds[playerID].Remove(guildID);

            guiderMessage.ShowMessage($"You have left {guild.name}.", Color.white);
            return true;
        }

        public void DissolveGuild(string guildID)
        {
            if (!guilds.ContainsKey(guildID))
                return;

            var guild = guilds[guildID];
            
            // Remove all members
            foreach (var memberID in guild.memberIDs.ToArray())
            {
                if (playerGuilds.ContainsKey(memberID))
                {
                    playerGuilds[memberID].Remove(guildID);
                }
            }

            guilds.Remove(guildID);
            guiderMessage.ShowMessage($"Guild '{guild.name}' has been dissolved.", Color.yellow);
        }

        public bool CreateGuildEvent(string playerID, string guildID, GuildEvent guildEvent)
        {
            if (!guilds.ContainsKey(guildID))
                return false;

            var guild = guilds[guildID];
            var rank = guild.memberRanks[playerID];

            if (!rank.canOrganizeEvents)
            {
                guiderMessage.ShowMessage("You don't have permission to organize events.", Color.red);
                return false;
            }

            if (guild.events.Count(e => e.isActive) >= maxActiveEvents)
            {
                guiderMessage.ShowMessage("Maximum number of active events reached.", Color.red);
                return false;
            }

            guild.events.Add(guildEvent);
            guiderMessage.ShowMessage($"Event '{guildEvent.name}' created successfully!", Color.green);
            return true;
        }

        public bool ContributeToTreasury(string playerID, string guildID, float amount)
        {
            if (!guilds.ContainsKey(guildID))
                return false;

            var guild = guilds[guildID];

            if (!guild.memberIDs.Contains(playerID))
                return false;

            if (amount <= 0 || amount > maxTreasuryContributionPerDay)
            {
                guiderMessage.ShowMessage($"Invalid contribution amount. Maximum daily contribution: {maxTreasuryContributionPerDay}", Color.red);
                return false;
            }

            guild.treasury += amount;
            
            if (!guild.memberContributions.ContainsKey(playerID))
                guild.memberContributions[playerID] = 0;
            
            guild.memberContributions[playerID] += amount;

            guiderMessage.ShowMessage($"Contributed {amount:F2} to guild treasury.", Color.green);
            return true;
        }

        public bool PromoteMember(string promoterID, string targetID, string guildID, string newRank)
        {
            if (!guilds.ContainsKey(guildID))
                return false;

            var guild = guilds[guildID];
            
            if (!guild.memberRanks.ContainsKey(promoterID) || !guild.memberRanks.ContainsKey(targetID))
                return false;

            var promoterRank = guild.memberRanks[promoterID];
            var targetRank = guild.memberRanks[targetID];

            if (!promoterRank.canPromote)
            {
                guiderMessage.ShowMessage("You don't have permission to promote members.", Color.red);
                return false;
            }

            if (!guild.memberRanks.ContainsKey(newRank))
            {
                guiderMessage.ShowMessage("Invalid rank specified.", Color.red);
                return false;
            }

            var newRankObj = guild.memberRanks[newRank];
            if (promoterRank.level <= newRankObj.level)
            {
                guiderMessage.ShowMessage("You cannot promote to a rank equal to or higher than your own.", Color.red);
                return false;
            }

            guild.memberRanks[targetID] = newRankObj;
            guiderMessage.ShowMessage($"Promoted {targetID} to {newRank}!", Color.green);
            return true;
        }

        public Guild GetGuild(string guildID)
        {
            return guilds.ContainsKey(guildID) ? guilds[guildID] : null;
        }

        public List<Guild> GetPlayerGuilds(string playerID)
        {
            if (!playerGuilds.ContainsKey(playerID))
                return new List<Guild>();

            return playerGuilds[playerID].Select(guildID => guilds[guildID]).ToList();
        }

        public List<Guild> GetRecruitingGuilds()
        {
            return guilds.Values.Where(g => g.isRecruiting).ToList();
        }

        public List<Guild> GetGuildsByType(GuildType type)
        {
            return guilds.Values.Where(g => g.type == type).ToList();
        }

        public bool HasPermission(string playerID, string guildID, string permission)
        {
            if (!guilds.ContainsKey(guildID) || !guilds[guildID].memberRanks.ContainsKey(playerID))
                return false;

            var rank = guilds[guildID].memberRanks[playerID];
            return rank.permissions.Contains(permission) || rank.permissions.Contains("all");
        }

        public void UpdateGuilds()
        {
            foreach (var guild in guilds.Values)
            {
                // Process daily upkeep
                guild.treasury -= dailyUpkeepCost * Time.deltaTime;

                // Check for completed events
                foreach (var evt in guild.events.Where(e => e.isActive))
                {
                    if (System.DateTime.Now > evt.endTime)
                    {
                        evt.isActive = false;
                    }
                }

                // Update achievements
                foreach (var achievement in guild.achievements.Where(a => !a.isCompleted))
                {
                    if (achievement.progress >= achievement.goal)
                    {
                        achievement.isCompleted = true;
                        achievement.completionDate = System.DateTime.Now;
                        guiderMessage.ShowMessage($"{guild.name} has earned the achievement: {achievement.name}!", Color.green);
                    }
                }
            }
        }
    }
}
