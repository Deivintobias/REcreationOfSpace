using UnityEngine;
using REcreationOfSpace.Crafting;
using REcreationOfSpace.Farming;

namespace REcreationOfSpace.Setup
{
    public class GameSetup : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private CraftingSystem craftingSystem;
        [SerializeField] private ResourceSystem resourceSystem;

        private void Start()
        {
            SetupDefaultResources();
            SetupCraftingRecipes();
            SetupCropTypes();
        }

        private void SetupDefaultResources()
        {
            if (resourceSystem == null) return;

            // Add starting resources
            var startingResources = new[]
            {
                new { type = "Wood", amount = 20 },
                new { type = "Stone", amount = 15 },
                new { type = "Iron", amount = 10 },
                new { type = "Seeds", amount = 5 },
                new { type = "Water", amount = 10 },
                new { type = "Gold", amount = 0 }
            };

            foreach (var resource in startingResources)
            {
                resourceSystem.AddResource(resource.type, resource.amount);
            }
        }

        private void SetupCraftingRecipes()
        {
            if (craftingSystem == null) return;

            // Basic crafting recipes (no workbench required)
            var basicRecipes = new[]
            {
                new CraftingSystem.CraftingRecipe
                {
                    resultItem = "Wooden Tools",
                    resultAmount = 1,
                    requiredItems = new[] { "Wood" },
                    requiredAmounts = new[] { 3 },
                    craftingTime = 2f,
                    requiredLevel = 1
                },
                new CraftingSystem.CraftingRecipe
                {
                    resultItem = "Stone Tools",
                    resultAmount = 1,
                    requiredItems = new[] { "Stone", "Wood" },
                    requiredAmounts = new[] { 3, 2 },
                    craftingTime = 3f,
                    requiredLevel = 2
                }
            };

            // Smithy recipes
            var smithyRecipes = new[]
            {
                new CraftingSystem.CraftingRecipe
                {
                    resultItem = "Iron Tools",
                    resultAmount = 1,
                    requiredItems = new[] { "Iron", "Wood" },
                    requiredAmounts = new[] { 3, 2 },
                    craftingTime = 5f,
                    requiredWorkbench = "Smithy",
                    requiredLevel = 3
                },
                new CraftingSystem.CraftingRecipe
                {
                    resultItem = "Iron Sword",
                    resultAmount = 1,
                    requiredItems = new[] { "Iron", "Wood" },
                    requiredAmounts = new[] { 5, 2 },
                    craftingTime = 8f,
                    requiredWorkbench = "Smithy",
                    requiredLevel = 4
                }
            };

            // Alchemy recipes
            var alchemyRecipes = new[]
            {
                new CraftingSystem.CraftingRecipe
                {
                    resultItem = "Health Potion",
                    resultAmount = 1,
                    requiredItems = new[] { "Herbs", "Water" },
                    requiredAmounts = new[] { 2, 1 },
                    craftingTime = 4f,
                    requiredWorkbench = "Alchemy",
                    requiredLevel = 2
                },
                new CraftingSystem.CraftingRecipe
                {
                    resultItem = "Fertilizer",
                    resultAmount = 2,
                    requiredItems = new[] { "Herbs", "Water", "Stone" },
                    requiredAmounts = new[] { 1, 1, 1 },
                    craftingTime = 3f,
                    requiredWorkbench = "Alchemy",
                    requiredLevel = 1
                }
            };

            // Add all recipes to crafting system
            foreach (var recipe in basicRecipes)
            {
                AddRecipeToSystem(recipe);
            }
            foreach (var recipe in smithyRecipes)
            {
                AddRecipeToSystem(recipe);
            }
            foreach (var recipe in alchemyRecipes)
            {
                AddRecipeToSystem(recipe);
            }
        }

        private void SetupCropTypes()
        {
            // Define crop types and their properties
            var cropTypes = new[]
            {
                new FarmPlot.CropData
                {
                    cropType = "Wheat",
                    growthTime = 60f, // 1 minute for testing, adjust as needed
                    growthStages = 4,
                    requiredResources = new[] { "Seeds", "Water" },
                    resourceAmounts = new[] { 1, 1 },
                    harvestedResource = "Wheat",
                    harvestAmount = 3
                },
                new FarmPlot.CropData
                {
                    cropType = "Herbs",
                    growthTime = 45f,
                    growthStages = 3,
                    requiredResources = new[] { "Seeds", "Water" },
                    resourceAmounts = new[] { 1, 2 },
                    harvestedResource = "Herbs",
                    harvestAmount = 2
                },
                new FarmPlot.CropData
                {
                    cropType = "Magic Mushrooms",
                    growthTime = 90f,
                    growthStages = 5,
                    requiredResources = new[] { "Seeds", "Water", "Fertilizer" },
                    resourceAmounts = new[] { 2, 2, 1 },
                    harvestedResource = "Magic Essence",
                    harvestAmount = 1
                }
            };

            // Add crop types to farm plots
            var farmPlots = FindObjectsOfType<FarmPlot>();
            foreach (var plot in farmPlots)
            {
                var plotField = plot.GetType().GetField("availableCrops", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                if (plotField != null)
                {
                    plotField.SetValue(plot, cropTypes);
                }
            }
        }

        private void AddRecipeToSystem(CraftingSystem.CraftingRecipe recipe)
        {
            var recipesField = craftingSystem.GetType().GetField("availableRecipes", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);

            if (recipesField != null)
            {
                var currentRecipes = (CraftingSystem.CraftingRecipe[])recipesField.GetValue(craftingSystem);
                var newRecipes = new CraftingSystem.CraftingRecipe[currentRecipes.Length + 1];
                currentRecipes.CopyTo(newRecipes, 0);
                newRecipes[currentRecipes.Length] = recipe;
                recipesField.SetValue(craftingSystem, newRecipes);
            }
        }
    }
}
