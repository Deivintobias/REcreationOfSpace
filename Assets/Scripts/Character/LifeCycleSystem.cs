using UnityEngine;
using System;
using System.Collections.Generic;
using REcreationOfSpace.World;

namespace REcreationOfSpace.Character
{
    public class LifeCycleSystem : MonoBehaviour
    {
        [System.Serializable]
        public class LifeStage
        {
            public string name;
            public float durationInYears;
            public Vector3 scaleMultiplier = Vector3.one;
            public float speedMultiplier = 1f;
            public float strengthMultiplier = 1f;
            public Color auraColor = Color.white;
            public ParticleSystem transitionEffect;
            public AudioClip transitionSound;
        }

        [Header("Life Cycle Settings")]
        [SerializeField] private float maxLifespanYears = 120f;
        [SerializeField] private float yearProgressionSpeed = 1f; // Years per real minute
        [SerializeField] private bool useGameDayAsYear = true;
        [SerializeField] private bool pauseAgingAtNight = true;

        [Header("Life Stages")]
        [SerializeField] private LifeStage[] lifeStages = new LifeStage[]
        {
            new LifeStage { name = "Infant", durationInYears = 2f, scaleMultiplier = new Vector3(0.3f, 0.3f, 0.3f), speedMultiplier = 0.5f, strengthMultiplier = 0.2f },
            new LifeStage { name = "Child", durationInYears = 10f, scaleMultiplier = new Vector3(0.6f, 0.6f, 0.6f), speedMultiplier = 1.2f, strengthMultiplier = 0.5f },
            new LifeStage { name = "Teen", durationInYears = 8f, scaleMultiplier = new Vector3(0.8f, 0.8f, 0.8f), speedMultiplier = 1.1f, strengthMultiplier = 0.8f },
            new LifeStage { name = "Young Adult", durationInYears = 20f, scaleMultiplier = Vector3.one, speedMultiplier = 1f, strengthMultiplier = 1f },
            new LifeStage { name = "Adult", durationInYears = 40f, scaleMultiplier = Vector3.one, speedMultiplier = 0.9f, strengthMultiplier = 0.9f },
            new LifeStage { name = "Elder", durationInYears = 40f, scaleMultiplier = new Vector3(0.95f, 0.95f, 0.95f), speedMultiplier = 0.7f, strengthMultiplier = 0.6f }
        };

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem agingParticles;
        [SerializeField] private Material characterMaterial;
        [SerializeField] private float auraIntensity = 0.5f;

        private float currentAge;
        private int currentStageIndex;
        private LifeStage currentStage;
        private DayNightSystem dayNightSystem;
        private PlayerController playerController;
        private Dictionary<string, float> baseStats = new Dictionary<string, float>();

        public event Action<float> OnAgeChanged;
        public event Action<LifeStage> OnLifeStageChanged;
        public event Action OnDeath;

        private void Start()
        {
            dayNightSystem = FindObjectOfType<DayNightSystem>();
            playerController = GetComponent<PlayerController>();

            if (playerController != null)
            {
                // Store base stats
                baseStats["speed"] = playerController.GetMoveSpeed();
                baseStats["strength"] = playerController.GetStrength();
            }

            // Start at birth
            SetAge(0);

            if (useGameDayAsYear && dayNightSystem != null)
            {
                dayNightSystem.OnDayNightChanged += OnDayNightChanged;
            }
        }

        private void Update()
        {
            if (currentAge >= maxLifespanYears)
                return;

            if (useGameDayAsYear)
            {
                // Age is handled by day/night cycle
                return;
            }

            if (pauseAgingAtNight && dayNightSystem != null && dayNightSystem.IsNight())
                return;

            // Progress age
            float ageIncrement = (yearProgressionSpeed / 60f) * Time.deltaTime;
            SetAge(currentAge + ageIncrement);
        }

        private void SetAge(float age)
        {
            currentAge = Mathf.Clamp(age, 0, maxLifespanYears);
            
            // Find current life stage
            float ageSum = 0;
            for (int i = 0; i < lifeStages.Length; i++)
            {
                ageSum += lifeStages[i].durationInYears;
                if (currentAge < ageSum)
                {
                    if (currentStageIndex != i)
                    {
                        ChangeLifeStage(i);
                    }
                    break;
                }
            }

            // Check for death
            if (currentAge >= maxLifespanYears)
            {
                Die();
            }

            OnAgeChanged?.Invoke(currentAge);
        }

        private void ChangeLifeStage(int newStageIndex)
        {
            currentStageIndex = newStageIndex;
            currentStage = lifeStages[currentStageIndex];

            // Apply stage effects
            if (playerController != null)
            {
                // Scale character
                transform.localScale = currentStage.scaleMultiplier;

                // Modify stats
                playerController.SetMoveSpeed(baseStats["speed"] * currentStage.speedMultiplier);
                playerController.SetStrength(baseStats["strength"] * currentStage.strengthMultiplier);
            }

            // Update visual effects
            if (characterMaterial != null)
            {
                characterMaterial.SetColor("_AuraColor", currentStage.auraColor);
                characterMaterial.SetFloat("_AuraIntensity", auraIntensity);
            }

            // Play transition effects
            if (currentStage.transitionEffect != null)
            {
                var effect = Instantiate(currentStage.transitionEffect, transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            if (currentStage.transitionSound != null)
            {
                AudioSource.PlayClipAtPoint(currentStage.transitionSound, transform.position);
            }

            OnLifeStageChanged?.Invoke(currentStage);
        }

        private void OnDayNightChanged(bool isNight)
        {
            if (!isNight && useGameDayAsYear)
            {
                // Age one year per day
                SetAge(currentAge + 1);
            }
        }

        private void Die()
        {
            // Trigger death event
            OnDeath?.Invoke();

            // Check if character is from Mount Sinai
            var sinaiCharacter = GetComponent<SinaiCharacter>();
            if (sinaiCharacter != null)
            {
                StartCoroutine(AscendToParadise());
            }
            else
            {
                // Regular death handling
                if (playerController != null)
                {
                    playerController.enabled = false;
                }

                // Play death effects
                if (agingParticles != null)
                {
                    agingParticles.Play();
                }
            }
        }

        private IEnumerator AscendToParadise()
        {
            // Play ascension effects
            if (agingParticles != null)
            {
                var main = agingParticles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(Color.white, Color.gold);
                agingParticles.Play();
            }

            // Fade out character
            var characterRenderer = GetComponentInChildren<Renderer>();
            if (characterRenderer != null)
            {
                float fadeTime = 2f;
                float elapsedTime = 0f;
                Material material = characterRenderer.material;
                Color startColor = material.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

                while (elapsedTime < fadeTime)
                {
                    elapsedTime += Time.deltaTime;
                    material.color = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
                    transform.position += Vector3.up * Time.deltaTime;
                    yield return null;
                }
            }

            // Find Paradise City
            var paradiseCity = FindObjectOfType<ParadiseCity>();
            if (paradiseCity != null)
            {
                // Transfer character data
                var characterData = new Dictionary<string, object>
                {
                    { "name", gameObject.name },
                    { "age", currentAge },
                    { "lifeStage", currentStage.name },
                    // Add any other relevant data
                };

                // Add character to Paradise City
                paradiseCity.AddAscendedCharacter(characterData);

                // Show ascension message
                var guiderMessage = FindObjectOfType<GuiderMessageUI>();
                if (guiderMessage != null)
                {
                    guiderMessage.ShowMessage($"{gameObject.name} has ascended to Paradise City", Color.gold);
                }
            }

            // Destroy original character
            Destroy(gameObject);
        }

        public float GetAge()
        {
            return currentAge;
        }

        public LifeStage GetCurrentStage()
        {
            return currentStage;
        }

        public float GetLifeProgress()
        {
            return currentAge / maxLifespanYears;
        }

        public string GetLifeStageText()
        {
            return currentStage?.name ?? "Unknown";
        }

        private void OnDestroy()
        {
            if (dayNightSystem != null)
            {
                dayNightSystem.OnDayNightChanged -= OnDayNightChanged;
            }
        }
    }
}
