using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialSetup : MonoBehaviour
{
    void Start()
    {
        // Create basic scene
        CreateBasicScene();
    }

    void CreateBasicScene()
    {
        // Create player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1, 0);
        
        // Add player components
        player.AddComponent<PlayerController>();
        player.AddComponent<NeuralNetwork>();
        player.AddComponent<ExperienceManager>();
        player.AddComponent<Health>();
        player.AddComponent<SinaiCharacter>();
        player.AddComponent<SionObserver>();

        // Create camera
        GameObject cameraObj = new GameObject("MainCamera");
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.AddComponent<AudioListener>();
        CameraController camController = cameraObj.AddComponent<CameraController>();
        cameraObj.transform.position = new Vector3(0, 5, -10);
        camController.SetTarget(player.transform);

        // Create light
        GameObject light = new GameObject("Directional Light");
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Create ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10);

        // Create First Guide
        GameObject guide = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guide.name = "FirstGuide";
        guide.transform.position = new Vector3(5, 1, 5);
        guide.AddComponent<FirstGuide>();
        guide.AddComponent<FirstGuideEffects>();

        // Create UI
        GameObject canvas = new GameObject("UI Canvas");
        Canvas canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Add UI components
        GameObject messagePanel = new GameObject("MessagePanel");
        messagePanel.transform.SetParent(canvas.transform);
        messagePanel.AddComponent<RectTransform>();
        messagePanel.AddComponent<GuiderMessageUI>();

        // Create basic world
        GameObject world = new GameObject("World");
        world.AddComponent<WorldGenerator>();
        world.AddComponent<LocalTerrainGenerator>();

        // Create Mount Sinai
        GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mountain.name = "MountSinai";
        mountain.transform.position = new Vector3(20, 10, 20);
        mountain.transform.localScale = new Vector3(10, 20, 10);
        mountain.AddComponent<MountSinai>();
        mountain.AddComponent<MountainTranscendence>();

        // Create Paradise City
        GameObject city = GameObject.CreatePrimitive(PrimitiveType.Cube);
        city.name = "ParadiseCity";
        city.transform.position = new Vector3(-20, 5, -20);
        city.transform.localScale = new Vector3(10, 10, 10);
        city.AddComponent<ParadiseCity>();

        Debug.Log("Basic scene created successfully!");
    }
}
