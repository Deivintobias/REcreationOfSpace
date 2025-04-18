using UnityEngine;
using System.Collections.Generic;
using REcreationOfSpace.Character;
using REcreationOfSpace.UI;

namespace REcreationOfSpace.Companion
{
    public class CompanionSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Companion
        {
            public string id;
            public string name;
            public CompanionType type;
            public CompanionCategory category;
            public float loyalty;
            public float happiness;
            public float energy;
            public float health;
            public float experience;
            public int level;
            public List<string> abilities = new List<string>();
            public Dictionary<string, float> stats = new Dictionary<string, float>();
            public bool isActive;
            public Vector3 position;
            public Quaternion rotation;
            public bool isMounted;
            public float trainingProgress;
            public System.DateTime lastFed;
            public System.DateTime lastRest;
        }

        public enum CompanionType
        {
            // Biblical Era Mounts
            Horse,
            Camel,
            Donkey,
            Mule,

            // Combat Pets
            Lion,
            Wolf,
            Eagle,
            Bear,

            // Utility Pets
            Dove,
            Sheep,
            Ox,
            Goat,

            // Special Companions
            Phoenix,
            Unicorn,
            Griffin,
            Dragon
        }

        public enum CompanionCategory
        {
            Mount,      // Can be ridden
            Combat,     // Assists in battle
            Utility,    // Helps with tasks
            Special     // Unique abilities
        }

        [Header("Companion Settings")]
        [SerializeField] private float maxLoyalty = 100f;
        [SerializeField] private float maxHappiness = 100f;
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float baseExperienceGain = 10f;
        [SerializeField] private float loyaltyDecayRate = 0.1f;
        [SerializeField] private float happinessDecayRate = 0.2f;
        [SerializeField] private float energyDecayRate = 0.3f;
        [SerializeField] private float mountSpeedMultiplier = 2f;
        [SerializeField] private int maxActiveCompanions = 3;
        [SerializeField] private float companionUpdateInterval = 1f;

        [Header("Training Settings")]
        [SerializeField] private float baseTrainingRate = 0.1f;
        [SerializeField] private float trainingBoostPerLevel = 0.05f;
        [SerializeField] private float maxTrainingTimePerDay = 4f;

        [Header("UI References")]
        [SerializeField] private GuiderMessageUI guiderMessage;

        private Dictionary<string, Companion> companions = new Dictionary<string, Companion>();
        private List<string> activeCompanionIds = new List<string>();
        private float updateTimer;

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= companionUpdateInterval)
            {
                UpdateCompanions();
                updateTimer = 0f;
            }
        }

        public bool AddCompanion(string name, CompanionType type)
        {
            string id = System.Guid.NewGuid().ToString();
            var companion = new Companion
            {
                id = id,
                name = name,
                type = type,
                category = GetCompanionCategory(type),
                loyalty = maxLoyalty / 2f,
                happiness = maxHappiness / 2f,
                energy = maxEnergy,
                health = 100f,
                experience = 0f,
                level = 1,
                isActive = false,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                isMounted = false,
                trainingProgress = 0f,
                lastFed = System.DateTime.Now,
                lastRest = System.DateTime.Now
            };

            // Initialize base stats based on type
            InitializeCompanionStats(companion);

            companions[id] = companion;
            guiderMessage.ShowMessage($"{name} has joined your companions!", Color.green);
            return true;
        }

        private void InitializeCompanionStats(Companion companion)
        {
            switch (companion.category)
            {
                case CompanionCategory.Mount:
                    companion.stats["Speed"] = 10f;
                    companion.stats["Stamina"] = 100f;
                    companion.stats["CarryWeight"] = 100f;
                    break;

                case CompanionCategory.Combat:
                    companion.stats["Attack"] = 15f;
                    companion.stats["Defense"] = 10f;
                    companion.stats["Agility"] = 12f;
                    break;

                case CompanionCategory.Utility:
                    companion.stats["Efficiency"] = 10f;
                    companion.stats["Range"] = 15f;
                    companion.stats["Capacity"] = 50f;
                    break;

                case CompanionCategory.Special:
                    companion.stats["Power"] = 20f;
                    companion.stats["Wisdom"] = 15f;
                    companion.stats["Influence"] = 25f;
                    break;
            }

            // Add common stats
            companion.stats["Loyalty"] = companion.loyalty;
            companion.stats["Happiness"] = companion.happiness;
            companion.stats["Energy"] = companion.energy;
        }

        private CompanionCategory GetCompanionCategory(CompanionType type)
        {
            if (type <= CompanionType.Mule)
                return CompanionCategory.Mount;
            if (type <= CompanionType.Bear)
                return CompanionCategory.Combat;
            if (type <= CompanionType.Goat)
                return CompanionCategory.Utility;
            return CompanionCategory.Special;
        }

        public bool ActivateCompanion(string companionId)
        {
            if (!companions.ContainsKey(companionId))
                return false;

            if (activeCompanionIds.Count >= maxActiveCompanions)
            {
                guiderMessage.ShowMessage($"Cannot activate more than {maxActiveCompanions} companions.", Color.red);
                return false;
            }

            var companion = companions[companionId];
            companion.isActive = true;
            activeCompanionIds.Add(companionId);

            guiderMessage.ShowMessage($"{companion.name} is now active!", Color.green);
            return true;
        }

        public bool DeactivateCompanion(string companionId)
        {
            if (!companions.ContainsKey(companionId))
                return false;

            var companion = companions[companionId];
            if (companion.isMounted)
            {
                guiderMessage.ShowMessage("Cannot deactivate while mounted.", Color.red);
                return false;
            }

            companion.isActive = false;
            activeCompanionIds.Remove(companionId);

            guiderMessage.ShowMessage($"{companion.name} is now resting.", Color.white);
            return true;
        }

        public bool MountCompanion(string companionId)
        {
            if (!companions.ContainsKey(companionId))
                return false;

            var companion = companions[companionId];
            
            if (companion.category != CompanionCategory.Mount)
            {
                guiderMessage.ShowMessage($"{companion.name} cannot be mounted.", Color.red);
                return false;
            }

            if (companion.energy < 20f)
            {
                guiderMessage.ShowMessage($"{companion.name} is too tired to be mounted.", Color.red);
                return false;
            }

            companion.isMounted = true;
            // Apply mount effects (speed boost, etc.)
            // This would interface with the player's movement system

            guiderMessage.ShowMessage($"Mounted {companion.name}!", Color.green);
            return true;
        }

        public bool DismountCompanion(string companionId)
        {
            if (!companions.ContainsKey(companionId))
                return false;

            var companion = companions[companionId];
            companion.isMounted = false;
            // Remove mount effects

            guiderMessage.ShowMessage($"Dismounted {companion.name}.", Color.white);
            return true;
        }

        public bool FeedCompanion(string companionId, float nutritionValue)
        {
            if (!companions.ContainsKey(companionId))
                return false;

            var companion = companions[companionId];
            
            if ((System.DateTime.Now - companion.lastFed).TotalHours < 4)
            {
                guiderMessage.ShowMessage($"{companion.name} is not hungry yet.", Color.yellow);
                return false;
            }

            companion.happiness += nutritionValue * 0.2f;
            companion.energy += nutritionValue * 0.5f;
            companion.lastFed = System.DateTime.Now;

            companion.happiness = Mathf.Clamp(companion.happiness, 0f, maxHappiness);
            companion.energy = Mathf.Clamp(companion.energy, 0f, maxEnergy);

            guiderMessage.ShowMessage($"Fed {companion.name}!", Color.green);
            return true;
        }

        public bool TrainCompanion(string companionId, float duration)
        {
            if (!companions.ContainsKey(companionId))
                return false;

            var companion = companions[companionId];

            if (companion.energy < 30f)
            {
                guiderMessage.ShowMessage($"{companion.name} is too tired to train.", Color.red);
                return false;
            }

            float trainingRate = baseTrainingRate + (companion.level - 1) * trainingBoostPerLevel;
            companion.trainingProgress += trainingRate * duration;
            companion.energy -= duration * 10f;

            if (companion.trainingProgress >= 100f)
            {
                LevelUpCompanion(companion);
                companion.trainingProgress = 0f;
            }

            companion.energy = Mathf.Max(0f, companion.energy);
            guiderMessage.ShowMessage($"Trained {companion.name}!", Color.green);
            return true;
        }

        private void LevelUpCompanion(Companion companion)
        {
            companion.level++;
            
            // Improve stats based on companion category
            foreach (var stat in companion.stats.Keys.ToList())
            {
                companion.stats[stat] *= 1.1f; // 10% increase per level
            }

            // Add new abilities at certain levels
            if (companion.level % 5 == 0)
            {
                AddNewAbility(companion);
            }

            guiderMessage.ShowMessage($"{companion.name} has reached level {companion.level}!", Color.green);
        }

        private void AddNewAbility(Companion companion)
        {
            string newAbility = "";
            switch (companion.category)
            {
                case CompanionCategory.Mount:
                    newAbility = companion.level == 5 ? "Sprint" :
                                companion.level == 10 ? "Endurance" :
                                companion.level == 15 ? "Sure-footed" :
                                "Master's Grace";
                    break;

                case CompanionCategory.Combat:
                    newAbility = companion.level == 5 ? "Quick Strike" :
                                companion.level == 10 ? "Guard" :
                                companion.level == 15 ? "Battle Cry" :
                                "War Master";
                    break;

                case CompanionCategory.Utility:
                    newAbility = companion.level == 5 ? "Keen Eye" :
                                companion.level == 10 ? "Resource Sense" :
                                companion.level == 15 ? "Efficiency" :
                                "Master Gatherer";
                    break;

                case CompanionCategory.Special:
                    newAbility = companion.level == 5 ? "Divine Touch" :
                                companion.level == 10 ? "Blessing" :
                                companion.level == 15 ? "Miracle" :
                                "Divine Grace";
                    break;
            }

            if (!string.IsNullOrEmpty(newAbility))
            {
                companion.abilities.Add(newAbility);
                guiderMessage.ShowMessage($"{companion.name} learned {newAbility}!", Color.green);
            }
        }

        private void UpdateCompanions()
        {
            foreach (var companion in companions.Values)
            {
                if (!companion.isActive)
                    continue;

                // Update stats
                companion.loyalty = Mathf.Max(0f, companion.loyalty - loyaltyDecayRate * Time.deltaTime);
                companion.happiness = Mathf.Max(0f, companion.happiness - happinessDecayRate * Time.deltaTime);
                companion.energy = Mathf.Max(0f, companion.energy - (companion.isMounted ? energyDecayRate * 2f : energyDecayRate) * Time.deltaTime);

                // Rest regeneration
                if ((System.DateTime.Now - companion.lastRest).TotalHours >= 8)
                {
                    companion.energy = maxEnergy;
                    companion.lastRest = System.DateTime.Now;
                }

                // Experience gain for active companions
                if (companion.isActive && !companion.isMounted)
                {
                    companion.experience += baseExperienceGain * Time.deltaTime;
                    if (companion.experience >= 100f * companion.level)
                    {
                        LevelUpCompanion(companion);
                        companion.experience = 0f;
                    }
                }

                // Update position for following behavior
                if (companion.isActive && !companion.isMounted)
                {
                    // Implement following logic here
                    // This would update companion.position and companion.rotation
                    // based on player position and obstacles
                }
            }
        }

        public Companion GetCompanion(string companionId)
        {
            return companions.ContainsKey(companionId) ? companions[companionId] : null;
        }

        public List<Companion> GetActiveCompanions()
        {
            return activeCompanionIds.Select(id => companions[id]).ToList();
        }

        public List<Companion> GetCompanionsByCategory(CompanionCategory category)
        {
            return companions.Values.Where(c => c.category == category).ToList();
        }

        public bool HasActiveMount()
        {
            return companions.Values.Any(c => c.isActive && c.isMounted);
        }

        public float GetCompanionStat(string companionId, string statName)
        {
            if (!companions.ContainsKey(companionId))
                return 0f;

            var companion = companions[companionId];
            return companion.stats.ContainsKey(statName) ? companion.stats[statName] : 0f;
        }

        public List<string> GetCompanionAbilities(string companionId)
        {
            if (!companions.ContainsKey(companionId))
                return new List<string>();

            return companions[companionId].abilities;
        }
    }
}
