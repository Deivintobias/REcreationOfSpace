using UnityEngine;
using System.Collections.Generic;

public class TradingSystem : MonoBehaviour
{
    [System.Serializable]
    public class TradeItem
    {
        public string itemName;
        public ItemType type;
        public float neuralValue; // How much this item contributes to neural development
        public string description;
    }

    public enum ItemType
    {
        Knowledge,    // Hidden texts, teachings
        Resource,     // Farming materials, building supplies
        Tool,         // Farming tools, meditation aids
        Artifact      // Special items that boost neural development
    }

    [Header("Trading Settings")]
    public float tradeRange = 5f;
    public float safeTradeDuration = 3f;
    public LayerMask traderMask;

    private List<TradeItem> inventory = new List<TradeItem>();
    private bool isTrading = false;
    private float tradeProgress = 0f;
    private TradingSystem currentTradePartner;

    // Example trade items
    private static readonly TradeItem[] availableItems = new TradeItem[]
    {
        new TradeItem {
            itemName = "Hidden Scripture",
            type = ItemType.Knowledge,
            neuralValue = 15f,
            description = "Ancient wisdom passed down in secret"
        },
        new TradeItem {
            itemName = "Meditation Crystal",
            type = ItemType.Tool,
            neuralValue = 10f,
            description = "Enhances meditation practice"
        },
        new TradeItem {
            itemName = "Truth Fragment",
            type = ItemType.Artifact,
            neuralValue = 20f,
            description = "A piece of pure truth"
        },
        new TradeItem {
            itemName = "Sacred Seeds",
            type = ItemType.Resource,
            neuralValue = 5f,
            description = "Special seeds for conscious farming"
        }
    };

    void Start()
    {
        // Add some initial items
        AddRandomItems(2);
    }

    void Update()
    {
        if (isTrading)
        {
            UpdateTradeProgress();
        }

        // Check for trade initiation
        if (Input.GetKeyDown(KeyCode.T))
        {
            TryInitiateTrade();
        }
    }

    private void AddRandomItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            inventory.Add(availableItems[Random.Range(0, availableItems.Length)]);
        }
    }

    private void TryInitiateTrade()
    {
        // Find potential trade partners
        Collider[] nearbyTraders = Physics.OverlapSphere(transform.position, tradeRange, traderMask);
        
        foreach (var trader in nearbyTraders)
        {
            TradingSystem otherTrader = trader.GetComponent<TradingSystem>();
            if (otherTrader != null && otherTrader != this)
            {
                InitiateTrade(otherTrader);
                break;
            }
        }
    }

    private void InitiateTrade(TradingSystem other)
    {
        // Check if both traders are Sinai characters
        SinaiCharacter thisSinai = GetComponent<SinaiCharacter>();
        SinaiCharacter otherSinai = other.GetComponent<SinaiCharacter>();

        if (thisSinai != null && otherSinai != null)
        {
            isTrading = true;
            currentTradePartner = other;
            tradeProgress = 0f;

            // Notify UI
            if (GuiderMessageUI.Instance != null)
            {
                GuiderMessageUI.Instance.ShowMessage("Beginning safe trade...");
            }

            // Start trade on other end
            other.AcceptTrade(this);
        }
    }

    public void AcceptTrade(TradingSystem initiator)
    {
        isTrading = true;
        currentTradePartner = initiator;
        tradeProgress = 0f;
    }

    private void UpdateTradeProgress()
    {
        if (currentTradePartner == null)
        {
            CancelTrade();
            return;
        }

        // Check if traders are still in range
        float distance = Vector3.Distance(transform.position, currentTradePartner.transform.position);
        if (distance > tradeRange)
        {
            CancelTrade();
            return;
        }

        // Progress the trade
        tradeProgress += Time.deltaTime;
        
        // Check for completion
        if (tradeProgress >= safeTradeDuration)
        {
            CompleteTrade();
        }
    }

    private void CompleteTrade()
    {
        if (currentTradePartner == null) return;

        // Exchange random items
        if (inventory.Count > 0 && currentTradePartner.inventory.Count > 0)
        {
            TradeItem myItem = inventory[Random.Range(0, inventory.Count)];
            TradeItem theirItem = currentTradePartner.inventory[Random.Range(0, currentTradePartner.inventory.Count)];

            // Remove items
            inventory.Remove(myItem);
            currentTradePartner.inventory.Remove(theirItem);

            // Add exchanged items
            inventory.Add(theirItem);
            currentTradePartner.inventory.Add(myItem);

            // Grant neural network experience
            GrantTradeExperience(myItem, theirItem);
        }

        // Notify UI
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Trade completed successfully!");
        }

        // Reset trade state
        isTrading = false;
        currentTradePartner = null;
        tradeProgress = 0f;
    }

    private void CancelTrade()
    {
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Trade cancelled - traders separated");
        }

        isTrading = false;
        currentTradePartner = null;
        tradeProgress = 0f;
    }

    private void GrantTradeExperience(TradeItem given, TradeItem received)
    {
        NeuralNetwork network = GetComponent<NeuralNetwork>();
        if (network != null)
        {
            // Calculate experience based on items exchanged
            float experience = (given.neuralValue + received.neuralValue) * 0.5f;
            network.GainExperience(experience);

            // Special boost for knowledge exchange
            if (given.type == ItemType.Knowledge && received.type == ItemType.Knowledge)
            {
                network.DevelopNode("Critical Thinking", experience * 0.5f);
            }
        }
    }

    public List<TradeItem> GetInventory()
    {
        return inventory;
    }

    public bool IsTrading()
    {
        return isTrading;
    }

    public float GetTradeProgress()
    {
        return tradeProgress / safeTradeDuration;
    }

    void OnDrawGizmosSelected()
    {
        // Draw trade range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tradeRange);
    }
}
