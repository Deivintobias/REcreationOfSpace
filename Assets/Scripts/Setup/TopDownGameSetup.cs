using UnityEngine;
using UnityEngine.SceneManagement;

public class TopDownGameSetup : MonoBehaviour
{
    void Start()
    {
        CreateLayeredWorld();
    }

    void CreateLayeredWorld()
    {
        // Create world structure
        GameObject worldStructure = new GameObject("WorldStructure");
        worldStructure.AddComponent<WorldStructure>();

        // Create Paradise City (epicenter)
        CreateParadiseCity();

        // Create Sion's Crust (surface layer)
        CreateSionCrust();

        // Create player starting in Sion's Crust
        CreatePlayer(new Vector3(0, 1, 0));

        // Create basic world elements
        CreateLighting();
        CreateUI();
    }

    void CreateParadiseCity()
    {
        GameObject paradiseCity = new GameObject("ParadiseCity");
        paradiseCity.transform.position = new Vector3(0, -50, 0); // Below the crust
        
        // Add city components
        paradiseCity.AddComponent<ParadiseCity>();
        paradiseCity.AddComponent<RespawnManager>();

        // Create city structure
        GameObject cityStructure = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cityStructure.transform.SetParent(paradiseCity.transform);
        cityStructure.transform.localScale = new Vector3(100, 20, 100);
        
        // Set golden material
        Renderer renderer = cityStructure.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(1f, 0.8f, 0.4f);
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.4f) * 0.5f);
        }
    }

    void CreateSionCrust()
    {
        GameObject sionCrust = new GameObject("SionCrust");
        sionCrust.transform.position = Vector3.zero;

        // Add terrain components
        sionCrust.AddComponent<WorldGenerator>();
        sionCrust.AddComponent<LocalTerrainGenerator>();

        // Create resource nodes on the crust
        CreateCrustResources();

        // Create structures
        CreateFarmingArea(new Vector3(10, 0, 10));
        CreateFarmingArea(new Vector3(-10, 0, -10));
        CreateHomesteadArea(new Vector3(0, 0, 15));
        CreateMountSinai();
    }

    void CreateCrustResources()
    {
        // Create Wood nodes
        CreateResourceNode("Wood", new Vector3(8, 0, 8), Color.green);
        CreateResourceNode("Wood", new Vector3(-8, 0, -8), Color.green);

        // Create Crystal nodes
        CreateResourceNode("Crystal", new Vector3(15, 0, 0), Color.yellow);
        CreateResourceNode("Crystal", new Vector3(-15, 0, 0), Color.yellow);

        // Create Water nodes
        CreateResourceNode("Water", new Vector3(0, 0, 12), Color.blue);
        CreateResourceNode("Water", new Vector3(0, 0, -12), Color.blue);

        // Create Stone nodes
        CreateResourceNode("Stone", new Vector3(12, 0, -12), Color.gray);
        CreateResourceNode("Stone", new Vector3(-12, 0, 12), Color.gray);

        // Create Energy nodes (rare)
        CreateResourceNode("Energy", new Vector3(18, 0, 18), Color.cyan, true);
        CreateResourceNode("Energy", new Vector3(-18, 0, -18), Color.cyan, true);
    }

    void CreateMountSinai()
    {
        GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mountain.name = "MountSinai";
        mountain.transform.position = new Vector3(20, 10, 20);
        mountain.transform.localScale = new Vector3(10, 20, 10);
        
        // Add mountain components
        mountain.AddComponent<MountSinai>();
        mountain.AddComponent<MountainTranscendence>();

        // Set material
        Renderer renderer = mountain.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0.7f, 0.7f, 0.8f);
        }
    }

    void CreatePlayer(Vector3 position)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        player.name = "Player";
        player.transform.position = position;
        player.transform.localScale = new Vector3(1f, 0.5f, 1f);
        
        // Add player components
        player.AddComponent<PlayerController>();
        player.AddComponent<NeuralNetwork>();
        player.AddComponent<ExperienceManager>();
        player.AddComponent<Health>();
        player.AddComponent<SinaiCharacter>();
        player.AddComponent<SionObserver>();
        player.AddComponent<TradingSystem>();
        player.AddComponent<TeamSystem>();
        player.AddComponent<ResourceSystem>();

        // Create and setup camera
        CreateCamera(player.transform);
    }

    void CreateCamera(Transform target)
    {
        GameObject cameraObj = new GameObject("MainCamera");
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.AddComponent<AudioListener>();
        CameraController camController = cameraObj.AddComponent<CameraController>();
        
        // Position camera for top-down view
        cameraObj.transform.position = new Vector3(0, 20, -10);
        cameraObj.transform.rotation = Quaternion.Euler(60, 0, 0);
        camController.SetTarget(target);
    }

    void CreateLighting()
    {
        GameObject light = new GameObject("Directional Light");
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    void CreateUI()
    {
        GameObject canvas = new GameObject("UI Canvas");
        Canvas canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        CreateHealthUI(canvas);
        CreateNeuralNetworkUI(canvas);
        CreateTeamUI(canvas);
        CreateResourceUI(canvas);
        CreateMessagePanel(canvas);
        CreateScreenFade(canvas);
    }

    // Previous UI creation methods remain the same
    // ... (keeping existing UI creation code)
}
