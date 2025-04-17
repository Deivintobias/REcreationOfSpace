using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace REcreationOfSpace.Crafting
{
    public class CraftingSystem : MonoBehaviour
    {
        [System.Serializable]
        public class CraftingRecipe
        {
            public string resultItem;
            public int resultAmount = 1;
            public string[] requiredItems;
            public int[] requiredAmounts;
            public float craftingTime = 2f;
            public string requiredWorkbench; // Leave empty if no workbench needed
            public int requiredLevel = 1; // Crafting skill level required
        }

        [System.Serializable]
        public class WorkbenchType
        {
            public string workbenchName;
            public string[] allowedRecipeTypes; // Categories of items that can be crafted here
            public GameObject workbenchPrefab;
        }

        [Header("Crafting Settings")]
        [SerializeField] private CraftingRecipe[] availableRecipes;
        [SerializeField] private WorkbenchType[] workbenchTypes;
        [SerializeField] private float skillGainPerCraft = 0.1f;

        [Header("UI References")]
        [SerializeField] private GameObject craftingUIPrefab;

        public UnityEvent<string, int> onItemCrafted;
        public UnityEvent<float> onCraftingProgress;
        public UnityEvent<int> onCraftingLevelUp;

        private ResourceSystem resourceSystem;
        private Dictionary<string, float> craftingProgress = new Dictionary<string, float>();
        private Dictionary<string, CraftingRecipe> activeRecipes = new Dictionary<string, CraftingRecipe>();
        private float craftingSkill = 1f;
        private int craftingLevel = 1;

        private void Start()
        {
            resourceSystem = FindObjectOfType<ResourceSystem>();
        }

        private void Update()
        {
            // Update active crafting processes
            var recipeKeys = new List<string>(activeRecipes.Keys);
            foreach (var key in recipeKeys)
            {
                UpdateCrafting(key);
            }
        }

        public bool CanCraftRecipe(string recipeName, string workbenchType = "")
        {
            var recipe = GetRecipe(recipeName);
            if (recipe == null || resourceSystem == null)
                return false;

            // Check crafting level requirement
            if (craftingLevel < recipe.requiredLevel)
                return false;

            // Check workbench requirement
            if (!string.IsNullOrEmpty(recipe.requiredWorkbench) && 
                recipe.requiredWorkbench != workbenchType)
                return false;

            // Check required resources
            for (int i = 0; i < recipe.requiredItems.Length; i++)
            {
                if (!resourceSystem.HasResource(recipe.requiredItems[i], recipe.requiredAmounts[i]))
                    return false;
            }

            return true;
        }

        public void StartCrafting(string recipeName, string craftingId)
        {
            if (!CanCraftRecipe(recipeName))
                return;

            var recipe = GetRecipe(recipeName);

            // Consume resources
            for (int i = 0; i < recipe.requiredItems.Length; i++)
            {
                resourceSystem.UseResource(recipe.requiredItems[i], recipe.requiredAmounts[i]);
            }

            // Start crafting process
            craftingProgress[craftingId] = 0f;
            activeRecipes[craftingId] = recipe;
        }

        public void CancelCrafting(string craftingId)
        {
            if (!activeRecipes.ContainsKey(craftingId))
                return;

            var recipe = activeRecipes[craftingId];

            // Return resources
            for (int i = 0; i < recipe.requiredItems.Length; i++)
            {
                resourceSystem.AddResource(recipe.requiredItems[i], recipe.requiredAmounts[i]);
            }

            // Remove crafting process
            activeRecipes.Remove(craftingId);
            craftingProgress.Remove(craftingId);
        }

        private void UpdateCrafting(string craftingId)
        {
            if (!activeRecipes.ContainsKey(craftingId))
                return;

            var recipe = activeRecipes[craftingId];
            
            // Update progress
            float progress = craftingProgress[craftingId];
            progress += Time.deltaTime / recipe.craftingTime;
            craftingProgress[craftingId] = progress;

            onCraftingProgress?.Invoke(progress);

            // Check for completion
            if (progress >= 1f)
            {
                CompleteCrafting(craftingId);
            }
        }

        private void CompleteCrafting(string craftingId)
        {
            var recipe = activeRecipes[craftingId];

            // Add crafted item
            if (resourceSystem != null)
            {
                resourceSystem.AddResource(recipe.resultItem, recipe.resultAmount);
            }

            // Update crafting skill
            craftingSkill += skillGainPerCraft;
            CheckLevelUp();

            // Notify listeners
            onItemCrafted?.Invoke(recipe.resultItem, recipe.resultAmount);

            // Clean up
            activeRecipes.Remove(craftingId);
            craftingProgress.Remove(craftingId);
        }

        private void CheckLevelUp()
        {
            int newLevel = Mathf.FloorToInt(craftingSkill);
            if (newLevel > craftingLevel)
            {
                craftingLevel = newLevel;
                onCraftingLevelUp?.Invoke(craftingLevel);
            }
        }

        private CraftingRecipe GetRecipe(string recipeName)
        {
            return System.Array.Find(availableRecipes, recipe => recipe.resultItem == recipeName);
        }

        public WorkbenchType GetWorkbenchType(string workbenchName)
        {
            return System.Array.Find(workbenchTypes, wb => wb.workbenchName == workbenchName);
        }

        public CraftingRecipe[] GetAvailableRecipes(string workbenchType = "")
        {
            if (string.IsNullOrEmpty(workbenchType))
            {
                return System.Array.FindAll(availableRecipes, 
                    recipe => string.IsNullOrEmpty(recipe.requiredWorkbench));
            }
            
            return System.Array.FindAll(availableRecipes, 
                recipe => recipe.requiredWorkbench == workbenchType);
        }

        public float GetCraftingProgress(string craftingId)
        {
            return craftingProgress.ContainsKey(craftingId) ? craftingProgress[craftingId] : 0f;
        }

        public int GetCraftingLevel()
        {
            return craftingLevel;
        }

        public bool IsCrafting(string craftingId)
        {
            return activeRecipes.ContainsKey(craftingId);
        }
    }
}
