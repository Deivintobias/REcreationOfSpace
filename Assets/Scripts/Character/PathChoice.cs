using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace REcreationOfSpace.Character
{
    public class PathChoice : MonoBehaviour
    {
        [Header("Path Settings")]
        [SerializeField] private float choiceInterval = 30f; // Time between choice opportunities
        [SerializeField] private float choiceWindowDuration = 10f; // How long the choice UI stays visible
        [SerializeField] private bool allowPathChange = true; // Can switch between paths multiple times

        [Header("UI References")]
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private Button sinaiButton;
        [SerializeField] private Button sionButton;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image timerFill;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem sinaiEffect;
        [SerializeField] private ParticleSystem sionEffect;
        [SerializeField] private Material sinaiMaterial;
        [SerializeField] private Material sionMaterial;

        private LifeCycleSystem lifeCycleSystem;
        private float nextChoiceTime;
        private float currentChoiceTimer;
        private bool isChoosingPath;
        private bool hasMadeChoice;

        public event Action<bool> OnPathChosen; // true for Sinai, false for Sion

        private void Start()
        {
            lifeCycleSystem = GetComponent<LifeCycleSystem>();
            if (lifeCycleSystem == null)
            {
                Debug.LogError("No LifeCycleSystem found!");
                return;
            }

            // Setup UI
            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }

            if (sinaiButton != null)
            {
                sinaiButton.onClick.AddListener(() => ChoosePath(true));
            }

            if (sionButton != null)
            {
                sionButton.onClick.AddListener(() => ChoosePath(false));
            }

            // Set initial choice time
            nextChoiceTime = Time.time + choiceInterval;
        }

        private void Update()
        {
            if (lifeCycleSystem.GetAge() < 18) // Must be adult to make choice
                return;

            if (!isChoosingPath && (!hasMadeChoice || allowPathChange) && Time.time >= nextChoiceTime)
            {
                StartChoiceWindow();
            }

            if (isChoosingPath)
            {
                UpdateChoiceWindow();
            }
        }

        private void StartChoiceWindow()
        {
            isChoosingPath = true;
            currentChoiceTimer = choiceWindowDuration;

            if (choicePanel != null)
            {
                choicePanel.SetActive(true);
            }

            if (descriptionText != null)
            {
                descriptionText.text = hasMadeChoice ? 
                    "Would you like to change your path?" :
                    "Choose your path: Sinai (ascension) or Sion (earthly power)";
            }

            // Show path preview effects
            ShowPathPreview();
        }

        private void UpdateChoiceWindow()
        {
            currentChoiceTimer -= Time.deltaTime;

            if (timerFill != null)
            {
                timerFill.fillAmount = currentChoiceTimer / choiceWindowDuration;
            }

            if (currentChoiceTimer <= 0)
            {
                CloseChoiceWindow();
            }
        }

        private void CloseChoiceWindow()
        {
            isChoosingPath = false;
            nextChoiceTime = Time.time + choiceInterval;

            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }

            // Hide preview effects
            HidePathPreview();
        }

        private void ChoosePath(bool chooseSinai)
        {
            hasMadeChoice = true;

            // Handle components
            var sinaiCharacter = GetComponent<SinaiCharacter>();
            var sionObserver = GetComponent<SionObserver>();

            if (chooseSinai)
            {
                // Choose Sinai path
                if (sinaiCharacter == null)
                {
                    sinaiCharacter = gameObject.AddComponent<SinaiCharacter>();
                }
                if (sionObserver != null)
                {
                    Destroy(sionObserver);
                }

                // Apply Sinai visual effects
                if (sinaiEffect != null)
                {
                    sinaiEffect.Play();
                }
                
                // Change character material
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null && sinaiMaterial != null)
                {
                    renderer.material = sinaiMaterial;
                }
            }
            else
            {
                // Choose Sion path
                if (sionObserver == null)
                {
                    sionObserver = gameObject.AddComponent<SionObserver>();
                }
                if (sinaiCharacter != null)
                {
                    Destroy(sinaiCharacter);
                }

                // Apply Sion visual effects
                if (sionEffect != null)
                {
                    sionEffect.Play();
                }

                // Change character material
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null && sionMaterial != null)
                {
                    renderer.material = sionMaterial;
                }
            }

            // Notify listeners
            OnPathChosen?.Invoke(chooseSinai);

            // Show message
            var guiderMessage = FindObjectOfType<GuiderMessageUI>();
            if (guiderMessage != null)
            {
                string message = chooseSinai ? 
                    "You have chosen the path of Sinai. Seek divine ascension." :
                    "You have chosen the path of Sion. Embrace earthly power.";
                guiderMessage.ShowMessage(message, chooseSinai ? Color.gold : Color.blue);
            }

            CloseChoiceWindow();
        }

        private void ShowPathPreview()
        {
            // Show preview effects when hovering over buttons
            if (sinaiButton != null)
            {
                var sinaiPreview = sinaiButton.gameObject.AddComponent<PathPreviewEffect>();
                sinaiPreview.Initialize(sinaiEffect, true);
            }

            if (sionButton != null)
            {
                var sionPreview = sionButton.gameObject.AddComponent<PathPreviewEffect>();
                sionPreview.Initialize(sionEffect, false);
            }
        }

        private void HidePathPreview()
        {
            if (sinaiButton != null)
            {
                var preview = sinaiButton.GetComponent<PathPreviewEffect>();
                if (preview != null)
                    Destroy(preview);
            }

            if (sionButton != null)
            {
                var preview = sionButton.GetComponent<PathPreviewEffect>();
                if (preview != null)
                    Destroy(preview);
            }
        }

        public bool HasChosenSinai()
        {
            return GetComponent<SinaiCharacter>() != null;
        }

        public bool HasChosenPath()
        {
            return hasMadeChoice;
        }
    }

    // Helper class for path preview effects
    public class PathPreviewEffect : MonoBehaviour
    {
        private ParticleSystem effectPrefab;
        private ParticleSystem activeEffect;
        private bool isSinai;

        public void Initialize(ParticleSystem effect, bool sinai)
        {
            effectPrefab = effect;
            isSinai = sinai;

            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onPointerEnter.AddListener(ShowPreview);
                button.onPointerExit.AddListener(HidePreview);
            }
        }

        private void ShowPreview()
        {
            if (effectPrefab != null && activeEffect == null)
            {
                activeEffect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                var main = activeEffect.main;
                main.startColor = isSinai ? Color.gold : Color.blue;
            }
        }

        private void HidePreview()
        {
            if (activeEffect != null)
            {
                Destroy(activeEffect.gameObject);
                activeEffect = null;
            }
        }

        private void OnDestroy()
        {
            if (activeEffect != null)
            {
                Destroy(activeEffect.gameObject);
            }
        }
    }
}
