using UnityEngine;
using System.Collections.Generic;
using System;
using REcreationOfSpace.UI;

namespace REcreationOfSpace.Tutorial
{
    public class TutorialSystem : MonoBehaviour
    {
        [System.Serializable]
        public class TutorialStep
        {
            public string id;
            public string title;
            [TextArea(3, 10)]
            public string description;
            public GameObject highlightObject;
            public Vector3 cameraPosition;
            public Vector3 cameraRotation;
            public bool requiresAction;
            public string requiredAction;
            public float displayDuration = 5f;
            public AudioClip voiceover;
            public bool pauseGameDuringStep;
            public string[] tags;
        }

        [Header("Tutorial Settings")]
        [SerializeField] private bool enableTutorial = true;
        [SerializeField] private bool showTutorialOnStart = true;
        [SerializeField] private bool canSkipTutorial = true;
        [SerializeField] private float stepTransitionDelay = 1f;
        [SerializeField] private string saveKey = "CompletedTutorials";

        [Header("Tutorial Steps")]
        [SerializeField] private TutorialStep[] basicMovementSteps;
        [SerializeField] private TutorialStep[] combatSteps;
        [SerializeField] private TutorialStep[] inventorySteps;
        [SerializeField] private TutorialStep[] lifeCycleSteps;
        [SerializeField] private TutorialStep[] pathChoiceSteps;
        [SerializeField] private TutorialStep[] farmingSteps;
        [SerializeField] private TutorialStep[] craftingSteps;

        [Header("UI References")]
        [SerializeField] private GuiderMessageUI guiderMessage;
        [SerializeField] private GameObject highlightPrefab;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private float highlightPulseSpeed = 1f;
        [SerializeField] private Color highlightColor = Color.yellow;

        private Dictionary<string, bool> completedTutorials = new Dictionary<string, bool>();
        private Queue<TutorialStep> currentSteps = new Queue<TutorialStep>();
        private TutorialStep currentStep;
        private GameObject currentHighlight;
        private bool isTutorialActive;
        private float stepTimer;

        public event Action<string> OnTutorialStarted;
        public event Action<string> OnTutorialCompleted;
        public event Action<TutorialStep> OnStepStarted;
        public event Action<TutorialStep> OnStepCompleted;

        private void Start()
        {
            LoadTutorialProgress();
            if (showTutorialOnStart && enableTutorial)
            {
                StartBasicTutorial();
            }
        }

        private void Update()
        {
            if (!isTutorialActive || currentStep == null) return;

            if (currentStep.requiresAction)
            {
                // Check if required action is completed
                if (CheckActionCompletion(currentStep.requiredAction))
                {
                    CompleteCurrentStep();
                }
            }
            else
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0)
                {
                    CompleteCurrentStep();
                }
            }

            // Update highlight effect
            if (currentHighlight != null)
            {
                float pulse = (Mathf.Sin(Time.time * highlightPulseSpeed) + 1f) * 0.5f;
                currentHighlight.GetComponent<Renderer>().material.SetFloat("_Intensity", pulse);
            }
        }

        public void StartBasicTutorial()
        {
            StartTutorialSequence("Basic", basicMovementSteps);
        }

        public void StartCombatTutorial()
        {
            StartTutorialSequence("Combat", combatSteps);
        }

        public void StartInventoryTutorial()
        {
            StartTutorialSequence("Inventory", inventorySteps);
        }

        public void StartLifeCycleTutorial()
        {
            StartTutorialSequence("LifeCycle", lifeCycleSteps);
        }

        public void StartPathChoiceTutorial()
        {
            StartTutorialSequence("PathChoice", pathChoiceSteps);
        }

        public void StartFarmingTutorial()
        {
            StartTutorialSequence("Farming", farmingSteps);
        }

        public void StartCraftingTutorial()
        {
            StartTutorialSequence("Crafting", craftingSteps);
        }

        private void StartTutorialSequence(string tutorialId, TutorialStep[] steps)
        {
            if (!enableTutorial || (completedTutorials.ContainsKey(tutorialId) && !canSkipTutorial))
            {
                return;
            }

            currentSteps.Clear();
            foreach (var step in steps)
            {
                currentSteps.Enqueue(step);
            }

            isTutorialActive = true;
            OnTutorialStarted?.Invoke(tutorialId);
            ShowNextStep();
        }

        private void ShowNextStep()
        {
            if (currentSteps.Count == 0)
            {
                EndCurrentTutorial();
                return;
            }

            // Clean up previous step
            if (currentHighlight != null)
            {
                Destroy(currentHighlight);
            }

            currentStep = currentSteps.Dequeue();
            stepTimer = currentStep.displayDuration;

            // Show tutorial message
            if (guiderMessage != null)
            {
                guiderMessage.ShowMessage($"{currentStep.title}\n{currentStep.description}", Color.white);
            }

            // Create highlight effect
            if (currentStep.highlightObject != null)
            {
                currentHighlight = Instantiate(highlightPrefab, currentStep.highlightObject.transform);
                currentHighlight.GetComponent<Renderer>().material.SetColor("_Color", highlightColor);
            }

            // Play voiceover
            if (currentStep.voiceover != null)
            {
                AudioSource.PlayClipAtPoint(currentStep.voiceover, Camera.main.transform.position);
            }

            // Move camera if specified
            if (currentStep.cameraPosition != Vector3.zero)
            {
                Camera.main.transform.position = currentStep.cameraPosition;
                Camera.main.transform.eulerAngles = currentStep.cameraRotation;
            }

            // Pause game if required
            if (currentStep.pauseGameDuringStep)
            {
                Time.timeScale = 0f;
            }

            OnStepStarted?.Invoke(currentStep);
        }

        private void CompleteCurrentStep()
        {
            OnStepCompleted?.Invoke(currentStep);

            // Resume game if paused
            if (currentStep.pauseGameDuringStep)
            {
                Time.timeScale = 1f;
            }

            // Wait before showing next step
            StartCoroutine(WaitAndShowNextStep());
        }

        private System.Collections.IEnumerator WaitAndShowNextStep()
        {
            yield return new WaitForSeconds(stepTransitionDelay);
            ShowNextStep();
        }

        private void EndCurrentTutorial()
        {
            isTutorialActive = false;
            currentStep = null;

            if (currentHighlight != null)
            {
                Destroy(currentHighlight);
            }

            // Mark tutorial as completed
            string tutorialId = currentSteps.Count > 0 ? currentSteps.Peek().id : "";
            if (!string.IsNullOrEmpty(tutorialId))
            {
                completedTutorials[tutorialId] = true;
                SaveTutorialProgress();
                OnTutorialCompleted?.Invoke(tutorialId);
            }
        }

        private bool CheckActionCompletion(string action)
        {
            // Implement action checking based on the required action string
            // This could check for key presses, item collection, etc.
            switch (action)
            {
                case "move":
                    return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
                case "jump":
                    return Input.GetButtonDown("Jump");
                case "attack":
                    return Input.GetButtonDown("Fire1");
                case "inventory":
                    return Input.GetKeyDown(KeyCode.I);
                // Add more action checks as needed
                default:
                    return false;
            }
        }

        private void SaveTutorialProgress()
        {
            string progress = string.Join(",", completedTutorials.Keys);
            PlayerPrefs.SetString(saveKey, progress);
            PlayerPrefs.Save();
        }

        private void LoadTutorialProgress()
        {
            string progress = PlayerPrefs.GetString(saveKey, "");
            if (!string.IsNullOrEmpty(progress))
            {
                string[] completed = progress.Split(',');
                foreach (string tutorial in completed)
                {
                    completedTutorials[tutorial] = true;
                }
            }
        }

        public bool IsTutorialCompleted(string tutorialId)
        {
            return completedTutorials.ContainsKey(tutorialId);
        }

        public void ResetTutorialProgress()
        {
            completedTutorials.Clear();
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();
        }

        public void SkipCurrentTutorial()
        {
            if (canSkipTutorial)
            {
                EndCurrentTutorial();
            }
        }

        public bool IsTutorialActive()
        {
            return isTutorialActive;
        }
    }
}
