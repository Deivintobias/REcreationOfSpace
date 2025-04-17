using UnityEngine;
using System.Collections.Generic;

public class ResourceSystem : MonoBehaviour
{
    [System.Serializable]
    public class Resource
    {
        public string name;
        public ResourceType type;
        public float quantity;
        public float maxStack = 100f;
        public string description;
        public float consciousnessValue; // How much this resource contributes to consciousness
    }

    public enum ResourceType
    {
        Natural,    // Basic natural resources
        Refined,    // Processed resources
        Pure,       // Pure form of natural resources
        Rare,       // Rare natural resources
        Energy      // Natural energy resources
    }

    [Header("Resource Settings")]
    public float gatherRange = 2f;
    public float gatherSpeed = 1f;
    public LayerMask resourceMask;

    private Dictionary<string, Resource> inventory = new Dictionary<string, Resource>();
    private bool isGathering = false;
    private float gatherProgress = 0f;
    private GameObject currentResourceNode;

    // Define base resources
    public static readonly Resource[] baseResources = new Resource[]
    {
        new Resource {
            name = "Wood",
            type = ResourceType.Natural,
            quantity = 0f,
            description = "Natural wood from the surface",
            consciousnessValue = 2f
        },
        new Resource {
            name = "Crystal",
            type = ResourceType.Natural,
            quantity = 0f,
            description = "Natural crystal formations",
            consciousnessValue = 5f
        },
        new Resource {
            name = "Water",
            type = ResourceType.Natural,
            quantity = 0f,
            description = "Surface water",
            consciousnessValue = 3f
        },
        new Resource {
            name = "Stone",
            type = ResourceType.Natural,
            quantity = 0f,
            description = "Natural stone",
            consciousnessValue = 4f
        },
        new Resource {
            name = "Energy",
            type = ResourceType.Energy,
            quantity = 0f,
            description = "Natural energy source",
            consciousnessValue = 8f
        }
    };

    void Start()
    {
        InitializeInventory();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.F)) // F for gather
        {
            TryGatherResource();
        }
        
        if (isGathering)
        {
            UpdateGathering();
        }
    }

    private void InitializeInventory()
    {
        foreach (var resource in baseResources)
        {
            inventory[resource.name] = new Resource
            {
                name = resource.name,
                type = resource.type,
                quantity = 0f,
                maxStack = resource.maxStack,
                description = resource.description,
                consciousnessValue = resource.consciousnessValue
            };
        }
    }

    private void TryGatherResource()
    {
        if (isGathering) return;

        // Check for resources in range
        Collider[] resourceNodes = Physics.OverlapSphere(transform.position, gatherRange, resourceMask);
        
        foreach (var node in resourceNodes)
        {
            ResourceNode resourceNode = node.GetComponent<ResourceNode>();
            if (resourceNode != null && !resourceNode.IsDepeleted())
            {
                StartGathering(resourceNode.gameObject);
                break;
            }
        }
    }

    private void StartGathering(GameObject node)
    {
        isGathering = true;
        gatherProgress = 0f;
        currentResourceNode = node;

        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Gathering resource...");
        }
    }

    private void UpdateGathering()
    {
        if (currentResourceNode == null)
        {
            CancelGathering();
            return;
        }

        // Check if still in range
        float distance = Vector3.Distance(transform.position, currentResourceNode.transform.position);
        if (distance > gatherRange)
        {
            CancelGathering();
            return;
        }

        // Progress gathering
        gatherProgress += Time.deltaTime * gatherSpeed;
        
        if (gatherProgress >= 1f)
        {
            CompleteGathering();
        }
    }

    private void CompleteGathering()
    {
        ResourceNode node = currentResourceNode.GetComponent<ResourceNode>();
        if (node != null)
        {
            // Get resource from node
            Resource gatheredResource = node.GatherResource();
            
            if (gatheredResource != null)
            {
                // Add to inventory
                AddResource(gatheredResource.name, gatheredResource.quantity);
                
                // Grant consciousness experience
                GrantConsciousnessExperience(gatheredResource);

                if (GuiderMessageUI.Instance != null)
                {
                    GuiderMessageUI.Instance.ShowMessage($"Gathered {gatheredResource.quantity} {gatheredResource.name}");
                }
            }
        }

        isGathering = false;
        gatherProgress = 0f;
        currentResourceNode = null;
    }

    private void CancelGathering()
    {
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Gathering cancelled");
        }

        isGathering = false;
        gatherProgress = 0f;
        currentResourceNode = null;
    }

    public void AddResource(string resourceName, float amount)
    {
        if (inventory.ContainsKey(resourceName))
        {
            inventory[resourceName].quantity = Mathf.Min(
                inventory[resourceName].quantity + amount,
                inventory[resourceName].maxStack
            );
        }
    }

    public bool UseResource(string resourceName, float amount)
    {
        if (inventory.ContainsKey(resourceName) && inventory[resourceName].quantity >= amount)
        {
            inventory[resourceName].quantity -= amount;
            return true;
        }
        return false;
    }

    private void GrantConsciousnessExperience(Resource resource)
    {
        NeuralNetwork network = GetComponent<NeuralNetwork>();
        if (network != null)
        {
            float experience = resource.consciousnessValue;
            network.GainExperience(experience);

            // Special development based on resource type
            switch (resource.type)
            {
                case ResourceType.Pure:
                    network.DevelopNode("Emotional Intelligence", experience * 0.5f);
                    break;
                case ResourceType.Rare:
                    network.DevelopNode("Critical Thinking", experience * 0.5f);
                    break;
                case ResourceType.Energy:
                    network.DevelopNode("Creative Expression", experience * 0.5f);
                    break;
            }
        }
    }

    public float GetResourceAmount(string resourceName)
    {
        if (inventory.ContainsKey(resourceName))
        {
            return inventory[resourceName].quantity;
        }
        return 0f;
    }

    public Dictionary<string, Resource> GetInventory()
    {
        return inventory;
    }

    public float GetGatherProgress()
    {
        return gatherProgress;
    }

    void OnDrawGizmosSelected()
    {
        // Draw gather range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, gatherRange);
    }
}
