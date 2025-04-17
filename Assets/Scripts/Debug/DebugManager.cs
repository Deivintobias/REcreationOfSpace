using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace REcreationOfSpace.Debug
{
    public class DebugManager : MonoBehaviour
    {
        [Header("Debug UI")]
        [SerializeField] private GameObject debugCanvas;
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI playerInfoText;
        [SerializeField] private TextMeshProUGUI worldInfoText;
        [SerializeField] private TextMeshProUGUI systemInfoText;
        [SerializeField] private Toggle debugModeToggle;

        [Header("Performance")]
        [SerializeField] private float fpsUpdateInterval = 0.5f;
        [SerializeField] private int targetFrameRate = 60;

        [Header("Debug Settings")]
        [SerializeField] private bool showColliders = false;
        [SerializeField] private bool showNavMesh = false;
        [SerializeField] private bool logSystemMessages = true;
        [SerializeField] private KeyCode debugToggleKey = KeyCode.F3;

        private float fpsAccumulator = 0f;
        private int framesAccumulated = 0;
        private float timeUntilNextFPSUpdate = 0f;
        private Queue<string> debugLog = new Queue<string>();
        private int maxLogEntries = 50;

        private PlayerController player;
        private WorldManager worldManager;
        private BiomeManager biomeManager;
        private Timeline timeline;
        private GraphicsManager graphicsManager;

        private void Start()
        {
            Application.targetFrameRate = targetFrameRate;
            InitializeDebugUI();
            FindGameComponents();
            
            // Subscribe to debug events
            Application.logMessageReceived += HandleLog;
        }

        private void InitializeDebugUI()
        {
            if (debugCanvas == null)
            {
                CreateDebugCanvas();
            }

            debugCanvas.SetActive(false);
        }

        private void CreateDebugCanvas()
        {
            debugCanvas = new GameObject("DebugCanvas");
            var canvas = debugCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Always on top
            debugCanvas.AddComponent<CanvasScaler>();
            debugCanvas.AddComponent<GraphicRaycaster>();

            // Create debug panels
            CreateDebugPanel("FPS", out fpsText);
            CreateDebugPanel("Player Info", out playerInfoText);
            CreateDebugPanel("World Info", out worldInfoText);
            CreateDebugPanel("System Info", out systemInfoText);

            // Create toggle
            var toggleObj = new GameObject("DebugToggle");
            toggleObj.transform.SetParent(debugCanvas.transform, false);
            debugModeToggle = toggleObj.AddComponent<Toggle>();
            debugModeToggle.onValueChanged.AddListener(OnDebugModeToggled);
        }

        private void CreateDebugPanel(string title, out TextMeshProUGUI textComponent)
        {
            var panel = new GameObject(title + "Panel");
            panel.transform.SetParent(debugCanvas.transform, false);

            var text = new GameObject("Text");
            text.transform.SetParent(panel.transform, false);
            textComponent = text.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = 16;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.TopLeft;
        }

        private void FindGameComponents()
        {
            player = FindObjectOfType<PlayerController>();
            worldManager = FindObjectOfType<WorldManager>();
            biomeManager = FindObjectOfType<BiomeManager>();
            timeline = FindObjectOfType<Timeline>();
            graphicsManager = FindObjectOfType<GraphicsManager>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(debugToggleKey))
            {
                ToggleDebugMode();
            }

            if (debugCanvas.activeSelf)
            {
                UpdateFPS();
                UpdateDebugInfo();
            }
        }

        private void UpdateFPS()
        {
            fpsAccumulator += Time.unscaledDeltaTime;
            framesAccumulated++;
            timeUntilNextFPSUpdate -= Time.unscaledDeltaTime;

            if (timeUntilNextFPSUpdate <= 0)
            {
                float fps = framesAccumulated / fpsAccumulator;
                fpsText.text = $"FPS: {fps:F1}";
                fpsText.color = fps < 30 ? Color.red : (fps < 50 ? Color.yellow : Color.green);

                fpsAccumulator = 0f;
                framesAccumulated = 0;
                timeUntilNextFPSUpdate = fpsUpdateInterval;
            }
        }

        private void UpdateDebugInfo()
        {
            if (player != null)
            {
                playerInfoText.text = GetPlayerDebugInfo();
            }

            if (worldManager != null)
            {
                worldInfoText.text = GetWorldDebugInfo();
            }

            systemInfoText.text = GetSystemDebugInfo();
        }

        private string GetPlayerDebugInfo()
        {
            return $"Player Info:\n" +
                   $"Position: {player.transform.position}\n" +
                   $"Health: {player.GetComponent<Health>()?.GetCurrentHealth() ?? 0}\n" +
                   $"Menu Open: {player.IsMenuOpen()}\n";
        }

        private string GetWorldDebugInfo()
        {
            var currentEra = timeline?.GetCurrentEra();
            var currentBiome = biomeManager?.GetBiomeAt(player.transform.position, player.transform.position.y);

            return $"World Info:\n" +
                   $"Current Era: {currentEra?.name ?? "Unknown"}\n" +
                   $"Current Year: {timeline?.GetCurrentYear() ?? 0}\n" +
                   $"Current Biome: {currentBiome?.name ?? "Unknown"}\n" +
                   $"Active Chunks: {worldManager?.GetType().GetField("chunks")?.GetValue(worldManager)?.ToString() ?? "0"}\n";
        }

        private string GetSystemDebugInfo()
        {
            return $"System Info:\n" +
                   $"Memory: {(System.GC.GetTotalMemory(false) / 1048576f):F1} MB\n" +
                   $"Quality Level: {QualitySettings.GetQualityLevel()}\n" +
                   $"Screen: {Screen.width}x{Screen.height}\n" +
                   $"Platform: {Application.platform}\n" +
                   $"Unity Version: {Application.unityVersion}\n";
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!logSystemMessages) return;

            string logEntry = $"[{System.DateTime.Now:HH:mm:ss}] {type}: {logString}";
            debugLog.Enqueue(logEntry);

            while (debugLog.Count > maxLogEntries)
            {
                debugLog.Dequeue();
            }

            UpdateSystemLog();
        }

        private void UpdateSystemLog()
        {
            if (systemInfoText != null)
            {
                systemInfoText.text = string.Join("\n", debugLog);
            }
        }

        private void ToggleDebugMode()
        {
            debugCanvas.SetActive(!debugCanvas.activeSelf);
            debugModeToggle.isOn = debugCanvas.activeSelf;

            if (showColliders)
            {
                ToggleColliderVisibility(debugCanvas.activeSelf);
            }

            if (showNavMesh)
            {
                ToggleNavMeshVisibility(debugCanvas.activeSelf);
            }
        }

        private void OnDebugModeToggled(bool isOn)
        {
            debugCanvas.SetActive(isOn);
        }

        private void ToggleColliderVisibility(bool show)
        {
            var colliders = FindObjectsOfType<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.GetComponent<MeshRenderer>() == null)
                {
                    var debugVisual = collider.gameObject.GetComponent<DebugColliderVisual>();
                    if (show)
                    {
                        if (debugVisual == null)
                        {
                            debugVisual = collider.gameObject.AddComponent<DebugColliderVisual>();
                        }
                        debugVisual.ShowCollider();
                    }
                    else if (debugVisual != null)
                    {
                        Destroy(debugVisual);
                    }
                }
            }
        }

        private void ToggleNavMeshVisibility(bool show)
        {
            // Implementation depends on your navigation system
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }
    }

    public class DebugColliderVisual : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        public void ShowCollider()
        {
            var collider = GetComponent<Collider>();
            if (collider == null) return;

            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.yellow;
                lineRenderer.endColor = Color.yellow;
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
            }

            if (collider is BoxCollider)
            {
                DrawBoxCollider(collider as BoxCollider);
            }
            else if (collider is SphereCollider)
            {
                DrawSphereCollider(collider as SphereCollider);
            }
        }

        private void DrawBoxCollider(BoxCollider boxCollider)
        {
            Vector3[] points = new Vector3[8];
            Vector3 center = boxCollider.center;
            Vector3 size = boxCollider.size;

            points[0] = transform.TransformPoint(center + Vector3.Scale(size * 0.5f, new Vector3(-1, -1, -1)));
            points[1] = transform.TransformPoint(center + Vector3.Scale(size * 0.5f, new Vector3(1, -1, -1)));
            points[2] = transform.TransformPoint(center + Vector3.Scale(size * 0.5f, new Vector3(1, -1, 1)));
            points[3] = transform.TransformPoint(center + Vector3.Scale(size * 0.5f, new Vector3(-1, -1, 1)));
            points[4] = transform.TransformPoint(center + Vector3.Scale(size * 0.5f, new Vector3(-1, 1, -1)));
            points[5] = transform.TransformPoint(center + Vector3.Scale(size * 0.5f, new Vector3(1, 1, -1)));
            points[6] = transform.TransformPoint(center + Vector3.Scale(size * 0.5f, new Vector3(1, 1, 1)));
            points[7] = transform.TransformPoint(center + Vector3.Scale(size * 0.5f, new Vector3(-1, 1, 1)));

            lineRenderer.positionCount = 16;
            lineRenderer.SetPositions(new Vector3[] {
                points[0], points[1], points[1], points[2], points[2], points[3], points[3], points[0],
                points[4], points[5], points[5], points[6], points[6], points[7], points[7], points[4]
            });
        }

        private void DrawSphereCollider(SphereCollider sphereCollider)
        {
            int segments = 32;
            lineRenderer.positionCount = segments + 1;

            float radius = sphereCollider.radius;
            Vector3 center = transform.TransformPoint(sphereCollider.center);

            for (int i = 0; i <= segments; i++)
            {
                float angle = ((float)i / segments) * 360f * Mathf.Deg2Rad;
                Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                lineRenderer.SetPosition(i, pos);
            }
        }
    }
}
