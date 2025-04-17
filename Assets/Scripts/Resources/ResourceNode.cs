using UnityEngine;
using UnityEngine.Events;

namespace REcreationOfSpace.Resources
{
    public class ResourceNode : MonoBehaviour
    {
        [Header("Resource Settings")]
        [SerializeField] private string resourceType = "Wood";
        [SerializeField] private int resourceAmount = 1;
        [SerializeField] private float respawnTime = 30f;
        [SerializeField] private bool isInfinite = false;
        [SerializeField] private int maxHarvests = 3;

        [Header("Effects")]
        [SerializeField] private ParticleSystem harvestEffect;
        [SerializeField] private AudioClip harvestSound;
        [SerializeField] private GameObject depleteEffect;
        [SerializeField] private GameObject visualObject;

        public UnityEvent<string, int> onResourceCollected; // Resource type, amount
        public UnityEvent onNodeDepleted;

        private int harvestCount = 0;
        private bool isDepleted = false;
        private float respawnTimer = 0f;
        private AudioSource audioSource;
        private ResourceSystem resourceSystem;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && harvestSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            resourceSystem = FindObjectOfType<ResourceSystem>();
        }

        private void Update()
        {
            if (isDepleted && !isInfinite)
            {
                respawnTimer += Time.deltaTime;
                if (respawnTimer >= respawnTime)
                {
                    Respawn();
                }
            }
        }

        public void Interact()
        {
            if (isDepleted)
                return;

            // Collect resource
            if (resourceSystem != null)
            {
                resourceSystem.AddResource(resourceType, resourceAmount);
            }

            // Play effects
            if (harvestEffect != null)
            {
                harvestEffect.Play();
            }

            if (audioSource != null && harvestSound != null)
            {
                audioSource.PlayOneShot(harvestSound);
            }

            // Notify listeners
            onResourceCollected?.Invoke(resourceType, resourceAmount);

            // Handle depletion
            harvestCount++;
            if (!isInfinite && harvestCount >= maxHarvests)
            {
                Deplete();
            }
        }

        private void Deplete()
        {
            isDepleted = true;
            respawnTimer = 0f;

            // Hide visual
            if (visualObject != null)
            {
                visualObject.SetActive(false);
            }

            // Show depletion effect
            if (depleteEffect != null)
            {
                Instantiate(depleteEffect, transform.position, Quaternion.identity);
            }

            // Notify listeners
            onNodeDepleted?.Invoke();
        }

        private void Respawn()
        {
            isDepleted = false;
            harvestCount = 0;
            respawnTimer = 0f;

            // Show visual
            if (visualObject != null)
            {
                visualObject.SetActive(true);
            }
        }

        public bool IsDepleted()
        {
            return isDepleted;
        }

        public string GetResourceType()
        {
            return resourceType;
        }

        public float GetRespawnProgress()
        {
            return isDepleted ? respawnTimer / respawnTime : 1f;
        }
    }
}
