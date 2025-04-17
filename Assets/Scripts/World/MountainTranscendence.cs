using UnityEngine;

public class MountainTranscendence : MonoBehaviour
{
    public static MountainTranscendence Instance { get; private set; }

    [Header("Transcendence Settings")]
    public Transform mountainPeak;
    public float transcendenceHeight = 1000f;
    public float transcendenceRadius = 10f;
    public float consciousnessThreshold = 90f; // Required freedom level

    [Header("Visual Effects")]
    public ParticleSystem transcendencePortal;
    public Light transcendenceLight;
    public float portalRotationSpeed = 30f;
    public float lightPulseSpeed = 1f;
    public Color portalColor = new Color(1f, 1f, 1f, 0.8f);

    [Header("Audio")]
    public AudioSource transcendenceAudio;
    public float audioFadeSpeed = 2f;

    private bool isPortalActive = false;
    private float currentRotation = 0f;

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
        if (transcendencePortal != null)
        {
            var main = transcendencePortal.main;
            main.startColor = portalColor;
            transcendencePortal.Stop();
        }

        if (transcendenceLight != null)
        {
            transcendenceLight.color = portalColor;
            transcendenceLight.intensity = 0f;
        }
    }

    private void Update()
    {
        if (isPortalActive)
        {
            UpdatePortalEffects();
        }
    }

    public void CheckForTranscendence(GameObject character)
    {
        if (!IsAtPeak(character))
            return;

        NeuralNetwork network = character.GetComponent<NeuralNetwork>();
        if (network == null)
            return;

        float freedomLevel = network.GetFreedomLevel();
        if (freedomLevel >= consciousnessThreshold)
        {
            ActivateTranscendencePortal(character);
        }
        else
        {
            ShowGuidanceMessage(freedomLevel);
        }
    }

    private bool IsAtPeak(GameObject character)
    {
        if (mountainPeak == null)
            return false;

        float distanceToPeak = Vector3.Distance(character.transform.position, mountainPeak.position);
        return distanceToPeak <= transcendenceRadius;
    }

    private void ActivateTranscendencePortal(GameObject character)
    {
        isPortalActive = true;

        // Position portal at peak
        if (transcendencePortal != null)
        {
            transcendencePortal.transform.position = mountainPeak.position + Vector3.up * 2f;
            transcendencePortal.Play();
        }

        if (transcendenceLight != null)
        {
            transcendenceLight.transform.position = mountainPeak.position + Vector3.up * 3f;
        }

        // Start transcendence sequence
        StartCoroutine(TranscendenceSequence(character));
    }

    private System.Collections.IEnumerator TranscendenceSequence(GameObject character)
    {
        // Initial message
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage(
                "The peak of Mount Sinai opens the way beyond the first world..."
            );
        }

        // Fade in effects
        float elapsedTime = 0f;
        while (elapsedTime < 3f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / 3f;

            // Intensify light
            if (transcendenceLight != null)
            {
                transcendenceLight.intensity = Mathf.Lerp(0f, 3f, t);
            }

            // Fade in audio
            if (transcendenceAudio != null)
            {
                transcendenceAudio.volume = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        // Hold for dramatic effect
        yield return new WaitForSeconds(2f);

        // Final message
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage(
                "Your consciousness has grown beyond the bounds of the first world..."
            );
        }

        yield return new WaitForSeconds(3f);

        // Begin transcendence
        StartCoroutine(TranscendCharacter(character));
    }

    private System.Collections.IEnumerator TranscendCharacter(GameObject character)
    {
        Vector3 startPos = character.transform.position;
        Vector3 portalPos = transcendencePortal.transform.position;
        float journeyLength = Vector3.Distance(startPos, portalPos);
        float elapsedTime = 0f;
        float duration = 5f;

        // Disable character controls
        PlayerController controller = character.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Ascend to portal
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // Use smooth step for more elegant motion
            t = Mathf.SmoothStep(0, 1, t);
            
            // Move character
            character.transform.position = Vector3.Lerp(startPos, portalPos, t);
            
            // Rotate character
            character.transform.Rotate(Vector3.up, 360f * Time.deltaTime);
            
            yield return null;
        }

        // Final flash
        if (ScreenFade.Instance != null)
        {
            yield return StartCoroutine(ScreenFade.Instance.FadeOut());
        }

        // Character has transcended
        Destroy(character);

        // Cleanup effects
        if (transcendencePortal != null)
        {
            transcendencePortal.Stop();
        }

        if (transcendenceLight != null)
        {
            transcendenceLight.intensity = 0f;
        }

        if (transcendenceAudio != null)
        {
            transcendenceAudio.Stop();
        }

        isPortalActive = false;
    }

    private void UpdatePortalEffects()
    {
        // Rotate portal
        currentRotation += portalRotationSpeed * Time.deltaTime;
        if (transcendencePortal != null)
        {
            transcendencePortal.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
        }

        // Pulse light
        if (transcendenceLight != null)
        {
            float pulse = (Mathf.Sin(Time.time * lightPulseSpeed) + 1f) * 0.5f;
            transcendenceLight.intensity = Mathf.Lerp(1f, 3f, pulse);
        }
    }

    private void ShowGuidanceMessage(float freedomLevel)
    {
        if (GuiderMessageUI.Instance == null)
            return;

        string message;
        float remaining = consciousnessThreshold - freedomLevel;

        if (remaining > 50f)
            message = "The path beyond requires much greater consciousness...";
        else if (remaining > 25f)
            message = "Your consciousness grows, but the way is not yet clear...";
        else if (remaining > 10f)
            message = "You draw near to transcendence, continue your growth...";
        else
            message = "The threshold of transcendence lies just ahead...";

        GuiderMessageUI.Instance.ShowMessage(message);
    }
}
