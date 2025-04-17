using UnityEngine;

namespace REcreationOfSpace.Inventory
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string id;
        public new string name;
        public string description;
        public ItemType type;
        public ItemRarity rarity;

        [Header("Visual")]
        public Sprite icon;
        public GameObject prefab;

        [Header("Properties")]
        public int maxStackSize = 99;
        public float weight = 1f;
        public bool isQuestItem = false;
        public string[] tags;

        [Header("Stats")]
        public float damage;
        public float armor;
        public float durability;
        public float healAmount;
        public float craftingSpeed;
        public float farmingSpeed;

        [Header("Requirements")]
        public int levelRequirement;
        public string[] requiredTags;
        public bool requiresDivineBlessing;

        [Header("Effects")]
        public ParticleSystem useEffect;
        public AudioClip useSound;
        public string[] statusEffects;

        private void OnValidate()
        {
            // Ensure ID is set
            if (string.IsNullOrEmpty(id))
            {
                id = name.ToLower().Replace(" ", "_");
            }

            // Validate stack size
            if (maxStackSize < 1)
                maxStackSize = 1;

            // Validate weight
            if (weight < 0)
                weight = 0;

            // Validate stats
            if (damage < 0) damage = 0;
            if (armor < 0) armor = 0;
            if (durability < 0) durability = 0;
            if (healAmount < 0) healAmount = 0;
            if (craftingSpeed < 0) craftingSpeed = 0;
            if (farmingSpeed < 0) farmingSpeed = 0;

            // Validate level requirement
            if (levelRequirement < 0)
                levelRequirement = 0;
        }

        public string GetTooltipText()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Basic info
            sb.AppendLine($"<color=#{GetRarityColor()}>{name}</color>");
            sb.AppendLine($"Type: {type}");
            sb.AppendLine();
            sb.AppendLine(description);
            sb.AppendLine();

            // Stats
            if (damage > 0)
                sb.AppendLine($"Damage: {damage}");
            if (armor > 0)
                sb.AppendLine($"Armor: {armor}");
            if (durability > 0)
                sb.AppendLine($"Durability: {durability}");
            if (healAmount > 0)
                sb.AppendLine($"Heal Amount: {healAmount}");
            if (craftingSpeed > 0)
                sb.AppendLine($"Crafting Speed: +{craftingSpeed}%");
            if (farmingSpeed > 0)
                sb.AppendLine($"Farming Speed: +{farmingSpeed}%");

            // Requirements
            if (levelRequirement > 0)
                sb.AppendLine($"\nRequired Level: {levelRequirement}");
            if (requiresDivineBlessing)
                sb.AppendLine("Requires Divine Blessing");

            // Additional info
            if (isQuestItem)
                sb.AppendLine("\n<color=yellow>Quest Item</color>");
            
            sb.AppendLine($"\nWeight: {weight:F1}");
            sb.AppendLine($"Max Stack: {maxStackSize}");

            return sb.ToString();
        }

        private string GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.Common => "FFFFFF",
                ItemRarity.Uncommon => "1EFF00",
                ItemRarity.Rare => "0070DD",
                ItemRarity.Epic => "A335EE",
                ItemRarity.Legendary => "FF8000",
                ItemRarity.Divine => "E6CC80",
                _ => "FFFFFF"
            };
        }

        public bool CanUse(GameObject user)
        {
            // Check level requirement
            if (levelRequirement > 0)
            {
                var exp = user.GetComponent<ExperienceManager>();
                if (exp != null && exp.GetLevel() < levelRequirement)
                    return false;
            }

            // Check divine blessing requirement
            if (requiresDivineBlessing)
            {
                // Implementation depends on your blessing system
                // return user.GetComponent<BlessingSystem>()?.HasBlessing() ?? false;
            }

            // Check required tags
            if (requiredTags != null && requiredTags.Length > 0)
            {
                foreach (var tag in requiredTags)
                {
                    // Implementation depends on your tag system
                    // if (!user.GetComponent<TagSystem>()?.HasTag(tag) ?? true)
                    //     return false;
                }
            }

            return true;
        }

        public void PlayUseEffects(Vector3 position)
        {
            if (useEffect != null)
            {
                var effect = Instantiate(useEffect, position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            if (useSound != null)
            {
                AudioSource.PlayClipAtPoint(useSound, position);
            }
        }
    }
}
