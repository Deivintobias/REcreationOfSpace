using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class GameSetup : EditorWindow
{
    [MenuItem("Game/Setup Game")]
    public static void ShowWindow()
    {
        GetWindow<GameSetup>("Game Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Setup", EditorStyles.boldLabel);

        if (GUILayout.Button("1. Create Materials"))
        {
            MaterialCreator.CreateBasicMaterials();
            Debug.Log("Materials created successfully!");
        }

        if (GUILayout.Button("2. Create Prefabs"))
        {
            PrefabCreator.CreateBasicPrefabs();
            Debug.Log("Prefabs created successfully!");
        }

        if (GUILayout.Button("3. Create Test Scene"))
        {
            CreateTestScene();
            Debug.Log("Test scene created successfully!");
        }

        if (GUILayout.Button("Do All Steps"))
        {
            MaterialCreator.CreateBasicMaterials();
            PrefabCreator.CreateBasicPrefabs();
            CreateTestScene();
            Debug.Log("All setup steps completed successfully!");
        }
    }

    private static void CreateTestScene()
    {
        // Create new scene
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create GameManager
        GameObject gameManager = new GameObject("GameManager");
        GameInitializer initializer = gameManager.AddComponent<GameInitializer>();

        // Assign prefabs
        initializer.playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPrefab.prefab");
        initializer.cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CameraPrefab.prefab");
        initializer.uiCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UICanvasPrefab.prefab");
        initializer.firstGuidePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FirstGuidePrefab.prefab");
        initializer.paradiseCityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ParadiseCityPrefab.prefab");

        // Add and setup WorldGenerator
        WorldGenerator worldGen = gameManager.AddComponent<WorldGenerator>();
        worldGen.worldSize = 1000;
        worldGen.chunkSize = 100;
        worldGen.maxHeight = 200f;
        initializer.worldGenerator = worldGen;

        // Add and setup LocalTerrainGenerator
        LocalTerrainGenerator terrainGen = gameManager.AddComponent<LocalTerrainGenerator>();
        terrainGen.localRadius = 50f;
        terrainGen.resolution = 100;
        initializer.terrainGenerator = terrainGen;

        // Add and setup CharacterGenerator
        CharacterGenerator charGen = gameManager.AddComponent<CharacterGenerator>();
        initializer.characterGenerator = charGen;

        // Add and setup CharacterVisualGenerator
        CharacterVisualGenerator visualGen = gameManager.AddComponent<CharacterVisualGenerator>();
        visualGen.characterBaseMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SinaiCharacter.mat");
        visualGen.glowMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Glow.mat");
        initializer.visualGenerator = visualGen;

        // Create directional light
        GameObject light = new GameObject("Directional Light");
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Create ground plane for testing
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "TemporaryGround";
        ground.transform.localScale = new Vector3(100f, 1f, 100f);
        ground.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Ground.mat");

        // Save scene
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/TestScene.unity");
    }
}
