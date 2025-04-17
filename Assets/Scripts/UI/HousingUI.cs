using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Housing;
using System.Collections.Generic;

namespace REcreationOfSpace.UI
{
    public class HousingUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private KeyCode toggleKey = KeyCode.H;
        [SerializeField] private TabGroup tabGroup;

        [Header("Property Listings")]
        [SerializeField] private RectTransform listingsContainer;
        [SerializeField] private GameObject propertyListingPrefab;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private Button refreshButton;

        [Header("Property Details")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Image propertyImage;
        [SerializeField] private TextMeshProUGUI propertyNameText;
        [SerializeField] private TextMeshProUGUI propertyDescriptionText;
        [SerializeField] private TextMeshProUGUI propertyDetailsText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI monthlyExpensesText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button rentButton;
        [SerializeField] private TMP_InputField downPaymentInput;

        [Header("Owned Properties")]
        [SerializeField] private RectTransform ownedContainer;
        [SerializeField] private GameObject ownedPropertyPrefab;
        [SerializeField] private TextMeshProUGUI totalAssetsText;
        [SerializeField] private TextMeshProUGUI totalExpensesText;

        [Header("Property Management")]
        [SerializeField] private GameObject managementPanel;
        [SerializeField] private TextMeshProUGUI currentPropertyText;
        [SerializeField] private TextMeshProUGUI conditionText;
        [SerializeField] private TextMeshProUGUI maintenanceText;
        [SerializeField] private TextMeshProUGUI utilitiesText;
        [SerializeField] private TextMeshProUGUI mortgageText;
        [SerializeField] private Button repairButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button sellButton;

        private HousingSystem housingSystem;
        private Dictionary<string, GameObject> propertyListings = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> ownedListings = new Dictionary<string, GameObject>();
        private HousingSystem.Property selectedProperty;
        private float updateTimer;

        private void Start()
        {
            housingSystem = FindObjectOfType<HousingSystem>();
            if (housingSystem == null)
            {
                Debug.LogError("No HousingSystem found!");
                return;
            }

            InitializeUI();
            UpdateUI();
            mainPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }

            if (mainPanel.activeSelf)
            {
                updateTimer += Time.deltaTime;
                if (updateTimer >= 1f)
                {
                    UpdateUI();
                    updateTimer = 0f;
                }
            }
        }

        private void InitializeUI()
        {
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(new List<string> { 
                    "Price: Low to High",
                    "Price: High to Low",
                    "Size: Small to Large",
                    "Size: Large to Small",
                    "Neighborhood Rating"
                });
                sortDropdown.onValueChanged.AddListener(OnSortChanged);
            }

            if (filterDropdown != null)
            {
                filterDropdown.ClearOptions();
                filterDropdown.AddOptions(new List<string> {
                    "All Properties",
                    "For Sale Only",
                    "For Rent Only",
                    "1+ Bedrooms",
                    "2+ Bedrooms",
                    "3+ Bedrooms"
                });
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshListings);

            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchaseClicked);

            if (rentButton != null)
                rentButton.onClick.AddListener(OnRentClicked);

            if (repairButton != null)
                repairButton.onClick.AddListener(OnRepairClicked);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (sellButton != null)
                sellButton.onClick.AddListener(OnSellClicked);
        }

        private void UpdateUI()
        {
            UpdatePropertyListings();
            UpdateOwnedProperties();
            if (selectedProperty != null)
            {
                UpdatePropertyDetails(selectedProperty);
            }
            UpdateTotals();
        }

        private void UpdatePropertyListings()
        {
            // Clear existing listings
            foreach (var listing in propertyListings.Values)
            {
                Destroy(listing);
            }
            propertyListings.Clear();

            // Create new listings
            var properties = housingSystem.GetAvailableProperties();
            foreach (var property in properties)
            {
                if (ShouldShowProperty(property))
                {
                    CreatePropertyListing(property);
                }
            }
        }

        private bool ShouldShowProperty(HousingSystem.Property property)
        {
            if (filterDropdown == null) return true;

            switch (filterDropdown.value)
            {
                case 1: return property.isForSale;
                case 2: return property.isForRent;
                case 3: return property.bedrooms >= 1;
                case 4: return property.bedrooms >= 2;
                case 5: return property.bedrooms >= 3;
                default: return true;
            }
        }

        private void CreatePropertyListing(HousingSystem.Property property)
        {
            GameObject listing = Instantiate(propertyListingPrefab, listingsContainer);
            propertyListings[property.id] = listing;

            var nameText = listing.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var priceText = listing.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            var detailsText = listing.transform.Find("DetailsText")?.GetComponent<TextMeshProUGUI>();
            var viewButton = listing.GetComponentInChildren<Button>();

            if (nameText != null) nameText.text = property.name;
            if (priceText != null)
            {
                if (property.isForSale)
                    priceText.text = $"For Sale: ${property.basePrice:N0}";
                if (property.isForRent)
                    priceText.text = $"For Rent: ${property.monthlyRent:N0}/mo";
            }
            if (detailsText != null)
            {
                detailsText.text = $"{property.bedrooms} bed, {property.bathrooms} bath\n{property.squareFootage:N0} sq ft";
            }

            if (viewButton != null)
            {
                viewButton.onClick.AddListener(() => SelectProperty(property));
            }
        }

        private void UpdateOwnedProperties()
        {
            // Clear existing listings
            foreach (var listing in ownedListings.Values)
            {
                Destroy(listing);
            }
            ownedListings.Clear();

            // Create listings for owned properties
            foreach (var property in housingSystem.GetOwnedProperties().Values)
            {
                CreateOwnedPropertyListing(property);
            }

            // Create listings for rented properties
            foreach (var property in housingSystem.GetRentedProperties().Values)
            {
                CreateOwnedPropertyListing(property, true);
            }
        }

        private void CreateOwnedPropertyListing(HousingSystem.Property property, bool isRented = false)
        {
            GameObject listing = Instantiate(ownedPropertyPrefab, ownedContainer);
            ownedListings[property.id] = listing;

            var nameText = listing.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var statusText = listing.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            var valueText = listing.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
            var manageButton = listing.GetComponentInChildren<Button>();

            if (nameText != null) nameText.text = property.name;
            if (statusText != null)
            {
                if (isRented)
                    statusText.text = $"Renting - ${property.monthlyRent:N0}/mo";
                else
                {
                    var mortgage = housingSystem.GetMortgages().ContainsKey(property.id) ? 
                        housingSystem.GetMortgages()[property.id] : null;
                    if (mortgage != null && mortgage.isActive)
                        statusText.text = $"Mortgaged - ${mortgage.monthlyPayment:N0}/mo";
                    else
                        statusText.text = "Owned";
                }
            }
            if (valueText != null)
                valueText.text = $"Value: ${property.basePrice:N0}";

            if (manageButton != null)
            {
                manageButton.onClick.AddListener(() => ShowManagementPanel(property));
            }
        }

        private void UpdateTotals()
        {
            float totalAssets = 0f;
            float totalExpenses = 0f;

            foreach (var property in housingSystem.GetOwnedProperties().Values)
            {
                totalAssets += property.basePrice;
                totalExpenses += property.maintenanceCost + property.utilityBaseCost;

                var mortgage = housingSystem.GetMortgages().ContainsKey(property.id) ?
                    housingSystem.GetMortgages()[property.id] : null;
                if (mortgage != null && mortgage.isActive)
                    totalExpenses += mortgage.monthlyPayment;
            }

            foreach (var property in housingSystem.GetRentedProperties().Values)
            {
                totalExpenses += property.monthlyRent;
            }

            if (totalAssetsText != null)
                totalAssetsText.text = $"Total Assets: ${totalAssets:N0}";
            if (totalExpensesText != null)
                totalExpensesText.text = $"Monthly Expenses: ${totalExpenses:N0}";
        }

        private void SelectProperty(HousingSystem.Property property)
        {
            selectedProperty = property;
            UpdatePropertyDetails(property);
            detailsPanel.SetActive(true);
        }

        private void UpdatePropertyDetails(HousingSystem.Property property)
        {
            propertyNameText.text = property.name;
            propertyDescriptionText.text = property.description;
            propertyDetailsText.text = $"{property.bedrooms} Bedrooms, {property.bathrooms} Bathrooms\n" +
                                     $"{property.squareFootage:N0} sq ft\n" +
                                     $"Condition: {property.condition}\n" +
                                     $"Neighborhood: {property.neighborhood}\n" +
                                     $"Rating: {property.neighborhoodRating:P0}";

            if (property.isForSale)
            {
                priceText.text = $"Price: ${property.basePrice:N0}";
                purchaseButton.gameObject.SetActive(true);
                downPaymentInput.gameObject.SetActive(true);
            }
            else
            {
                purchaseButton.gameObject.SetActive(false);
                downPaymentInput.gameObject.SetActive(false);
            }

            if (property.isForRent)
            {
                monthlyExpensesText.text = $"Monthly Rent: ${property.monthlyRent:N0}\n" +
                                         $"Security Deposit: ${property.monthlyRent * 2:N0}";
                rentButton.gameObject.SetActive(true);
            }
            else
            {
                rentButton.gameObject.SetActive(false);
            }
        }

        private void ShowManagementPanel(HousingSystem.Property property)
        {
            managementPanel.SetActive(true);
            currentPropertyText.text = property.name;
            conditionText.text = $"Condition: {property.condition}";
            maintenanceText.text = $"Monthly Maintenance: ${property.maintenanceCost:N0}";
            utilitiesText.text = $"Monthly Utilities: ${property.utilityBaseCost:N0}";

            var mortgage = housingSystem.GetMortgages().ContainsKey(property.id) ?
                housingSystem.GetMortgages()[property.id] : null;
            if (mortgage != null && mortgage.isActive)
            {
                mortgageText.text = $"Mortgage: ${mortgage.monthlyPayment:N0}/mo\n" +
                                  $"Remaining: ${mortgage.remainingBalance:N0}";
            }
            else
            {
                mortgageText.text = "No Mortgage";
            }

            repairButton.interactable = property.condition.ToLower() != "excellent";
            upgradeButton.interactable = true;
            sellButton.interactable = !housingSystem.GetRentedProperties().ContainsKey(property.id);
        }

        private void OnSortChanged(int index)
        {
            // Implement sorting logic
            RefreshListings();
        }

        private void OnFilterChanged(int index)
        {
            RefreshListings();
        }

        private void OnPurchaseClicked()
        {
            if (selectedProperty == null) return;

            float downPayment;
            if (!float.TryParse(downPaymentInput.text, out downPayment))
            {
                Debug.LogWarning("Invalid down payment amount");
                return;
            }

            if (housingSystem.TryPurchaseProperty(selectedProperty.id, downPayment))
            {
                detailsPanel.SetActive(false);
                UpdateUI();
            }
        }

        private void OnRentClicked()
        {
            if (selectedProperty == null) return;

            if (housingSystem.TryRentProperty(selectedProperty.id))
            {
                detailsPanel.SetActive(false);
                UpdateUI();
            }
        }

        private void OnRepairClicked()
        {
            // Implement repair logic
        }

        private void OnUpgradeClicked()
        {
            // Implement upgrade logic
        }

        private void OnSellClicked()
        {
            // Implement sell logic
        }

        private void RefreshListings()
        {
            UpdatePropertyListings();
        }

        public void TogglePanel()
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            if (mainPanel.activeSelf)
            {
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            if (sortDropdown != null)
                sortDropdown.onValueChanged.RemoveListener(OnSortChanged);

            if (filterDropdown != null)
                filterDropdown.onValueChanged.RemoveListener(OnFilterChanged);

            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(RefreshListings);

            if (purchaseButton != null)
                purchaseButton.onClick.RemoveListener(OnPurchaseClicked);

            if (rentButton != null)
                rentButton.onClick.RemoveListener(OnRentClicked);

            if (repairButton != null)
                repairButton.onClick.RemoveListener(OnRepairClicked);

            if (upgradeButton != null)
                upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

            if (sellButton != null)
                sellButton.onClick.RemoveListener(OnSellClicked);
        }
    }
}
