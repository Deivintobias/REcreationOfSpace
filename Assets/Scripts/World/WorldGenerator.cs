using UnityEngine;

namespace REcreationOfSpace.World
{
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject terrainPrefab;
        [SerializeField] private int worldSize = 100;
        [SerializeField] private float tileSize = 1f;

        private LocalTerrainGenerator terrainGenerator;

        public void Initialize()
        {
            // Get or add the terrain generator component
            terrainGenerator = GetComponent<LocalTerrainGenerator>();
            if (terrainGenerator == null)
                terrainGenerator = gameObject.AddComponent<LocalTerrainGenerator>();

            // Generate initial world structure
            GenerateWorld();

            // Set up world systems
            SetupWorldSystems();
        }

        private void GenerateWorld()
        {
            // Create base terrain
            if (terrainPrefab != null)
            {
                var terrain = Instantiate(terrainPrefab, Vector3.zero, Quaternion.identity);
                terrain.transform.parent = transform;
            }

            // Generate terrain features using LocalTerrainGenerator
            if (terrainGenerator != null)
            {
                terrainGenerator.GenerateTerrain(worldSize, tileSize);
            }
        }

        private void SetupWorldSystems()
        {
            // Initialize world manager
            var worldManager = GetComponent<WorldManager>();
            if (worldManager != null)
                worldManager.enabled = true;

            // Set up resource nodes
            var resourceSystem = GetComponent<ResourceSystem>();
            if (resourceSystem != null)
                resourceSystem.enabled = true;

            // Initialize spawners
            var characterSpawner = GetComponent<CharacterSpawner>();
            if (characterSpawner != null)
                characterSpawner.enabled = true;
        }
    }
}
