using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance { get; private set; }

    [Header("Fade Settings")]
    public Image fadePanel;
    public float fadeSpeed = 1f;
    public Color fadeColor = Color.black;

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

        // Ensure we have a fade panel
        if (fadePanel == null)
        {
            Debug.LogError("Fade panel not assigned to ScreenFade!");
        }
        else
        {
            // Initialize fade panel
            fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
    }

    public System.Collections.IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float alpha = fadePanel.color.a;

        // Fade to full opacity
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
    }

    public System.Collections.IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;

        float alpha = fadePanel.color.a;

        // Fade to transparent
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadePanel.gameObject.SetActive(false);
    }
}
