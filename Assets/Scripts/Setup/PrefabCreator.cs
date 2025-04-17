using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

public class PrefabCreator : MonoBehaviour
{
    public static void CreateBasicPrefabs()
    {
        CreateDirectories();
        CreatePlayerPrefab();
        CreateCameraPrefab();
        CreateUICanvasPrefab();
        CreateFirstGuidePrefab();
        CreateParadiseCityPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateDirectories()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
    }

    private static void CreatePlayerPrefab()
    {
        GameObject player = new GameObject("PlayerPrefab");
        
        // Add required components
        player.AddComponent<CharacterController>();
        CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.5f;
        
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Add visual placeholder
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.SetParent(player.transform);
        visual.transform.localPosition = Vector3.zero;
        DestroyImmediate(visual.GetComponent<Collider>());

        // Save prefab
        string prefabPath = "Assets/Prefabs/PlayerPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
        DestroyImmediate(player);
    }

    private static void CreateCameraPrefab()
    {
        GameObject camera = new GameObject("CameraPrefab");
        
        // Add components
        Camera cam = camera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = Color.black;
        cam.fieldOfView = 60f;
        
        camera.AddComponent<AudioListener>();
        camera.AddComponent<CameraController>();

        // Save prefab
        string prefabPath = "Assets/Prefabs/CameraPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(camera, prefabPath);
        DestroyImmediate(camera);
    }

    private static void CreateUICanvasPrefab()
    {
        GameObject canvas = new GameObject("UICanvasPrefab");
        
        // Add canvas components
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        // Create basic UI elements
        CreateHealthUI(canvas);
        CreateNeuralNetworkUI(canvas);
        CreateMessagePanel(canvas);
        CreatePortalPrompt(canvas);
        CreateScreenFade(canvas);

        // Save prefab
        string prefabPath = "Assets/Prefabs/UICanvasPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
        DestroyImmediate(canvas);
    }

    private static void CreateHealthUI(GameObject parent)
    {
        GameObject healthUI = new GameObject("HealthUI");
        healthUI.transform.SetParent(parent.transform);
        
        RectTransform rect = healthUI.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(100, -50);
        rect.sizeDelta = new Vector2(200, 30);

        healthUI.AddComponent<HealthUI>();
    }

    private static void CreateNeuralNetworkUI(GameObject parent)
    {
        GameObject networkUI = new GameObject("NeuralNetworkUI");
        networkUI.transform.SetParent(parent.transform);
        
        RectTransform rect = networkUI.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-100, -50);
        rect.sizeDelta = new Vector2(200, 30);

        networkUI.AddComponent<NeuralNetworkUI>();
    }

    private static void CreateMessagePanel(GameObject parent)
    {
        GameObject messagePanel = new GameObject("MessagePanel");
        messagePanel.transform.SetParent(parent.transform);
        
        RectTransform rect = messagePanel.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 100);
        rect.sizeDelta = new Vector2(400, 100);

        messagePanel.AddComponent<GuiderMessageUI>();
    }

    private static void CreatePortalPrompt(GameObject parent)
    {
        GameObject portalPrompt = new GameObject("PortalPrompt");
        portalPrompt.transform.SetParent(parent.transform);
        
        RectTransform rect = portalPrompt.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(300, 50);

        portalPrompt.AddComponent<PortalPromptUI>();
    }

    private static void CreateScreenFade(GameObject parent)
    {
        GameObject fadePanel = new GameObject("ScreenFade");
        fadePanel.transform.SetParent(parent.transform);
        
        RectTransform rect = fadePanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image image = fadePanel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);

        fadePanel.AddComponent<ScreenFade>();
    }

    private static void CreateFirstGuidePrefab()
    {
        GameObject guide = new GameObject("FirstGuidePrefab");
        
        // Add visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.transform.SetParent(guide.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(1f, 2f, 1f);
        DestroyImmediate(visual.GetComponent<Collider>());

        // Add components
        guide.AddComponent<FirstGuide>();
        guide.AddComponent<FirstGuideEffects>();

        // Save prefab
        string prefabPath = "Assets/Prefabs/FirstGuidePrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(guide, prefabPath);
        DestroyImmediate(guide);
    }

    private static void CreateParadiseCityPrefab()
    {
        GameObject city = new GameObject("ParadiseCityPrefab");
        
        // Add basic city structure
        GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);
        structure.transform.SetParent(city.transform);
        structure.transform.localPosition = Vector3.zero;
        structure.transform.localScale = new Vector3(20f, 10f, 20f);
        DestroyImmediate(structure.GetComponent<Collider>());

        // Add components
        city.AddComponent<ParadiseCity>();

        // Save prefab
        string prefabPath = "Assets/Prefabs/ParadiseCityPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(city, prefabPath);
        DestroyImmediate(city);
    }
}
#endif
