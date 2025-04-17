using UnityEngine;
using UnityEngine.UI;
using TMPro;
using REcreationOfSpace.World;

namespace REcreationOfSpace.UI
{
    public class TimeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI periodText;
        [SerializeField] private Image dayNightIcon;
        [SerializeField] private Image timeProgressBar;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Visual Settings")]
        [SerializeField] private Sprite sunIcon;
        [SerializeField] private Sprite moonIcon;
        [SerializeField] private Color dayColor = new Color(1f, 0.92f, 0.016f);
        [SerializeField] private Color nightColor = new Color(0.1f, 0.1f, 0.3f);
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private bool autoHide = true;
        [SerializeField] private float autoHideDelay = 5f;
        [SerializeField] private KeyCode toggleKey = KeyCode.T;

        [Header("Animation")]
        [SerializeField] private bool useAnimation = true;
        [SerializeField] private float iconRotationSpeed = 10f;
        [SerializeField] private float iconBobAmplitude = 5f;
        [SerializeField] private float iconBobSpeed = 2f;

        private DayNightSystem dayNightSystem;
        private float autoHideTimer;
        private bool isVisible = true;
        private Vector3 iconOriginalPosition;

        private void Start()
        {
            dayNightSystem = FindObjectOfType<DayNightSystem>();
            if (dayNightSystem == null)
            {
                Debug.LogError("No DayNightSystem found!");
                return;
            }

            if (dayNightIcon != null)
                iconOriginalPosition = dayNightIcon.transform.localPosition;

            // Subscribe to events
            dayNightSystem.OnTimeChanged += UpdateTimeDisplay;
            dayNightSystem.OnDayNightChanged += UpdateDayNightDisplay;

            // Initialize display
            UpdateTimeDisplay(dayNightSystem.GetCurrentHour() / 24f);
            UpdateDayNightDisplay(dayNightSystem.IsNight());

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
            if (useAnimation && dayNightIcon != null)
            {
                AnimateIcon();
            }

            // Update canvas group alpha
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, isVisible ? 1f : 0f, Time.deltaTime * fadeSpeed);
            }
        }

        private void UpdateTimeDisplay(float timeRatio)
        {
            if (timeText != null)
            {
                timeText.text = dayNightSystem.GetTimeString();
            }

            if (timeProgressBar != null)
            {
                timeProgressBar.fillAmount = timeRatio;
            }

            if (periodText != null)
            {
                int hour = Mathf.FloorToInt(dayNightSystem.GetCurrentHour());
                string period;

                if (hour >= 5 && hour < 12)
                    period = "Morning";
                else if (hour >= 12 && hour < 17)
                    period = "Afternoon";
                else if (hour >= 17 && hour < 22)
                    period = "Evening";
                else
                    period = "Night";

                periodText.text = period;
            }

            // Reset auto-hide timer on time change
            if (autoHide)
                autoHideTimer = autoHideDelay;
        }

        private void UpdateDayNightDisplay(bool isNight)
        {
            if (dayNightIcon != null)
            {
                dayNightIcon.sprite = isNight ? moonIcon : sunIcon;
                dayNightIcon.color = isNight ? nightColor : dayColor;
            }

            if (timeProgressBar != null)
            {
                timeProgressBar.color = isNight ? nightColor : dayColor;
            }
        }

        private void AnimateIcon()
        {
            if (dayNightIcon == null) return;

            // Rotate the icon
            dayNightIcon.transform.Rotate(Vector3.forward * iconRotationSpeed * Time.deltaTime);

            // Bob the icon up and down
            Vector3 newPos = iconOriginalPosition;
            newPos.y += Mathf.Sin(Time.time * iconBobSpeed) * iconBobAmplitude;
            dayNightIcon.transform.localPosition = newPos;
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
            if (dayNightSystem != null)
            {
                dayNightSystem.OnTimeChanged -= UpdateTimeDisplay;
                dayNightSystem.OnDayNightChanged -= UpdateDayNightDisplay;
            }
        }
    }
}
