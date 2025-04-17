using UnityEngine;
using System.Collections.Generic;

namespace REcreationOfSpace.World
{
    public class ChunkManager : MonoBehaviour
    {
        [System.Serializable]
        public class TerrainType
        {
            public string name;
            public float height;
            public Color color;
            public GameObject[] vegetationPrefabs;
            public float vegetationDensity;
        }

        [Header("Chunk Settings")]
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private int viewDistance = 5;
        [SerializeField] private Transform player;
        [SerializeField] private Material terrainMaterial;

        [Header("Generation Settings")]
        [SerializeField] private int seed = 0;
        [SerializeField] private BiomeManager biomeManager;

        [Header("Historical Settings")]
        [SerializeField] private Timeline timeline;
        [SerializeField] private float historicalLocationSpacing = 1000f;

        private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int currentChunkCoord;
        private Queue<GameObject> chunkPool = new Queue<GameObject>();
        private HashSet<Vector2Int> historicalLocations = new HashSet<Vector2Int>();

        private void Start()
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }

            if (seed == 0)
            {
                seed = Random.Range(-10000, 10000);
            }

            if (timeline == null)
            {
                timeline = FindObjectOfType<Timeline>();
            }

            UpdateChunks();
        }

        private void Update()
        {
            Vector2Int newChunkCoord = GetChunkCoordFromPosition(player.position);
            if (newChunkCoord != currentChunkCoord)
            {
                currentChunkCoord = newChunkCoord;
                UpdateChunks();
            }
        }

        private void UpdateChunks()
        {
            // Hide chunks that are too far
            List<Vector2Int> chunksToRemove = new List<Vector2Int>();
            foreach (var chunk in chunks)
            {
                if (Vector2Int.Distance(chunk.Key, currentChunkCoord) > viewDistance)
                {
                    chunksToRemove.Add(chunk.Key);
                }
            }

            foreach (var coord in chunksToRemove)
            {
                RecycleChunk(chunks[coord]);
                chunks.Remove(coord);
            }

            // Generate visible chunks
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    Vector2Int viewedChunk = currentChunkCoord + new Vector2Int(x, z);
                    if (!chunks.ContainsKey(viewedChunk))
                    {
                        CreateChunk(viewedChunk);
                    }
                }
            }
        }

        private void CreateChunk(Vector2Int coord)
        {
            GameObject chunk;
            if (chunkPool.Count > 0)
            {
                chunk = chunkPool.Dequeue();
                chunk.SetActive(true);
            }
            else
            {
                chunk = new GameObject($"Chunk_{coord.x}_{coord.y}");
                chunk.AddComponent<MeshFilter>();
                chunk.AddComponent<MeshRenderer>();
                chunk.AddComponent<MeshCollider>();
            }

            chunk.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
            GenerateChunkMesh(chunk, coord);
            chunks.Add(coord, chunk);

            // Check for historical locations
            if (ShouldGenerateHistoricalLocation(coord))
            {
                GenerateHistoricalLocation(chunk, coord);
            }
        }

        private void GenerateChunkMesh(GameObject chunk, Vector2Int coord)
        {
            int resolution = chunkSize + 1;
            Vector3[] vertices = new Vector3[resolution * resolution];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
            Vector2[] uvs = new Vector2[vertices.Length];

            float[,] heightMap = GenerateHeightMap(coord, resolution);

            // Generate vertices
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float height = heightMap[x, z];
                    vertices[z * resolution + x] = new Vector3(x, height, z);
                    uvs[z * resolution + x] = new Vector2(x / (float)resolution, z / (float)resolution);
                }
            }

            // Generate triangles
            int tris = 0;
            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    triangles[tris] = z * resolution + x;
                    triangles[tris + 1] = (z + 1) * resolution + x;
                    triangles[tris + 2] = z * resolution + x + 1;
                    triangles[tris + 3] = z * resolution + x + 1;
                    triangles[tris + 4] = (z + 1) * resolution + x;
                    triangles[tris + 5] = (z + 1) * resolution + x + 1;
                    tris += 6;
                }
            }

            // Apply mesh
            var meshFilter = chunk.GetComponent<MeshFilter>();
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;

            // Apply material and collider
            var renderer = chunk.GetComponent<MeshRenderer>();
            renderer.material = terrainMaterial;
            var collider = chunk.GetComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            // Add vegetation
            AddVegetation(chunk, heightMap);
        }

        private float[,] GenerateHeightMap(Vector2Int coord, int resolution)
        {
            if (biomeManager == null)
            {
                biomeManager = FindObjectOfType<BiomeManager>();
                if (biomeManager == null)
                {
                    var biomeObj = new GameObject("BiomeManager");
                    biomeManager = biomeObj.AddComponent<BiomeManager>();
                }
            }

            float[,] heightMap = new float[resolution, resolution];
            Vector2 offset = new Vector2(coord.x * (chunkSize - 1), coord.y * (chunkSize - 1));

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 worldPos = new Vector2(x + offset.x, z + offset.y);
                    heightMap[x, z] = biomeManager.GetTerrainHeight(worldPos);
                }
            }

            return heightMap;
        }

        private void AddVegetation(GameObject chunk, float[,] heightMap)
        {
            if (biomeManager == null)
                return;

            int resolution = heightMap.GetLength(0);
            for (int z = 1; z < resolution - 1; z++)
            {
                for (int x = 1; x < resolution - 1; x++)
                {
                    float height = heightMap[x, z];
                    Vector3 worldPos = new Vector3(x, height, z) + chunk.transform.position;
                    BiomeManager.Biome biome = biomeManager.GetBiomeAt(worldPos, height);

                    // Add vegetation based on biome
                    if (biome.vegetationPrefabs != null && 
                        biome.vegetationPrefabs.Length > 0 && 
                        Random.value < biome.vegetationDensity)
                    {
                        GameObject prefab = biome.vegetationPrefabs[Random.Range(0, biome.vegetationPrefabs.Length)];
                        GameObject vegetation = Instantiate(prefab, worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0), chunk.transform);
                        float scale = Random.Range(0.8f, 1.2f);
                        vegetation.transform.localScale *= scale;
                    }

                    // Add rocks based on biome
                    if (biome.rockPrefabs != null && 
                        biome.rockPrefabs.Length > 0 && 
                        Random.value < biome.rockDensity)
                    {
                        GameObject prefab = biome.rockPrefabs[Random.Range(0, biome.rockPrefabs.Length)];
                        GameObject rock = Instantiate(prefab, worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0), chunk.transform);
                        float scale = Random.Range(0.8f, 1.2f);
                        rock.transform.localScale *= scale;
                    }
                }
            }
        }

        private bool ShouldGenerateHistoricalLocation(Vector2Int coord)
        {
            if (historicalLocations.Contains(coord))
                return false;

            // Check spacing from other historical locations
            foreach (var loc in historicalLocations)
            {
                if (Vector2Int.Distance(coord, loc) < historicalLocationSpacing)
                    return false;
            }

            // Random chance based on current era
            float chance = 0.01f; // 1% base chance
            if (timeline != null)
            {
                var currentEra = timeline.GetCurrentEra();
                if (currentEra != null)
                {
                    // Increase chance for biblical eras
                    if (currentEra.startYear < 0)
                        chance *= 2f;
                }
            }

            return Random.value < chance;
        }

        private void GenerateHistoricalLocation(GameObject chunk, Vector2Int coord)
        {
            if (timeline == null)
                return;

            historicalLocations.Add(coord);

            // Get a random position within the chunk
            Vector3 position = chunk.transform.position + new Vector3(
                Random.Range(0, chunkSize),
                0,
                Random.Range(0, chunkSize)
            );

            // Create historical location
            var evt = timeline.GetEventByName("Creation"); // Default to creation event
            if (evt != null && evt.eventPrefab != null)
            {
                GameObject location = Instantiate(evt.eventPrefab, position, Quaternion.identity, chunk.transform);
                var info = location.AddComponent<HistoricalInfo>();
                info.Initialize(evt);
            }
        }

        private void RecycleChunk(GameObject chunk)
        {
            chunk.SetActive(false);
            chunkPool.Enqueue(chunk);
        }

        private Vector2Int GetChunkCoordFromPosition(Vector3 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / chunkSize),
                Mathf.FloorToInt(position.z / chunkSize)
            );
        }

        public Vector3 GetHeightAtPosition(Vector3 position)
        {
            Vector2Int chunkCoord = GetChunkCoordFromPosition(position);
            if (chunks.TryGetValue(chunkCoord, out GameObject chunk))
            {
                RaycastHit hit;
                if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit))
                {
                    return hit.point;
                }
            }
            return position;
        }
    }
}
