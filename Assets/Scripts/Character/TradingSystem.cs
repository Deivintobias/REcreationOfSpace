using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace REcreationOfSpace.Character
{
    public class TradingSystem : MonoBehaviour
    {
        [System.Serializable]
        public class TradeItem
        {
            public string itemType;
            public int amount;
            public int price;
            public bool isSelling; // true = NPC sells, false = NPC buys
        }

        [Header("Trading Settings")]
        [SerializeField] private List<TradeItem> availableItems = new List<TradeItem>();
        [SerializeField] private float priceFluctuation = 0.2f;
        [SerializeField] private float restockTime = 300f; // 5 minutes

        [Header("UI Events")]
        public UnityEvent onTradeStart;
        public UnityEvent onTradeEnd;
        public UnityEvent<TradeItem> onItemTraded;

        private ResourceSystem resourceSystem;
        private Dictionary<string, float> restockTimers = new Dictionary<string, float>();
        private bool isTrading = false;

        private void Start()
        {
            resourceSystem = FindObjectOfType<ResourceSystem>();

            // Initialize restock timers
            foreach (var item in availableItems)
            {
                if (item.isSelling)
                {
                    restockTimers[item.itemType] = 0f;
                }
            }
        }

        private void Update()
        {
            // Handle restocking
            foreach (var timer in new Dictionary<string, float>(restockTimers))
            {
                if (restockTimers[timer.Key] > 0)
                {
                    restockTimers[timer.Key] -= Time.deltaTime;
                    if (restockTimers[timer.Key] <= 0)
                    {
                        RestockItem(timer.Key);
                    }
                }
            }
        }

        public void StartTrading()
        {
            if (isTrading)
                return;

            isTrading = true;
            onTradeStart?.Invoke();
        }

        public void EndTrading()
        {
            if (!isTrading)
                return;

            isTrading = false;
            onTradeEnd?.Invoke();
        }

        public bool TryBuyItem(string itemType)
        {
            if (!isTrading || resourceSystem == null)
                return false;

            var item = availableItems.Find(i => i.itemType == itemType && i.isSelling);
            if (item == null)
                return false;

            // Check if item is in stock
            if (restockTimers.ContainsKey(itemType) && restockTimers[itemType] > 0)
                return false;

            // Check if player has enough resources
            if (!resourceSystem.HasResource("Gold", item.price))
                return false;

            // Process transaction
            resourceSystem.UseResource("Gold", item.price);
            resourceSystem.AddResource(item.itemType, item.amount);

            // Start restock timer
            restockTimers[itemType] = restockTime;

            onItemTraded?.Invoke(item);
            return true;
        }

        public bool TrySellItem(string itemType, int amount)
        {
            if (!isTrading || resourceSystem == null)
                return false;

            var item = availableItems.Find(i => i.itemType == itemType && !i.isSelling);
            if (item == null)
                return false;

            // Check if player has enough of the item
            if (!resourceSystem.HasResource(itemType, amount))
                return false;

            // Calculate sell price with random fluctuation
            float priceModifier = 1f + Random.Range(-priceFluctuation, priceFluctuation);
            int sellPrice = Mathf.RoundToInt(item.price * amount * priceModifier);

            // Process transaction
            resourceSystem.UseResource(itemType, amount);
            resourceSystem.AddResource("Gold", sellPrice);

            onItemTraded?.Invoke(item);
            return true;
        }

        private void RestockItem(string itemType)
        {
            restockTimers[itemType] = 0f;
        }

        public List<TradeItem> GetAvailableItems()
        {
            return new List<TradeItem>(availableItems);
        }

        public bool IsItemInStock(string itemType)
        {
            return !restockTimers.ContainsKey(itemType) || restockTimers[itemType] <= 0;
        }

        public float GetRestockTime(string itemType)
        {
            return restockTimers.ContainsKey(itemType) ? restockTimers[itemType] : 0f;
        }

        public bool IsTrading()
        {
            return isTrading;
        }
    }
}
