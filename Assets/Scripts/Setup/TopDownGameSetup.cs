using UnityEngine;

namespace REcreationOfSpace.Setup
{
    public class TopDownGameSetup : MonoBehaviour
    {
        [Header("Scene Setup")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 playerSpawnPoint = Vector3.zero;
        [SerializeField] private GameObject mainCameraPrefab;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -10);

        [Header("UI Setup")]
        [SerializeField] private GameObject uiCanvasPrefab;
        [SerializeField] private GameObject healthUIPrefab;
        [SerializeField] private GameObject resourceUIPrefab;
        [SerializeField] private GameObject teamUIPrefab;
        [SerializeField] private GameObject neuralNetworkUIPrefab;

        [Header("World Setup")]
        [SerializeField] private GameObject worldManagerPrefab;
        [SerializeField] private GameObject resourceSystemPrefab;
        [SerializeField] private int worldSeed = 0;
        [SerializeField] private bool randomizeSeed = true;

        private GameObject player;
        private GameObject mainCamera;
        private Canvas uiCanvas;

        private void Start()
        {
            if (randomizeSeed)
            {
                worldSeed = Random.Range(0, 999999);
            }
            Random.InitState(worldSeed);

            SetupWorld();
            SetupPlayer();
            SetupCamera();
            SetupUI();
        }

        private void SetupWorld()
        {
            // Create world manager
            if (worldManagerPrefab != null)
            {
                var worldManager = Instantiate(worldManagerPrefab);
                worldManager.name = "WorldManager";
            }

            // Create resource system
            if (resourceSystemPrefab != null)
            {
                var resourceSystem = Instantiate(resourceSystemPrefab);
                resourceSystem.name = "ResourceSystem";
            }
        }

        private void SetupPlayer()
        {
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, playerSpawnPoint, Quaternion.identity);
                player.name = "Player";
                player.tag = "Player";

                // Ensure required components
                if (!player.GetComponent<PlayerController>())
                {
                    player.AddComponent<PlayerController>();
                }

                if (!player.GetComponent<CombatController>())
                {
                    player.AddComponent<CombatController>();
                }

                if (!player.GetComponent<Health>())
                {
                    player.AddComponent<Health>();
                }

                if (!player.GetComponent<ExperienceManager>())
                {
                    player.AddComponent<ExperienceManager>();
                }

                if (!player.GetComponent<TeamSystem>())
                {
                    var teamSystem = player.AddComponent<TeamSystem>();
                    teamSystem.enabled = true;
                }
            }
        }

        private void SetupCamera()
        {
            if (mainCameraPrefab != null && player != null)
            {
                mainCamera = Instantiate(mainCameraPrefab);
                mainCamera.name = "MainCamera";

                var cameraController = mainCamera.GetComponent<CameraController>();
                if (cameraController != null)
                {
                    cameraController.SetTarget(player.transform);
                    cameraController.SetOffset(cameraOffset);
                }
            }
        }

        private void SetupUI()
        {
            if (uiCanvasPrefab != null)
            {
                var canvasObj = Instantiate(uiCanvasPrefab);
                canvasObj.name = "UICanvas";
                uiCanvas = canvasObj.GetComponent<Canvas>();

                if (uiCanvas != null)
                {
                    // Create health UI
                    if (healthUIPrefab != null)
                    {
                        var healthUI = Instantiate(healthUIPrefab, uiCanvas.transform);
                        healthUI.name = "HealthUI";
                    }

                    // Create resource UI
                    if (resourceUIPrefab != null)
                    {
                        var resourceUI = Instantiate(resourceUIPrefab, uiCanvas.transform);
                        resourceUI.name = "ResourceUI";
                    }

                    // Create team UI
                    if (teamUIPrefab != null)
                    {
                        var teamUI = Instantiate(teamUIPrefab, uiCanvas.transform);
                        teamUI.name = "TeamUI";
                    }

                    // Create neural network UI
                    if (neuralNetworkUIPrefab != null)
                    {
                        var nnUI = Instantiate(neuralNetworkUIPrefab, uiCanvas.transform);
                        nnUI.name = "NeuralNetworkUI";
                    }
                }
            }
        }

        public GameObject GetPlayer()
        {
            return player;
        }

        public GameObject GetMainCamera()
        {
            return mainCamera;
        }

        public Canvas GetUICanvas()
        {
            return uiCanvas;
        }

        public int GetWorldSeed()
        {
            return worldSeed;
        }
    }
}
