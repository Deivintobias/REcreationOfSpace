using UnityEngine;
using UnityEngine.Events;

namespace REcreationOfSpace.World
{
    public class WorldStructure : MonoBehaviour
    {
        [System.Serializable]
        public class ResourceRequirement
        {
            public string resourceType;
            public int amount;
        }

        [Header("Structure Settings")]
        [SerializeField] private string structureType;
        [SerializeField] private bool isBuilt = false;
        [SerializeField] private ResourceRequirement[] buildRequirements;
        [SerializeField] private float buildTime = 10f;

        [Header("Production")]
        [SerializeField] private string producedResourceType;
        [SerializeField] private float productionInterval = 60f;
        [SerializeField] private int productionAmount = 1;

        [Header("Effects")]
        [SerializeField] private GameObject constructionEffect;
        [SerializeField] private GameObject productionEffect;
        [SerializeField] private AudioClip constructionSound;
        [SerializeField] private AudioClip productionSound;

        public UnityEvent onConstructionComplete;
        public UnityEvent<string, int> onResourceProduced;

        private float constructionProgress = 0f;
        private float productionTimer = 0f;
        private ResourceSystem resourceSystem;
        private AudioSource audioSource;

        private void Start()
        {
            resourceSystem = FindObjectOfType<ResourceSystem>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Start production timer if already built
            if (isBuilt)
            {
                productionTimer = productionInterval;
            }
        }

        private void Update()
        {
            if (!isBuilt)
            {
                UpdateConstruction();
            }
            else if (!string.IsNullOrEmpty(producedResourceType))
            {
                UpdateProduction();
            }
        }

        private void UpdateConstruction()
        {
            constructionProgress += Time.deltaTime;
            
            if (constructionProgress >= buildTime)
            {
                CompleteConstruction();
            }
        }

        private void UpdateProduction()
        {
            productionTimer += Time.deltaTime;
            
            if (productionTimer >= productionInterval)
            {
                ProduceResource();
                productionTimer = 0f;
            }
        }

        public bool CanStartConstruction()
        {
            if (isBuilt || resourceSystem == null)
                return false;

            // Check resource requirements
            foreach (var requirement in buildRequirements)
            {
                if (!resourceSystem.HasResource(requirement.resourceType, requirement.amount))
                {
                    return false;
                }
            }

            return true;
        }

        public void StartConstruction()
        {
            if (!CanStartConstruction())
                return;

            // Consume resources
            foreach (var requirement in buildRequirements)
            {
                resourceSystem.UseResource(requirement.resourceType, requirement.amount);
            }

            // Start construction
            constructionProgress = 0f;

            // Play effects
            if (constructionEffect != null)
            {
                Instantiate(constructionEffect, transform.position, Quaternion.identity);
            }

            if (audioSource != null && constructionSound != null)
            {
                audioSource.PlayOneShot(constructionSound);
            }
        }

        private void CompleteConstruction()
        {
            isBuilt = true;
            constructionProgress = buildTime;

            // Notify listeners
            onConstructionComplete?.Invoke();
        }

        private void ProduceResource()
        {
            if (!isBuilt || resourceSystem == null)
                return;

            // Add resource
            resourceSystem.AddResource(producedResourceType, productionAmount);

            // Play effects
            if (productionEffect != null)
            {
                Instantiate(productionEffect, transform.position, Quaternion.identity);
            }

            if (audioSource != null && productionSound != null)
            {
                audioSource.PlayOneShot(productionSound);
            }

            // Notify listeners
            onResourceProduced?.Invoke(producedResourceType, productionAmount);
        }

        public float GetConstructionProgress()
        {
            return isBuilt ? 1f : constructionProgress / buildTime;
        }

        public float GetProductionProgress()
        {
            return isBuilt ? productionTimer / productionInterval : 0f;
        }

        public bool IsBuilt()
        {
            return isBuilt;
        }

        public string GetStructureType()
        {
            return structureType;
        }

        public ResourceRequirement[] GetBuildRequirements()
        {
            return buildRequirements;
        }
    }
}
