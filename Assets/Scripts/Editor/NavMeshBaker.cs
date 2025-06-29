using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.AI;

public class NavMeshBaker : EditorWindow
{
    [MenuItem("Tools/REcreationOfSpace/Bake NavMesh for Active Scene")]
    public static void BakeActiveSceneNavMesh()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Cannot bake NavMesh while in Play Mode. Please exit Play Mode and try again.", "OK");
            return;
        }

        var activeScene = EditorSceneManager.GetActiveScene();
        if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.path))
        {
            EditorUtility.DisplayDialog("Error", "No active scene found or scene is not saved. Please open and save a scene.", "OK");
            return;
        }

        Debug.Log($"Starting NavMesh bake for scene: {activeScene.name}");

        // Get NavMesh build settings. You might want to customize these.
        // For simplicity, we'll use default settings or allow Unity to use its current settings.
        // NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0); // Get default agent settings

        // Mark all static GameObjects with MeshRenderer or Terrain as Navigation Static
        // This is a broad approach; ideally, you'd have specific layers or tags.
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        int markedStaticCount = 0;
        foreach (GameObject go in allObjects)
        {
            if (go.isStatic) // Check if the GameObject itself is marked static
            {
                if (go.GetComponent<MeshRenderer>() != null || go.GetComponent<Terrain>() != null)
                {
                    if (!GameObjectUtility.AreStaticEditorFlagsSet(go, StaticEditorFlags.NavigationStatic))
                    {
                        GameObjectUtility.SetStaticEditorFlags(go, GameObjectUtility.GetStaticEditorFlags(go) | StaticEditorFlags.NavigationStatic);
                        markedStaticCount++;
                    }
                }
            }
        }
        Debug.Log($"Marked {markedStaticCount} GameObjects as Navigation Static if they weren't already.");

        // Trigger the NavMesh build process.
        // This uses the settings configured in the Navigation window (Window > AI > Navigation).
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        Debug.Log("NavMesh baking process initiated. Check the Navigation window for progress and results.");
        EditorUtility.DisplayDialog("NavMesh Baking", "NavMesh baking process initiated. This might take a few moments. Check the console and Navigation window for details.", "OK");
    }

    // Optional: A method to clear existing NavMesh data if needed
    [MenuItem("Tools/REcreationOfSpace/Clear NavMesh for Active Scene")]
    public static void ClearActiveSceneNavMesh()
    {
         if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Cannot clear NavMesh while in Play Mode.", "OK");
            return;
        }
        // NavMesh.RemoveAllNavMeshData(); // This is an older API
        NavMeshBuilder.ClearAllNavMeshes(); // For current NavMeshSurface based systems
        Debug.Log("Cleared all NavMesh data for the active scene's NavMeshSurfaces (if any). If using built-in NavMesh, rebake to clear.");
        EditorUtility.DisplayDialog("NavMesh Clear", "Attempted to clear NavMesh data. If you are using NavMeshSurface components, their data will be cleared. For the built-in NavMesh, you typically just rebake.", "OK");
    }
}
