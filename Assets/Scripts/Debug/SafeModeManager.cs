using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace REcreationOfSpace.Debug
{
    public class SafeModeManager : MonoBehaviour
    {
        [Header("Safe Mode UI")]
        [SerializeField] private GameObject safeModeCanvas;
        [SerializeField] private TextMeshProUGUI diagnosticsText;
        [SerializeField] private Button startNormalButton;
        [SerializeField] private Button startSafeButton;
        [SerializeField] private Button exitButton;

        [Header("Diagnostic Settings")]
        [SerializeField] private bool checkScriptErrors = true;
        [SerializeField] private bool checkMissingReferences = true;
        [SerializeField] private bool checkPerformance = true;
        [SerializeField] private bool checkGraphicsCapabilities = true;

        private List<string> diagnosticResults = new List<string>();
        private bool isInSafeMode = false;

        private void Awake()
        {
            // Check if we're starting in safe mode
            isInSafeMode = PlayerPrefs.GetInt("SafeMode", 0) == 1;

            if (isInSafeMode || Application.isEditor)
            {
                InitializeSafeMode();
            }
            else
            {
                StartNormalMode();
            }
        }

        private void InitializeSafeMode()
        {
            // Create UI if not assigned
            if (safeModeCanvas == null)
            {
                CreateSafeModeUI();
            }

            // Run diagnostics
            RunDiagnostics();

            // Setup button listeners
            if (startNormalButton != null)
                startNormalButton.onClick.AddListener(StartNormalMode);
            if (startSafeButton != null)
                startSafeButton.onClick.AddListener(StartSafeMode);
            if (exitButton != null)
                exitButton.onClick.AddListener(() => Application.Quit());

            // Show results
            UpdateDiagnosticsDisplay();
        }

        private void CreateSafeModeUI()
        {
            safeModeCanvas = new GameObject("SafeModeCanvas");
            var canvas = safeModeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            safeModeCanvas.AddComponent<CanvasScaler>();
            safeModeCanvas.AddComponent<GraphicRaycaster>();

            // Create background panel
            var panel = CreateUIPanel(safeModeCanvas.transform, "Background");
            var image = panel.GetComponent<Image>();
            image.color = new Color(0, 0, 0, 0.9f);

            // Create title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "SAFE MODE";
            titleText.fontSize = 36;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.yellow;

            // Create diagnostics text area
            var diagObj = new GameObject("Diagnostics");
            diagObj.transform.SetParent(panel.transform, false);
            diagnosticsText = diagObj.AddComponent<TextMeshProUGUI>();
            diagnosticsText.fontSize = 16;
            diagnosticsText.alignment = TextAlignmentOptions.TopLeft;
            diagnosticsText.color = Color.white;

            // Create buttons
            startNormalButton = CreateButton(panel.transform, "Start Normal Mode", new Vector2(0, -150));
            startSafeButton = CreateButton(panel.transform, "Start Safe Mode", new Vector2(0, -200));
            exitButton = CreateButton(panel.transform, "Exit", new Vector2(0, -250));
        }

        private GameObject CreateUIPanel(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var image = obj.AddComponent<Image>();
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return obj;
        }

        private Button CreateButton(Transform parent, string text, Vector2 position)
        {
            var obj = new GameObject(text + "Button");
            obj.transform.SetParent(parent, false);
            
            var button = obj.AddComponent<Button>();
            var image = obj.AddComponent<Image>();
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 40);
            rect.anchoredPosition = position;

            return button;
        }

        private void RunDiagnostics()
        {
            diagnosticResults.Clear();
            diagnosticResults.Add("=== Diagnostic Results ===\n");

            if (checkScriptErrors)
                CheckScriptErrors();
            if (checkMissingReferences)
                CheckMissingReferences();
            if (checkPerformance)
                CheckPerformance();
            if (checkGraphicsCapabilities)
                CheckGraphicsCapabilities();

            // Add system info
            diagnosticResults.Add("\n=== System Information ===");
            diagnosticResults.Add($"OS: {SystemInfo.operatingSystem}");
            diagnosticResults.Add($"CPU: {SystemInfo.processorType}");
            diagnosticResults.Add($"GPU: {SystemInfo.graphicsDeviceName}");
            diagnosticResults.Add($"Memory: {SystemInfo.systemMemorySize} MB");
            diagnosticResults.Add($"Unity Version: {Application.unityVersion}");
        }

        private void CheckScriptErrors()
        {
            diagnosticResults.Add("\n=== Script Diagnostics ===");
            var components = FindObjectsOfType<MonoBehaviour>();
            int errorCount = 0;

            foreach (var component in components)
            {
                if (component == null)
                {
                    errorCount++;
                    diagnosticResults.Add($"Missing script on {component.gameObject.name}");
                }
            }

            if (errorCount == 0)
                diagnosticResults.Add("No script errors found");
            else
                diagnosticResults.Add($"Found {errorCount} script errors");
        }

        private void CheckMissingReferences()
        {
            diagnosticResults.Add("\n=== Reference Checks ===");
            var objects = FindObjectsOfType<GameObject>();
            int missingCount = 0;

            foreach (var obj in objects)
            {
                var components = obj.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component == null)
                    {
                        missingCount++;
                        diagnosticResults.Add($"Missing component on {obj.name}");
                    }
                }
            }

            if (missingCount == 0)
                diagnosticResults.Add("No missing references found");
            else
                diagnosticResults.Add($"Found {missingCount} missing references");
        }

        private void CheckPerformance()
        {
            diagnosticResults.Add("\n=== Performance Check ===");
            
            // Check system requirements
            bool meetsRequirements = true;
            
            if (SystemInfo.systemMemorySize < 4096) // 4GB minimum
            {
                diagnosticResults.Add("WARNING: Low system memory");
                meetsRequirements = false;
            }

            if (SystemInfo.processorFrequency < 2000) // 2GHz minimum
            {
                diagnosticResults.Add("WARNING: CPU may be too slow");
                meetsRequirements = false;
            }

            if (meetsRequirements)
                diagnosticResults.Add("System meets minimum requirements");
        }

        private void CheckGraphicsCapabilities()
        {
            diagnosticResults.Add("\n=== Graphics Capabilities ===");

            if (!SystemInfo.supportsComputeShaders)
                diagnosticResults.Add("WARNING: Compute shaders not supported");

            if (!SystemInfo.supports3DTextures)
                diagnosticResults.Add("WARNING: 3D textures not supported");

            if (SystemInfo.graphicsMemorySize < 1024)
                diagnosticResults.Add("WARNING: Low graphics memory");

            diagnosticResults.Add($"Graphics API: {SystemInfo.graphicsDeviceType}");
            diagnosticResults.Add($"Max Texture Size: {SystemInfo.maxTextureSize}");
        }

        private void UpdateDiagnosticsDisplay()
        {
            if (diagnosticsText != null)
            {
                diagnosticsText.text = string.Join("\n", diagnosticResults);
            }
        }

        private void StartNormalMode()
        {
            PlayerPrefs.SetInt("SafeMode", 0);
            PlayerPrefs.Save();
            
            // Disable safe mode features
            if (safeModeCanvas != null)
                safeModeCanvas.SetActive(false);

            // Load main scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void StartSafeMode()
        {
            PlayerPrefs.SetInt("SafeMode", 1);
            PlayerPrefs.Save();
            
            // Enable safe mode features
            isInSafeMode = true;
            
            // Apply safe mode settings
            QualitySettings.SetQualityLevel(0); // Lowest quality
            QualitySettings.shadowDistance = 0;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.masterTextureLimit = 2; // Lower resolution textures
            
            // Disable intensive features
            DisableIntensiveFeatures();

            // Load main scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void DisableIntensiveFeatures()
        {
            // Disable post processing
            var postProcessing = FindObjectOfType<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
            if (postProcessing != null)
                postProcessing.enabled = false;

            // Reduce particle systems
            var particles = FindObjectsOfType<ParticleSystem>();
            foreach (var ps in particles)
            {
                var main = ps.main;
                main.maxParticles = Mathf.Min(main.maxParticles, 100);
            }

            // Disable real-time shadows
            var lights = FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                light.shadows = LightShadows.None;
            }
        }

        public bool IsInSafeMode()
        {
            return isInSafeMode;
        }

        public List<string> GetDiagnosticResults()
        {
            return diagnosticResults;
        }
    }
}
