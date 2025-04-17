using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace REcreationOfSpace.World
{
    public class HistoricalInfo : MonoBehaviour, IInteractable
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI yearText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI scriptureText;
        [SerializeField] private float displayDistance = 5f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem highlightEffect;
        [SerializeField] private Light spotLight;
        [SerializeField] private Material glowMaterial;
        [SerializeField] private float glowIntensity = 1f;

        private Timeline.HistoricalEvent eventData;
        private bool isPlayerNearby = false;
        private Material originalMaterial;
        private Renderer objectRenderer;

        private void Start()
        {
            // Create UI if not assigned
            if (infoPanel == null)
            {
                CreateInfoUI();
            }

            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                originalMaterial = objectRenderer.material;
            }

            // Hide UI initially
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }

        private void CreateInfoUI()
        {
            // Create canvas
            var canvasObj = new GameObject("InfoCanvas");
            canvasObj.transform.SetParent(transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create panel
            infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(canvasObj.transform, false);
            var panelImage = infoPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            var rectTransform = infoPanel.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 200);

            // Create text elements
            titleText = CreateTextElement("TitleText", infoPanel.transform, 24);
            yearText = CreateTextElement("YearText", infoPanel.transform, 18);
            descriptionText = CreateTextElement("DescriptionText", infoPanel.transform, 16);
            scriptureText = CreateTextElement("ScriptureText", infoPanel.transform, 16);

            // Position canvas in world space
            canvas.transform.localPosition = Vector3.up * 2f;
            canvas.transform.localRotation = Quaternion.identity;
        }

        private TextMeshProUGUI CreateTextElement(string name, Transform parent, int fontSize)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        public void Initialize(Timeline.HistoricalEvent evt)
        {
            eventData = evt;
            
            if (titleText != null)
                titleText.text = evt.name;
            
            if (yearText != null)
                yearText.text = evt.year < 0 ? $"{-evt.year} BC" : $"{evt.year} AD";
            
            if (descriptionText != null)
                descriptionText.text = evt.description;
            
            if (scriptureText != null && evt.relatedScriptures != null && evt.relatedScriptures.Length > 0)
                scriptureText.text = string.Join("\n", evt.relatedScriptures);
        }

        public void OnInteractionRangeEntered(GameObject interactor)
        {
            isPlayerNearby = true;
            ShowHighlight(true);

            if (infoPanel != null)
            {
                infoPanel.SetActive(true);
                // Make panel face player
                if (interactor != null)
                {
                    Vector3 directionToPlayer = interactor.transform.position - transform.position;
                    directionToPlayer.y = 0;
                    infoPanel.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
                }
            }
        }

        public void OnInteractionRangeExited(GameObject interactor)
        {
            isPlayerNearby = false;
            ShowHighlight(false);

            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }

        private void ShowHighlight(bool show)
        {
            // Particle effect
            if (highlightEffect != null)
            {
                if (show)
                    highlightEffect.Play();
                else
                    highlightEffect.Stop();
            }

            // Spotlight
            if (spotLight != null)
            {
                spotLight.enabled = show;
            }

            // Glow effect
            if (objectRenderer != null && glowMaterial != null)
            {
                objectRenderer.material = show ? glowMaterial : originalMaterial;
                if (show)
                {
                    glowMaterial.SetFloat("_GlowIntensity", glowIntensity);
                }
            }
        }

        private void Update()
        {
            if (isPlayerNearby && infoPanel != null)
            {
                // Update panel position to always face player
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector3 directionToPlayer = player.transform.position - transform.position;
                    directionToPlayer.y = 0;
                    infoPanel.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
                }
            }
        }

        public Timeline.HistoricalEvent GetEventData()
        {
            return eventData;
        }
    }
}
