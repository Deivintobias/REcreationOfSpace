using UnityEngine;
using System.Collections.Generic;

public class LocalTerrainGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TerrainDetail
    {
        public GameObject[] vegetationPrefabs;
        public GameObject[] rockPrefabs;
        public Material groundMaterial;
        public float detailDensity = 1f;
        public float heightVariation = 1f;
    }

    [Header("Terrain Settings")]
    public float localRadius = 50f; // Radius around character
    public int resolution = 100; // Grid resolution
    public float maxHeight = 5f;
    public float noiseScale = 20f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Terrain Types")]
    public TerrainDetail sionTerrain = new TerrainDetail
    {
        detailDensity = 0.8f,
        heightVariation = 0.5f
    };

    public TerrainDetail sinaiTerrain = new TerrainDetail
    {
        detailDensity = 1.2f,
        heightVariation = 1.5f
    };

    private Dictionary<Vector2Int, GameObject> activeTerrainChunks = new Dictionary<Vector2Int, GameObject>();
    private Transform followTarget;
    private Vector3 lastUpdatePosition;
    private float updateThreshold = 10f;

    public void Initialize(Transform target)
    {
        followTarget = target;
        lastUpdatePosition = target.position;
        GenerateInitialTerrain();
    }

    private void Update()
    {
        if (followTarget == null) return;

        // Check if we need to update terrain
        if (Vector3.Distance(followTarget.position, lastUpdatePosition) > updateThreshold)
        {
            UpdateLocalTerrain();
            lastUpdatePosition = followTarget.position;
        }
    }

    private void GenerateInitialTerrain()
    {
        Vector3 center = followTarget.position;
        int chunksPerSide = Mathf.CeilToInt(localRadius / resolution);

        for (int x = -chunksPerSide; x <= chunksPerSide; x++)
        {
            for (int z = -chunksPerSide; z <= chunksPerSide; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, z);
                if (IsChunkInRange(chunkCoord))
                {
                    GenerateTerrainChunk(chunkCoord);
                }
            }
        }
    }

    private void UpdateLocalTerrain()
    {
        // Remove out of range chunks
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeTerrainChunks)
        {
            if (!IsChunkInRange(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunk in chunksToRemove)
        {
            Destroy(activeTerrainChunks[chunk].gameObject);
            activeTerrainChunks.Remove(chunk);
        }

        // Add new chunks in range
        Vector3 center = followTarget.position;
        int chunksPerSide = Mathf.CeilToInt(localRadius / resolution);

        for (int x = -chunksPerSide; x <= chunksPerSide; x++)
        {
            for (int z = -chunksPerSide; z <= chunksPerSide; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, z);
                if (IsChunkInRange(chunkCoord) && !activeTerrainChunks.ContainsKey(chunkCoord))
                {
                    GenerateTerrainChunk(chunkCoord);
                }
            }
        }
    }

    private void GenerateTerrainChunk(Vector2Int chunkCoord)
    {
        GameObject chunk = new GameObject($"TerrainChunk_{chunkCoord.x}_{chunkCoord.y}");
        chunk.transform.parent = transform;

        // Generate mesh
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = chunk.AddComponent<MeshCollider>();

        // Create terrain mesh
        Mesh mesh = GenerateTerrainMesh(chunkCoord);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        // Apply material based on location
        bool isNearSinai = IsNearMountSinai(chunkCoord);
        TerrainDetail terrainDetail = isNearSinai ? sinaiTerrain : sionTerrain;
        meshRenderer.material = terrainDetail.groundMaterial;

        // Add details
        AddTerrainDetails(chunk, terrainDetail);

        activeTerrainChunks.Add(chunkCoord, chunk);
    }

    private Mesh GenerateTerrainMesh(Vector2Int chunkCoord)
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        int[] triangles = new int[resolution * resolution * 6];
        Vector2[] uvs = new Vector2[vertices.Length];

        float chunkSize = localRadius * 2f / resolution;
        Vector3 chunkOffset = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);

        // Generate vertices
        for (int i = 0, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float xPos = x * chunkSize;
                float zPos = z * chunkSize;
                
                // Generate height using multiple octaves of noise
                float height = 0;
                float amplitude = 1;
                float frequency = 1;
                
                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = (xPos + chunkOffset.x) / noiseScale * frequency;
                    float sampleZ = (zPos + chunkOffset.z) / noiseScale * frequency;
                    
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);
                    height += perlinValue * amplitude;
                    
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                vertices[i] = new Vector3(xPos, height * maxHeight, zPos) + chunkOffset;
                uvs[i] = new Vector2(x / (float)resolution, z / (float)resolution);
                i++;
            }
        }

        // Generate triangles
        int vert = 0;
        int tris = 0;
        
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + resolution + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + resolution + 1;
                triangles[tris + 5] = vert + resolution + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void AddTerrainDetails(GameObject chunk, TerrainDetail detail)
    {
        // Add vegetation
        if (detail.vegetationPrefabs != null && detail.vegetationPrefabs.Length > 0)
        {
            int detailCount = Mathf.FloorToInt(resolution * detail.detailDensity);
            for (int i = 0; i < detailCount; i++)
            {
                Vector3 randomPoint = GetRandomPointOnTerrain(chunk);
                if (randomPoint != Vector3.zero)
                {
                    GameObject prefab = detail.vegetationPrefabs[Random.Range(0, detail.vegetationPrefabs.Length)];
                    GameObject vegetation = Instantiate(prefab, randomPoint, Quaternion.Euler(0, Random.Range(0, 360), 0), chunk.transform);
                    vegetation.transform.localScale *= Random.Range(0.8f, 1.2f);
                }
            }
        }

        // Add rocks
        if (detail.rockPrefabs != null && detail.rockPrefabs.Length > 0)
        {
            int rockCount = Mathf.FloorToInt(resolution * detail.detailDensity * 0.3f);
            for (int i = 0; i < rockCount; i++)
            {
                Vector3 randomPoint = GetRandomPointOnTerrain(chunk);
                if (randomPoint != Vector3.zero)
                {
                    GameObject prefab = detail.rockPrefabs[Random.Range(0, detail.rockPrefabs.Length)];
                    GameObject rock = Instantiate(prefab, randomPoint, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), 0), chunk.transform);
                    rock.transform.localScale *= Random.Range(0.5f, 1.5f);
                }
            }
        }
    }

    private Vector3 GetRandomPointOnTerrain(GameObject chunk)
    {
        MeshCollider collider = chunk.GetComponent<MeshCollider>();
        if (collider == null) return Vector3.zero;

        Bounds bounds = collider.bounds;
        Vector3 randomPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.max.y + 10f,
            Random.Range(bounds.min.z, bounds.max.z)
        );

        RaycastHit hit;
        if (Physics.Raycast(randomPoint, Vector3.down, out hit, 20f))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private bool IsChunkInRange(Vector2Int chunkCoord)
    {
        Vector3 chunkCenter = new Vector3(chunkCoord.x * resolution, 0, chunkCoord.y * resolution);
        return Vector3.Distance(chunkCenter, followTarget.position) < localRadius * 1.5f;
    }

    private bool IsNearMountSinai(Vector2Int chunkCoord)
    {
        Vector3 chunkCenter = new Vector3(chunkCoord.x * resolution, 0, chunkCoord.y * resolution);
        if (MountSinai.Instance != null)
        {
            return Vector3.Distance(chunkCenter, MountSinai.Instance.transform.position) < MountSinai.Instance.GetMountainRadius();
        }
        return false;
    }
}
