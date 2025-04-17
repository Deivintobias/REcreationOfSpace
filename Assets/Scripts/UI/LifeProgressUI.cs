using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Character;

namespace REcreationOfSpace.UI
{
    public class LifeProgressUI : MonoBehaviour
    {
        [Header("Progress Bar")]
        [SerializeField] private Image progressBar;
        [SerializeField] private Image stageIndicator;
        [SerializeField] private float indicatorMoveSpeed = 2f;
        [SerializeField] private float indicatorPulseSpeed = 1f;
        [SerializeField] private float indicatorPulseScale = 0.2f;

        [Header("Stage Colors")]
        [SerializeField] private Color infantColor = new Color(0.9f, 0.7f, 0.9f);
        [SerializeField] private Color childColor = new Color(0.7f, 0.9f, 0.7f);
        [SerializeField] private Color teenColor = new Color(0.7f, 0.7f, 0.9f);
        [SerializeField] private Color youngAdultColor = new Color(0.9f, 0.9f, 0.7f);
        [SerializeField] private Color adultColor = new Color(0.9f, 0.7f, 0.7f);
        [SerializeField] private Color elderColor = new Color(0.7f, 0.9f, 0.9f);

        [Header("Stage Icons")]
        [SerializeField] private Sprite infantIcon;
        [SerializeField] private Sprite childIcon;
        [SerializeField] private Sprite teenIcon;
        [SerializeField] private Sprite youngAdultIcon;
        [SerializeField] private Sprite adultIcon;
        [SerializeField] private Sprite elderIcon;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI milestoneText;
        [SerializeField] private Image stageImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Milestone Messages")]
        [SerializeField] private string[] infantMilestones;
        [SerializeField] private string[] childMilestones;
        [SerializeField] private string[] teenMilestones;
        [SerializeField] private string[] youngAdultMilestones;
        [SerializeField] private string[] adultMilestones;
        [SerializeField] private string[] elderMilestones;

        private LifeCycleSystem lifeCycleSystem;
        private float targetIndicatorPosition;
        private Vector3 originalIndicatorScale;
        private float lastMilestoneAge;
        private float milestoneInterval = 5f; // Years between milestones

        private void Start()
        {
            lifeCycleSystem = FindObjectOfType<LifeCycleSystem>();
            if (lifeCycleSystem == null)
            {
                Debug.LogError("No LifeCycleSystem found!");
                return;
            }

            originalIndicatorScale = stageIndicator.transform.localScale;
            
            // Subscribe to events
            lifeCycleSystem.OnAgeChanged += UpdateProgress;
            lifeCycleSystem.OnLifeStageChanged += UpdateStage;

            // Initialize display
            UpdateProgress(lifeCycleSystem.GetAge());
            UpdateStage(lifeCycleSystem.GetCurrentStage());
        }

        private void Update()
        {
            // Animate stage indicator
            if (stageIndicator != null)
            {
                // Move indicator smoothly
                float currentX = stageIndicator.transform.localPosition.x;
                float newX = Mathf.Lerp(currentX, targetIndicatorPosition, Time.deltaTime * indicatorMoveSpeed);
                stageIndicator.transform.localPosition = new Vector3(newX, stageIndicator.transform.localPosition.y, 0);

                // Pulse animation
                float pulse = 1f + Mathf.Sin(Time.time * indicatorPulseSpeed) * indicatorPulseScale;
                stageIndicator.transform.localScale = originalIndicatorScale * pulse;
            }
        }

        private void UpdateProgress(float age)
        {
            if (progressBar != null)
            {
                float progress = age / lifeCycleSystem.GetType().GetField("maxLifespanYears").GetValue(lifeCycleSystem) as float? ?? 120f;
                progressBar.fillAmount = progress;
            }

            if (ageText != null)
            {
                ageText.text = $"Age: {Mathf.Floor(age)} years";
            }

            // Check for milestones
            if (age - lastMilestoneAge >= milestoneInterval)
            {
                ShowMilestone(age);
                lastMilestoneAge = age;
            }
        }

        private void UpdateStage(LifeCycleSystem.LifeStage stage)
        {
            if (stage == null) return;

            // Update stage text
            if (stageText != null)
            {
                stageText.text = stage.name;
            }

            // Update stage icon
            if (stageImage != null)
            {
                stageImage.sprite = GetStageIcon(stage.name);
            }

            // Update colors
            Color stageColor = GetStageColor(stage.name);
            if (progressBar != null)
            {
                progressBar.color = stageColor;
            }
            if (stageIndicator != null)
            {
                stageIndicator.color = stageColor;
            }

            // Update indicator position based on life stage
            float progress = GetStageProgress(stage.name);
            targetIndicatorPosition = progressBar.rectTransform.rect.width * progress;

            // Show stage transition message
            ShowStageMilestone(stage.name);
        }

        private Sprite GetStageIcon(string stageName)
        {
            return stageName switch
            {
                "Infant" => infantIcon,
                "Child" => childIcon,
                "Teen" => teenIcon,
                "Young Adult" => youngAdultIcon,
                "Adult" => adultIcon,
                "Elder" => elderIcon,
                _ => null
            };
        }

        private Color GetStageColor(string stageName)
        {
            return stageName switch
            {
                "Infant" => infantColor,
                "Child" => childColor,
                "Teen" => teenColor,
                "Young Adult" => youngAdultColor,
                "Adult" => adultColor,
                "Elder" => elderColor,
                _ => Color.white
            };
        }

        private float GetStageProgress(string stageName)
        {
            return stageName switch
            {
                "Infant" => 0f,
                "Child" => 0.2f,
                "Teen" => 0.4f,
                "Young Adult" => 0.6f,
                "Adult" => 0.8f,
                "Elder" => 1f,
                _ => 0f
            };
        }

        private void ShowMilestone(float age)
        {
            string[] milestones = GetCurrentStageMilestones();
            if (milestones != null && milestones.Length > 0)
            {
                int index = Mathf.FloorToInt((age % milestoneInterval) / milestoneInterval * milestones.Length);
                index = Mathf.Clamp(index, 0, milestones.Length - 1);
                
                if (milestoneText != null)
                {
                    milestoneText.text = milestones[index];
                    StartCoroutine(FadeMilestoneText());
                }
            }
        }

        private string[] GetCurrentStageMilestones()
        {
            var stage = lifeCycleSystem.GetCurrentStage();
            if (stage == null) return null;

            return stage.name switch
            {
                "Infant" => infantMilestones,
                "Child" => childMilestones,
                "Teen" => teenMilestones,
                "Young Adult" => youngAdultMilestones,
                "Adult" => adultMilestones,
                "Elder" => elderMilestones,
                _ => null
            };
        }

        private void ShowStageMilestone(string stageName)
        {
            string message = stageName switch
            {
                "Infant" => "A new life begins...",
                "Child" => "Growing and learning...",
                "Teen" => "Discovering identity...",
                "Young Adult" => "Embracing purpose...",
                "Adult" => "Walking in wisdom...",
                "Elder" => "Sharing life's lessons...",
                _ => ""
            };

            if (milestoneText != null)
            {
                milestoneText.text = message;
                StartCoroutine(FadeMilestoneText());
            }
        }

        private System.Collections.IEnumerator FadeMilestoneText()
        {
            if (milestoneText == null) yield break;

            // Fade in
            float elapsed = 0f;
            float fadeDuration = 0.5f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                milestoneText.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            // Wait
            yield return new WaitForSeconds(3f);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                milestoneText.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
        }

        private void OnDestroy()
        {
            if (lifeCycleSystem != null)
            {
                lifeCycleSystem.OnAgeChanged -= UpdateProgress;
                lifeCycleSystem.OnLifeStageChanged -= UpdateStage;
            }
        }
    }
}
