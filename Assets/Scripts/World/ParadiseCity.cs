using UnityEngine;

public class ParadiseCity : MonoBehaviour
{
    public static ParadiseCity Instance { get; private set; }

    [Header("City Settings")]
    public float cityRadius = 100f; // The radius of Paradise City
    public Transform cityCenter; // The central point of the city
    
    [Header("Enlightenment Zones")]
    public Transform[] meditationSpots; // Special locations for meditation
    public Transform[] wisdomTeachers; // NPCs that provide guidance
    public Transform[] healingFountains; // Restoration points

    [Header("Visual Effects")]
    public ParticleSystem[] enlightenmentParticles;
    public Light[] cityLights;
    public float lightPulseSpeed = 1f;
    public float lightIntensityMin = 0.5f;
    public float lightIntensityMax = 1.5f;

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
        // Ensure city is positioned at world epicenter
        if (RespawnManager.Instance != null)
        {
            transform.position = RespawnManager.Instance.epicenterPosition;
        }

        InitializeCity();
    }

    private void Update()
    {
        // Animate city lights
        AnimateCityLights();
        
        // Check for players in meditation spots
        CheckMeditationSpots();
    }

    private void InitializeCity()
    {
        // Start enlightenment particles
        if (enlightenmentParticles != null)
        {
            foreach (var particles in enlightenmentParticles)
            {
                if (particles != null)
                {
                    particles.Play();
                }
            }
        }

        // Initialize healing fountains
        if (healingFountains != null)
        {
            foreach (var fountain in healingFountains)
            {
                if (fountain != null)
                {
                    // Add healing trigger collider
                    SphereCollider healingZone = fountain.gameObject.AddComponent<SphereCollider>();
                    healingZone.isTrigger = true;
                    healingZone.radius = 5f;
                }
            }
        }
    }

    private void AnimateCityLights()
    {
        if (cityLights == null) return;

        float pulseValue = (Mathf.Sin(Time.time * lightPulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(lightIntensityMin, lightIntensityMax, pulseValue);

        foreach (var light in cityLights)
        {
            if (light != null)
            {
                light.intensity = intensity;
            }
        }
    }

    private void CheckMeditationSpots()
    {
        if (meditationSpots == null) return;

        foreach (var spot in meditationSpots)
        {
            if (spot == null) continue;

            // Check for players near meditation spots
            Collider[] colliders = Physics.OverlapSphere(spot.position, 3f);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    // Grant meditation experience
                    ExperienceManager expManager = collider.GetComponent<ExperienceManager>();
                    if (expManager != null)
                    {
                        expManager.GainMeditationExperience(Time.deltaTime);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Welcome the player
            Debug.Log("Welcome to Paradise City, where enlightenment awaits...");
            
            // Heal the player
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Heal(playerHealth.maxHealth);
            }
        }
    }

    // Helper to visualize city boundaries in editor
    // Get a safe spawn position within Paradise City
    public Vector3 GetSpawnPosition()
    {
        if (cityCenter != null)
        {
            // Try to find a clear spot near the city center
            for (float radius = 0; radius < cityRadius; radius += 5f)
            {
                for (float angle = 0; angle < 360; angle += 45f)
                {
                    // Calculate position in a spiral pattern
                    float radian = angle * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(
                        Mathf.Cos(radian) * radius,
                        0f,
                        Mathf.Sin(radian) * radius
                    );
                    
                    Vector3 testPosition = cityCenter.position + offset;
                    
                    // Check if position is clear
                    if (!Physics.CheckSphere(testPosition, 1f))
                    {
                        // Found a clear spot
                        return testPosition;
                    }
                }
            }
            
            // If no clear spot found, return city center
            return cityCenter.position;
        }
        
        // Fallback to transform position
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cityRadius);

        if (meditationSpots != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var spot in meditationSpots)
            {
                if (spot != null)
                {
                    Gizmos.DrawWireSphere(spot.position, 3f);
                }
            }
        }

        if (healingFountains != null)
        {
            Gizmos.color = Color.green;
            foreach (var fountain in healingFountains)
            {
                if (fountain != null)
                {
                    Gizmos.DrawWireSphere(fountain.position, 5f);
                }
            }
        }

        // Visualize spawn search area
        if (cityCenter != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(cityCenter.position, 5f);
            Gizmos.DrawLine(cityCenter.position, cityCenter.position + Vector3.up * 5f);
        }
    }
}
