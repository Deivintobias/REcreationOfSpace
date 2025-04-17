using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace REcreationOfSpace.UI
{
    public class CharacterMenu : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterClass
        {
            public string className;
            public Sprite classIcon;
            public string description;
            public float healthMultiplier = 1f;
            public float damageMultiplier = 1f;
            public float craftingBonus = 0f;
            public float farmingBonus = 0f;
        }

        [Header("Menu Panels")]
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private GameObject skillsPanel;
        [SerializeField] private GameObject equipmentPanel;
        [SerializeField] private GameObject classPanel;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI experienceText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI craftingLevelText;
        [SerializeField] private TextMeshProUGUI farmingLevelText;

        [Header("Equipment Slots")]
        [SerializeField] private Image weaponSlot;
        [SerializeField] private Image armorSlot;
        [SerializeField] private Image accessorySlot;
        [SerializeField] private Image toolSlot;

        [Header("Character Classes")]
        [SerializeField] private CharacterClass[] availableClasses;
        [SerializeField] private Transform classContainer;
        [SerializeField] private GameObject classButtonPrefab;

        private ExperienceManager experienceManager;
        private Health health;
        private CombatController combat;
        private CraftingSystem crafting;
        private CharacterClass currentClass;
        private Dictionary<string, float> skillLevels = new Dictionary<string, float>();

        private void Start()
        {
            // Get components
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                experienceManager = player.GetComponent<ExperienceManager>();
                health = player.GetComponent<Health>();
                combat = player.GetComponent<CombatController>();
            }

            crafting = FindObjectOfType<CraftingSystem>();

            // Initialize skill levels
            skillLevels["Farming"] = 1f;
            skillLevels["Mining"] = 1f;
            skillLevels["Woodcutting"] = 1f;
            skillLevels["Fishing"] = 1f;

            // Create class selection buttons
            CreateClassButtons();

            // Hide all panels initially
            HideAllPanels();
            statsPanel.SetActive(true);

            // Update UI
            UpdateStats();
        }

        private void CreateClassButtons()
        {
            if (classContainer == null || classButtonPrefab == null)
                return;

            foreach (var characterClass in availableClasses)
            {
                var buttonObj = Instantiate(classButtonPrefab, classContainer);
                var button = buttonObj.GetComponent<Button>();
                var icon = buttonObj.GetComponentInChildren<Image>();
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (icon != null)
                    icon.sprite = characterClass.classIcon;
                if (text != null)
                    text.text = characterClass.className;

                button.onClick.AddListener(() => SelectClass(characterClass));
            }
        }

        public void ShowPanel(string panelName)
        {
            HideAllPanels();
            switch (panelName.ToLower())
            {
                case "stats":
                    statsPanel.SetActive(true);
                    break;
                case "skills":
                    skillsPanel.SetActive(true);
                    break;
                case "equipment":
                    equipmentPanel.SetActive(true);
                    break;
                case "class":
                    classPanel.SetActive(true);
                    break;
            }
            UpdateStats();
        }

        private void HideAllPanels()
        {
            statsPanel.SetActive(false);
            skillsPanel.SetActive(false);
            equipmentPanel.SetActive(false);
            classPanel.SetActive(false);
        }

        private void SelectClass(CharacterClass newClass)
        {
            currentClass = newClass;

            // Apply class modifiers
            if (health != null)
            {
                // Modify max health
                var healthField = health.GetType().GetField("maxHealth",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (healthField != null)
                {
                    int baseHealth = 100;
                    healthField.SetValue(health, Mathf.RoundToInt(baseHealth * newClass.healthMultiplier));
                    health.Heal(999); // Full heal after changing max health
                }
            }

            if (combat != null)
            {
                // Modify damage
                var damageField = combat.GetType().GetField("attackDamage",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (damageField != null)
                {
                    int baseDamage = 10;
                    damageField.SetValue(combat, Mathf.RoundToInt(baseDamage * newClass.damageMultiplier));
                }
            }

            UpdateStats();
        }

        private void UpdateStats()
        {
            if (experienceManager != null)
            {
                levelText.text = $"Level: {experienceManager.GetCurrentLevel()}";
                experienceText.text = $"XP: {Mathf.Round(experienceManager.GetLevelProgress() * 100)}%";
            }

            if (health != null)
            {
                healthText.text = $"Health: {Mathf.Round(health.GetHealthPercentage() * 100)}%";
            }

            if (combat != null)
            {
                var damageField = combat.GetType().GetField("attackDamage",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (damageField != null)
                {
                    int damage = (int)damageField.GetValue(combat);
                    damageText.text = $"Damage: {damage}";
                }
            }

            if (crafting != null)
            {
                craftingLevelText.text = $"Crafting: {crafting.GetCraftingLevel()}";
            }

            farmingLevelText.text = $"Farming: {Mathf.Floor(skillLevels["Farming"])}";
        }

        public void GainSkillExperience(string skillName, float amount)
        {
            if (!skillLevels.ContainsKey(skillName))
                return;

            float currentLevel = skillLevels[skillName];
            float bonusMultiplier = (currentClass != null && skillName == "Farming") ? currentClass.farmingBonus : 1f;
            
            skillLevels[skillName] += amount * bonusMultiplier;
            UpdateStats();
        }

        public float GetSkillLevel(string skillName)
        {
            return skillLevels.ContainsKey(skillName) ? skillLevels[skillName] : 1f;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            UpdateStats();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }
    }
}
