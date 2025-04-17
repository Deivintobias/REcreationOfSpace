using UnityEngine;

public class MountSinai : MonoBehaviour
{
    public static MountSinai Instance { get; private set; }

    [Header("Mountain Settings")]
    public float mountainHeight = 1000f;
    public float baseRadius = 500f;
    public Transform mountainPeak;
    public Transform[] sacredPaths; // Paths leading up the mountain

    [Header("Sacred Zones")]
    public Transform[] meditationPoints; // Special meditation spots on the mountain
    public Transform[] enlightenmentZones; // Areas of heightened consciousness
    public float sacredZoneRadius = 10f;

    [Header("Visual Effects")]
    public ParticleSystem[] mountainMist;
    public Light[] sacredLights;
    public float lightPulseSpeed = 0.5f;
    public float lightIntensityMin = 0.2f;
    public float lightIntensityMax = 1f;

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
        InitializeMountain();
    }

    private void Update()
    {
        AnimateSacredEffects();
        CheckMeditationPoints();
    }

    private void InitializeMountain()
    {
        // Start mountain mist effects
        if (mountainMist != null)
        {
            foreach (var mist in mountainMist)
            {
                if (mist != null)
                {
                    mist.Play();
                }
            }
        }

        // Initialize sacred zones
        foreach (var zone in enlightenmentZones)
        {
            if (zone != null)
            {
                // Add trigger collider for sacred zones
                SphereCollider zoneCollider = zone.gameObject.AddComponent<SphereCollider>();
                zoneCollider.isTrigger = true;
                zoneCollider.radius = sacredZoneRadius;
            }
        }
    }

    private void AnimateSacredEffects()
    {
        if (sacredLights == null) return;

        float pulseValue = (Mathf.Sin(Time.time * lightPulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(lightIntensityMin, lightIntensityMax, pulseValue);

        foreach (var light in sacredLights)
        {
            if (light != null)
            {
                light.intensity = intensity;
            }
        }
    }

    private void CheckMeditationPoints()
    {
        if (meditationPoints == null) return;

        foreach (var point in meditationPoints)
        {
            if (point == null) continue;

            Collider[] colliders = Physics.OverlapSphere(point.position, sacredZoneRadius);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    OnPlayerEnterMeditationPoint(collider.gameObject, point);
                }
            }
        }
    }

    private void OnPlayerEnterMeditationPoint(GameObject player, Transform meditationPoint)
    {
        // Grant heightened meditation experience
        ExperienceManager expManager = player.GetComponent<ExperienceManager>();
        if (expManager != null)
        {
            // Mountain meditation is more powerful
            expManager.GainMeditationExperience(Time.deltaTime * 2f);
        }

        // Trigger special guidance
        GuiderSystem guider = player.GetComponent<GuiderSystem>();
        if (guider != null)
        {
            float height = meditationPoint.position.y;
            string message = GetMountainMessage(height / mountainHeight);
            guider.OnSignificantEvent("MountainMeditation");
        }
    }

    private string GetMountainMessage(float heightPercentage)
    {
        if (heightPercentage > 0.9f)
            return "At the peak of Mount Sinai, truth reveals itself in perfect clarity.";
        else if (heightPercentage > 0.7f)
            return "The higher you climb, the clearer your understanding becomes.";
        else if (heightPercentage > 0.5f)
            return "Mount Sinai's sacred heights offer deeper insights into existence.";
        else if (heightPercentage > 0.3f)
            return "Each step up the mountain brings you closer to enlightenment.";
        else
            return "The path up Mount Sinai beckons those who seek higher truth.";
    }

    public bool IsOnMountain(Vector3 position)
    {
        float distanceFromCenter = Vector2.Distance(
            new Vector2(position.x, position.z),
            new Vector2(transform.position.x, transform.position.z)
        );

        return distanceFromCenter <= baseRadius;
    }

    public float GetMountainHeight(Vector3 position)
    {
        if (!IsOnMountain(position))
            return 0f;

        float distanceFromCenter = Vector2.Distance(
            new Vector2(position.x, position.z),
            new Vector2(transform.position.x, transform.position.z)
        );

        // Calculate height based on distance from center (simple cone shape)
        return Mathf.Lerp(mountainHeight, 0f, distanceFromCenter / baseRadius);
    }

    private void OnDrawGizmos()
    {
        // Draw mountain base
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, baseRadius);

        // Draw mountain peak
        if (mountainPeak != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, mountainPeak.position);
            Gizmos.DrawWireSphere(mountainPeak.position, 10f);
        }

        // Draw meditation points
        if (meditationPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in meditationPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, sacredZoneRadius);
                }
            }
        }

        // Draw enlightenment zones
        if (enlightenmentZones != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var zone in enlightenmentZones)
            {
                if (zone != null)
                {
                    Gizmos.DrawWireSphere(zone.position, sacredZoneRadius);
                }
            }
        }
    }
}
