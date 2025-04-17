using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.Character;

namespace REcreationOfSpace.UI
{
    public class LifeCycleUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image lifeProgressBar;
        [SerializeField] private Image stageProgressBar;
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private Image stageIcon;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Stage Icons")]
        [SerializeField] private Sprite infantIcon;
        [SerializeField] private Sprite childIcon;
        [SerializeField] private Sprite teenIcon;
        [SerializeField] private Sprite youngAdultIcon;
        [SerializeField] private Sprite adultIcon;
        [SerializeField] private Sprite elderIcon;

        [Header("Visual Settings")]
        [SerializeField] private Gradient lifeProgressColors;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private bool autoHide = true;
        [SerializeField] private float autoHideDelay = 5f;
        [SerializeField] private KeyCode toggleKey = KeyCode.L;

        [Header("Animation")]
        [SerializeField] private bool useAnimation = true;
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float pulseScale = 0.1f;
        [SerializeField] private ParticleSystem stageTransitionEffect;

        private LifeCycleSystem lifeCycleSystem;
        private float autoHideTimer;
        private bool isVisible = true;
        private Vector3 iconOriginalScale;

        private void Start()
        {
            lifeCycleSystem = FindObjectOfType<LifeCycleSystem>();
            if (lifeCycleSystem == null)
            {
                Debug.LogError("No LifeCycleSystem found!");
                return;
            }

            if (stageIcon != null)
                iconOriginalScale = stageIcon.transform.localScale;

            // Subscribe to events
            lifeCycleSystem.OnAgeChanged += UpdateAgeDisplay;
            lifeCycleSystem.OnLifeStageChanged += UpdateStageDisplay;

            // Initialize display
            UpdateAgeDisplay(lifeCycleSystem.GetAge());
            UpdateStageDisplay(lifeCycleSystem.GetCurrentStage());

            if (autoHide)
                autoHideTimer = autoHideDelay;
        }

        private void Update()
        {
            // Handle visibility toggle
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleVisibility();
            }

            // Handle auto-hide
            if (autoHide && isVisible)
            {
                autoHideTimer -= Time.deltaTime;
                if (autoHideTimer <= 0)
                {
                    SetVisibility(false);
                }
            }

            // Animate icon
            if (useAnimation && stageIcon != null && isVisible)
            {
                AnimateIcon();
            }

            // Update canvas group alpha
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, isVisible ? 1f : 0f, Time.deltaTime * fadeSpeed);
            }
        }

        private void UpdateAgeDisplay(float age)
        {
            if (ageText != null)
            {
                ageText.text = $"Age: {Mathf.Floor(age)} years";
            }

            if (lifeProgressBar != null)
            {
                float progress = lifeCycleSystem.GetLifeProgress();
                lifeProgressBar.fillAmount = progress;
                lifeProgressBar.color = lifeProgressColors.Evaluate(progress);
            }

            // Reset auto-hide timer
            if (autoHide)
                autoHideTimer = autoHideDelay;
        }

        private void UpdateStageDisplay(LifeCycleSystem.LifeStage stage)
        {
            if (stage == null) return;

            if (stageText != null)
            {
                stageText.text = stage.name;
            }

            if (stageIcon != null)
            {
                // Set appropriate icon based on stage
                stageIcon.sprite = GetStageIcon(stage.name);
                stageIcon.color = stage.auraColor;
            }

            // Play transition effect
            if (stageTransitionEffect != null)
            {
                var effect = Instantiate(stageTransitionEffect, transform);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            // Reset auto-hide timer
            if (autoHide)
                autoHideTimer = autoHideDelay;
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

        private void AnimateIcon()
        {
            if (stageIcon == null) return;

            // Pulse animation
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            stageIcon.transform.localScale = iconOriginalScale * scale;
        }

        public void ToggleVisibility()
        {
            SetVisibility(!isVisible);
        }

        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            if (visible && autoHide)
            {
                autoHideTimer = autoHideDelay;
            }
        }

        private void OnDestroy()
        {
            if (lifeCycleSystem != null)
            {
                lifeCycleSystem.OnAgeChanged -= UpdateAgeDisplay;
                lifeCycleSystem.OnLifeStageChanged -= UpdateStageDisplay;
            }
        }
    }
}
