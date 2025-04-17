using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    [Header("Experience Settings")]
    public float combatExpMultiplier = 1f;
    public float explorationExpMultiplier = 1f;
    public float creativeExpMultiplier = 1.5f;
    public float socialExpMultiplier = 1.2f;
    public float meditationExpMultiplier = 2f;

    [Header("Node Development Settings")]
    [Tooltip("Which nodes benefit most from which activities")]
    public string[] combatNodes = { "Basic Consciousness", "Self Awareness" };
    public string[] explorationNodes = { "Critical Thinking", "Self Awareness" };
    public string[] creativeNodes = { "Creative Expression", "Emotional Intelligence" };
    public string[] socialNodes = { "Emotional Intelligence", "Self Awareness" };
    public string[] meditationNodes = { "True Freedom", "Critical Thinking" };

    private NeuralNetwork neuralNetwork;

    private void Start()
    {
        neuralNetwork = GetComponent<NeuralNetwork>();
        if (neuralNetwork == null)
        {
            Debug.LogError("NeuralNetwork component not found!");
        }
    }

    // Combat experience from defeating enemies or surviving battles
    public void GainCombatExperience(float baseAmount)
    {
        if (neuralNetwork == null) return;

        float experience = baseAmount * combatExpMultiplier;
        foreach (string nodeName in combatNodes)
        {
            neuralNetwork.DevelopNode(nodeName, experience);
        }
        
        // Small general development for all nodes
        neuralNetwork.GainExperience(experience * 0.1f);
    }

    // Exploration experience from discovering new areas or collecting items
    public void GainExplorationExperience(float baseAmount)
    {
        if (neuralNetwork == null) return;

        float experience = baseAmount * explorationExpMultiplier;
        foreach (string nodeName in explorationNodes)
        {
            neuralNetwork.DevelopNode(nodeName, experience);
        }
        
        neuralNetwork.GainExperience(experience * 0.1f);
    }

    // Creative experience from crafting, building, or solving puzzles
    public void GainCreativeExperience(float baseAmount)
    {
        if (neuralNetwork == null) return;

        float experience = baseAmount * creativeExpMultiplier;
        foreach (string nodeName in creativeNodes)
        {
            neuralNetwork.DevelopNode(nodeName, experience);
        }
        
        neuralNetwork.GainExperience(experience * 0.15f);
    }

    // Social experience from interacting with NPCs or other players
    public void GainSocialExperience(float baseAmount)
    {
        if (neuralNetwork == null) return;

        float experience = baseAmount * socialExpMultiplier;
        foreach (string nodeName in socialNodes)
        {
            neuralNetwork.DevelopNode(nodeName, experience);
        }
        
        neuralNetwork.GainExperience(experience * 0.12f);
    }

    // Meditation experience from quiet contemplation or spiritual activities
    public void GainMeditationExperience(float baseAmount)
    {
        if (neuralNetwork == null) return;

        float experience = baseAmount * meditationExpMultiplier;
        foreach (string nodeName in meditationNodes)
        {
            neuralNetwork.DevelopNode(nodeName, experience);
        }
        
        neuralNetwork.GainExperience(experience * 0.2f);
    }

    // Special milestone experience for major achievements
    public void GainMilestoneExperience(float baseAmount, string primaryNode)
    {
        if (neuralNetwork == null) return;

        // Major boost to specific node
        neuralNetwork.DevelopNode(primaryNode, baseAmount * 2f);
        
        // Moderate boost to all nodes
        neuralNetwork.GainExperience(baseAmount);
    }

    // Experience from dying and respawning at the epicenter
    public void GainDeathExperience()
    {
        if (neuralNetwork == null) return;

        // Death provides unique insights into consciousness and self-awareness
        neuralNetwork.DevelopNode("Basic Consciousness", 5f);
        neuralNetwork.DevelopNode("Self Awareness", 7f);
        
        // Small development boost to all nodes from the experience
        neuralNetwork.GainExperience(3f);
    }

    // Check if character has achieved true freedom
    public bool HasAchievedTrueFreedom()
    {
        return neuralNetwork != null && neuralNetwork.GetNodeDevelopment("True Freedom") >= 100f;
    }
}
