using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace REcreationOfSpace.Farming
{
    public class FarmPlot : MonoBehaviour
    {
        [System.Serializable]
        public class CropData
        {
            public string cropType;
            public float growthTime;
            public int growthStages;
            public GameObject[] stagePrefabs;
            public string[] requiredResources;
            public int[] resourceAmounts;
            public string harvestedResource;
            public int harvestAmount;
        }

        [Header("Plot Settings")]
        [SerializeField] private bool isPlowed = false;
        [SerializeField] private bool isWatered = false;
        [SerializeField] private float waterDepletionTime = 300f; // 5 minutes
        [SerializeField] private Material plowedMaterial;
        [SerializeField] private Material unplowedMaterial;

        [Header("Crop Settings")]
        [SerializeField] private CropData[] availableCrops;
        [SerializeField] private float fertilizeBonus = 0.5f; // Growth speed bonus

        public UnityEvent<string, int> onCropHarvested;
        public UnityEvent<float> onGrowthProgress;

        private CropData currentCrop;
        private float growthProgress = 0f;
        private int currentStage = 0;
        private bool isFertilized = false;
        private GameObject currentCropObject;
        private ResourceSystem resourceSystem;
        private float waterTimer;

        private void Start()
        {
            resourceSystem = FindObjectOfType<ResourceSystem>();
            UpdateVisuals();
        }

        private void Update()
        {
            if (isWatered)
            {
                waterTimer += Time.deltaTime;
                if (waterTimer >= waterDepletionTime)
                {
                    isWatered = false;
                    UpdateVisuals();
                }
            }

            if (currentCrop != null && isPlowed && isWatered)
            {
                UpdateGrowth();
            }
        }

        public bool CanPlow()
        {
            return !isPlowed && currentCrop == null;
        }

        public void Plow()
        {
            if (!CanPlow()) return;
            isPlowed = true;
            UpdateVisuals();
        }

        public bool CanWater()
        {
            return isPlowed && !isWatered;
        }

        public void Water()
        {
            if (!CanWater()) return;
            isWatered = true;
            waterTimer = 0f;
            UpdateVisuals();
        }

        public bool CanPlant(string cropType)
        {
            if (!isPlowed || currentCrop != null)
                return false;

            var crop = GetCropData(cropType);
            if (crop == null || resourceSystem == null)
                return false;

            // Check if player has required resources
            for (int i = 0; i < crop.requiredResources.Length; i++)
            {
                if (!resourceSystem.HasResource(crop.requiredResources[i], crop.resourceAmounts[i]))
                    return false;
            }

            return true;
        }

        public void Plant(string cropType)
        {
            if (!CanPlant(cropType)) return;

            var crop = GetCropData(cropType);
            
            // Consume resources
            for (int i = 0; i < crop.requiredResources.Length; i++)
            {
                resourceSystem.UseResource(crop.requiredResources[i], crop.resourceAmounts[i]);
            }

            // Start growing
            currentCrop = crop;
            growthProgress = 0f;
            currentStage = 0;
            UpdateCropVisuals();
        }

        public bool CanFertilize()
        {
            return isPlowed && currentCrop != null && !isFertilized && 
                   resourceSystem != null && resourceSystem.HasResource("Fertilizer", 1);
        }

        public void Fertilize()
        {
            if (!CanFertilize()) return;
            
            resourceSystem.UseResource("Fertilizer", 1);
            isFertilized = true;
        }

        public bool CanHarvest()
        {
            return currentCrop != null && growthProgress >= 1f;
        }

        public void Harvest()
        {
            if (!CanHarvest()) return;

            if (resourceSystem != null)
            {
                resourceSystem.AddResource(currentCrop.harvestedResource, currentCrop.harvestAmount);
            }

            onCropHarvested?.Invoke(currentCrop.harvestedResource, currentCrop.harvestAmount);

            // Reset plot
            if (currentCropObject != null)
                Destroy(currentCropObject);
            
            currentCrop = null;
            growthProgress = 0f;
            currentStage = 0;
            isPlowed = false;
            isWatered = false;
            isFertilized = false;
            
            UpdateVisuals();
        }

        private void UpdateGrowth()
        {
            float growthRate = Time.deltaTime / currentCrop.growthTime;
            if (isFertilized)
                growthRate *= (1f + fertilizeBonus);

            growthProgress += growthRate;
            growthProgress = Mathf.Min(growthProgress, 1f);

            // Update growth stage
            int newStage = Mathf.FloorToInt(growthProgress * (currentCrop.growthStages - 1));
            if (newStage != currentStage)
            {
                currentStage = newStage;
                UpdateCropVisuals();
            }

            onGrowthProgress?.Invoke(growthProgress);
        }

        private void UpdateVisuals()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = isPlowed ? plowedMaterial : unplowedMaterial;
                
                // Could add water effect/shader here
                if (isWatered)
                {
                    // Darken material or add water overlay
                }
            }
        }

        private void UpdateCropVisuals()
        {
            if (currentCropObject != null)
                Destroy(currentCropObject);

            if (currentCrop != null && currentCrop.stagePrefabs != null && currentStage < currentCrop.stagePrefabs.Length)
            {
                var prefab = currentCrop.stagePrefabs[currentStage];
                if (prefab != null)
                {
                    currentCropObject = Instantiate(prefab, transform.position, Quaternion.identity);
                    currentCropObject.transform.parent = transform;
                }
            }
        }

        private CropData GetCropData(string cropType)
        {
            return System.Array.Find(availableCrops, crop => crop.cropType == cropType);
        }

        public float GetGrowthProgress()
        {
            return growthProgress;
        }

        public string GetCurrentCropType()
        {
            return currentCrop?.cropType;
        }

        public bool IsPlowed()
        {
            return isPlowed;
        }

        public bool IsWatered()
        {
            return isWatered;
        }

        public bool IsFertilized()
        {
            return isFertilized;
        }
    }
}
