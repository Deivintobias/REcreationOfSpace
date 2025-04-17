using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace REcreationOfSpace.Resources
{
    public class ResourceSystem : MonoBehaviour
    {
        [System.Serializable]
        public class ResourceData
        {
            public string type;
            public int amount;
            public int maxAmount = 999;
            public Sprite icon;
        }

        [Header("Resources")]
        [SerializeField] private List<ResourceData> startingResources = new List<ResourceData>();

        public UnityEvent<string, int> onResourceChanged; // Resource type, new amount
        public UnityEvent<string> onResourceMaxed; // Resource type

        private Dictionary<string, ResourceData> resources = new Dictionary<string, ResourceData>();

        private void Awake()
        {
            // Initialize resources
            foreach (var resource in startingResources)
            {
                resources[resource.type] = new ResourceData
                {
                    type = resource.type,
                    amount = resource.amount,
                    maxAmount = resource.maxAmount,
                    icon = resource.icon
                };
            }
        }

        public void AddResource(string type, int amount)
        {
            if (amount <= 0)
                return;

            // Create resource type if it doesn't exist
            if (!resources.ContainsKey(type))
            {
                resources[type] = new ResourceData
                {
                    type = type,
                    amount = 0,
                    maxAmount = 999,
                    icon = null
                };
            }

            var resource = resources[type];
            int newAmount = Mathf.Min(resource.amount + amount, resource.maxAmount);
            resource.amount = newAmount;

            // Notify listeners
            onResourceChanged?.Invoke(type, newAmount);

            // Check if maxed
            if (newAmount >= resource.maxAmount)
            {
                onResourceMaxed?.Invoke(type);
            }
        }

        public bool UseResource(string type, int amount)
        {
            if (!resources.ContainsKey(type) || amount <= 0)
                return false;

            var resource = resources[type];
            if (resource.amount < amount)
                return false;

            resource.amount -= amount;
            onResourceChanged?.Invoke(type, resource.amount);
            return true;
        }

        public int GetResourceAmount(string type)
        {
            return resources.ContainsKey(type) ? resources[type].amount : 0;
        }

        public bool HasResource(string type, int amount)
        {
            return resources.ContainsKey(type) && resources[type].amount >= amount;
        }

        public Sprite GetResourceIcon(string type)
        {
            return resources.ContainsKey(type) ? resources[type].icon : null;
        }

        public List<string> GetAllResourceTypes()
        {
            return new List<string>(resources.Keys);
        }

        public void ClearResources()
        {
            foreach (var resource in resources.Values)
            {
                resource.amount = 0;
                onResourceChanged?.Invoke(resource.type, 0);
            }
        }

        public bool IsResourceMaxed(string type)
        {
            return resources.ContainsKey(type) && resources[type].amount >= resources[type].maxAmount;
        }
    }
}
