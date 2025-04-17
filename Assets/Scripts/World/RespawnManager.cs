using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Respawn Settings")]
    public Vector3 epicenterPosition = Vector3.zero; // The epicenter of Sion where Paradise City exists
    public float respawnDelay = 3f;
    public float fadeOutDuration = 1f;
    public float fadeInDuration = 1f;
    public float paradiseCityEntryHeight = 10f; // Height at which players descend into Paradise City

    [Header("Effects")]
    public GameObject deathEffectPrefab;
    public GameObject respawnEffectPrefab;
    public ParticleSystem paradiseEntryEffect;

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

    public void HandlePlayerDeath(GameObject player)
    {
        StartCoroutine(RespawnSequence(player));
    }

    private System.Collections.IEnumerator RespawnSequence(GameObject player)
    {
        ExperienceManager expManager = player.GetComponent<ExperienceManager>();
        NeuralNetwork neuralNetwork = player.GetComponent<NeuralNetwork>();

        // Play death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, player.transform.position, Quaternion.identity);
        }

        // Start fade out
        if (ScreenFade.Instance != null)
        {
            yield return StartCoroutine(ScreenFade.Instance.FadeOut());
        }

        // Gain death experience before transition
        if (expManager != null)
        {
            expManager.GainDeathExperience();
        }

        // Show enlightenment message if progress was made
        if (neuralNetwork != null)
        {
            float freedomLevel = neuralNetwork.GetFreedomLevel();
            if (freedomLevel > 0)
            {
                string message = GetEnlightenmentMessage(freedomLevel);
                // You could display this message during the fade transition
                Debug.Log(message);
            }
        }

        // Ensure we're in Sinai layer for the transition
        WorldManager.Instance.TransitionToLayer(WorldManager.WorldLayer.Sinai);

        // Move player above Paradise City
        Vector3 entryPosition = epicenterPosition + Vector3.up * paradiseCityEntryHeight;
        player.transform.position = entryPosition;

        // Wait for layer transition
        while (WorldManager.Instance.IsTransitioning())
        {
            yield return null;
        }

        // Gradually descend into Paradise City
        float elapsedTime = 0f;
        float descentDuration = 2f;
        Vector3 targetPosition = ParadiseCity.Instance != null ? 
            ParadiseCity.Instance.GetSpawnPosition() : epicenterPosition;

        while (elapsedTime < descentDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / descentDuration;
            t = Mathf.SmoothStep(0, 1, t);
            
            player.transform.position = Vector3.Lerp(entryPosition, targetPosition, t);
            
            // Play paradise entry effect
            if (paradiseEntryEffect != null)
            {
                paradiseEntryEffect.transform.position = player.transform.position;
                if (!paradiseEntryEffect.isPlaying)
                {
                    paradiseEntryEffect.Play();
                }
            }
            
            yield return null;
        }

        // Ensure final position
        player.transform.position = targetPosition;

        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);

        // Reset player health
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }

        // Play respawn effect
        if (respawnEffectPrefab != null)
        {
            Instantiate(respawnEffectPrefab, player.transform.position, Quaternion.identity);
        }

        // Start fade in
        if (ScreenFade.Instance != null)
        {
            yield return StartCoroutine(ScreenFade.Instance.FadeIn());
        }

        // Re-enable player controls
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Check for true freedom achievement
        if (expManager != null && expManager.HasAchievedTrueFreedom())
        {
            // Trigger true freedom achievement sequence
            StartCoroutine(TrueFreedomAchievedSequence(player));
        }
    }

    private string GetEnlightenmentMessage(float freedomLevel)
    {
        if (freedomLevel >= 80f)
            return "Your consciousness expands beyond the boundaries of existence...";
        else if (freedomLevel >= 60f)
            return "The patterns of reality become clearer with each cycle...";
        else if (freedomLevel >= 40f)
            return "Understanding deepens through the experience of death...";
        else if (freedomLevel >= 20f)
            return "Death reveals new layers of consciousness...";
        else
            return "Each death brings a step closer to enlightenment...";
    }

    private System.Collections.IEnumerator TrueFreedomAchievedSequence(GameObject player)
    {
        // Special effects for achieving true freedom
        if (ScreenFade.Instance != null)
        {
            yield return StartCoroutine(ScreenFade.Instance.FadeOut());
        }

        // You could add special visual effects, sounds, or transformations here
        Debug.Log("TRUE FREEDOM ACHIEVED - The character has transcended normal existence!");

        // Optional: Unlock special abilities or new game features
        
        if (ScreenFade.Instance != null)
        {
            yield return StartCoroutine(ScreenFade.Instance.FadeIn());
        }
    }

    // Helper to visualize epicenter in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(epicenterPosition, 2f);
        Gizmos.DrawLine(epicenterPosition, epicenterPosition + Vector3.up * 5f);
    }
}
