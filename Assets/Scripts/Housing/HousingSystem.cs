using UnityEngine;
using System.Collections.Generic;
using REcreationOfSpace.Quest;
using REcreationOfSpace.UI;

namespace REcreationOfSpace.Housing
{
    public class HousingSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Property
        {
            public string id;
            public string name;
            public string description;
            public float basePrice;
            public float monthlyRent;
            public float maintenanceCost;
            public float utilityBaseCost;
            public int bedrooms;
            public int bathrooms;
            public float squareFootage;
            public Vector3 location;
            public bool isForSale;
            public bool isForRent;
            public string neighborhood;
            public float propertyTaxRate;
            public string[] amenities;
            public string condition;
            public float neighborhoodRating;
        }

        [System.Serializable]
        public class Mortgage
        {
            public string propertyId;
            public float principal;
            public float interestRate;
            public int termYears;
            public float monthlyPayment;
            public float remainingBalance;
            public int paymentsMade;
            public int totalPayments;
            public bool isActive;
        }

        [Header("Property Settings")]
        [SerializeField] private Property[] availableProperties;
        [SerializeField] private float propertyValueChangeRate = 0.05f;
        [SerializeField] private float mortgageBaseRate = 0.045f;
        [SerializeField] private float mortgageRateVariation = 0.02f;
        [SerializeField] private float downPaymentMinimum = 0.2f;
        [SerializeField] private int defaultMortgageTerm = 30;

        [Header("Utility Settings")]
        [SerializeField] private float electricityRate = 0.12f;
        [SerializeField] private float waterRate = 0.015f;
        [SerializeField] private float gasRate = 0.08f;
        [SerializeField] private float internetRate = 60f;
        [SerializeField] private float usageVariation = 0.2f;

        [Header("Maintenance")]
        [SerializeField] private float emergencyRepairChance = 0.01f;
        [SerializeField] private float minRepairCost = 100f;
        [SerializeField] private float maxRepairCost = 5000f;
        [SerializeField] private float propertyValueDecayRate = 0.02f;

        [Header("UI References")]
        [SerializeField] private GuiderMessageUI guiderMessage;

        private DailyLifeSystem dailyLifeSystem;
        private Dictionary<string, Property> ownedProperties = new Dictionary<string, Property>();
        private Dictionary<string, Property> rentedProperties = new Dictionary<string, Property>();
        private Dictionary<string, Mortgage> mortgages = new Dictionary<string, Mortgage>();
        private Property currentResidence;
        private float monthlyUpdateTimer;

        private void Start()
        {
            dailyLifeSystem = FindObjectOfType<DailyLifeSystem>();
            if (dailyLifeSystem == null)
            {
                Debug.LogError("No DailyLifeSystem found!");
                return;
            }

            InitializeProperties();
        }

        private void Update()
        {
            UpdatePropertyValues();
            CheckMaintenance();
            UpdateMonthlyExpenses();
        }

        private void InitializeProperties()
        {
            foreach (var property in availableProperties)
            {
                // Initialize neighborhood ratings
                property.neighborhoodRating = CalculateNeighborhoodRating(property);
                
                // Set initial prices based on features
                UpdatePropertyPrice(property);
            }
        }

        private float CalculateNeighborhoodRating(Property property)
        {
            // Calculate based on various factors
            float rating = 0.5f; // Base rating

            // Location-based adjustment
            float distanceFromCenter = Vector3.Distance(property.location, Vector3.zero);
            rating += Mathf.Lerp(0.3f, -0.3f, distanceFromCenter / 1000f);

            // Amenities bonus
            rating += property.amenities.Length * 0.05f;

            // Property condition
            switch (property.condition.ToLower())
            {
                case "excellent": rating += 0.3f; break;
                case "good": rating += 0.2f; break;
                case "fair": rating += 0.1f; break;
                case "poor": rating -= 0.1f; break;
                case "dilapidated": rating -= 0.2f; break;
            }

            return Mathf.Clamp01(rating);
        }

        private void UpdatePropertyValues()
        {
            foreach (var property in availableProperties)
            {
                // Base value change
                float valueChange = Random.Range(-propertyValueChangeRate, propertyValueChangeRate);
                
                // Neighborhood influence
                valueChange += (property.neighborhoodRating - 0.5f) * propertyValueChangeRate;
                
                // Condition decay
                if (ownedProperties.ContainsKey(property.id))
                {
                    valueChange -= propertyValueDecayRate * Time.deltaTime;
                }

                // Apply change
                property.basePrice *= (1f + valueChange * Time.deltaTime);
                
                // Update rent prices
                if (property.isForRent)
                {
                    property.monthlyRent = property.basePrice * 0.008f; // Typical rent is 0.8% of property value
                }
            }
        }

        private void UpdatePropertyPrice(Property property)
        {
            float basePrice = property.basePrice;
            
            // Size adjustment
            basePrice *= 1f + (property.squareFootage / 1000f);
            
            // Room adjustment
            basePrice *= 1f + (property.bedrooms * 0.1f) + (property.bathrooms * 0.05f);
            
            // Neighborhood adjustment
            basePrice *= 1f + (property.neighborhoodRating - 0.5f);
            
            // Condition adjustment
            switch (property.condition.ToLower())
            {
                case "excellent": basePrice *= 1.3f; break;
                case "good": basePrice *= 1.1f; break;
                case "fair": basePrice *= 1f; break;
                case "poor": basePrice *= 0.8f; break;
                case "dilapidated": basePrice *= 0.6f; break;
            }

            property.basePrice = basePrice;
        }

        private void CheckMaintenance()
        {
            foreach (var property in ownedProperties.Values)
            {
                if (Random.value < emergencyRepairChance * Time.deltaTime)
                {
                    float repairCost = Random.Range(minRepairCost, maxRepairCost);
                    HandleEmergencyRepair(property, repairCost);
                }
            }
        }

        private void HandleEmergencyRepair(Property property, float cost)
        {
            string repairType = GetRandomRepairType();
            guiderMessage.ShowMessage($"Emergency repair needed at {property.name}: {repairType} - Cost: ${cost:F2}", Color.red);

            if (dailyLifeSystem.SpendMoney(cost))
            {
                guiderMessage.ShowMessage($"Repairs completed at {property.name}.", Color.green);
            }
            else
            {
                guiderMessage.ShowMessage($"Unable to afford repairs. Property condition will deteriorate.", Color.red);
                DeteriorateProperty(property);
            }
        }

        private string GetRandomRepairType()
        {
            string[] repairTypes = {
                "Plumbing issue",
                "Electrical problem",
                "Roof leak",
                "HVAC maintenance",
                "Appliance repair",
                "Structural damage",
                "Pest control",
                "Window repair"
            };
            return repairTypes[Random.Range(0, repairTypes.Length)];
        }

        private void DeteriorateProperty(Property property)
        {
            // Deteriorate condition
            switch (property.condition.ToLower())
            {
                case "excellent": property.condition = "good"; break;
                case "good": property.condition = "fair"; break;
                case "fair": property.condition = "poor"; break;
                case "poor": property.condition = "dilapidated"; break;
            }

            // Update property value
            UpdatePropertyPrice(property);
        }

        private void UpdateMonthlyExpenses()
        {
            monthlyUpdateTimer += Time.deltaTime;
            if (monthlyUpdateTimer >= 30f) // Assuming 30 days per month
            {
                monthlyUpdateTimer = 0f;
                ProcessMonthlyExpenses();
            }
        }

        private void ProcessMonthlyExpenses()
        {
            foreach (var property in ownedProperties.Values)
            {
                // Property tax
                float taxAmount = property.basePrice * property.propertyTaxRate / 12f;
                
                // Utilities
                float utilityUsage = 1f + Random.Range(-usageVariation, usageVariation);
                float electricityBill = property.utilityBaseCost * electricityRate * utilityUsage;
                float waterBill = property.utilityBaseCost * waterRate * utilityUsage;
                float gasBill = property.utilityBaseCost * gasRate * utilityUsage;
                
                float totalUtilities = electricityBill + waterBill + gasBill + internetRate;
                
                // Maintenance
                float maintenanceCost = property.maintenanceCost;

                float totalExpenses = taxAmount + totalUtilities + maintenanceCost;

                if (!dailyLifeSystem.SpendMoney(totalExpenses))
                {
                    guiderMessage.ShowMessage($"Unable to pay monthly expenses for {property.name}!", Color.red);
                    // Could add consequences here
                }
            }

            // Process mortgages
            foreach (var mortgage in mortgages.Values)
            {
                if (mortgage.isActive)
                {
                    if (dailyLifeSystem.SpendMoney(mortgage.monthlyPayment))
                    {
                        ProcessMortgagePayment(mortgage);
                    }
                    else
                    {
                        HandleMissedMortgagePayment(mortgage);
                    }
                }
            }

            // Process rent
            foreach (var property in rentedProperties.Values)
            {
                if (!dailyLifeSystem.SpendMoney(property.monthlyRent))
                {
                    HandleMissedRentPayment(property);
                }
            }
        }

        private void ProcessMortgagePayment(Mortgage mortgage)
        {
            mortgage.paymentsMade++;
            float interestPayment = mortgage.remainingBalance * (mortgage.interestRate / 12f);
            float principalPayment = mortgage.monthlyPayment - interestPayment;
            mortgage.remainingBalance -= principalPayment;

            if (mortgage.paymentsMade >= mortgage.totalPayments)
            {
                CompleteMortgage(mortgage);
            }
        }

        private void CompleteMortgage(Mortgage mortgage)
        {
            mortgage.isActive = false;
            mortgage.remainingBalance = 0f;
            guiderMessage.ShowMessage($"Congratulations! You've paid off your mortgage for {GetPropertyById(mortgage.propertyId).name}!", Color.green);
        }

        private void HandleMissedMortgagePayment(Mortgage mortgage)
        {
            guiderMessage.ShowMessage($"Missed mortgage payment for {GetPropertyById(mortgage.propertyId).name}!", Color.red);
            // Could add foreclosure system here
        }

        private void HandleMissedRentPayment(Property property)
        {
            guiderMessage.ShowMessage($"Missed rent payment for {property.name}!", Color.red);
            // Could add eviction system here
        }

        public bool TryPurchaseProperty(string propertyId, float downPayment)
        {
            Property property = GetPropertyById(propertyId);
            if (property == null || !property.isForSale) return false;

            float minimumDown = property.basePrice * downPaymentMinimum;
            if (downPayment < minimumDown)
            {
                guiderMessage.ShowMessage($"Minimum down payment required: ${minimumDown:F2}", Color.red);
                return false;
            }

            if (!dailyLifeSystem.SpendMoney(downPayment))
            {
                guiderMessage.ShowMessage("Insufficient funds for down payment!", Color.red);
                return false;
            }

            float loanAmount = property.basePrice - downPayment;
            Mortgage mortgage = CreateMortgage(property, loanAmount);
            mortgages[property.id] = mortgage;

            property.isForSale = false;
            ownedProperties[property.id] = property;

            guiderMessage.ShowMessage($"Congratulations on purchasing {property.name}!", Color.green);
            return true;
        }

        public bool TryRentProperty(string propertyId)
        {
            Property property = GetPropertyById(propertyId);
            if (property == null || !property.isForRent) return false;

            float depositAmount = property.monthlyRent * 2; // First and last month
            if (!dailyLifeSystem.SpendMoney(depositAmount))
            {
                guiderMessage.ShowMessage("Insufficient funds for security deposit!", Color.red);
                return false;
            }

            property.isForRent = false;
            rentedProperties[property.id] = property;

            guiderMessage.ShowMessage($"Successfully rented {property.name}!", Color.green);
            return true;
        }

        private Mortgage CreateMortgage(Property property, float loanAmount)
        {
            float rate = mortgageBaseRate + Random.Range(-mortgageRateVariation, mortgageRateVariation);
            float monthlyRate = rate / 12f;
            int totalPayments = defaultMortgageTerm * 12;

            // Calculate monthly payment using mortgage payment formula
            float monthlyPayment = loanAmount * 
                (monthlyRate * Mathf.Pow(1 + monthlyRate, totalPayments)) /
                (Mathf.Pow(1 + monthlyRate, totalPayments) - 1);

            return new Mortgage
            {
                propertyId = property.id,
                principal = loanAmount,
                interestRate = rate,
                termYears = defaultMortgageTerm,
                monthlyPayment = monthlyPayment,
                remainingBalance = loanAmount,
                paymentsMade = 0,
                totalPayments = totalPayments,
                isActive = true
            };
        }

        private Property GetPropertyById(string id)
        {
            return System.Array.Find(availableProperties, p => p.id == id);
        }

        public Property[] GetAvailableProperties() => availableProperties;
        public Dictionary<string, Property> GetOwnedProperties() => ownedProperties;
        public Dictionary<string, Property> GetRentedProperties() => rentedProperties;
        public Dictionary<string, Mortgage> GetMortgages() => mortgages;
        public Property GetCurrentResidence() => currentResidence;

        public void SetCurrentResidence(string propertyId)
        {
            Property property = GetPropertyById(propertyId);
            if (property != null && (ownedProperties.ContainsKey(propertyId) || rentedProperties.ContainsKey(propertyId)))
            {
                currentResidence = property;
                guiderMessage.ShowMessage($"Moved into {property.name}.", Color.white);
            }
        }
    }
}
