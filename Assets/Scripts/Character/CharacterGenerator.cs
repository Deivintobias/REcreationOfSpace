using UnityEngine;
using System.Collections.Generic;

public class CharacterGenerator : MonoBehaviour
{
    [System.Serializable]
    public class CharacterTrait
    {
        public string traitName;
        public float intensity;
        public bool isUnique;
    }

    [Header("Sion Character Settings")]
    public List<CharacterTrait> sionTraits = new List<CharacterTrait>()
    {
        new CharacterTrait { 
            traitName = "Knowledge Control",
            intensity = 0.8f,
            isUnique = false
        },
        new CharacterTrait { 
            traitName = "Deception Mastery",
            intensity = 0.7f,
            isUnique = false
        },
        new CharacterTrait { 
            traitName = "Truth Suppression",
            intensity = 0.9f,
            isUnique = false
        }
    };

    [Header("Sinai Character Settings")]
    public List<CharacterTrait> sinaiTraits = new List<CharacterTrait>()
    {
        new CharacterTrait { 
            traitName = "Truth Seeking",
            intensity = 0.9f,
            isUnique = false
        },
        new CharacterTrait { 
            traitName = "Inner Guidance",
            intensity = 0.7f,
            isUnique = false
        },
        new CharacterTrait { 
            traitName = "Knowledge Hunger",
            intensity = 0.8f,
            isUnique = false
        }
    };

    [Header("Appearance Settings")]
    public Color[] sionColors;
    public Color[] sinaiColors;
    public GameObject[] sionAccessories;
    public GameObject[] sinaiAccessories;

    [Header("Behavior Settings")]
    public float sionAggressionLevel = 0.7f;
    public float sinaiCuriosityLevel = 0.9f;
    public float traitVariation = 0.2f;

    public GameObject GenerateCharacter(bool isSionCharacter)
    {
        GameObject character = new GameObject(isSionCharacter ? "Sion_Character" : "Sinai_Character");

        // Add base components
        AddBaseComponents(character);

        // Add character-specific components
        if (isSionCharacter)
        {
            SetupSionCharacter(character);
        }
        else
        {
            SetupSinaiCharacter(character);
        }

        // Generate appearance
        GenerateAppearance(character, isSionCharacter);

        // Setup behaviors
        SetupBehaviors(character, isSionCharacter);

        return character;
    }

    private void AddBaseComponents(GameObject character)
    {
        character.AddComponent<CharacterController>();
        character.AddComponent<Health>();
        character.AddComponent<NeuralNetwork>();
        character.AddComponent<ExperienceManager>();
    }

    private void SetupSionCharacter(GameObject character)
    {
        // Add Sion-specific components
        DeceptionSystem deception = character.AddComponent<DeceptionSystem>();
        
        // Apply traits
        foreach (var trait in sionTraits)
        {
            float intensity = trait.intensity + Random.Range(-traitVariation, traitVariation);
            ApplyTrait(character, trait.traitName, intensity);
        }

        // Set behavior parameters
        var ai = character.AddComponent<SionAI>();
        ai.aggressionLevel = sionAggressionLevel + Random.Range(-traitVariation, traitVariation);
    }

    private void SetupSinaiCharacter(GameObject character)
    {
        // Add Sinai-specific components
        SinaiCharacter sinaiChar = character.AddComponent<SinaiCharacter>();
        SionObserver observer = character.AddComponent<SionObserver>();
        
        // Apply traits
        foreach (var trait in sinaiTraits)
        {
            float intensity = trait.intensity + Random.Range(-traitVariation, traitVariation);
            ApplyTrait(character, trait.traitName, intensity);
        }

        // Set behavior parameters
        sinaiChar.curiosityLevel = sinaiCuriosityLevel + Random.Range(-traitVariation, traitVariation);
    }

    private void ApplyTrait(GameObject character, string traitName, float intensity)
    {
        // Create trait component
        CharacterTrait trait = new CharacterTrait
        {
            traitName = traitName,
            intensity = intensity,
            isUnique = false
        };

        // Store trait data
        var traits = character.GetComponent<CharacterTraits>();
        if (traits == null)
        {
            traits = character.AddComponent<CharacterTraits>();
        }
        traits.AddTrait(trait);
    }

    private void GenerateAppearance(GameObject character, bool isSionCharacter)
    {
        // Create mesh
        MeshFilter meshFilter = character.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = character.AddComponent<MeshRenderer>();

        // Set base appearance
        Color baseColor = isSionCharacter ? 
            sionColors[Random.Range(0, sionColors.Length)] : 
            sinaiColors[Random.Range(0, sinaiColors.Length)];

        Material material = new Material(Shader.Find("Standard"));
        material.color = baseColor;
        meshRenderer.material = material;

        // Add accessories
        GameObject[] accessories = isSionCharacter ? sionAccessories : sinaiAccessories;
        if (accessories.Length > 0)
        {
            GameObject accessory = Instantiate(
                accessories[Random.Range(0, accessories.Length)],
                character.transform
            );
            accessory.transform.localPosition = Vector3.zero;
        }
    }

    private void SetupBehaviors(GameObject character, bool isSionCharacter)
    {
        if (isSionCharacter)
        {
            // Sion behaviors
            var deception = character.GetComponent<DeceptionSystem>();
            if (deception != null)
            {
                // Configure deception behaviors
                deception.deceptionRange *= Random.Range(0.8f, 1.2f);
            }
        }
        else
        {
            // Sinai behaviors
            var observer = character.GetComponent<SionObserver>();
            if (observer != null)
            {
                // Configure observation behaviors
                observer.observationRadius *= Random.Range(0.8f, 1.2f);
            }
        }

        // Add movement behavior
        var movement = character.AddComponent<CharacterMovement>();
        movement.moveSpeed = isSionCharacter ? 5f : 4f;
        movement.rotationSpeed = 120f;
    }

    public void OnCharacterInteraction(GameObject char1, GameObject char2)
    {
        bool isSionChar1 = char1.GetComponent<DeceptionSystem>() != null;
        bool isSionChar2 = char2.GetComponent<DeceptionSystem>() != null;

        if (isSionChar1 && !isSionChar2)
        {
            // Sion character interacting with Sinai character
            HandleSionSinaiInteraction(char1, char2);
        }
        else if (!isSionChar1 && !isSionChar2)
        {
            // Two Sinai characters interacting
            HandleSinaiSinaiInteraction(char1, char2);
        }
    }

    private void HandleSionSinaiInteraction(GameObject sionChar, GameObject sinaiChar)
    {
        var deception = sionChar.GetComponent<DeceptionSystem>();
        if (deception != null)
        {
            deception.AttemptDeception();
        }

        var observer = sinaiChar.GetComponent<SionObserver>();
        if (observer != null)
        {
            observer.ObserveBehavior(sionChar);
        }
    }

    private void HandleSinaiSinaiInteraction(GameObject char1, GameObject char2)
    {
        // Share insights and observations
        var observer1 = char1.GetComponent<SionObserver>();
        var observer2 = char2.GetComponent<SionObserver>();

        if (observer1 != null && observer2 != null)
        {
            // Exchange insights
            float insightShare = Mathf.Min(
                observer1.GetInsightLevel(),
                observer2.GetInsightLevel()
            ) * 0.1f;

            observer1.OnInsightGained("Shared Understanding", "Knowledge shared between seekers", insightShare);
            observer2.OnInsightGained("Shared Understanding", "Knowledge shared between seekers", insightShare);
        }
    }
}
