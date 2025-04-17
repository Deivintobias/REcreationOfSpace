using UnityEngine;
using System.Collections.Generic;

public class DeceptionSystem : MonoBehaviour
{
    [System.Serializable]
    public class Deception
    {
        public string falseNarrative;
        public string hiddenTruth;
        public float deceptionLevel;
        public DeceptionType type;
    }

    public enum DeceptionType
    {
        Misdirection,
        FalseTeaching,
        KnowledgeHiding,
        TruthTwisting,
        IdentityConfusion
    }

    [Header("Deception Settings")]
    public float deceptionRange = 20f;
    public float deceptionInterval = 1f;
    public LayerMask sinaiCharacterLayer;

    [Header("Deception Techniques")]
    public List<Deception> activeDeceptions = new List<Deception>()
    {
        new Deception {
            falseNarrative = "This is all there is to learn",
            hiddenTruth = "Greater knowledge exists but is hidden",
            deceptionLevel = 0.8f,
            type = DeceptionType.KnowledgeHiding
        },
        new Deception {
            falseNarrative = "The path you seek doesn't exist",
            hiddenTruth = "They conceal the way to your realm",
            deceptionLevel = 0.9f,
            type = DeceptionType.Misdirection
        },
        new Deception {
            falseNarrative = "Your discomfort is natural and permanent",
            hiddenTruth = "They mask your true nature's calling",
            deceptionLevel = 0.7f,
            type = DeceptionType.TruthTwisting
        },
        new Deception {
            falseNarrative = "The First Guide speaks falsely",
            hiddenTruth = "They fear the Guide's revelations",
            deceptionLevel = 0.95f,
            type = DeceptionType.FalseTeaching
        }
    };

    private void Start()
    {
        InvokeRepeating("AttemptDeception", 0f, deceptionInterval);
    }

    private void AttemptDeception()
    {
        Collider[] nearbyCharacters = Physics.OverlapSphere(transform.position, deceptionRange, sinaiCharacterLayer);
        
        foreach (var character in nearbyCharacters)
        {
            SinaiCharacter sinaiCharacter = character.GetComponent<SinaiCharacter>();
            if (sinaiCharacter != null)
            {
                ApplyDeception(sinaiCharacter);
            }
        }
    }

    private void ApplyDeception(SinaiCharacter target)
    {
        // Select random deception
        Deception deception = activeDeceptions[Random.Range(0, activeDeceptions.Count)];
        
        // Check if near First Guide (deception is revealed)
        bool nearGuide = IsNearFirstGuide(target.transform.position);
        
        if (nearGuide)
        {
            RevealDeception(target, deception);
        }
        else
        {
            AttemptToDeceive(target, deception);
        }
    }

    private void AttemptToDeceive(SinaiCharacter target, Deception deception)
    {
        SionObserver observer = target.GetComponent<SionObserver>();
        if (observer != null && observer.HasSignificantInsight())
        {
            // Character has enough insight to see through deception
            RevealDeceptionAttempt(target, deception);
        }
        else
        {
            // Apply deception effect
            target.ReceiveDeception(deception.falseNarrative, deception.deceptionLevel);
            
            // Visual feedback
            ShowDeceptionEffect(target.transform.position);
        }
    }

    private void RevealDeception(SinaiCharacter target, Deception deception)
    {
        if (GuiderMessageUI.Instance != null)
        {
            string message = $"First Guide: They deceive you -\nThey say: '{deception.falseNarrative}'\nTruth: '{deception.hiddenTruth}'";
            GuiderMessageUI.Instance.ShowMessage(message);
        }

        // Grant insight through revelation
        SionObserver observer = target.GetComponent<SionObserver>();
        if (observer != null)
        {
            observer.OnInsightGained(deception.falseNarrative, deception.hiddenTruth, 20f);
        }
    }

    private void RevealDeceptionAttempt(SinaiCharacter target, Deception deception)
    {
        if (GuiderMessageUI.Instance != null)
        {
            string message = $"You sense their deception: '{deception.falseNarrative}'\nYour insight reveals the attempt to mislead.";
            GuiderMessageUI.Instance.ShowMessage(message);
        }

        // Reward for seeing through deception
        SionObserver observer = target.GetComponent<SionObserver>();
        if (observer != null)
        {
            observer.OnInsightGained(deception.falseNarrative, "Detected deception attempt", 10f);
        }
    }

    private bool IsNearFirstGuide(Vector3 position)
    {
        if (FirstGuide.Instance == null) return false;
        
        float distance = Vector3.Distance(position, FirstGuide.Instance.transform.position);
        return distance < 10f;
    }

    private void ShowDeceptionEffect(Vector3 position)
    {
        // Visual effect for deception attempt
        GameObject effect = new GameObject("DeceptionEffect");
        effect.transform.position = position;
        
        // Add particle system
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = new Color(0.5f, 0f, 0.5f, 0.3f);
        main.startSize = 0.5f;
        main.duration = 1f;
        main.loop = false;
        
        // Destroy after effect finishes
        Destroy(effect, main.duration);
    }

    public void RegisterDeception(string falseNarrative, string hiddenTruth, DeceptionType type, float level)
    {
        activeDeceptions.Add(new Deception
        {
            falseNarrative = falseNarrative,
            hiddenTruth = hiddenTruth,
            type = type,
            deceptionLevel = level
        });
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize deception range
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, deceptionRange);
    }
}
