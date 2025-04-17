using UnityEngine;
using UnityEngine.UI;

public class PortalPromptUI : MonoBehaviour
{
    public static PortalPromptUI Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject promptPanel;
    public Text promptText;
    public float fadeSpeed = 2f;
    
    [Header("Position Settings")]
    public Vector2 screenOffset = new Vector2(0f, 100f);
    
    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private Camera mainCamera;

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

        canvasGroup = promptPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = promptPanel.AddComponent<CanvasGroup>();
        }
        
        mainCamera = Camera.main;
        HidePrompt(); // Ensure we start hidden
    }

    public void ShowPrompt(string text, Vector3 worldPosition)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }

        // Convert world position to screen position
        if (mainCamera != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
            screenPos.x += screenOffset.x;
            screenPos.y += screenOffset.y;
            promptPanel.transform.position = screenPos;
        }

        isShowing = true;
        StartCoroutine(FadeCoroutine(1f));
    }

    public void HidePrompt()
    {
        isShowing = false;
        StartCoroutine(FadeCoroutine(0f));
    }

    private System.Collections.IEnumerator FadeCoroutine(float targetAlpha)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            float newAlpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            canvasGroup.alpha = newAlpha;
            yield return null;
        }

        if (targetAlpha == 0f)
        {
            promptPanel.SetActive(false);
        }
        else
        {
            promptPanel.SetActive(true);
        }
    }

    private void Update()
    {
        if (isShowing && mainCamera != null)
        {
            // Update position if showing (in case the portal or camera moves)
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            screenPos.x += screenOffset.x;
            screenPos.y += screenOffset.y;
            promptPanel.transform.position = screenPos;
        }
    }
}
