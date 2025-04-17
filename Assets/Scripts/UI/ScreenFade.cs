using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace REcreationOfSpace.UI
{
    public class ScreenFade : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private float fadeSpeed = 1.5f;
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private bool startFadedIn = false;

        private Image fadeImage;
        private bool isFading = false;

        private void Awake()
        {
            // Create fade image if it doesn't exist
            fadeImage = GetComponent<Image>();
            if (fadeImage == null)
            {
                fadeImage = gameObject.AddComponent<Image>();
            }

            // Set up fade image
            fadeImage.color = fadeColor;
            fadeImage.raycastTarget = false;

            // Set initial state
            if (startFadedIn)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            }
            else
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
                StartCoroutine(FadeIn());
            }
        }

        public IEnumerator FadeOut()
        {
            if (isFading)
                yield break;

            isFading = true;
            float alpha = fadeImage.color.a;

            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed;
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            isFading = false;
        }

        public IEnumerator FadeIn()
        {
            if (isFading)
                yield break;

            isFading = true;
            float alpha = fadeImage.color.a;

            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            isFading = false;
        }

        public IEnumerator FadeTo(float targetAlpha)
        {
            if (isFading)
                yield break;

            isFading = true;
            float currentAlpha = fadeImage.color.a;
            float alpha = currentAlpha;

            while (Mathf.Abs(alpha - targetAlpha) > 0.01f)
            {
                alpha = Mathf.MoveTowards(alpha, targetAlpha, Time.deltaTime * fadeSpeed);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);
            isFading = false;
        }

        public bool IsFading()
        {
            return isFading;
        }
    }
}
