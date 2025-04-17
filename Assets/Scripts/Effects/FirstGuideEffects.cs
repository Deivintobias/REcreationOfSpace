using UnityEngine;

public class FirstGuideEffects : MonoBehaviour
{
    [Header("Light Effects")]
    public Light truthLight;
    public float pulseSpeed = 1f;
    public float minIntensity = 0.5f;
    public float maxIntensity = 3f;
    public Color truthColor = new Color(1f, 0.92f, 0.6f); // Warm enlightenment color
    public Color resistanceColor = new Color(0.6f, 0.8f, 1f); // Cool resistance color

    [Header("Particle Systems")]
    public ParticleSystem enlightenmentParticles;
    public ParticleSystem truthRevealParticles;
    public ParticleSystem resistanceParticles;

    [Header("Visual Settings")]
    public float enlightenmentRadius = 5f;
    public float revealDuration = 3f;
    public float fadeSpeed = 2f;

    private bool isRevealing = false;
    private float currentRevealTime = 0f;
    private Material enlightenmentMaterial;

    private void Start()
    {
        // Initialize light
        if (truthLight != null)
        {
            truthLight.color = truthColor;
            truthLight.intensity = minIntensity;
        }

        // Create enlightenment material
        enlightenmentMaterial = new Material(Shader.Find("Standard"));
        enlightenmentMaterial.SetColor("_EmissionColor", truthColor);
        enlightenmentMaterial.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        if (isRevealing)
        {
            UpdateRevealEffect();
        }
        
        PulsateLight();
    }

    public void TriggerTeachingEffect(string teachingType)
    {
        switch (teachingType)
        {
            case "Truth":
                RevealTruth();
                break;
            case "Resistance":
                ShowResistance();
                break;
            case "Enlightenment":
                TriggerEnlightenment();
                break;
        }
    }

    private void RevealTruth()
    {
        if (truthRevealParticles != null)
        {
            truthRevealParticles.Clear();
            var main = truthRevealParticles.main;
            main.startColor = truthColor;
            truthRevealParticles.Play();
        }

        if (truthLight != null)
        {
            truthLight.color = truthColor;
            truthLight.intensity = maxIntensity;
        }

        isRevealing = true;
        currentRevealTime = 0f;
    }

    private void ShowResistance()
    {
        if (resistanceParticles != null)
        {
            resistanceParticles.Clear();
            var main = resistanceParticles.main;
            main.startColor = resistanceColor;
            resistanceParticles.Play();
        }

        if (truthLight != null)
        {
            truthLight.color = resistanceColor;
            truthLight.intensity = maxIntensity * 1.5f;
        }
    }

    private void TriggerEnlightenment()
    {
        if (enlightenmentParticles != null)
        {
            enlightenmentParticles.Clear();
            var main = enlightenmentParticles.main;
            main.startColor = Color.Lerp(truthColor, resistanceColor, 0.5f);
            enlightenmentParticles.Play();
        }

        // Create expanding ring effect
        CreateEnlightenmentRing();
    }

    private void UpdateRevealEffect()
    {
        currentRevealTime += Time.deltaTime;
        
        if (currentRevealTime >= revealDuration)
        {
            isRevealing = false;
            if (truthLight != null)
            {
                truthLight.intensity = minIntensity;
            }
        }
        else
        {
            float revealProgress = currentRevealTime / revealDuration;
            float intensity = Mathf.Lerp(maxIntensity, minIntensity, revealProgress);
            
            if (truthLight != null)
            {
                truthLight.intensity = intensity;
            }
        }
    }

    private void PulsateLight()
    {
        if (truthLight != null && !isRevealing)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            truthLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        }
    }

    private void CreateEnlightenmentRing()
    {
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = transform.position + Vector3.up * 0.1f;
        ring.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
        ring.GetComponent<Renderer>().material = enlightenmentMaterial;
        
        // Remove collider as it's just a visual effect
        Destroy(ring.GetComponent<Collider>());
        
        // Add expansion behavior
        StartCoroutine(ExpandRing(ring));
    }

    private System.Collections.IEnumerator ExpandRing(GameObject ring)
    {
        float expandDuration = 2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < expandDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / expandDuration;
            
            // Expand ring
            float currentRadius = Mathf.Lerp(0.1f, enlightenmentRadius, progress);
            ring.transform.localScale = new Vector3(currentRadius, 0.05f, currentRadius);
            
            // Fade out
            Color currentColor = enlightenmentMaterial.GetColor("_EmissionColor");
            currentColor.a = 1f - progress;
            enlightenmentMaterial.SetColor("_EmissionColor", currentColor);
            
            yield return null;
        }
        
        Destroy(ring);
    }

    public void OnFirstGuideNearby()
    {
        // Create subtle ambient effects when near the First Guide
        if (enlightenmentParticles != null)
        {
            var emission = enlightenmentParticles.emission;
            emission.rateOverTime = 20f;
        }

        if (truthLight != null)
        {
            truthLight.intensity = maxIntensity * 0.5f;
        }
    }

    public void OnFirstGuideAway()
    {
        // Reduce effects when moving away
        if (enlightenmentParticles != null)
        {
            var emission = enlightenmentParticles.emission;
            emission.rateOverTime = 5f;
        }

        if (truthLight != null)
        {
            truthLight.intensity = minIntensity;
        }
    }
}
