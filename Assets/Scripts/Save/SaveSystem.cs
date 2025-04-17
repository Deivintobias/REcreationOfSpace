using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json;

namespace REcreationOfSpace.Save
{
    public class SaveSystem : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string saveFolder = "Saves";
        [SerializeField] private string fileExtension = ".save";
        [SerializeField] private bool useEncryption = true;
        [SerializeField] private bool useCompression = true;
        [SerializeField] private int maxSaveSlots = 5;
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes

        private string savePath;
        private float nextAutoSave;
        private Dictionary<Type, ISaveable> saveables = new Dictionary<Type, ISaveable>();
        private static readonly string encryptionKey = "REcreationOfSpace_SaveKey";

        public event Action<string> OnSaveStarted;
        public event Action<string> OnSaveCompleted;
        public event Action<string> OnLoadStarted;
        public event Action<string> OnLoadCompleted;
        public event Action<string> OnSaveError;
        public event Action<string> OnLoadError;

        private void Awake()
        {
            InitializeSaveSystem();
        }

        private void InitializeSaveSystem()
        {
            // Set up save directory
            savePath = Path.Combine(Application.persistentDataPath, saveFolder);
            Directory.CreateDirectory(savePath);

            // Find all saveable components
            FindSaveables();

            // Set up auto-save
            if (autoSave)
            {
                nextAutoSave = Time.time + autoSaveInterval;
            }
        }

        private void Update()
        {
            if (autoSave && Time.time >= nextAutoSave)
            {
                AutoSave();
                nextAutoSave = Time.time + autoSaveInterval;
            }
        }

        private void FindSaveables()
        {
            var saveableComponents = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
            foreach (var saveable in saveableComponents)
            {
                saveables[saveable.GetType()] = saveable;
            }
        }

        public void SaveGame(string slotName)
        {
            try
            {
                OnSaveStarted?.Invoke(slotName);

                var saveData = new SaveData
                {
                    saveTime = DateTime.Now,
                    version = Application.version,
                    gameData = new Dictionary<string, object>()
                };

                // Collect data from all saveable components
                foreach (var saveable in saveables.Values)
                {
                    var data = saveable.Save();
                    if (data != null)
                    {
                        saveData.gameData[saveable.GetType().Name] = data;
                    }
                }

                // Convert to JSON
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);

                // Compress if enabled
                if (useCompression)
                {
                    json = CompressString(json);
                }

                // Encrypt if enabled
                if (useEncryption)
                {
                    json = EncryptString(json, encryptionKey);
                }

                // Write to file
                string filePath = GetSaveFilePath(slotName);
                File.WriteAllText(filePath, json);

                OnSaveCompleted?.Invoke(slotName);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving game: {e.Message}");
                OnSaveError?.Invoke(e.Message);
            }
        }

        public void LoadGame(string slotName)
        {
            try
            {
                OnLoadStarted?.Invoke(slotName);

                string filePath = GetSaveFilePath(slotName);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Save file not found: {slotName}");
                }

                string json = File.ReadAllText(filePath);

                // Decrypt if enabled
                if (useEncryption)
                {
                    json = DecryptString(json, encryptionKey);
                }

                // Decompress if enabled
                if (useCompression)
                {
                    json = DecompressString(json);
                }

                // Parse save data
                var saveData = JsonConvert.DeserializeObject<SaveData>(json);

                // Verify version compatibility
                if (saveData.version != Application.version)
                {
                    Debug.LogWarning($"Save file version mismatch. Expected {Application.version}, got {saveData.version}");
                }

                // Load data into components
                foreach (var saveable in saveables.Values)
                {
                    string typeName = saveable.GetType().Name;
                    if (saveData.gameData.ContainsKey(typeName))
                    {
                        saveable.Load(saveData.gameData[typeName]);
                    }
                }

                OnLoadCompleted?.Invoke(slotName);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading game: {e.Message}");
                OnLoadError?.Invoke(e.Message);
            }
        }

        public SaveSlotInfo[] GetSaveSlots()
        {
            var slots = new List<SaveSlotInfo>();
            var files = Directory.GetFiles(savePath, $"*{fileExtension}");

            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    string slotName = Path.GetFileNameWithoutExtension(file);
                    string json = File.ReadAllText(file);

                    if (useEncryption)
                        json = DecryptString(json, encryptionKey);
                    if (useCompression)
                        json = DecompressString(json);

                    var saveData = JsonConvert.DeserializeObject<SaveData>(json);

                    slots.Add(new SaveSlotInfo
                    {
                        name = slotName,
                        saveTime = saveData.saveTime,
                        version = saveData.version,
                        fileSize = info.Length
                    });
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error reading save slot {file}: {e.Message}");
                }
            }

            return slots.OrderByDescending(s => s.saveTime).ToArray();
        }

        public void DeleteSave(string slotName)
        {
            string filePath = GetSaveFilePath(slotName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private void AutoSave()
        {
            SaveGame("AutoSave");
        }

        private string GetSaveFilePath(string slotName)
        {
            return Path.Combine(savePath, $"{slotName}{fileExtension}");
        }

        private string CompressString(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new System.IO.Compression.GZipStream(mso, System.IO.Compression.CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        private string DecompressString(string input)
        {
            var bytes = Convert.FromBase64String(input);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new System.IO.Compression.GZipStream(msi, System.IO.Compression.CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return System.Text.Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        private string EncryptString(string input, string key)
        {
            byte[] inputArray = System.Text.Encoding.UTF8.GetBytes(input);
            var tripleDES = new System.Security.Cryptography.TripleDESCryptoServiceProvider
            {
                Key = System.Text.Encoding.UTF8.GetBytes(key.PadRight(24, '0')),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            var cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        private string DecryptString(string input, string key)
        {
            byte[] inputArray = Convert.FromBase64String(input);
            var tripleDES = new System.Security.Cryptography.TripleDESCryptoServiceProvider
            {
                Key = System.Text.Encoding.UTF8.GetBytes(key.PadRight(24, '0')),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            var cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return System.Text.Encoding.UTF8.GetString(resultArray);
        }
    }

    [Serializable]
    public class SaveData
    {
        public DateTime saveTime;
        public string version;
        public Dictionary<string, object> gameData;
    }

    public class SaveSlotInfo
    {
        public string name;
        public DateTime saveTime;
        public string version;
        public long fileSize;
    }

    public interface ISaveable
    {
        object Save();
        void Load(object data);
    }
}
