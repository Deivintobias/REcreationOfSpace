using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject cameraPrefab;
    public GameObject uiCanvasPrefab;
    public GameObject firstGuidePrefab;
    public GameObject paradiseCityPrefab;

    [Header("World Generation")]
    public WorldGenerator worldGenerator;
    public LocalTerrainGenerator terrainGenerator;
    public CharacterGenerator characterGenerator;
    public CharacterVisualGenerator visualGenerator;

    [Header("Testing Options")]
    public bool startAsSinaiCharacter = true;
    public Vector3 startPosition = new Vector3(0, 1, 0);
    public bool spawnTestNPCs = true;
    public int numberOfTestNPCs = 5;

    private void Start()
    {
        SetupWorld();
        SetupPlayer();
        SetupNPCs();
        SetupUI();
    }

    private void SetupWorld()
    {
        // Initialize world generator
        if (worldGenerator == null)
        {
            worldGenerator = gameObject.AddComponent<WorldGenerator>();
        }
        worldGenerator.GenerateWorld();

        // Setup Mount Sinai
        GameObject mountain = new GameObject("Mount_Sinai");
        mountain.AddComponent<MountSinai>();
        mountain.AddComponent<MountainTranscendence>();

        // Setup Paradise City
        if (paradiseCityPrefab != null)
        {
            Instantiate(paradiseCityPrefab, Vector3.zero, Quaternion.identity);
        }

        // Setup First Guide
        if (firstGuidePrefab != null)
        {
            Instantiate(firstGuidePrefab, new Vector3(10, 0, 10), Quaternion.identity);
        }

        // Setup terrain generator
        if (terrainGenerator == null)
        {
            terrainGenerator = gameObject.AddComponent<LocalTerrainGenerator>();
        }
    }

    private void SetupPlayer()
    {
        // Create player
        GameObject player = Instantiate(playerPrefab, startPosition, Quaternion.identity);
        
        // Add required components
        player.AddComponent<PlayerController>();
        player.AddComponent<NeuralNetwork>();
        player.AddComponent<ExperienceManager>();
        player.AddComponent<Health>();

        // Setup character type specific components
        if (startAsSinaiCharacter)
        {
            player.AddComponent<SinaiCharacter>();
            player.AddComponent<SionObserver>();
        }

        // Generate visuals
        if (visualGenerator != null)
        {
            visualGenerator.GenerateCharacterVisuals(player, !startAsSinaiCharacter);
        }

        // Setup camera
        if (cameraPrefab != null)
        {
            GameObject camera = Instantiate(cameraPrefab);
            CameraController cameraController = camera.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.SetTarget(player.transform);
            }
        }

        // Initialize terrain around player
        if (terrainGenerator != null)
        {
            terrainGenerator.Initialize(player.transform);
        }
    }

    private void SetupNPCs()
    {
        if (!spawnTestNPCs) return;

        for (int i = 0; i < numberOfTestNPCs; i++)
        {
            // Determine NPC type
            bool isSionCharacter = Random.value > 0.3f; // 70% Sion, 30% Sinai

            // Generate position
            Vector3 randomPos = Random.insideUnitSphere * 20f;
            randomPos.y = 1f;

            // Create NPC
            GameObject npc = new GameObject(isSionCharacter ? "Sion_NPC" : "Sinai_NPC");
            npc.transform.position = randomPos;

            // Add components
            npc.AddComponent<CharacterController>();
            npc.AddComponent<NeuralNetwork>();
            npc.AddComponent<ExperienceManager>();
            npc.AddComponent<Health>();

            if (isSionCharacter)
            {
                npc.AddComponent<DeceptionSystem>();
            }
            else
            {
                npc.AddComponent<SinaiCharacter>();
                npc.AddComponent<SionObserver>();
            }

            // Generate visuals
            if (visualGenerator != null)
            {
                visualGenerator.GenerateCharacterVisuals(npc, isSionCharacter);
            }
        }
    }

    private void SetupUI()
    {
        if (uiCanvasPrefab != null)
        {
            GameObject canvas = Instantiate(uiCanvasPrefab);
            
            // Add UI components
            canvas.AddComponent<GuiderMessageUI>();
            canvas.AddComponent<HealthUI>();
            canvas.AddComponent<NeuralNetworkUI>();
            canvas.AddComponent<PortalPromptUI>();
            canvas.AddComponent<ScreenFade>();
        }
    }
}
