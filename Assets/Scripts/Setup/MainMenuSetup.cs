using UnityEngine;

namespace REcreationOfSpace.Setup
{
    public class MainMenuSetup : MonoBehaviour
    {
        [Header("Scene Setup")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Light mainLight;
        [SerializeField] private GameObject backgroundObject;

        [Header("Menu Setup")]
        [SerializeField] private GameObject gameInitializerPrefab;

        private void Awake()
        {
            SetupScene();
            SetupMenus();
        }

        private void SetupScene()
        {
            // Create main camera if not assigned
            if (mainCamera == null)
            {
                var cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
                mainCamera.transform.position = new Vector3(0, 1, -10);
                mainCamera.transform.LookAt(Vector3.zero);
            }

            // Create main light if not assigned
            if (mainLight == null)
            {
                var lightObj = new GameObject("Directional Light");
                mainLight = lightObj.AddComponent<Light>();
                mainLight.type = LightType.Directional;
                mainLight.intensity = 1f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Create background if not assigned
            if (backgroundObject == null)
            {
                CreateDefaultBackground();
            }
        }

        private void CreateDefaultBackground()
        {
            // Create a simple background plane
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Background";
            plane.transform.position = Vector3.zero;
            plane.transform.localScale = new Vector3(10, 1, 10);

            // Create and assign material
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.2f, 0.3f, 0.4f); // Dark blue-gray
            plane.GetComponent<Renderer>().material = material;

            backgroundObject = plane;
        }

        private void SetupMenus()
        {
            // Create game initializer if not assigned
            if (gameInitializerPrefab != null)
            {
                Instantiate(gameInitializerPrefab);
            }
            else
            {
                var initializerObj = new GameObject("GameInitializer");
                initializerObj.AddComponent<GameInitializer>();
            }

            // Set up event system if needed
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void OnValidate()
        {
            // Ensure required components are assigned
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainLight == null)
                mainLight = FindObjectOfType<Light>();
        }
    }
}
