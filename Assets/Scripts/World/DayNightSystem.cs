using UnityEngine;
using System;

namespace REcreationOfSpace.World
{
    public class DayNightSystem : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private float dayLengthInMinutes = 24f;
        [SerializeField] private float startingHour = 6f;
        [SerializeField] private bool pauseTimeAtNight = false;
        [SerializeField] private Vector2 nightTimeRange = new Vector2(22f, 5f);

        [Header("Sun Settings")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Gradient sunColor;
        [SerializeField] private AnimationCurve sunIntensity;
        [SerializeField] private float maxSunIntensity = 1f;

        [Header("Moon Settings")]
        [SerializeField] private Light moonLight;
        [SerializeField] private Gradient moonColor;
        [SerializeField] private AnimationCurve moonIntensity;
        [SerializeField] private float maxMoonIntensity = 0.3f;

        [Header("Atmosphere")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Gradient skyColor;
        [SerializeField] private Gradient horizonColor;
        [SerializeField] private Gradient groundColor;
        [SerializeField] private AnimationCurve starVisibility;
        [SerializeField] private float maxStarIntensity = 1f;

        [Header("Environment")]
        [SerializeField] private AnimationCurve ambientIntensity;
        [SerializeField] private float maxAmbientIntensity = 1f;
        [SerializeField] private float fogDensityDay = 0.01f;
        [SerializeField] private float fogDensityNight = 0.02f;

        private float currentTime; // In hours
        private float timeRatio; // 0-1 representing current time of day
        private bool isNight;

        public event Action<float> OnTimeChanged;
        public event Action<bool> OnDayNightChanged;
        public event Action<float> OnHourChanged;

        private void Start()
        {
            if (directionalLight == null)
                directionalLight = FindObjectOfType<Light>();

            InitializeTime();
            UpdateCelestialBodies();
            UpdateAtmosphere();
            UpdateEnvironment();
        }

        private void InitializeTime()
        {
            currentTime = startingHour;
            timeRatio = currentTime / 24f;
            isNight = IsNightTime(currentTime);
            
            if (moonLight != null)
                moonLight.gameObject.SetActive(isNight);
        }

        private void Update()
        {
            if (pauseTimeAtNight && isNight)
                return;

            UpdateTime();
            UpdateCelestialBodies();
            UpdateAtmosphere();
            UpdateEnvironment();
        }

        private void UpdateTime()
        {
            float prevHour = Mathf.Floor(currentTime);
            
            // Update time
            float timeIncrement = (24f / (dayLengthInMinutes * 60f)) * Time.deltaTime;
            currentTime += timeIncrement;
            if (currentTime >= 24f)
                currentTime -= 24f;

            timeRatio = currentTime / 24f;
            OnTimeChanged?.Invoke(timeRatio);

            // Check for hour change
            float newHour = Mathf.Floor(currentTime);
            if (newHour != prevHour)
            {
                OnHourChanged?.Invoke(newHour);
            }

            // Check for day/night change
            bool newIsNight = IsNightTime(currentTime);
            if (newIsNight != isNight)
            {
                isNight = newIsNight;
                OnDayNightChanged?.Invoke(isNight);

                if (moonLight != null)
                    moonLight.gameObject.SetActive(isNight);
            }
        }

        private void UpdateCelestialBodies()
        {
            // Update sun
            if (directionalLight != null)
            {
                float sunRotation = timeRatio * 360f;
                directionalLight.transform.rotation = Quaternion.Euler(sunRotation, -30f, 0f);
                
                float sunElevation = Mathf.Sin(sunRotation * Mathf.Deg2Rad);
                float sunIntensityMultiplier = sunIntensity.Evaluate(timeRatio);
                
                directionalLight.intensity = maxSunIntensity * sunIntensityMultiplier;
                directionalLight.color = sunColor.Evaluate(timeRatio);
            }

            // Update moon
            if (moonLight != null && isNight)
            {
                float moonRotation = (timeRatio * 360f) + 180f;
                moonLight.transform.rotation = Quaternion.Euler(moonRotation, -30f, 0f);
                
                float moonIntensityMultiplier = moonIntensity.Evaluate(timeRatio);
                moonLight.intensity = maxMoonIntensity * moonIntensityMultiplier;
                moonLight.color = moonColor.Evaluate(timeRatio);
            }
        }

        private void UpdateAtmosphere()
        {
            if (skyboxMaterial != null)
            {
                // Update sky colors
                skyboxMaterial.SetColor("_SkyColor", skyColor.Evaluate(timeRatio));
                skyboxMaterial.SetColor("_HorizonColor", horizonColor.Evaluate(timeRatio));
                skyboxMaterial.SetColor("_GroundColor", groundColor.Evaluate(timeRatio));

                // Update stars
                float starIntensityMultiplier = starVisibility.Evaluate(timeRatio);
                skyboxMaterial.SetFloat("_StarIntensity", maxStarIntensity * starIntensityMultiplier);
            }
        }

        private void UpdateEnvironment()
        {
            // Update ambient lighting
            float ambientMultiplier = ambientIntensity.Evaluate(timeRatio);
            RenderSettings.ambientIntensity = maxAmbientIntensity * ambientMultiplier;

            // Update fog
            float fogBlend = Mathf.Lerp(fogDensityDay, fogDensityNight, isNight ? 1f : 0f);
            RenderSettings.fogDensity = fogBlend;
        }

        private bool IsNightTime(float time)
        {
            if (nightTimeRange.x > nightTimeRange.y) // Night spans midnight
            {
                return time >= nightTimeRange.x || time <= nightTimeRange.y;
            }
            else // Normal night range
            {
                return time >= nightTimeRange.x && time <= nightTimeRange.y;
            }
        }

        public float GetCurrentHour()
        {
            return currentTime;
        }

        public string GetTimeString()
        {
            int hours = Mathf.FloorToInt(currentTime);
            int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
            return $"{hours:00}:{minutes:00}";
        }

        public bool IsNight()
        {
            return isNight;
        }

        public void SetTime(float hour)
        {
            currentTime = Mathf.Clamp(hour, 0f, 24f);
            timeRatio = currentTime / 24f;
            isNight = IsNightTime(currentTime);

            UpdateCelestialBodies();
            UpdateAtmosphere();
            UpdateEnvironment();

            OnTimeChanged?.Invoke(timeRatio);
            OnDayNightChanged?.Invoke(isNight);
            OnHourChanged?.Invoke(Mathf.Floor(currentTime));
        }

        public void SetDayLength(float minutes)
        {
            dayLengthInMinutes = Mathf.Max(1f, minutes);
        }

        public void ToggleTimeFreeze()
        {
            pauseTimeAtNight = !pauseTimeAtNight;
        }
    }
}
