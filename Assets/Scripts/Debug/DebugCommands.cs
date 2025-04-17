using UnityEngine;
using System.Collections.Generic;
using System;

namespace REcreationOfSpace.Debug
{
    public class DebugCommands : MonoBehaviour
    {
        private static Dictionary<string, Action<string[]>> commands;
        private string inputField = "";
        private Vector2 scrollPosition;
        private bool showConsole = false;
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;

        private PlayerController player;
        private WorldManager worldManager;
        private Timeline timeline;
        private BiomeManager biomeManager;
        private GraphicsManager graphicsManager;

        private void Start()
        {
            InitializeCommands();
            FindGameComponents();
        }

        private void InitializeCommands()
        {
            commands = new Dictionary<string, Action<string[]>>
            {
                // Player commands
                { "tp", TeleportCommand },
                { "heal", HealCommand },
                { "god", GodModeCommand },
                { "speed", SpeedCommand },

                // World commands
                { "time", TimeCommand },
                { "weather", WeatherCommand },
                { "biome", BiomeCommand },
                { "spawn", SpawnCommand },

                // System commands
                { "fps", FPSCommand },
                { "memory", MemoryCommand },
                { "reload", ReloadCommand },
                { "clear", ClearCommand },
                { "help", HelpCommand }
            };
        }

        private void FindGameComponents()
        {
            player = FindObjectOfType<PlayerController>();
            worldManager = FindObjectOfType<WorldManager>();
            timeline = FindObjectOfType<Timeline>();
            biomeManager = FindObjectOfType<BiomeManager>();
            graphicsManager = FindObjectOfType<GraphicsManager>();
        }

        private void OnGUI()
        {
            if (!showConsole) return;

            // Console background
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height * 0.3f), "Debug Console");

            // Command history
            Rect historyRect = new Rect(5, 20, Screen.width - 10, Screen.height * 0.3f - 50);
            scrollPosition = GUI.BeginScrollView(historyRect, scrollPosition, 
                new Rect(0, 0, Screen.width - 30, commandHistory.Count * 20));
            
            for (int i = 0; i < commandHistory.Count; i++)
            {
                GUI.Label(new Rect(5, i * 20, Screen.width - 40, 20), commandHistory[i]);
            }
            
            GUI.EndScrollView();

            // Input field
            GUI.SetNextControlName("CommandInput");
            inputField = GUI.TextField(new Rect(5, Screen.height * 0.3f - 25, Screen.width - 10, 20), inputField);

            if (GUI.GetNameOfFocusedControl() == "CommandInput")
            {
                HandleInputNavigation();
            }
        }

        private void HandleInputNavigation()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ExecuteCommand(inputField);
                inputField = "";
                historyIndex = -1;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (historyIndex < commandHistory.Count - 1)
                {
                    historyIndex++;
                    inputField = commandHistory[commandHistory.Count - 1 - historyIndex];
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (historyIndex > 0)
                {
                    historyIndex--;
                    inputField = commandHistory[commandHistory.Count - 1 - historyIndex];
                }
                else if (historyIndex == 0)
                {
                    historyIndex = -1;
                    inputField = "";
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                showConsole = !showConsole;
                if (showConsole)
                {
                    GUI.FocusControl("CommandInput");
                }
            }
        }

        private void ExecuteCommand(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            commandHistory.Add("> " + input);
            string[] parts = input.Split(' ');
            string command = parts[0].ToLower();
            string[] args = new string[parts.Length - 1];
            Array.Copy(parts, 1, args, 0, parts.Length - 1);

            if (commands.ContainsKey(command))
            {
                try
                {
                    commands[command](args);
                }
                catch (Exception e)
                {
                    LogError($"Error executing command: {e.Message}");
                }
            }
            else
            {
                LogError($"Unknown command: {command}");
            }
        }

        #region Command Implementations

        private void TeleportCommand(string[] args)
        {
            if (args.Length != 3 || !float.TryParse(args[0], out float x) ||
                !float.TryParse(args[1], out float y) || !float.TryParse(args[2], out float z))
            {
                LogError("Usage: tp <x> <y> <z>");
                return;
            }

            if (player != null)
            {
                player.transform.position = new Vector3(x, y, z);
                Log($"Teleported to {x}, {y}, {z}");
            }
        }

        private void HealCommand(string[] args)
        {
            if (player != null)
            {
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.Heal(float.MaxValue);
                    Log("Player healed");
                }
            }
        }

        private void GodModeCommand(string[] args)
        {
            if (player != null)
            {
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.SetInvulnerable(!health.IsInvulnerable());
                    Log($"God mode {(health.IsInvulnerable() ? "enabled" : "disabled")}");
                }
            }
        }

        private void SpeedCommand(string[] args)
        {
            if (args.Length != 1 || !float.TryParse(args[0], out float speed))
            {
                LogError("Usage: speed <multiplier>");
                return;
            }

            if (player != null)
            {
                // Assuming moveSpeed is accessible
                var field = player.GetType().GetField("moveSpeed", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(player, speed);
                    Log($"Player speed set to {speed}");
                }
            }
        }

        private void TimeCommand(string[] args)
        {
            if (args.Length != 1 || !int.TryParse(args[0], out int year))
            {
                LogError("Usage: time <year>");
                return;
            }

            if (timeline != null)
            {
                timeline.SetCurrentEra(year);
                Log($"Time set to year {year}");
            }
        }

        private void WeatherCommand(string[] args)
        {
            if (args.Length != 1)
            {
                LogError("Usage: weather <type> (rain/snow/dust/clear)");
                return;
            }

            if (graphicsManager != null)
            {
                string type = args[0].ToLower();
                bool enable = type != "clear";
                graphicsManager.SetWeatherEffect(type, enable);
                Log($"Weather set to {type}");
            }
        }

        private void BiomeCommand(string[] args)
        {
            if (args.Length != 1)
            {
                LogError("Usage: biome <name>");
                return;
            }

            // This would require additional implementation to change biomes
            Log($"Biome command not fully implemented");
        }

        private void SpawnCommand(string[] args)
        {
            if (args.Length < 1)
            {
                LogError("Usage: spawn <prefabName> [count]");
                return;
            }

            int count = args.Length > 1 && int.TryParse(args[1], out int c) ? c : 1;
            string prefabName = args[0];

            // This would require access to your prefab system
            Log($"Spawn command not fully implemented");
        }

        private void FPSCommand(string[] args)
        {
            if (args.Length != 1 || !int.TryParse(args[0], out int targetFPS))
            {
                LogError("Usage: fps <target>");
                return;
            }

            Application.targetFrameRate = targetFPS;
            Log($"Target FPS set to {targetFPS}");
        }

        private void MemoryCommand(string[] args)
        {
            float memoryUsage = System.GC.GetTotalMemory(false) / 1048576f;
            Log($"Memory usage: {memoryUsage:F2} MB");
        }

        private void ReloadCommand(string[] args)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void ClearCommand(string[] args)
        {
            commandHistory.Clear();
            scrollPosition = Vector2.zero;
        }

        private void HelpCommand(string[] args)
        {
            Log("Available commands:");
            foreach (var command in commands.Keys)
            {
                Log($"  {command}");
            }
        }

        #endregion

        private void Log(string message)
        {
            commandHistory.Add(message);
            Debug.Log(message);
        }

        private void LogError(string message)
        {
            commandHistory.Add($"Error: {message}");
            Debug.LogError(message);
        }
    }
}
