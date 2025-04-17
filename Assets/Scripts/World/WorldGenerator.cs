using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    [System.Serializable]
    public class BiomeSettings
    {
        public string biomeName;
        public GameObject[] terrainPrefabs;
        public GameObject[] vegetationPrefabs;
        public GameObject[] structurePrefabs;
        public float elevation;
        public Color groundColor;
    }

    [Header("World Settings")]
    public int worldSize = 1000;
    public int chunkSize = 100;
    public float maxHeight = 200f;
    public Transform worldCenter;

    [Header("Mount Sinai Settings")]
    public float mountainRadius = 200f;
    public float mountainHeight = 1000f;
    public int mountainDetailLevel = 8;
    public Material mountainMaterial;

    [Header("Biomes")]
    public BiomeSettings[] biomes;
    public float biomeBlendDistance = 10f;

    [Header("Character Generation")]
    public GameObject sionCharacterPrefab;
    public GameObject sinaiCharacterPrefab;
    public int initialSionPopulation = 50;
    public int initialSinaiPopulation = 20;
    public float characterSpawnRadius = 100f;

    [Header("Structure Settings")]
    public GameObject paradiseCityPrefab;
    public GameObject[] sionStructurePrefabs;
    public float structureDensity = 0.1f;

    private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
    private List<GameObject> activeCharacters = new List<GameObject>();

    private void Start()
    {
        GenerateWorld();
        SpawnInitialCharacters();
    }

    private void GenerateWorld()
    {
        // Generate base terrain
        GenerateBaseTerrain();

        // Create Mount Sinai
        GenerateMountSinai();

        // Place Paradise City
        PlaceParadiseCity();

        // Generate structures
        GenerateStructures();

        // Add vegetation and details
        AddWorldDetails();
    }

    private void GenerateBaseTerrain()
    {
        int chunksPerSide = worldSize / chunkSize;
        for (int x = -chunksPerSide/2; x < chunksPerSide/2; x++)
        {
            for (int z = -chunksPerSide/2; z < chunksPerSide/2; z++)
            {
                GenerateChunk(new Vector2Int(x, z));
            }
        }
    }

    private void GenerateChunk(Vector2Int chunkCoord)
    {
        GameObject chunk = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunk.transform.position = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);

        // Generate mesh
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = chunk.AddComponent<MeshCollider>();

        // Create terrain mesh
        Mesh mesh = GenerateTerrainMesh(chunkCoord);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        // Apply biome materials
        meshRenderer.material = GetBiomeMaterial(chunkCoord);

        chunks[chunkCoord] = chunk;
    }

    private Mesh GenerateTerrainMesh(Vector2Int chunkCoord)
    {
        Mesh mesh = new Mesh();
        // Generate vertices, triangles, etc. based on noise and biome settings
        // This would be your terrain generation algorithm
        return mesh;
    }

    private Material GetBiomeMaterial(Vector2Int chunkCoord)
    {
        // Determine biome based on position and noise
        // Return appropriate material
        return null;
    }

    private void GenerateMountSinai()
    {
        GameObject mountain = new GameObject("Mount_Sinai");
        mountain.transform.position = Vector3.zero;

        // Generate mountain mesh
        MeshFilter meshFilter = mountain.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = mountain.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = mountain.AddComponent<MeshCollider>();

        // Create mountain mesh
        Mesh mesh = GenerateMountainMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.material = mountainMaterial;

        // Add mountain script
        mountain.AddComponent<MountSinai>();
    }

    private Mesh GenerateMountainMesh()
    {
        Mesh mesh = new Mesh();
        // Generate mountain vertices using heightmap or procedural generation
        // This would create the distinctive Mount Sinai shape
        return mesh;
    }

    private void PlaceParadiseCity()
    {
        if (paradiseCityPrefab != null)
        {
            Vector3 cityPosition = Vector3.zero; // At world epicenter
            Instantiate(paradiseCityPrefab, cityPosition, Quaternion.identity);
        }
    }

    private void GenerateStructures()
    {
        int structureCount = Mathf.FloorToInt(worldSize * worldSize * structureDensity);
        
        for (int i = 0; i < structureCount; i++)
        {
            Vector3 position = GetRandomWorldPosition();
            if (IsValidStructurePosition(position))
            {
                PlaceStructure(position);
            }
        }
    }

    private void PlaceStructure(Vector3 position)
    {
        if (sionStructurePrefabs.Length == 0) return;

        GameObject prefab = sionStructurePrefabs[Random.Range(0, sionStructurePrefabs.Length)];
        GameObject structure = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
    }

    private void AddWorldDetails()
    {
        foreach (var biome in biomes)
        {
            // Add vegetation
            foreach (var prefab in biome.vegetationPrefabs)
            {
                int count = Random.Range(50, 200);
                for (int i = 0; i < count; i++)
                {
                    Vector3 position = GetRandomWorldPosition();
                    if (IsBiomePosition(position, biome))
                    {
                        Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                    }
                }
            }
        }
    }

    private void SpawnInitialCharacters()
    {
        // Spawn Sion characters
        for (int i = 0; i < initialSionPopulation; i++)
        {
            SpawnCharacter(true);
        }

        // Spawn Sinai characters
        for (int i = 0; i < initialSinaiPopulation; i++)
        {
            SpawnCharacter(false);
        }
    }

    private void SpawnCharacter(bool isSionCharacter)
    {
        Vector3 position = GetRandomSpawnPosition();
        GameObject prefab = isSionCharacter ? sionCharacterPrefab : sinaiCharacterPrefab;
        
        GameObject character = Instantiate(prefab, position, Quaternion.identity);
        
        if (!isSionCharacter)
        {
            // Setup Sinai character components
            SinaiCharacter sinaiChar = character.GetComponent<SinaiCharacter>();
            if (sinaiChar == null)
            {
                sinaiChar = character.AddComponent<SinaiCharacter>();
            }
            
            // Add observer component
            SionObserver observer = character.GetComponent<SionObserver>();
            if (observer == null)
            {
                observer = character.AddComponent<SionObserver>();
            }
        }
        
        activeCharacters.Add(character);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * characterSpawnRadius;
        Vector3 position = new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // Raycast to find ground
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit))
        {
            return hit.point + Vector3.up;
        }
        
        return position + Vector3.up;
    }

    private Vector3 GetRandomWorldPosition()
    {
        float x = Random.Range(-worldSize/2f, worldSize/2f);
        float z = Random.Range(-worldSize/2f, worldSize/2f);
        return new Vector3(x, 0, z);
    }

    private bool IsValidStructurePosition(Vector3 position)
    {
        // Check distance from other structures, mountain, and city
        return true; // Implement proper checks
    }

    private bool IsBiomePosition(Vector3 position, BiomeSettings biome)
    {
        // Check if position matches biome parameters
        return true; // Implement proper checks
    }

    public void OnCharacterDeath(GameObject character)
    {
        activeCharacters.Remove(character);
        
        // Spawn replacement if needed
        if (character.GetComponent<SinaiCharacter>() != null)
        {
            SpawnCharacter(false);
        }
        else
        {
            SpawnCharacter(true);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw world boundaries
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(worldSize, maxHeight, worldSize));
        
        // Draw spawn radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, characterSpawnRadius);
    }
}
