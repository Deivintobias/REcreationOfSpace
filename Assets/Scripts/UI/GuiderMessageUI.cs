using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GuiderMessageUI : MonoBehaviour
{
    public static GuiderMessageUI Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject messagePanel;
    public Text messageText;
    public Image backgroundPanel;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 1f;
    public float displayDuration = 5f;
    public float fadeOutDuration = 1f;
    public float messageYOffset = 100f;
    
    [Header("Visual Style")]
    public Color textColor = new Color(1f, 1f, 1f, 1f);
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);
    public float backgroundBlur = 2f;

    private bool isDisplayingMessage = false;
    private Coroutine currentMessageCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize UI elements
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.color = textColor;
        }

        if (backgroundPanel != null)
        {
            backgroundPanel.color = backgroundColor;
        }
    }

    public void ShowMessage(string message)
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        currentMessageCoroutine = StartCoroutine(DisplayMessageSequence(message));
    }

    private IEnumerator DisplayMessageSequence(string message)
    {
        if (messagePanel == null || messageText == null) yield break;

        // Prepare message
        messagePanel.SetActive(true);
        messageText.text = message;
        
        // Set initial state
        CanvasGroup canvasGroup = messagePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = messagePanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;

        // Initial position below
        Vector3 startPos = messagePanel.transform.localPosition;
        startPos.y -= messageYOffset;
        messagePanel.transform.localPosition = startPos;

        // Fade in and rise
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;
            
            // Smooth step for more elegant animation
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // Fade in
            canvasGroup.alpha = t;
            
            // Rise up
            Vector3 newPos = startPos;
            newPos.y += messageYOffset * t;
            messagePanel.transform.localPosition = newPos;
            
            yield return null;
        }

        // Ensure final state
        canvasGroup.alpha = 1f;
        messagePanel.transform.localPosition = startPos + new Vector3(0f, messageYOffset, 0f);

        // Hold message
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsedTime = 0f;
        Vector3 finalPos = messagePanel.transform.localPosition;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;
            
            // Smooth step for more elegant animation
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // Fade out
            canvasGroup.alpha = 1f - t;
            
            // Rise slightly
            Vector3 newPos = finalPos;
            newPos.y += (messageYOffset * 0.3f) * t;
            messagePanel.transform.localPosition = newPos;
            
            yield return null;
        }

        // Hide panel
        messagePanel.SetActive(false);
        isDisplayingMessage = false;
    }

    // Helper method to check if we're currently showing a message
    public bool IsShowingMessage()
    {
        return isDisplayingMessage;
    }

    // Method to immediately hide any active message
    public void HideMessage()
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        isDisplayingMessage = false;
    }
}
