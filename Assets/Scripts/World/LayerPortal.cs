using UnityEngine;

public class LayerPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    public WorldManager.WorldLayer targetLayer;
    public float interactionRadius = 2f;
    public string promptText = "Press E to travel";
    
    [Header("Visual Effects")]
    public ParticleSystem portalParticles;
    public float particleIntensityMultiplier = 2f;

    private bool playerInRange = false;
    private WorldManager worldManager;

    private void Start()
    {
        worldManager = WorldManager.Instance;
        if (worldManager == null)
        {
            Debug.LogError("WorldManager not found in scene!");
        }

        // Initialize portal particles
        if (portalParticles != null)
        {
            var emission = portalParticles.emission;
            emission.rateOverTimeMultiplier = particleIntensityMultiplier;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !worldManager.IsTransitioning())
        {
            InitiateTransition();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowPrompt(true);
            
            // Increase particle effect intensity
            if (portalParticles != null)
            {
                var emission = portalParticles.emission;
                emission.rateOverTimeMultiplier = particleIntensityMultiplier * 2f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowPrompt(false);
            
            // Reset particle effect intensity
            if (portalParticles != null)
            {
                var emission = portalParticles.emission;
                emission.rateOverTimeMultiplier = particleIntensityMultiplier;
            }
        }
    }

    private void InitiateTransition()
    {
        // Only allow transition if we're not already in the target layer
        if (worldManager.GetCurrentLayer() != targetLayer)
        {
            worldManager.TransitionToLayer(targetLayer);
            
            // You could add transition effects here
            if (portalParticles != null)
            {
                portalParticles.Play();
            }
        }
    }

    private void ShowPrompt(bool show)
    {
        if (show)
        {
            PortalPromptUI.Instance?.ShowPrompt(promptText, transform.position);
        }
        else
        {
            PortalPromptUI.Instance?.HidePrompt();
        }
    }

    // Visualize the interaction radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
