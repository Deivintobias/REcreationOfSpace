using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResourceUI : MonoBehaviour
{
    [System.Serializable]
    public class ResourceDisplay
    {
        public string resourceName;
        public Image icon;
        public Text quantityText;
        public Image progressBar;
    }

    [Header("UI References")]
    public GameObject resourcePanel;
    public GameObject resourceDisplayPrefab;
    public Transform resourceContainer;
    public Image gatherProgressBar;
    public Text gatherText;

    [Header("Visual Settings")]
    public Color naturalColor = Color.green;
    public Color sacredColor = Color.yellow;
    public Color knowledgeColor = Color.blue;
    public Color energyColor = Color.magenta;
    public Color refinedColor = Color.cyan;

    private Dictionary<string, ResourceDisplay> resourceDisplays = new Dictionary<string, ResourceDisplay>();
    private ResourceSystem resourceSystem;
    private bool isGathering = false;

    void Start()
    {
        resourceSystem = FindObjectOfType<ResourceSystem>();
        if (resourceSystem != null)
        {
            InitializeResourceDisplays();
        }

        // Initially hide gather progress
        if (gatherProgressBar != null)
        {
            gatherProgressBar.gameObject.SetActive(false);
        }
        if (gatherText != null)
        {
            gatherText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (resourceSystem != null)
        {
            UpdateResourceDisplays();
            UpdateGatherProgress();
        }
    }

    private void InitializeResourceDisplays()
    {
        // Clear existing displays
        foreach (Transform child in resourceContainer)
        {
            Destroy(child.gameObject);
        }
        resourceDisplays.Clear();

        // Create displays for each resource type
        foreach (var resource in ResourceSystem.baseResources)
        {
            CreateResourceDisplay(resource);
        }
    }

    private void CreateResourceDisplay(ResourceSystem.Resource resource)
    {
        if (resourceDisplayPrefab == null || resourceContainer == null) return;

        // Instantiate display
        GameObject displayObj = Instantiate(resourceDisplayPrefab, resourceContainer);
        ResourceDisplay display = new ResourceDisplay();

        // Setup components
        display.icon = displayObj.transform.Find("Icon")?.GetComponent<Image>();
        display.quantityText = displayObj.transform.Find("QuantityText")?.GetComponent<Text>();
        display.progressBar = displayObj.transform.Find("ProgressBar")?.GetComponent<Image>();

        // Set initial values
        if (display.icon != null)
        {
            display.icon.color = GetResourceColor(resource.type);
        }
        if (display.quantityText != null)
        {
            display.quantityText.text = $"{resource.name}: 0";
        }
        if (display.progressBar != null)
        {
            display.progressBar.fillAmount = 0f;
        }

        resourceDisplays[resource.name] = display;
    }

    private void UpdateResourceDisplays()
    {
        var inventory = resourceSystem.GetInventory();
        foreach (var kvp in inventory)
        {
            if (resourceDisplays.TryGetValue(kvp.Key, out ResourceDisplay display))
            {
                // Update quantity text
                if (display.quantityText != null)
                {
                    display.quantityText.text = $"{kvp.Key}: {Mathf.Floor(kvp.Value.quantity)}";
                }

                // Update progress bar
                if (display.progressBar != null)
                {
                    display.progressBar.fillAmount = kvp.Value.quantity / kvp.Value.maxStack;
                }
            }
        }
    }

    private void UpdateGatherProgress()
    {
        float progress = resourceSystem.GetGatherProgress();
        bool currentlyGathering = progress > 0f && progress < 1f;

        // Show/hide gather progress
        if (currentlyGathering != isGathering)
        {
            isGathering = currentlyGathering;
            if (gatherProgressBar != null)
            {
                gatherProgressBar.gameObject.SetActive(isGathering);
            }
            if (gatherText != null)
            {
                gatherText.gameObject.SetActive(isGathering);
            }
        }

        // Update progress
        if (isGathering)
        {
            if (gatherProgressBar != null)
            {
                gatherProgressBar.fillAmount = progress;
            }
            if (gatherText != null)
            {
                gatherText.text = $"Gathering... {Mathf.Floor(progress * 100)}%";
            }
        }
    }

    private Color GetResourceColor(ResourceSystem.ResourceType type)
    {
        switch (type)
        {
            case ResourceSystem.ResourceType.Natural:
                return naturalColor;
            case ResourceSystem.ResourceType.Sacred:
                return sacredColor;
            case ResourceSystem.ResourceType.Knowledge:
                return knowledgeColor;
            case ResourceSystem.ResourceType.Energy:
                return energyColor;
            case ResourceSystem.ResourceType.Refined:
                return refinedColor;
            default:
                return Color.white;
        }
    }

    public void ToggleVisibility()
    {
        if (resourcePanel != null)
        {
            resourcePanel.SetActive(!resourcePanel.activeSelf);
        }
    }
}
