using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [System.Serializable]
    public class ResourceYield
    {
        public string resourceName;
        public float baseAmount = 1f;
        public float regenerationTime = 300f; // 5 minutes
        public bool isInfinite = false;
    }

    [Header("Resource Settings")]
    public ResourceYield yield;
    public float remainingAmount;
    public float lastGatherTime;
    public ParticleSystem gatherEffect;
    public AudioClip gatherSound;

    [Header("Visual Settings")]
    public Material depleetedMaterial;
    public Vector3 depleetedScale = new Vector3(0.8f, 0.8f, 0.8f);
    
    private Material originalMaterial;
    private Vector3 originalScale;
    private bool isDepleeted = false;
    private AudioSource audioSource;

    void Start()
    {
        // Store original appearance
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
        }
        originalScale = transform.localScale;

        // Initialize audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // Full 3D sound
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        // Initialize resource amount
        remainingAmount = yield.baseAmount;
        lastGatherTime = Time.time;
    }

    void Update()
    {
        // Check for regeneration
        if (isDepleeted && !yield.isInfinite)
        {
            if (Time.time - lastGatherTime >= yield.regenerationTime)
            {
                Regenerate();
            }
        }
    }

    public ResourceSystem.Resource GatherResource()
    {
        if (isDepleeted) return null;

        // Create gathered resource
        ResourceSystem.Resource gathered = new ResourceSystem.Resource
        {
            name = yield.resourceName,
            quantity = yield.baseAmount,
            type = GetResourceType(yield.resourceName)
        };

        // Update node state
        if (!yield.isInfinite)
        {
            remainingAmount -= yield.baseAmount;
            if (remainingAmount <= 0)
            {
                SetDepleeted();
            }
        }

        // Play effects
        PlayGatherEffects();

        return gathered;
    }

    private ResourceSystem.ResourceType GetResourceType(string resourceName)
    {
        // Match resource name to type
        foreach (var baseResource in ResourceSystem.baseResources)
        {
            if (baseResource.name == resourceName)
            {
                return baseResource.type;
            }
        }
        return ResourceSystem.ResourceType.Natural; // Default type
    }

    private void SetDepleeted()
    {
        isDepleeted = true;
        lastGatherTime = Time.time;

        // Update appearance
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && depleetedMaterial != null)
        {
            renderer.material = depleetedMaterial;
        }

        transform.localScale = depleetedScale;
    }

    private void Regenerate()
    {
        isDepleeted = false;
        remainingAmount = yield.baseAmount;

        // Restore appearance
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
        }

        transform.localScale = originalScale;

        // Play regeneration effect
        if (gatherEffect != null)
        {
            ParticleSystem effect = Instantiate(gatherEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
    }

    private void PlayGatherEffects()
    {
        // Particle effect
        if (gatherEffect != null)
        {
            ParticleSystem effect = Instantiate(gatherEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Sound effect
        if (gatherSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gatherSound);
        }
    }

    public bool IsDepeleted()
    {
        return isDepleeted;
    }

    public float GetRegenerationProgress()
    {
        if (!isDepleeted || yield.isInfinite) return 1f;
        
        float timeSinceGather = Time.time - lastGatherTime;
        return Mathf.Clamp01(timeSinceGather / yield.regenerationTime);
    }

    void OnDrawGizmos()
    {
        // Draw resource type indicator
        Gizmos.color = GetResourceColor(yield.resourceName);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    private Color GetResourceColor(string resourceName)
    {
        // Color coding for resource types
        switch (GetResourceType(resourceName))
        {
            case ResourceSystem.ResourceType.Natural:
                return Color.green;
            case ResourceSystem.ResourceType.Sacred:
                return Color.yellow;
            case ResourceSystem.ResourceType.Knowledge:
                return Color.blue;
            case ResourceSystem.ResourceType.Energy:
                return Color.magenta;
            default:
                return Color.white;
        }
    }
}
