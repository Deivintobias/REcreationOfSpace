using UnityEngine;
using System.Collections.Generic;

namespace REcreationOfSpace.World
{
    public class BiomeManager : MonoBehaviour
    {
        [System.Serializable]
        public class Biome
        {
            public string name;
            public float minHeight;
            public float maxHeight;
            public float minMoisture;
            public float minTemperature;
            public Color groundColor;
            public GameObject[] vegetationPrefabs;
            public GameObject[] rockPrefabs;
            public float vegetationDensity;
            public float rockDensity;
        }

        [Header("Biome Settings")]
        [SerializeField] private Biome[] biomes;
        [SerializeField] private float temperatureScale = 100f;
        [SerializeField] private float moistureScale = 50f;
        [SerializeField] private float heightScale = 50f;

        [Header("Terrain Features")]
        [SerializeField] private float mountainFrequency = 0.01f;
        [SerializeField] private float plateauFrequency = 0.005f;
        [SerializeField] private float valleyFrequency = 0.008f;
        [SerializeField] private float riverFrequency = 0.003f;

        private void Start()
        {
            InitializeDefaultBiomes();
        }

        private void InitializeDefaultBiomes()
        {
            if (biomes == null || biomes.Length == 0)
            {
                biomes = new Biome[]
                {
                    // Ocean biome
                    new Biome
                    {
                        name = "Ocean",
                        minHeight = -100f,
                        maxHeight = 0f,
                        minMoisture = 0.8f,
                        minTemperature = -10f,
                        groundColor = new Color(0.1f, 0.2f, 0.4f)
                    },

                    // Beach biome
                    new Biome
                    {
                        name = "Beach",
                        minHeight = 0f,
                        maxHeight = 2f,
                        minMoisture = 0.3f,
                        minTemperature = 15f,
                        groundColor = new Color(0.95f, 0.95f, 0.8f),
                        rockPrefabs = new GameObject[0],
                        rockDensity = 0.1f
                    },

                    // Plains biome
                    new Biome
                    {
                        name = "Plains",
                        minHeight = 2f,
                        maxHeight = 20f,
                        minMoisture = 0.3f,
                        minTemperature = 10f,
                        groundColor = new Color(0.5f, 0.8f, 0.3f),
                        vegetationDensity = 0.7f
                    },

                    // Forest biome
                    new Biome
                    {
                        name = "Forest",
                        minHeight = 5f,
                        maxHeight = 50f,
                        minMoisture = 0.6f,
                        minTemperature = 5f,
                        groundColor = new Color(0.3f, 0.6f, 0.3f),
                        vegetationDensity = 0.9f
                    },

                    // Mountain biome
                    new Biome
                    {
                        name = "Mountain",
                        minHeight = 50f,
                        maxHeight = 200f,
                        minMoisture = 0.2f,
                        minTemperature = -15f,
                        groundColor = new Color(0.5f, 0.5f, 0.5f),
                        rockDensity = 0.8f
                    },

                    // Desert biome
                    new Biome
                    {
                        name = "Desert",
                        minHeight = 1f,
                        maxHeight = 30f,
                        minMoisture = 0f,
                        minTemperature = 25f,
                        groundColor = new Color(0.95f, 0.9f, 0.6f),
                        rockDensity = 0.3f
                    }
                };
            }
        }

        public Biome GetBiomeAt(Vector3 position, float height)
        {
            float moisture = GetMoisture(position);
            float temperature = GetTemperature(position);

            foreach (var biome in biomes)
            {
                if (height >= biome.minHeight && height <= biome.maxHeight &&
                    moisture >= biome.minMoisture &&
                    temperature >= biome.minTemperature)
                {
                    return biome;
                }
            }

            // Default to plains if no biome matches
            return biomes[2];
        }

        private float GetMoisture(Vector3 position)
        {
            float moisture = Mathf.PerlinNoise(
                position.x / moistureScale,
                position.z / moistureScale
            );

            // Increase moisture near water bodies
            if (position.y < 2f)
            {
                moisture += (2f - position.y) * 0.2f;
            }

            return Mathf.Clamp01(moisture);
        }

        private float GetTemperature(Vector3 position)
        {
            // Base temperature varies with latitude (z coordinate)
            float baseTemp = Mathf.Lerp(35f, -15f, Mathf.Abs(position.z) / temperatureScale);

            // Decrease temperature with elevation
            float heightTemp = Mathf.Max(0, position.y * 0.1f);
            
            // Add some noise for local variations
            float tempVariation = Mathf.PerlinNoise(
                position.x / temperatureScale,
                position.z / temperatureScale
            ) * 10f - 5f;

            return baseTemp - heightTemp + tempVariation;
        }

        public float GetTerrainHeight(Vector2 position)
        {
            // Base continent shape
            float height = Mathf.PerlinNoise(
                position.x / heightScale,
                position.y / heightScale
            ) * 100f - 50f;

            // Add mountains
            height += GetMountainHeight(position);

            // Add plateaus
            height += GetPlateauHeight(position);

            // Add valleys
            height += GetValleyDepth(position);

            // Add rivers
            height += GetRiverDepth(position);

            return height;
        }

        private float GetMountainHeight(Vector2 position)
        {
            float mountain = Mathf.PerlinNoise(
                position.x * mountainFrequency,
                position.y * mountainFrequency
            );
            
            mountain = Mathf.Pow(mountain, 3) * 200f;
            return mountain;
        }

        private float GetPlateauHeight(Vector2 position)
        {
            float plateau = Mathf.PerlinNoise(
                position.x * plateauFrequency,
                position.y * plateauFrequency
            );

            // Create sharp transitions for plateau edges
            plateau = Mathf.Round(plateau * 3) / 3f * 50f;
            return plateau;
        }

        private float GetValleyDepth(Vector2 position)
        {
            float valley = Mathf.PerlinNoise(
                position.x * valleyFrequency,
                position.y * valleyFrequency
            );

            valley = -valley * 30f;
            return valley;
        }

        private float GetRiverDepth(Vector2 position)
        {
            float river = Mathf.PerlinNoise(
                position.x * riverFrequency,
                position.y * riverFrequency
            );

            // Create river channels
            if (river > 0.7f)
            {
                river = -20f;
            }
            else
            {
                river = 0f;
            }

            return river;
        }
    }
}
