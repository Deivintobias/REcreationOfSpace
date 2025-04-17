using UnityEngine;
using System.Collections.Generic;

public class NeuralNetwork : MonoBehaviour
{
    [System.Serializable]
    public class NeuralNode
    {
        public string name;
        public float development;
        public float maxDevelopment = 100f;
        public string[] dependencies;
        public bool isUnlocked;
    }

    [Header("Neural Network Settings")]
    public float baseGrowthRate = 1f;
    public float experienceMultiplier = 1f;

    [Header("Network Nodes")]
    public List<NeuralNode> nodes = new List<NeuralNode>()
    {
        new NeuralNode { 
            name = "Basic Consciousness",
            development = 0f,
            isUnlocked = true,
            dependencies = new string[] {}
        },
        new NeuralNode {
            name = "Self Awareness",
            development = 0f,
            dependencies = new string[] { "Basic Consciousness" }
        },
        new NeuralNode {
            name = "Critical Thinking",
            development = 0f,
            dependencies = new string[] { "Self Awareness" }
        },
        new NeuralNode {
            name = "Emotional Intelligence",
            development = 0f,
            dependencies = new string[] { "Self Awareness" }
        },
        new NeuralNode {
            name = "Creative Expression",
            development = 0f,
            dependencies = new string[] { "Emotional Intelligence", "Critical Thinking" }
        },
        new NeuralNode {
            name = "True Freedom",
            development = 0f,
            dependencies = new string[] { 
                "Creative Expression",
                "Critical Thinking",
                "Emotional Intelligence"
            }
        }
    };

    private Dictionary<string, NeuralNode> nodeMap;
    private float totalDevelopment = 0f;

    void Start()
    {
        InitializeNetwork();
        UpdateNodeMap();
        CheckNodeUnlocks();
    }

    void InitializeNetwork()
    {
        nodeMap = new Dictionary<string, NeuralNode>();
        foreach (var node in nodes)
        {
            nodeMap[node.name] = node;
        }
    }

    public void GainExperience(float amount)
    {
        // Apply experience to unlocked nodes
        foreach (var node in nodes)
        {
            if (node.isUnlocked && node.development < node.maxDevelopment)
            {
                float growth = amount * baseGrowthRate * experienceMultiplier;
                node.development = Mathf.Min(node.development + growth, node.maxDevelopment);
            }
        }

        UpdateTotalDevelopment();
        CheckNodeUnlocks();
        NotifyUI();
    }

    public void DevelopNode(string nodeName, float amount)
    {
        if (nodeMap.ContainsKey(nodeName))
        {
            var node = nodeMap[nodeName];
            if (node.isUnlocked)
            {
                node.development = Mathf.Min(node.development + amount, node.maxDevelopment);
                UpdateTotalDevelopment();
                CheckNodeUnlocks();
                NotifyUI();
            }
        }
    }

    private void CheckNodeUnlocks()
    {
        bool anyUnlocked = false;

        foreach (var node in nodes)
        {
            if (!node.isUnlocked && AreNodeDependenciesMet(node))
            {
                node.isUnlocked = true;
                anyUnlocked = true;
                
                // Notify about unlocked node
                if (GuiderMessageUI.Instance != null)
                {
                    GuiderMessageUI.Instance.ShowMessage($"Neural Network expanded: {node.name} unlocked!");
                }
            }
        }

        if (anyUnlocked)
        {
            NotifyUI();
        }
    }

    private bool AreNodeDependenciesMet(NeuralNode node)
    {
        if (node.dependencies == null || node.dependencies.Length == 0)
            return true;

        foreach (string dependency in node.dependencies)
        {
            if (nodeMap.ContainsKey(dependency))
            {
                var dependencyNode = nodeMap[dependency];
                if (!dependencyNode.isUnlocked || dependencyNode.development < 50f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void UpdateTotalDevelopment()
    {
        totalDevelopment = 0f;
        float maxPossible = 0f;

        foreach (var node in nodes)
        {
            if (node.isUnlocked)
            {
                totalDevelopment += node.development;
                maxPossible += node.maxDevelopment;
            }
        }

        // Convert to percentage
        totalDevelopment = (maxPossible > 0) ? (totalDevelopment / maxPossible) * 100f : 0f;
    }

    private void NotifyUI()
    {
        var ui = FindObjectOfType<NeuralNetworkUI>();
        if (ui != null)
        {
            ui.UpdateDisplay();
        }
    }

    public float GetNodeDevelopment(string nodeName)
    {
        if (nodeMap.ContainsKey(nodeName))
        {
            return nodeMap[nodeName].development;
        }
        return 0f;
    }

    public bool IsNodeUnlocked(string nodeName)
    {
        if (nodeMap.ContainsKey(nodeName))
        {
            return nodeMap[nodeName].isUnlocked;
        }
        return false;
    }

    public float GetTotalDevelopment()
    {
        return totalDevelopment;
    }

    public float GetFreedomLevel()
    {
        if (nodeMap.ContainsKey("True Freedom"))
        {
            return nodeMap["True Freedom"].development;
        }
        return 0f;
    }

    public List<string> GetUnlockedNodes()
    {
        List<string> unlockedNodes = new List<string>();
        foreach (var node in nodes)
        {
            if (node.isUnlocked)
            {
                unlockedNodes.Add(node.name);
            }
        }
        return unlockedNodes;
    }

    public Dictionary<string, float> GetNodeDevelopments()
    {
        Dictionary<string, float> developments = new Dictionary<string, float>();
        foreach (var node in nodes)
        {
            developments[node.name] = node.development;
        }
        return developments;
    }
}
