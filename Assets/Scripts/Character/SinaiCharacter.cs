using UnityEngine;

public class SinaiCharacter : MonoBehaviour
{
    [Header("Character Settings")]
    public bool isSinaiCharacter = true;
    public float discomfortLevel = 0f;
    public float guidanceNeed = 0f;
    
    [Header("Environmental Effects")]
    public float discomfortRate = 1f;
    public float recoveryRate = 2f;
    public float maxDiscomfort = 100f;
    
    [Header("Visual Feedback")]
    public ParticleSystem discomfortParticles;
    public Color discomfortColor = new Color(0.7f, 0.3f, 0.3f);
    public float particleIntensityMultiplier = 1f;

    private NeuralNetwork neuralNetwork;
    private bool hasFoundFirstGuide = false;
    private float timeInSionArea = 0f;
    private Vector3? lastComfortablePosition = null;

    private void Start()
    {
        neuralNetwork = GetComponent<NeuralNetwork>();
        
        if (discomfortParticles != null)
        {
            var main = discomfortParticles.main;
            main.startColor = discomfortColor;
        }
    }

    private void Update()
    {
        if (!isSinaiCharacter) return;

        if (!hasFoundFirstGuide)
        {
            UpdateDiscomfort();
            UpdateGuidanceNeed();
            UpdateVisualFeedback();
        }
    }

    private void UpdateDiscomfort()
    {
        // Increase discomfort while in Sion's area
        if (!IsInComfortZone())
        {
            timeInSionArea += Time.deltaTime;
            discomfortLevel = Mathf.Min(discomfortLevel + (discomfortRate * Time.deltaTime), maxDiscomfort);
            
            // Apply movement penalties when highly uncomfortable
            if (discomfortLevel > 70f)
            {
                SlowMovement();
            }

            // Show guidance messages
            if (timeInSionArea > 10f && !hasFoundFirstGuide)
            {
                ShowGuidanceHint();
            }
        }
        else
        {
            // Recover when near First Guide or in comfort zones
            discomfortLevel = Mathf.Max(0f, discomfortLevel - (recoveryRate * Time.deltaTime));
            timeInSionArea = 0f;
        }
    }

    private void UpdateGuidanceNeed()
    {
        guidanceNeed = Mathf.Min(100f, guidanceNeed + (Time.deltaTime * (discomfortLevel / 50f)));
        
        if (guidanceNeed > 80f && !hasFoundFirstGuide)
        {
            // Strong urge to seek the First Guide
            HighlightFirstGuideDirection();
        }
    }

    private bool IsInComfortZone()
    {
        // Check if near First Guide
        if (FirstGuide.Instance != null)
        {
            float distanceToGuide = Vector3.Distance(transform.position, FirstGuide.Instance.transform.position);
            if (distanceToGuide < 10f)
            {
                hasFoundFirstGuide = true;
                OnFindFirstGuide();
                return true;
            }
        }

        // Check if on Mount Sinai
        if (MountSinai.Instance != null)
        {
            return MountSinai.Instance.IsOnMountain(transform.position);
        }

        return false;
    }

    private void SlowMovement()
    {
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            // Reduce movement speed based on discomfort
            float speedMultiplier = 1f - (discomfortLevel / maxDiscomfort);
            controller.moveSpeed *= speedMultiplier;
        }
    }

    private void ShowGuidanceHint()
    {
        if (GuiderMessageUI.Instance == null) return;

        if (timeInSionArea > 30f)
        {
            GuiderMessageUI.Instance.ShowMessage(
                "This realm weighs heavily upon you. Seek the First Guide for understanding."
            );
        }
        else if (timeInSionArea > 20f)
        {
            GuiderMessageUI.Instance.ShowMessage(
                "You feel out of place in Sion's domain. The First Guide can show you the way."
            );
        }
        else
        {
            GuiderMessageUI.Instance.ShowMessage(
                "As a child of Sinai, you sense this is not your realm."
            );
        }
    }

    private void HighlightFirstGuideDirection()
    {
        if (FirstGuide.Instance == null) return;

        // Create a visual indicator pointing toward the First Guide
        Vector3 directionToGuide = FirstGuide.Instance.transform.position - transform.position;
        
        if (discomfortParticles != null)
        {
            // Adjust particles to point toward guide
            discomfortParticles.transform.rotation = Quaternion.LookRotation(directionToGuide);
            
            // Increase particle intensity
            var emission = discomfortParticles.emission;
            emission.rateOverTime = 20f * particleIntensityMultiplier * (guidanceNeed / 100f);
        }
    }

    private void OnFindFirstGuide()
    {
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage(
                "The First Guide's presence brings clarity. Here you can learn the path to your true realm."
            );
        }

        // Grant immediate relief
        discomfortLevel = 0f;
        guidanceNeed = 0f;

        // Stop discomfort particles
        if (discomfortParticles != null)
        {
            discomfortParticles.Stop();
        }

        // Begin guidance sequence
        StartCoroutine(FirstGuideTeachingSequence());
    }

    private System.Collections.IEnumerator FirstGuideTeachingSequence()
    {
        yield return new WaitForSeconds(2f);

        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage(
                "The First Guide: Child of Sinai, I shall show you the way to your realm."
            );
        }

        yield return new WaitForSeconds(4f);

        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage(
                "The First Guide: Through understanding and growth, you shall find your path home."
            );
        }

        // Grant special neural network boost
        if (neuralNetwork != null)
        {
            neuralNetwork.GainExperience(20f);
        }
    }

    // Call this when the character receives guidance
    public void ReceiveGuidance(float amount)
    {
        if (!isSinaiCharacter) return;

        // Reduce discomfort
        discomfortLevel = Mathf.Max(0f, discomfortLevel - amount);

        // Grant neural network experience
        if (neuralNetwork != null)
        {
            neuralNetwork.GainExperience(amount * 0.5f);
        }

        // Mark last comfortable position
        lastComfortablePosition = transform.position;
    }

    // Get the last known comfortable position
    public Vector3? GetLastComfortablePosition()
    {
        return lastComfortablePosition;
    }

    // Check if the character is currently struggling
    public bool IsStruggling()
    {
        return isSinaiCharacter && !hasFoundFirstGuide && discomfortLevel > 50f;
    }
}
