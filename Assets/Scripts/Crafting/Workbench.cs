using UnityEngine;
using UnityEngine.Events;

namespace REcreationOfSpace.Crafting
{
    public class Workbench : MonoBehaviour, IInteractable
    {
        [Header("Workbench Settings")]
        [SerializeField] private string workbenchType;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private GameObject workbenchModel;
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private bool requiresConstantPresence = true;

        [Header("Effects")]
        [SerializeField] private ParticleSystem craftingEffect;
        [SerializeField] private AudioClip craftingSound;
        [SerializeField] private Light workbenchLight;

        public UnityEvent onWorkbenchEnter;
        public UnityEvent onWorkbenchExit;

        private CraftingSystem craftingSystem;
        private CraftingUI craftingUI;
        private AudioSource audioSource;
        private bool isPlayerNearby = false;
        private bool isInUse = false;

        private void Start()
        {
            craftingSystem = FindObjectOfType<CraftingSystem>();
            craftingUI = FindObjectOfType<CraftingUI>();
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && craftingSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            // Subscribe to crafting events
            if (craftingSystem != null)
            {
                craftingSystem.onItemCrafted.AddListener(OnItemCrafted);
            }
        }

        private void Update()
        {
            if (!isPlayerNearby || craftingUI == null)
                return;

            // Check for interaction input
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!isInUse)
                {
                    StartUsing();
                }
                else
                {
                    StopUsing();
                }
            }

            // Check if player moved too far
            if (requiresConstantPresence && isInUse)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (distance > interactionRange)
                    {
                        StopUsing();
                    }
                }
            }
        }

        public void OnInteractionRangeEntered(GameObject interactor)
        {
            if (!interactor.CompareTag("Player"))
                return;

            isPlayerNearby = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }

        public void OnInteractionRangeExited(GameObject interactor)
        {
            if (!interactor.CompareTag("Player"))
                return;

            isPlayerNearby = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (requiresConstantPresence)
            {
                StopUsing();
            }
        }

        private void StartUsing()
        {
            isInUse = true;

            // Show crafting UI
            craftingUI.Show(workbenchType);

            // Play effects
            if (craftingEffect != null)
            {
                craftingEffect.Play();
            }

            if (workbenchLight != null)
            {
                workbenchLight.enabled = true;
            }

            onWorkbenchEnter?.Invoke();
        }

        private void StopUsing()
        {
            isInUse = false;

            // Hide crafting UI
            craftingUI.Hide();

            // Stop effects
            if (craftingEffect != null)
            {
                craftingEffect.Stop();
            }

            if (workbenchLight != null)
            {
                workbenchLight.enabled = false;
            }

            onWorkbenchExit?.Invoke();
        }

        private void OnItemCrafted(string item, int amount)
        {
            // Play crafting sound
            if (audioSource != null && craftingSound != null)
            {
                audioSource.PlayOneShot(craftingSound);
            }

            // Could add more feedback here like particle effects or animations
        }

        private void OnDestroy()
        {
            if (craftingSystem != null)
            {
                craftingSystem.onItemCrafted.RemoveListener(OnItemCrafted);
            }
        }

        public string GetWorkbenchType()
        {
            return workbenchType;
        }

        public bool IsInUse()
        {
            return isInUse;
        }
    }

    // Interface for interactable objects
    public interface IInteractable
    {
        void OnInteractionRangeEntered(GameObject interactor);
        void OnInteractionRangeExited(GameObject interactor);
    }
}
