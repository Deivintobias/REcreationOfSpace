using UnityEngine;
using System.Collections.Generic;
using REcreationOfSpace.Character;
using REcreationOfSpace.UI;

namespace REcreationOfSpace.Social
{
    public class ClanSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Clan
        {
            public string id;
            public string name;
            public string description;
            public string founderID;
            public List<string> elderIDs = new List<string>();
            public List<string> memberIDs = new List<string>();
            public Dictionary<string, ClanRole> memberRoles = new Dictionary<string, ClanRole>();
            public string banner;
            public string motto;
            public ClanFocus focus;
            public float influence;
            public List<ClanAlliance> alliances = new List<ClanAlliance>();
            public List<string> rivalClans = new List<string>();
            public Dictionary<string, float> memberHonor = new Dictionary<string, float>();
            public List<ClanTradition> traditions = new List<ClanTradition>();
            public bool isOpen;
            public int maxMembers = 50;
            public float minimumHonorRequirement = 0f;
        }

        [System.Serializable]
        public class ClanRole
        {
            public string name;
            public int tier;
            public List<string> permissions;
            public float honorRequirement;
            public bool canPromote;
            public bool canRecruit;
            public bool canManageAlliances;
            public bool canDeclareRivals;
        }

        [System.Serializable]
        public class ClanAlliance
        {
            public string clanID;
            public AllianceType type;
            public System.DateTime formationDate;
            public float strength;
            public List<string> sharedGoals;
            public bool isActive;
        }

        [System.Serializable]
        public class ClanTradition
        {
            public string name;
            public string description;
            public TraditionType type;
            public float honorBonus;
            public System.DateTime establishedDate;
            public List<string> requirements;
            public bool isActive;
        }

        public enum ClanFocus
        {
            Warrior,    // Combat and strength
            Mystic,     // Spiritual and divine
            Merchant,   // Trade and wealth
            Scholar,    // Knowledge and wisdom
            Guardian,   // Protection and defense
            Artisan,    // Crafting and creation
            Explorer,   // Discovery and adventure
            Diplomat    // Relations and influence
        }

        public enum AllianceType
        {
            Trade,          // Economic cooperation
            Military,       // Combat support
            Cultural,       // Shared traditions
            Knowledge,      // Information exchange
            Protection,     // Mutual defense
            Development    // Joint projects
        }

        public enum TraditionType
        {
            Ritual,         // Regular ceremonies
            Training,       // Skill development
            Celebration,    // Special events
            Challenge,      // Tests of worth
            Service,        // Community aid
            Pilgrimage     // Sacred journeys
        }

        [Header("Clan Settings")]
        [SerializeField] private float clanCreationCost = 50000f;
        [SerializeField] private int minMembersForClan = 3;
        [SerializeField] private float influenceGainRate = 0.1f;
        [SerializeField] private float influenceDecayRate = 0.05f;
        [SerializeField] private float allianceStrengthGainRate = 0.1f;
        [SerializeField] private int maxAlliances = 5;
        [SerializeField] private int maxRivals = 3;
        [SerializeField] private float rivalInfluencePenalty = 0.2f;

        [Header("Honor System")]
        [SerializeField] private float baseHonorGain = 1f;
        [SerializeField] private float honorLossMultiplier = 2f;
        [SerializeField] private float maxHonor = 1000f;
        [SerializeField] private float minHonor = -100f;

        [Header("UI References")]
        [SerializeField] private GuiderMessageUI guiderMessage;

        private Dictionary<string, Clan> clans = new Dictionary<string, Clan>();
        private Dictionary<string, List<string>> playerClans = new Dictionary<string, List<string>>();

        public bool CreateClan(string playerID, string clanName, string description, ClanFocus focus)
        {
            if (playerClans.ContainsKey(playerID) && playerClans[playerID].Count > 0)
            {
                guiderMessage.ShowMessage("You are already a member of a clan.", Color.red);
                return false;
            }

            string clanID = System.Guid.NewGuid().ToString();
            var clan = new Clan
            {
                id = clanID,
                name = clanName,
                description = description,
                founderID = playerID,
                focus = focus,
                influence = 0f,
                isOpen = false
            };

            // Set up default roles
            var roles = new Dictionary<string, ClanRole>
            {
                ["Founder"] = new ClanRole
                {
                    name = "Founder",
                    tier = 4,
                    permissions = new List<string> { "all" },
                    honorRequirement = 0f,
                    canPromote = true,
                    canRecruit = true,
                    canManageAlliances = true,
                    canDeclareRivals = true
                },
                ["Elder"] = new ClanRole
                {
                    name = "Elder",
                    tier = 3,
                    permissions = new List<string> { "promote", "recruit", "alliance_view" },
                    honorRequirement = 500f,
                    canPromote = true,
                    canRecruit = true,
                    canManageAlliances = false,
                    canDeclareRivals = false
                },
                ["Warrior"] = new ClanRole
                {
                    name = "Warrior",
                    tier = 2,
                    permissions = new List<string> { "participate", "train" },
                    honorRequirement = 100f,
                    canPromote = false,
                    canRecruit = false,
                    canManageAlliances = false,
                    canDeclareRivals = false
                },
                ["Initiate"] = new ClanRole
                {
                    name = "Initiate",
                    tier = 1,
                    permissions = new List<string> { "participate" },
                    honorRequirement = 0f,
                    canPromote = false,
                    canRecruit = false,
                    canManageAlliances = false,
                    canDeclareRivals = false
                }
            };

            clan.memberRoles = roles;
            clan.memberRoles[playerID] = roles["Founder"];
            clan.memberIDs.Add(playerID);
            clan.memberHonor[playerID] = 100f; // Starting honor for founder

            clans[clanID] = clan;

            if (!playerClans.ContainsKey(playerID))
            {
                playerClans[playerID] = new List<string>();
            }
            playerClans[playerID].Add(clanID);

            guiderMessage.ShowMessage($"Clan '{clanName}' founded successfully!", Color.green);
            return true;
        }

        public bool JoinClan(string playerID, string clanID)
        {
            if (!clans.ContainsKey(clanID))
                return false;

            if (playerClans.ContainsKey(playerID) && playerClans[playerID].Count > 0)
            {
                guiderMessage.ShowMessage("You are already a member of a clan.", Color.red);
                return false;
            }

            var clan = clans[clanID];

            if (clan.memberIDs.Contains(playerID))
            {
                guiderMessage.ShowMessage("You are already a member of this clan.", Color.yellow);
                return false;
            }

            if (clan.memberIDs.Count >= clan.maxMembers)
            {
                guiderMessage.ShowMessage("This clan has reached its maximum member capacity.", Color.red);
                return false;
            }

            if (!clan.isOpen)
            {
                guiderMessage.ShowMessage("This clan is not accepting new members.", Color.red);
                return false;
            }

            clan.memberIDs.Add(playerID);
            clan.memberRoles[playerID] = clan.memberRoles["Initiate"];
            clan.memberHonor[playerID] = 0f;

            if (!playerClans.ContainsKey(playerID))
            {
                playerClans[playerID] = new List<string>();
            }
            playerClans[playerID].Add(clanID);

            guiderMessage.ShowMessage($"Successfully joined {clan.name}!", Color.green);
            return true;
        }

        public bool LeaveClan(string playerID, string clanID)
        {
            if (!clans.ContainsKey(clanID))
                return false;

            var clan = clans[clanID];

            if (!clan.memberIDs.Contains(playerID))
                return false;

            if (playerID == clan.founderID)
            {
                if (clan.memberIDs.Count > 1)
                {
                    guiderMessage.ShowMessage("You must transfer leadership before leaving the clan.", Color.red);
                    return false;
                }
                else
                {
                    // Last member leaving, dissolve the clan
                    DissolveClan(clanID);
                }
            }

            clan.memberIDs.Remove(playerID);
            clan.memberRoles.Remove(playerID);
            clan.memberHonor.Remove(playerID);
            playerClans[playerID].Remove(clanID);

            guiderMessage.ShowMessage($"You have left {clan.name}.", Color.white);
            return true;
        }

        public void DissolveClan(string clanID)
        {
            if (!clans.ContainsKey(clanID))
                return;

            var clan = clans[clanID];
            
            // Remove all members
            foreach (var memberID in clan.memberIDs.ToArray())
            {
                if (playerClans.ContainsKey(memberID))
                {
                    playerClans[memberID].Remove(clanID);
                }
            }

            // Break alliances
            foreach (var alliance in clan.alliances.Where(a => a.isActive))
            {
                if (clans.ContainsKey(alliance.clanID))
                {
                    var alliedClan = clans[alliance.clanID];
                    alliedClan.alliances.RemoveAll(a => a.clanID == clanID);
                }
            }

            // Remove from rival lists
            foreach (var rivalID in clan.rivalClans)
            {
                if (clans.ContainsKey(rivalID))
                {
                    clans[rivalID].rivalClans.Remove(clanID);
                }
            }

            clans.Remove(clanID);
            guiderMessage.ShowMessage($"Clan '{clan.name}' has been dissolved.", Color.yellow);
        }

        public bool FormAlliance(string clanID1, string clanID2, AllianceType type)
        {
            if (!clans.ContainsKey(clanID1) || !clans.ContainsKey(clanID2))
                return false;

            var clan1 = clans[clanID1];
            var clan2 = clans[clanID2];

            if (clan1.alliances.Count >= maxAlliances || clan2.alliances.Count >= maxAlliances)
            {
                guiderMessage.ShowMessage("Maximum number of alliances reached.", Color.red);
                return false;
            }

            if (clan1.rivalClans.Contains(clanID2) || clan2.rivalClans.Contains(clanID1))
            {
                guiderMessage.ShowMessage("Cannot form alliance with a rival clan.", Color.red);
                return false;
            }

            var alliance = new ClanAlliance
            {
                clanID = clanID2,
                type = type,
                formationDate = System.DateTime.Now,
                strength = 0f,
                isActive = true
            };

            var reciprocalAlliance = new ClanAlliance
            {
                clanID = clanID1,
                type = type,
                formationDate = System.DateTime.Now,
                strength = 0f,
                isActive = true
            };

            clan1.alliances.Add(alliance);
            clan2.alliances.Add(reciprocalAlliance);

            guiderMessage.ShowMessage($"Alliance formed between {clan1.name} and {clan2.name}!", Color.green);
            return true;
        }

        public bool DeclareRivalry(string clanID1, string clanID2)
        {
            if (!clans.ContainsKey(clanID1) || !clans.ContainsKey(clanID2))
                return false;

            var clan1 = clans[clanID1];
            var clan2 = clans[clanID2];

            if (clan1.rivalClans.Count >= maxRivals)
            {
                guiderMessage.ShowMessage("Maximum number of rivals reached.", Color.red);
                return false;
            }

            // Break any existing alliance
            var existingAlliance = clan1.alliances.Find(a => a.clanID == clanID2 && a.isActive);
            if (existingAlliance != null)
            {
                existingAlliance.isActive = false;
                var reciprocalAlliance = clan2.alliances.Find(a => a.clanID == clanID1 && a.isActive);
                if (reciprocalAlliance != null)
                {
                    reciprocalAlliance.isActive = false;
                }
            }

            clan1.rivalClans.Add(clanID2);
            clan2.rivalClans.Add(clanID1);

            guiderMessage.ShowMessage($"Rivalry declared between {clan1.name} and {clan2.name}!", Color.yellow);
            return true;
        }

        public void UpdateClans()
        {
            foreach (var clan in clans.Values)
            {
                // Update influence
                float influenceChange = 0f;

                // Base influence gain from members
                influenceChange += clan.memberIDs.Count * influenceGainRate * Time.deltaTime;

                // Alliance bonuses
                foreach (var alliance in clan.alliances.Where(a => a.isActive))
                {
                    influenceChange += alliance.strength * allianceStrengthGainRate * Time.deltaTime;
                    alliance.strength += allianceStrengthGainRate * Time.deltaTime;
                }

                // Rival penalties
                influenceChange -= clan.rivalClans.Count * rivalInfluencePenalty * Time.deltaTime;

                // Natural decay
                influenceChange -= clan.influence * influenceDecayRate * Time.deltaTime;

                clan.influence = Mathf.Max(0f, clan.influence + influenceChange);

                // Update traditions
                foreach (var tradition in clan.traditions.Where(t => t.isActive))
                {
                    // Apply tradition effects
                    foreach (var memberID in clan.memberIDs)
                    {
                        if (clan.memberHonor.ContainsKey(memberID))
                        {
                            clan.memberHonor[memberID] += tradition.honorBonus * Time.deltaTime;
                        }
                    }
                }

                // Clamp honor values
                foreach (var memberID in clan.memberIDs)
                {
                    if (clan.memberHonor.ContainsKey(memberID))
                    {
                        clan.memberHonor[memberID] = Mathf.Clamp(clan.memberHonor[memberID], minHonor, maxHonor);
                    }
                }
            }
        }

        public bool AddTradition(string clanID, ClanTradition tradition)
        {
            if (!clans.ContainsKey(clanID))
                return false;

            var clan = clans[clanID];
            
            if (clan.traditions.Any(t => t.name == tradition.name))
            {
                guiderMessage.ShowMessage("This tradition already exists.", Color.red);
                return false;
            }

            tradition.establishedDate = System.DateTime.Now;
            tradition.isActive = true;
            clan.traditions.Add(tradition);

            guiderMessage.ShowMessage($"New tradition '{tradition.name}' established!", Color.green);
            return true;
        }

        public bool ModifyHonor(string clanID, string playerID, float amount)
        {
            if (!clans.ContainsKey(clanID))
                return false;

            var clan = clans[clanID];
            
            if (!clan.memberHonor.ContainsKey(playerID))
                return false;

            float oldHonor = clan.memberHonor[playerID];
            float newHonor = Mathf.Clamp(oldHonor + amount, minHonor, maxHonor);
            clan.memberHonor[playerID] = newHonor;

            if (amount > 0)
                guiderMessage.ShowMessage($"Gained {amount:F0} honor!", Color.green);
            else
                guiderMessage.ShowMessage($"Lost {-amount:F0} honor!", Color.red);

            return true;
        }

        public Clan GetClan(string clanID)
        {
            return clans.ContainsKey(clanID) ? clans[clanID] : null;
        }

        public List<Clan> GetPlayerClans(string playerID)
        {
            if (!playerClans.ContainsKey(playerID))
                return new List<Clan>();

            return playerClans[playerID].Select(clanID => clans[clanID]).ToList();
        }

        public List<Clan> GetOpenClans()
        {
            return clans.Values.Where(c => c.isOpen).ToList();
        }

        public List<Clan> GetClansByFocus(ClanFocus focus)
        {
            return clans.Values.Where(c => c.focus == focus).ToList();
        }

        public float GetPlayerHonor(string clanID, string playerID)
        {
            if (!clans.ContainsKey(clanID))
                return 0f;

            var clan = clans[clanID];
            return clan.memberHonor.ContainsKey(playerID) ? clan.memberHonor[playerID] : 0f;
        }

        public bool HasPermission(string clanID, string playerID, string permission)
        {
            if (!clans.ContainsKey(clanID) || !clans[clanID].memberRoles.ContainsKey(playerID))
                return false;

            var role = clans[clanID].memberRoles[playerID];
            return role.permissions.Contains(permission) || role.permissions.Contains("all");
        }
    }
}
