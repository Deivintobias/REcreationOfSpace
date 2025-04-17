using UnityEngine;
using System.Collections.Generic;

namespace REcreationOfSpace.World
{
    public class WorldManager : MonoBehaviour
    {
        [Header("World Generation")]
        [SerializeField] private int worldSize = 100;
        [SerializeField] private float resourceDensity = 0.1f;
        [SerializeField] private float enemyDensity = 0.05f;
        [SerializeField] private float farmlandDensity = 0.02f;

        [Header("Historical Settings")]
        [SerializeField] private Timeline timeline;
        [SerializeField] private bool startInModernEra = true;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Material skyboxMaterial;

        [Header("Prefabs")]
        [SerializeField] private GameObject[] resourcePrefabs;
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private GameObject[] structurePrefabs;
        [SerializeField] private GameObject farmPlotPrefab;

        [Header("Workbenches")]
        [SerializeField] private GameObject basicWorkbenchPrefab;
        [SerializeField] private GameObject smithyWorkbenchPrefab;
        [SerializeField] private GameObject alchemyWorkbenchPrefab;
        [SerializeField] private float workbenchSpacing = 30f;

        [Header("Special Areas")]
        [SerializeField] private GameObject paradiseCityPrefab;
        [SerializeField] private GameObject mountSinaiPrefab;
        [SerializeField] private float specialAreaSpacing = 50f;

        private WorldGenerator worldGenerator;
        private List<GameObject> spawnedEntities = new List<GameObject>();
        private Dictionary<Vector2Int, WorldStructure> worldStructures = new Dictionary<Vector2Int, WorldStructure>();

        private void Start()
        {
            worldGenerator = GetComponent<WorldGenerator>();
            if (worldGenerator == null)
            {
                worldGenerator = gameObject.AddComponent<WorldGenerator>();
            }

            if (timeline == null)
            {
                timeline = gameObject.AddComponent<Timeline>();
            }

            GenerateWorld();
        }

        [Header("Chunk Settings")]
        [SerializeField] private ChunkManager chunkManager;
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private TerrainType[] terrainTypes;

        private void GenerateWorld()
        {
            // Initialize chunk manager if not assigned
            if (chunkManager == null)
            {
                var chunkObj = new GameObject("ChunkManager");
                chunkManager = chunkObj.AddComponent<ChunkManager>();
                chunkManager.transform.SetParent(transform);
            }

            // Set up timeline
            if (startInModernEra)
            {
                timeline.SetCurrentEra(2025);
            }
            else
            {
                timeline.SetCurrentEra(-4000); // Start at Creation
            }

            // Configure chunk manager
            var chunkManagerField = chunkManager.GetType().GetField("terrainMaterial", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (chunkManagerField != null)
                chunkManagerField.SetValue(chunkManager, terrainMaterial);

            var terrainTypesField = chunkManager.GetType().GetField("terrainTypes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (terrainTypesField != null)
                terrainTypesField.SetValue(chunkManager, terrainTypes);

            // Set timeline reference
            var timelineField = chunkManager.GetType().GetField("timeline", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (timelineField != null)
                timelineField.SetValue(chunkManager, timeline);
        }

        private void GenerateHistoricalLocations()
        {
            // Special historical locations are handled by the Timeline component
            if (paradiseCityPrefab != null)
            {
                var gardenLocation = timeline.GetLocationByName("Garden of Eden");
                if (gardenLocation == null)
                {
                    var paradise = Instantiate(paradiseCityPrefab, new Vector3(worldSize * 0.25f, 0, worldSize * 0.25f), Quaternion.identity);
                    spawnedEntities.Add(paradise);
                }
            }

            if (mountSinaiPrefab != null)
            {
                var sinaiLocation = timeline.GetLocationByName("Mount Sinai");
                if (sinaiLocation == null)
                {
                    var sinai = Instantiate(mountSinaiPrefab, new Vector3(-worldSize * 0.25f, 0, -worldSize * 0.25f), Quaternion.identity);
                    spawnedEntities.Add(sinai);
                }
            }
        }

        private void GenerateFarmland()
        {
            int farmPlotCount = Mathf.RoundToInt(worldSize * worldSize * farmlandDensity);
            float plotSize = 2f;

            for (int i = 0; i < farmPlotCount; i++)
            {
                Vector3 clusterCenter = GetRandomPosition();
                int plotsInCluster = Random.Range(4, 9);

                for (int x = 0; x < Mathf.Sqrt(plotsInCluster); x++)
                {
                    for (int z = 0; z < Mathf.Sqrt(plotsInCluster); z++)
                    {
                        Vector3 plotPosition = clusterCenter + new Vector3(x * plotSize, 0, z * plotSize);
                        if (farmPlotPrefab != null && !IsNearHistoricalLocation(plotPosition))
                        {
                            var plot = Instantiate(farmPlotPrefab, plotPosition, Quaternion.identity);
                            spawnedEntities.Add(plot);
                        }
                    }
                }
            }
        }

        private void GenerateWorkbenches()
        {
            SpawnWorkbench(basicWorkbenchPrefab, new Vector3(worldSize * 0.1f, 0, worldSize * 0.1f));
            SpawnWorkbench(smithyWorkbenchPrefab, new Vector3(-worldSize * 0.1f, 0, worldSize * 0.1f));
            SpawnWorkbench(alchemyWorkbenchPrefab, new Vector3(0, 0, worldSize * 0.2f));

            for (int i = 0; i < 3; i++)
            {
                Vector3 position = GetRandomPosition();
                if (!IsNearHistoricalLocation(position))
                {
                    GameObject prefab = Random.value < 0.5f ? basicWorkbenchPrefab : smithyWorkbenchPrefab;
                    SpawnWorkbench(prefab, position);
                }
            }
        }

        private void SpawnWorkbench(GameObject prefab, Vector3 position)
        {
            if (prefab != null && !IsNearHistoricalLocation(position))
            {
                var workbench = Instantiate(prefab, position, Quaternion.identity);
                spawnedEntities.Add(workbench);
            }
        }

        private void SpawnResource()
        {
            if (resourcePrefabs == null || resourcePrefabs.Length == 0)
                return;

            Vector3 position = GetRandomPosition();
            if (!IsNearHistoricalLocation(position))
            {
                GameObject prefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];
                var resource = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                spawnedEntities.Add(resource);
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
                return;

            Vector3 position = GetRandomPosition();
            if (!IsNearHistoricalLocation(position))
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                var enemy = Instantiate(prefab, position, Quaternion.identity);
                spawnedEntities.Add(enemy);

                var ai = enemy.GetComponent<EnemyAI>();
                if (ai == null)
                {
                    ai = enemy.AddComponent<EnemyAI>();
                }
            }
        }

        private void GenerateStructures()
        {
            if (structurePrefabs == null || structurePrefabs.Length == 0)
                return;

            float spacing = specialAreaSpacing;
            int gridSize = Mathf.RoundToInt(worldSize / spacing);

            for (int x = -gridSize/2; x <= gridSize/2; x++)
            {
                for (int z = -gridSize/2; z <= gridSize/2; z++)
                {
                    Vector3 position = new Vector3(x * spacing, 0, z * spacing);
                    if (IsNearHistoricalLocation(position))
                        continue;

                    if (Random.value < 0.3f)
                    {
                        GameObject prefab = structurePrefabs[Random.Range(0, structurePrefabs.Length)];
                        var structure = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                        spawnedEntities.Add(structure);

                        var worldStructure = structure.GetComponent<WorldStructure>();
                        if (worldStructure != null)
                        {
                            worldStructures[new Vector2Int(x, z)] = worldStructure;
                        }
                    }
                }
            }
        }

        private Vector3 GetRandomPosition()
        {
            float halfSize = worldSize * 0.5f;
            return new Vector3(
                Random.Range(-halfSize, halfSize),
                0,
                Random.Range(-halfSize, halfSize)
            );
        }

        private bool IsNearHistoricalLocation(Vector3 position)
        {
            float minDistance = specialAreaSpacing * 0.5f;
            var historicalEvents = timeline.GetType().GetField("historicalEvents",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (historicalEvents != null)
            {
                var events = (Timeline.HistoricalEvent[])historicalEvents.GetValue(timeline);
                foreach (var evt in events)
                {
                    if (Vector3.Distance(position, evt.location) < minDistance)
                        return true;
                }
            }
            return false;
        }

        public WorldStructure GetStructureAt(Vector2Int gridPosition)
        {
            return worldStructures.ContainsKey(gridPosition) ? worldStructures[gridPosition] : null;
        }

        public void CleanupEntity(GameObject entity)
        {
            if (spawnedEntities.Contains(entity))
            {
                spawnedEntities.Remove(entity);
                Destroy(entity);
            }
        }

        public float GetWorldSize()
        {
            return worldSize;
        }

        public Timeline GetTimeline()
        {
            return timeline;
        }

        private void OnDestroy()
        {
            foreach (var entity in spawnedEntities)
            {
                if (entity != null)
                    Destroy(entity);
            }
        }
    }
}
