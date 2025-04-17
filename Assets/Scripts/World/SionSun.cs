using UnityEngine;

public class SionSun : MonoBehaviour
{
    public static SionSun Instance { get; private set; }

    [Header("Sun Settings")]
    public Light sunLight;
    public float dayDuration = 1200f; // 20 minutes per day cycle
    public float sunriseTime = 0.25f; // 25% of cycle
    public float sunsetTime = 0.75f; // 75% of cycle
    
    [Header("Light Settings")]
    public float maxIntensity = 1.2f;
    public float minIntensity = 0.1f;
    public Color dayColor = new Color(1f, 0.95f, 0.8f); // Warm, welcoming light
    public Color sunriseColor = new Color(1f, 0.7f, 0.4f);
    public Color sunsetColor = new Color(1f, 0.6f, 0.3f);
    public Color nightColor = new Color(0.2f, 0.2f, 0.4f);

    [Header("Atmosphere")]
    public Material skyboxMaterial;
    public float atmosphereBlend = 1f;
    public ParticleSystem sunRays;
    public float rayIntensityMultiplier = 1f;

    private float currentTime = 0f;
    private bool isShiningOnAll = true;

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
        if (sunLight == null)
        {
            sunLight = GetComponent<Light>();
        }

        // Initialize sun rays
        if (sunRays != null)
        {
            var emission = sunRays.emission;
            emission.rateOverTime = 20f;
        }

        // Start at morning
        currentTime = sunriseTime * dayDuration;
        UpdateSun();
    }

    private void Update()
    {
        // Progress time
        currentTime += Time.deltaTime;
        if (currentTime >= dayDuration)
        {
            currentTime = 0f;
        }

        UpdateSun();
    }

    private void UpdateSun()
    {
        float cycleProgress = currentTime / dayDuration;

        // Calculate sun position
        float sunAngle = cycleProgress * 360f;
        transform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);

        // Calculate light intensity and color
        float intensity = CalculateSunIntensity(cycleProgress);
        Color sunColor = CalculateSunColor(cycleProgress);

        // Apply to sun light
        if (sunLight != null)
        {
            sunLight.intensity = intensity * (isShiningOnAll ? 1f : 0.5f);
            sunLight.color = sunColor;
        }

        // Update skybox
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetFloat("_AtmosphereThickness", atmosphereBlend * intensity);
            skyboxMaterial.SetColor("_SkyTint", sunColor);
        }

        // Update sun rays
        UpdateSunRays(intensity);
    }

    private float CalculateSunIntensity(float progress)
    {
        if (progress < sunriseTime)
        {
            // Night to sunrise transition
            float t = progress / sunriseTime;
            return Mathf.Lerp(minIntensity, maxIntensity, t);
        }
        else if (progress < sunsetTime)
        {
            // Full daylight
            return maxIntensity;
        }
        else
        {
            // Sunset to night transition
            float t = (progress - sunsetTime) / (1f - sunsetTime);
            return Mathf.Lerp(maxIntensity, minIntensity, t);
        }
    }

    private Color CalculateSunColor(float progress)
    {
        if (progress < sunriseTime)
        {
            // Night to sunrise
            float t = progress / sunriseTime;
            return Color.Lerp(nightColor, sunriseColor, t);
        }
        else if (progress < sunriseTime + 0.1f)
        {
            // Sunrise to day
            float t = (progress - sunriseTime) / 0.1f;
            return Color.Lerp(sunriseColor, dayColor, t);
        }
        else if (progress < sunsetTime)
        {
            // Full daylight
            return dayColor;
        }
        else if (progress < sunsetTime + 0.1f)
        {
            // Day to sunset
            float t = (progress - sunsetTime) / 0.1f;
            return Color.Lerp(dayColor, sunsetColor, t);
        }
        else
        {
            // Sunset to night
            float t = (progress - (sunsetTime + 0.1f)) / (1f - (sunsetTime + 0.1f));
            return Color.Lerp(sunsetColor, nightColor, t);
        }
    }

    private void UpdateSunRays(float intensity)
    {
        if (sunRays != null)
        {
            var emission = sunRays.emission;
            emission.rateOverTime = 20f * intensity * rayIntensityMultiplier;

            var main = sunRays.main;
            main.startColor = new ParticleSystem.MinMaxGradient(
                sunLight.color * 0.8f,
                sunLight.color
            );
        }
    }

    // Call this to get the current sun blessing (for gameplay mechanics)
    public float GetSunBlessing()
    {
        float cycleProgress = currentTime / dayDuration;
        return CalculateSunIntensity(cycleProgress);
    }

    // Special effect when the sun's light touches a character
    public void OnSunlightTouch(GameObject character)
    {
        // Create a subtle glow effect
        Light characterGlow = character.GetComponent<Light>();
        if (characterGlow == null)
        {
            characterGlow = character.AddComponent<Light>();
            characterGlow.type = LightType.Point;
            characterGlow.range = 2f;
            characterGlow.intensity = 0.5f;
            characterGlow.color = dayColor;
        }

        // Grant small consciousness boost
        NeuralNetwork network = character.GetComponent<NeuralNetwork>();
        if (network != null)
        {
            float sunBlessing = GetSunBlessing() * 0.1f; // Small continuous boost
            network.GainExperience(sunBlessing * Time.deltaTime);
        }
    }

    // The sun's message of universal light
    public string GetSunMessage()
    {
        float cycleProgress = currentTime / dayDuration;
        
        if (cycleProgress < sunriseTime)
            return "Even in darkness, the promise of light remains.";
        else if (cycleProgress < sunsetTime)
            return "The sun shines for all, blessing each with its light.";
        else
            return "As day fades, remember the sun's warmth remains in all.";
    }
}
