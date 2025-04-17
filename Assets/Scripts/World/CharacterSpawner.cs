using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public static CharacterSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public Transform sionSpawnPoint; // Default spawn point on the surface
    public float spawnHeight = 1f; // Height above ground to spawn characters

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (sionSpawnPoint == null)
        {
            Debug.LogWarning("No spawn point set! Using CharacterSpawner position.");
            sionSpawnPoint = transform;
        }

        // Ensure WorldManager starts in Sion layer
        WorldManager worldManager = WorldManager.Instance;
        if (worldManager != null)
        {
            worldManager.EnsureSionLayer();
        }

        // Move any existing characters to spawn point
        MoveExistingCharactersToSurface();
    }

    public Vector3 GetSpawnPosition()
    {
        Vector3 spawnPos = sionSpawnPoint.position;
        spawnPos.y += spawnHeight;

        // Raycast to find ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            spawnPos.y = hit.point.y + spawnHeight;
        }

        return spawnPos;
    }

    public void SpawnCharacter(GameObject characterPrefab)
    {
        if (characterPrefab == null)
        {
            Debug.LogError("No character prefab provided!");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        GameObject newCharacter = Instantiate(characterPrefab, spawnPos, Quaternion.identity);
        
        // Ensure the character has necessary components
        SetupCharacter(newCharacter);
    }

    private void SetupCharacter(GameObject character)
    {
        // Add required components if they don't exist
        if (!character.GetComponent<Health>())
        {
            character.AddComponent<Health>();
        }

        // If it's a player
        if (character.CompareTag("Player"))
        {
            if (!character.GetComponent<PlayerController>())
            {
                character.AddComponent<PlayerController>();
            }
            if (!character.GetComponent<CombatController>())
            {
                character.AddComponent<CombatController>();
            }
        }
        // If it's an enemy
        else if (character.CompareTag("Enemy"))
        {
            if (!character.GetComponent<EnemyAI>())
            {
                character.AddComponent<EnemyAI>();
            }
        }
    }

    private void MoveExistingCharactersToSurface()
    {
        // Move all existing characters to the surface
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject character in players)
        {
            character.transform.position = GetSpawnPosition();
        }

        foreach (GameObject character in enemies)
        {
            character.transform.position = GetSpawnPosition();
        }
    }

    // Helper method to visualize spawn point in editor
    private void OnDrawGizmos()
    {
        if (sionSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(sionSpawnPoint.position + Vector3.up * spawnHeight, 1f);
            Gizmos.DrawLine(sionSpawnPoint.position, sionSpawnPoint.position + Vector3.up * spawnHeight);
        }
    }
}
