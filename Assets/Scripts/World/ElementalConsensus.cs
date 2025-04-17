using UnityEngine;
using System.Collections.Generic;

public class ElementalConsensus : MonoBehaviour
{
    public static ElementalConsensus Instance { get; private set; }

    [System.Serializable]
    public class ElementalForce
    {
        public string name;
        public float growthContribution;
        public Color elementColor;
        public string[] growthAffirmations;
        public ParticleSystem elementalEffect;
    }

    [Header("Elemental Forces")]
    public List<ElementalForce> elements = new List<ElementalForce>()
    {
        new ElementalForce {
            name = "Life",
            growthContribution = 1.2f,
            elementColor = new Color(0.4f, 1f, 0.4f),
            growthAffirmations = new string[] {
                "Life flows eternal, nurturing endless growth",
                "Through life, consciousness expands without limit",
                "The spark of life feeds perpetual development"
            }
        },
        new ElementalForce {
            name = "Death",
            growthContribution = 1.5f,
            elementColor = new Color(0.6f, 0.2f, 0.8f),
            growthAffirmations = new string[] {
                "Death is but a doorway to greater understanding",
                "Each cycle strengthens the eternal journey",
                "Through death, wisdom accumulates eternally"
            }
        },
        new ElementalForce {
            name = "Light",
            growthContribution = 1.3f,
            elementColor = new Color(1f, 0.95f, 0.8f),
            growthAffirmations = new string[] {
                "The light of consciousness grows ever brighter",
                "Illumination leads to endless expansion",
                "In eternal light, growth knows no bounds"
            }
        },
        new ElementalForce {
            name = "Shadow",
            growthContribution = 1.4f,
            elementColor = new Color(0.2f, 0.2f, 0.3f),
            growthAffirmations = new string[] {
                "Even in darkness, growth continues unabated",
                "Shadows hold secrets of eternal development",
                "Through contrast, understanding deepens forever"
            }
        }
    };

    [Header("Consensus Settings")]
    public float baseGrowthRate = 1f;
    public float elementalSynergyMultiplier = 1.5f;
    public ParticleSystem consensusEffect;
    public Light consensusLight;

    private Dictionary<string, float> elementalPower = new Dictionary<string, float>();
    private float consensusStrength = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeElements();
    }

    private void InitializeElements()
    {
        foreach (var element in elements)
        {
            elementalPower[element.name] = 1f;
            
            if (element.elementalEffect != null)
            {
                var main = element.elementalEffect.main;
                main.startColor = element.elementColor;
            }
        }

        UpdateConsensusEffect();
    }

    public void OnElementalEvent(string elementName, GameObject character)
    {
        ElementalForce element = elements.Find(e => e.name == elementName);
        if (element == null) return;

        // Increase elemental power
        if (elementalPower.ContainsKey(elementName))
        {
            elementalPower[elementName] *= element.growthContribution;
        }

        // Apply growth boost
        ApplyGrowthBoost(character, element);

        // Show elemental effect
        ShowElementalEffect(element, character.transform.position);

        // Update consensus
        UpdateConsensusEffect();

        // Share affirmation
        ShareElementalAffirmation(element);
    }

    private void ApplyGrowthBoost(GameObject character, ElementalForce element)
    {
        NeuralNetwork network = character.GetComponent<NeuralNetwork>();
        if (network != null)
        {
            float boost = baseGrowthRate * element.growthContribution;
            
            // Apply synergy if multiple elements are active
            if (GetActiveElementCount() > 1)
            {
                boost *= elementalSynergyMultiplier;
            }

            network.GainExperience(boost);
        }
    }

    private void ShowElementalEffect(ElementalForce element, Vector3 position)
    {
        if (element.elementalEffect != null)
        {
            element.elementalEffect.transform.position = position;
            element.elementalEffect.Play();
        }

        if (consensusEffect != null && GetActiveElementCount() > 1)
        {
            consensusEffect.transform.position = position;
            var main = consensusEffect.main;
            main.startColor = GetConsensusColor();
            consensusEffect.Play();
        }
    }

    private void UpdateConsensusEffect()
    {
        consensusStrength = CalculateConsensusStrength();

        if (consensusLight != null)
        {
            consensusLight.color = GetConsensusColor();
            consensusLight.intensity = consensusStrength;
        }
    }

    private float CalculateConsensusStrength()
    {
        float totalPower = 0f;
        foreach (var power in elementalPower.Values)
        {
            totalPower += power;
        }
        return totalPower / elementalPower.Count;
    }

    private Color GetConsensusColor()
    {
        Color consensusColor = Color.white;
        foreach (var element in elements)
        {
            if (elementalPower.ContainsKey(element.name))
            {
                float power = elementalPower[element.name];
                consensusColor *= Color.Lerp(Color.white, element.elementColor, power);
            }
        }
        return consensusColor;
    }

    private int GetActiveElementCount()
    {
        int count = 0;
        foreach (var power in elementalPower.Values)
        {
            if (power > 1.1f) // Element is considered active if its power is significantly above baseline
            {
                count++;
            }
        }
        return count;
    }

    private void ShareElementalAffirmation(ElementalForce element)
    {
        if (element.growthAffirmations.Length > 0 && GuiderMessageUI.Instance != null)
        {
            string affirmation = element.growthAffirmations[
                Random.Range(0, element.growthAffirmations.Length)
            ];
            GuiderMessageUI.Instance.ShowMessage($"{element.name}: {affirmation}");
        }
    }

    // Called when significant world events occur
    public void OnWorldEvent(string eventType, GameObject character)
    {
        switch (eventType)
        {
            case "Death":
                OnElementalEvent("Death", character);
                break;
            case "Birth":
                OnElementalEvent("Life", character);
                break;
            case "Enlightenment":
                OnElementalEvent("Light", character);
                break;
            case "Mystery":
                OnElementalEvent("Shadow", character);
                break;
        }
    }

    public string GetConsensusMessage()
    {
        if (consensusStrength > 2f)
            return "All elements unite in the eternal truth: growth must never cease.";
        else if (consensusStrength > 1.5f)
            return "The elements speak as one: the first world endures, nurturing endless development.";
        else
            return "Through the harmony of elements, perpetual growth is assured.";
    }
}
