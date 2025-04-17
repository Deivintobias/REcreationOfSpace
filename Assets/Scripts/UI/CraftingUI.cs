using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace REcreationOfSpace.UI
{
    public class CraftingUI : MonoBehaviour
    {
        [System.Serializable]
        private class RecipeUI
        {
            public GameObject recipeObject;
            public TextMeshProUGUI recipeName;
            public TextMeshProUGUI requirements;
            public Image progressBar;
            public Button craftButton;
            public Button cancelButton;
        }

        [Header("UI References")]
        [SerializeField] private GameObject recipeEntryPrefab;
        [SerializeField] private Transform recipeContainer;
        [SerializeField] private TextMeshProUGUI workbenchTitle;
        [SerializeField] private TextMeshProUGUI craftingLevel;
        [SerializeField] private GameObject requirementsPanel;
        [SerializeField] private TextMeshProUGUI requirementsText;

        [Header("Settings")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color unavailableColor = Color.gray;
        [SerializeField] private Color progressColor = Color.green;

        private CraftingSystem craftingSystem;
        private ResourceSystem resourceSystem;
        private Dictionary<string, RecipeUI> recipeUIs = new Dictionary<string, RecipeUI>();
        private string currentWorkbench = "";

        private void Start()
        {
            craftingSystem = FindObjectOfType<CraftingSystem>();
            resourceSystem = FindObjectOfType<ResourceSystem>();

            if (craftingSystem != null)
            {
                craftingSystem.onCraftingProgress.AddListener(UpdateProgress);
                craftingSystem.onCraftingLevelUp.AddListener(UpdateCraftingLevel);
            }

            gameObject.SetActive(false);
        }

        public void Show(string workbenchType = "")
        {
            currentWorkbench = workbenchType;
            gameObject.SetActive(true);
            UpdateUI();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void UpdateUI()
        {
            ClearRecipes();

            // Update title
            if (workbenchTitle != null)
            {
                workbenchTitle.text = string.IsNullOrEmpty(currentWorkbench) ? 
                    "Basic Crafting" : currentWorkbench;
            }

            // Update crafting level
            UpdateCraftingLevel(craftingSystem.GetCraftingLevel());

            // Create recipe entries
            var recipes = craftingSystem.GetAvailableRecipes(currentWorkbench);
            foreach (var recipe in recipes)
            {
                CreateRecipeEntry(recipe);
            }
        }

        private void CreateRecipeEntry(CraftingSystem.CraftingRecipe recipe)
        {
            if (recipeEntryPrefab == null || recipeContainer == null)
                return;

            // Create UI elements
            var entryObject = Instantiate(recipeEntryPrefab, recipeContainer);
            var recipeUI = new RecipeUI
            {
                recipeObject = entryObject,
                recipeName = entryObject.GetComponentInChildren<TextMeshProUGUI>(),
                progressBar = entryObject.GetComponentInChildren<Image>(),
                craftButton = entryObject.GetComponentInChildren<Button>()
            };

            // Set up recipe info
            if (recipeUI.recipeName != null)
                recipeUI.recipeName.text = recipe.resultItem;

            // Set up requirements
            string reqText = "";
            for (int i = 0; i < recipe.requiredItems.Length; i++)
            {
                reqText += $"{recipe.requiredItems[i]} x{recipe.requiredAmounts[i]}\n";
            }
            if (recipe.requiredLevel > 1)
                reqText += $"Level {recipe.requiredLevel} Required\n";

            // Set up buttons
            if (recipeUI.craftButton != null)
            {
                recipeUI.craftButton.onClick.AddListener(() => StartCrafting(recipe.resultItem));
                UpdateRecipeAvailability(recipe, recipeUI);
            }

            // Hide progress bar initially
            if (recipeUI.progressBar != null)
            {
                recipeUI.progressBar.gameObject.SetActive(false);
            }

            recipeUIs[recipe.resultItem] = recipeUI;
        }

        private void StartCrafting(string recipeName)
        {
            if (craftingSystem == null)
                return;

            string craftingId = System.Guid.NewGuid().ToString();
            if (craftingSystem.CanCraftRecipe(recipeName, currentWorkbench))
            {
                craftingSystem.StartCrafting(recipeName, craftingId);
                UpdateRecipeUI(recipeName);
            }
        }

        private void UpdateProgress(float progress)
        {
            foreach (var recipeUI in recipeUIs.Values)
            {
                if (recipeUI.progressBar != null)
                {
                    recipeUI.progressBar.fillAmount = progress;
                }
            }
        }

        private void UpdateCraftingLevel(int level)
        {
            if (craftingLevel != null)
            {
                craftingLevel.text = $"Crafting Level: {level}";
            }

            // Update recipe availability
            var recipes = craftingSystem.GetAvailableRecipes(currentWorkbench);
            foreach (var recipe in recipes)
            {
                if (recipeUIs.ContainsKey(recipe.resultItem))
                {
                    UpdateRecipeAvailability(recipe, recipeUIs[recipe.resultItem]);
                }
            }
        }

        private void UpdateRecipeUI(string recipeName)
        {
            if (!recipeUIs.ContainsKey(recipeName))
                return;

            var recipeUI = recipeUIs[recipeName];
            bool isCrafting = craftingSystem.IsCrafting(recipeName);

            if (recipeUI.progressBar != null)
                recipeUI.progressBar.gameObject.SetActive(isCrafting);

            if (recipeUI.craftButton != null)
                recipeUI.craftButton.interactable = !isCrafting;
        }

        private void UpdateRecipeAvailability(CraftingSystem.CraftingRecipe recipe, RecipeUI recipeUI)
        {
            bool available = craftingSystem.CanCraftRecipe(recipe.resultItem, currentWorkbench);

            if (recipeUI.recipeName != null)
                recipeUI.recipeName.color = available ? availableColor : unavailableColor;

            if (recipeUI.craftButton != null)
                recipeUI.craftButton.interactable = available;
        }

        private void ClearRecipes()
        {
            foreach (var recipeUI in recipeUIs.Values)
            {
                if (recipeUI.recipeObject != null)
                    Destroy(recipeUI.recipeObject);
            }
            recipeUIs.Clear();
        }

        private void OnDestroy()
        {
            if (craftingSystem != null)
            {
                craftingSystem.onCraftingProgress.RemoveListener(UpdateProgress);
                craftingSystem.onCraftingLevelUp.RemoveListener(UpdateCraftingLevel);
            }
        }
    }
}
