using UnityEngine;

public class CharacterVisualGenerator : MonoBehaviour
{
    [System.Serializable]
    public class CharacterVisualSettings
    {
        [Header("Body Settings")]
        public float minHeight = 1.7f;
        public float maxHeight = 2.0f;
        public float minWidth = 0.4f;
        public float maxWidth = 0.6f;
        
        [Header("Color Settings")]
        public Color primaryColor;
        public Color secondaryColor;
        public float colorVariation = 0.1f;
        
        [Header("Glow Settings")]
        public bool hasGlow;
        public Color glowColor;
        public float glowIntensity = 1f;
        public float glowPulseSpeed = 1f;
    }

    [Header("Sion Character Visuals")]
    public CharacterVisualSettings sionSettings = new CharacterVisualSettings
    {
        primaryColor = new Color(0.8f, 0.8f, 0.9f), // Light, cold color
        secondaryColor = new Color(0.6f, 0.6f, 0.7f),
        hasGlow = true,
        glowColor = new Color(0.5f, 0.5f, 1f)
    };

    [Header("Sinai Character Visuals")]
    public CharacterVisualSettings sinaiSettings = new CharacterVisualSettings
    {
        primaryColor = new Color(0.9f, 0.8f, 0.7f), // Warm, earthy color
        secondaryColor = new Color(0.7f, 0.6f, 0.5f),
        hasGlow = true,
        glowColor = new Color(1f, 0.8f, 0.5f)
    };

    [Header("Visual Components")]
    public Material characterBaseMaterial;
    public Material glowMaterial;
    public GameObject glowEffectPrefab;

    public void GenerateCharacterVisuals(GameObject character, bool isSionCharacter)
    {
        CharacterVisualSettings settings = isSionCharacter ? sionSettings : sinaiSettings;
        
        // Generate base mesh
        GenerateCharacterMesh(character, settings);
        
        // Apply materials and colors
        ApplyVisualProperties(character, settings);
        
        // Add glow effects
        if (settings.hasGlow)
        {
            AddGlowEffect(character, settings);
        }
        
        // Add animator
        AddCharacterAnimator(character);
    }

    private void GenerateCharacterMesh(GameObject character, CharacterVisualSettings settings)
    {
        // Create body parts
        GameObject body = CreateBodyPart(character, "Body", settings);
        GameObject head = CreateBodyPart(body, "Head", settings, 0.3f);
        
        // Position head
        float bodyHeight = body.transform.localScale.y;
        head.transform.localPosition = new Vector3(0, bodyHeight/2f + 0.15f, 0);
        
        // Add distinctive features based on character type
        if (settings == sionSettings)
        {
            // Sion characters have angular, geometric features
            CreateGeometricFeatures(character);
        }
        else
        {
            // Sinai characters have flowing, organic features
            CreateOrganicFeatures(character);
        }
    }

    private GameObject CreateBodyPart(GameObject parent, string name, CharacterVisualSettings settings, float scaleFactor = 1f)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        part.name = name;
        part.transform.SetParent(parent.transform);
        part.transform.localPosition = Vector3.zero;

        // Randomize size within constraints
        float height = Random.Range(settings.minHeight, settings.maxHeight) * scaleFactor;
        float width = Random.Range(settings.minWidth, settings.maxWidth) * scaleFactor;
        part.transform.localScale = new Vector3(width, height, width);

        return part;
    }

    private void CreateGeometricFeatures(GameObject character)
    {
        // Add angular accessories
        GameObject[] geometricParts = new GameObject[3];
        for (int i = 0; i < geometricParts.Length; i++)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.transform.SetParent(character.transform);
            part.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            // Position in geometric pattern
            float angle = i * 120f * Mathf.Deg2Rad;
            part.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.3f,
                0.5f,
                Mathf.Sin(angle) * 0.3f
            );
            
            geometricParts[i] = part;
        }
    }

    private void CreateOrganicFeatures(GameObject character)
    {
        // Add flowing accessories
        GameObject[] organicParts = new GameObject[5];
        for (int i = 0; i < organicParts.Length; i++)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part.transform.SetParent(character.transform);
            part.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            
            // Position in spiral pattern
            float angle = i * 72f * Mathf.Deg2Rad;
            float radius = 0.2f + i * 0.05f;
            part.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                0.3f + i * 0.1f,
                Mathf.Sin(angle) * radius
            );
            
            organicParts[i] = part;
        }
    }

    private void ApplyVisualProperties(GameObject character, CharacterVisualSettings settings)
    {
        // Create materials
        Material baseMaterial = new Material(characterBaseMaterial);
        
        // Apply colors with variation
        Color primaryWithVariation = AddColorVariation(settings.primaryColor, settings.colorVariation);
        Color secondaryWithVariation = AddColorVariation(settings.secondaryColor, settings.colorVariation);
        
        baseMaterial.SetColor("_Color", primaryWithVariation);
        baseMaterial.SetColor("_EmissionColor", secondaryWithVariation);
        
        // Apply to all renderers
        foreach (Renderer renderer in character.GetComponentsInChildren<Renderer>())
        {
            renderer.material = baseMaterial;
        }
    }

    private void AddGlowEffect(GameObject character, CharacterVisualSettings settings)
    {
        if (glowEffectPrefab == null) return;

        GameObject glowEffect = Instantiate(glowEffectPrefab, character.transform);
        glowEffect.transform.localPosition = Vector3.zero;
        
        // Setup glow parameters
        ParticleSystem particles = glowEffect.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            var main = particles.main;
            main.startColor = settings.glowColor;
            
            var emission = particles.emission;
            emission.rateOverTime = settings.glowIntensity * 10f;
        }
        
        // Add pulsing behavior
        GlowPulse glowPulse = glowEffect.AddComponent<GlowPulse>();
        glowPulse.pulseSpeed = settings.glowPulseSpeed;
        glowPulse.glowColor = settings.glowColor;
        glowPulse.intensity = settings.glowIntensity;
    }

    private void AddCharacterAnimator(GameObject character)
    {
        Animator animator = character.AddComponent<Animator>();
        // Setup animator parameters and states
        // This would connect to your animation system
    }

    private Color AddColorVariation(Color baseColor, float variation)
    {
        return new Color(
            baseColor.r + Random.Range(-variation, variation),
            baseColor.g + Random.Range(-variation, variation),
            baseColor.b + Random.Range(-variation, variation),
            baseColor.a
        );
    }
}

// Helper class for glow effect
public class GlowPulse : MonoBehaviour
{
    public float pulseSpeed = 1f;
    public Color glowColor = Color.white;
    public float intensity = 1f;
    
    private Light glowLight;
    
    private void Start()
    {
        glowLight = gameObject.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = glowColor;
        glowLight.range = 2f;
    }
    
    private void Update()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        glowLight.intensity = pulse * intensity;
    }
}
