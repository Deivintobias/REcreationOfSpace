using UnityEngine;

namespace REcreationOfSpace.Visual
{
    public class LocalTerrainGenerator : MonoBehaviour
    {
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private float heightScale = 5f;
        [SerializeField] private float noiseScale = 0.3f;

        public void GenerateTerrain(int size, float tileSize)
        {
            // Create terrain mesh
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();

            // Generate mesh data
            var mesh = new Mesh();
            var vertices = new Vector3[(size + 1) * (size + 1)];
            var triangles = new int[size * size * 6];
            var uvs = new Vector2[(size + 1) * (size + 1)];

            // Generate vertices and UVs
            for (int i = 0; i <= size; i++)
            {
                for (int j = 0; j <= size; j++)
                {
                    float x = j * tileSize;
                    float z = i * tileSize;
                    float y = GenerateHeight(x, z);
                    vertices[i * (size + 1) + j] = new Vector3(x, y, z);
                    uvs[i * (size + 1) + j] = new Vector2((float)j / size, (float)i / size);
                }
            }

            // Generate triangles
            int tris = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int vertIndex = i * (size + 1) + j;
                    triangles[tris] = vertIndex;
                    triangles[tris + 1] = vertIndex + size + 1;
                    triangles[tris + 2] = vertIndex + 1;
                    triangles[tris + 3] = vertIndex + 1;
                    triangles[tris + 4] = vertIndex + size + 1;
                    triangles[tris + 5] = vertIndex + size + 2;
                    tris += 6;
                }
            }

            // Apply mesh data
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            // Apply mesh to components
            meshFilter.mesh = mesh;
            meshRenderer.material = terrainMaterial != null ? terrainMaterial : new Material(Shader.Find("Standard"));

            // Add collider
            var meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        private float GenerateHeight(float x, float z)
        {
            float xCoord = x * noiseScale;
            float zCoord = z * noiseScale;
            return Mathf.PerlinNoise(xCoord, zCoord) * heightScale;
        }
    }
}
