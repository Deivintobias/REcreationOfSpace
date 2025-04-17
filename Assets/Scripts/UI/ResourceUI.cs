using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace REcreationOfSpace.UI
{
    public class ResourceUI : MonoBehaviour
    {
        [System.Serializable]
        private class ResourceDisplay
        {
            public string resourceType;
            public Image icon;
            public TextMeshProUGUI amountText;
            public Slider progressBar;
        }

        [Header("UI Settings")]
        [SerializeField] private GameObject resourceDisplayPrefab;
        [SerializeField] private Transform resourceContainer;
        [SerializeField] private bool showProgressBars = true;
        [SerializeField] private string amountFormat = "{0}/{1}";

        [Header("Animation")]
        [SerializeField] private float updateSpeed = 5f;
        [SerializeField] private bool animateChanges = true;

        private Dictionary<string, ResourceDisplay> resourceDisplays = new Dictionary<string, ResourceDisplay>();
        private ResourceSystem resourceSystem;
        private Dictionary<string, float> displayedAmounts = new Dictionary<string, float>();

        private void Start()
        {
            // Find resource system
            resourceSystem = FindObjectOfType<ResourceSystem>();
            if (resourceSystem != null)
            {
                // Subscribe to events
                resourceSystem.onResourceChanged.AddListener(UpdateResource);
                resourceSystem.onResourceMaxed.AddListener(OnResourceMaxed);

                // Create displays for existing resources
                foreach (string resourceType in resourceSystem.GetAllResourceTypes())
                {
                    CreateResourceDisplay(resourceType);
                }
            }
        }

        private void Update()
        {
            if (!animateChanges)
                return;

            // Smoothly update displayed amounts
            foreach (var kvp in displayedAmounts)
            {
                string resourceType = kvp.Key;
                float currentDisplay = kvp.Value;
                float targetAmount = resourceSystem.GetResourceAmount(resourceType);

                if (Mathf.Abs(currentDisplay - targetAmount) > 0.01f)
                {
                    float newAmount = Mathf.Lerp(currentDisplay, targetAmount, Time.deltaTime * updateSpeed);
                    displayedAmounts[resourceType] = newAmount;
                    UpdateDisplayAmount(resourceType, (int)newAmount);
                }
            }
        }

        private void CreateResourceDisplay(string resourceType)
        {
            if (resourceDisplays.ContainsKey(resourceType))
                return;

            GameObject displayObj = Instantiate(resourceDisplayPrefab, resourceContainer);
            ResourceDisplay display = new ResourceDisplay
            {
                resourceType = resourceType,
                icon = displayObj.GetComponentInChildren<Image>(),
                amountText = displayObj.GetComponentInChildren<TextMeshProUGUI>(),
                progressBar = displayObj.GetComponentInChildren<Slider>()
            };

            // Set up icon
            if (display.icon != null)
            {
                display.icon.sprite = resourceSystem.GetResourceIcon(resourceType);
            }

            // Initialize progress bar
            if (display.progressBar != null)
            {
                display.progressBar.gameObject.SetActive(showProgressBars);
            }

            resourceDisplays[resourceType] = display;
            displayedAmounts[resourceType] = resourceSystem.GetResourceAmount(resourceType);

            // Update initial display
            UpdateResource(resourceType, resourceSystem.GetResourceAmount(resourceType));
        }

        private void UpdateResource(string type, int amount)
        {
            if (!resourceDisplays.ContainsKey(type))
            {
                CreateResourceDisplay(type);
            }

            if (!animateChanges)
            {
                UpdateDisplayAmount(type, amount);
                displayedAmounts[type] = amount;
            }
            else
            {
                displayedAmounts[type] = amount;
            }
        }

        private void UpdateDisplayAmount(string type, int amount)
        {
            if (!resourceDisplays.ContainsKey(type))
                return;

            var display = resourceDisplays[type];

            // Update text
            if (display.amountText != null)
            {
                display.amountText.text = string.Format(amountFormat, amount, resourceSystem.IsResourceMaxed(type) ? "MAX" : "999");
            }

            // Update progress bar
            if (display.progressBar != null && showProgressBars)
            {
                display.progressBar.value = amount / 999f;
            }
        }

        private void OnResourceMaxed(string type)
        {
            if (!resourceDisplays.ContainsKey(type))
                return;

            var display = resourceDisplays[type];
            
            // Visual feedback for maxed resource
            if (display.amountText != null)
            {
                display.amountText.color = Color.yellow;
            }
        }

        private void OnDestroy()
        {
            if (resourceSystem != null)
            {
                resourceSystem.onResourceChanged.RemoveListener(UpdateResource);
                resourceSystem.onResourceMaxed.RemoveListener(OnResourceMaxed);
            }
        }
    }
}
