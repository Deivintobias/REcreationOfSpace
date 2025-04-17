using UnityEngine;
using System.Collections.Generic;

public class SionObserver : MonoBehaviour
{
    [System.Serializable]
    public class ObservationInsight
    {
        public string spokenClaim;
        public string observedAction;
        public float insightValue;
    }

    [Header("Observation Settings")]
    public float observationRadius = 15f;
    public float observationInterval = 0.5f;
    public LayerMask sionCharacterLayer;

    [Header("Insight Generation")]
    public float insightAccumulationRate = 1f;
    public float maxInsightLevel = 100f;
    
    private SinaiCharacter sinaiCharacter;
    private NeuralNetwork neuralNetwork;
    private float currentInsightLevel = 0f;
    private List<ObservationInsight> insights = new List<ObservationInsight>();

    private void Start()
    {
        sinaiCharacter = GetComponent<SinaiCharacter>();
        neuralNetwork = GetComponent<NeuralNetwork>();
        
        if (sinaiCharacter != null && sinaiCharacter.isSinaiCharacter)
        {
            InvokeRepeating("ObserveSionBehavior", 0f, observationInterval);
        }
    }

    private void ObserveSionBehavior()
    {
        if (!sinaiCharacter.isSinaiCharacter) return;

        Collider[] nearbyCharacters = Physics.OverlapSphere(transform.position, observationRadius, sionCharacterLayer);
        
        foreach (var character in nearbyCharacters)
        {
            ObserveCharacter(character.gameObject);
        }
    }

    private void ObserveCharacter(GameObject sionCharacter)
    {
        // Example observations of contradictions
        ObservationInsight insight = GenerateInsight(sionCharacter);
        if (insight != null)
        {
            insights.Add(insight);
            ProcessInsight(insight);
        }
    }

    private ObservationInsight GenerateInsight(GameObject sionCharacter)
    {
        // Generate insights based on observed behaviors
        ObservationInsight insight = new ObservationInsight();
        
        // Example contradictions
        if (IsNearParadiseCity(sionCharacter))
        {
            insight.spokenClaim = "We welcome all to Paradise City";
            insight.observedAction = "Guards prevent certain characters from entering";
            insight.insightValue = 15f;
        }
        else if (IsUsingResources(sionCharacter))
        {
            insight.spokenClaim = "Resources are shared equally";
            insight.observedAction = "Resources are restricted to certain groups";
            insight.insightValue = 10f;
        }
        else if (IsInSocialInteraction(sionCharacter))
        {
            insight.spokenClaim = "All are treated as equals";
            insight.observedAction = "Different treatment based on origin";
            insight.insightValue = 12f;
        }

        return insight;
    }

    private void ProcessInsight(ObservationInsight insight)
    {
        // Accumulate insight
        currentInsightLevel = Mathf.Min(currentInsightLevel + insight.insightValue, maxInsightLevel);

        // Share insight through First Guide if nearby
        if (IsNearFirstGuide())
        {
            RevealInsightThroughGuide(insight);
        }

        // Grant neural network experience based on insight
        if (neuralNetwork != null)
        {
            float experienceGain = insight.insightValue * 0.5f;
            neuralNetwork.GainExperience(experienceGain);
        }
    }

    private void RevealInsightThroughGuide(ObservationInsight insight)
    {
        if (GuiderMessageUI.Instance == null) return;

        string message = $"First Guide: Observe the difference - \nThey say: '{insight.spokenClaim}'\nYet do: '{insight.observedAction}'";
        GuiderMessageUI.Instance.ShowMessage(message);

        // Special neural network boost for understanding through the First Guide
        if (neuralNetwork != null)
        {
            float guidedInsightBonus = insight.insightValue * 2f;
            neuralNetwork.GainExperience(guidedInsightBonus);
        }
    }

    private bool IsNearFirstGuide()
    {
        if (FirstGuide.Instance == null) return false;
        
        float distanceToGuide = Vector3.Distance(transform.position, FirstGuide.Instance.transform.position);
        return distanceToGuide < 10f;
    }

    private bool IsNearParadiseCity(GameObject character)
    {
        if (ParadiseCity.Instance == null) return false;
        
        float distanceToCity = Vector3.Distance(character.transform.position, ParadiseCity.Instance.transform.position);
        return distanceToCity < 30f;
    }

    private bool IsUsingResources(GameObject character)
    {
        // Example resource usage detection
        return Physics.OverlapSphere(character.transform.position, 5f, LayerMask.GetMask("Resources")).Length > 0;
    }

    private bool IsInSocialInteraction(GameObject character)
    {
        // Example social interaction detection
        return Physics.OverlapSphere(character.transform.position, 3f, LayerMask.GetMask("Characters")).Length > 1;
    }

    public void OnInsightGained(string spokenClaim, string observedAction, float value)
    {
        ObservationInsight newInsight = new ObservationInsight
        {
            spokenClaim = spokenClaim,
            observedAction = observedAction,
            insightValue = value
        };

        ProcessInsight(newInsight);
    }

    // Get current insight level
    public float GetInsightLevel()
    {
        return currentInsightLevel;
    }

    // Get all accumulated insights
    public List<ObservationInsight> GetInsights()
    {
        return insights;
    }

    // Check if character has gained significant insight
    public bool HasSignificantInsight()
    {
        return currentInsightLevel > 70f;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize observation radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, observationRadius);
    }
}
