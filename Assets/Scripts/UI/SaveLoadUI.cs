using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using REcreationOfSpace.Save;

namespace REcreationOfSpace.UI
{
    public class SaveLoadUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject saveLoadPanel;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_InputField newSlotInput;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("UI Settings")]
        [SerializeField] private Color selectedSlotColor = new Color(0.8f, 0.8f, 1f);
        [SerializeField] private float messageDisplayTime = 3f;
        [SerializeField] private string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        private SaveSystem saveSystem;
        private SaveSlotButton selectedSlot;
        private float messageTimer;

        private void Start()
        {
            saveSystem = FindObjectOfType<SaveSystem>();
            if (saveSystem == null)
            {
                Debug.LogError("No SaveSystem found!");
                return;
            }

            InitializeUI();
            SubscribeToEvents();
            RefreshSlotList();

            // Hide panel initially
            saveLoadPanel.SetActive(false);
        }

        private void InitializeUI()
        {
            // Setup buttons
            saveButton.onClick.AddListener(OnSaveClicked);
            loadButton.onClick.AddListener(OnLoadClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            closeButton.onClick.AddListener(() => saveLoadPanel.SetActive(false));

            // Disable action buttons initially
            UpdateButtonStates();
        }

        private void SubscribeToEvents()
        {
            saveSystem.OnSaveStarted += OnSaveStarted;
            saveSystem.OnSaveCompleted += OnSaveCompleted;
            saveSystem.OnLoadStarted += OnLoadStarted;
            saveSystem.OnLoadCompleted += OnLoadCompleted;
            saveSystem.OnSaveError += OnSaveError;
            saveSystem.OnLoadError += OnLoadError;
        }

        private void Update()
        {
            // Update message display
            if (messageTimer > 0)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0)
                {
                    messageText.text = "";
                }
            }
        }

        public void Show(bool isSaveMode = true)
        {
            saveLoadPanel.SetActive(true);
            RefreshSlotList();
            saveButton.gameObject.SetActive(isSaveMode);
            loadButton.gameObject.SetActive(!isSaveMode);
            newSlotInput.gameObject.SetActive(isSaveMode);
        }

        private void RefreshSlotList()
        {
            // Clear existing slots
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }

            // Get save slots
            var slots = saveSystem.GetSaveSlots();

            // Create slot buttons
            foreach (var slotInfo in slots)
            {
                CreateSlotButton(slotInfo);
            }

            selectedSlot = null;
            UpdateButtonStates();
        }

        private void CreateSlotButton(SaveSlotInfo slotInfo)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            var slotButton = slotObj.AddComponent<SaveSlotButton>();
            slotButton.Initialize(slotInfo);

            // Setup button visuals
            var buttonText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{slotInfo.name}\n{slotInfo.saveTime.ToString(dateTimeFormat)}\nSize: {FormatFileSize(slotInfo.fileSize)}";
            }

            // Add click listener
            var button = slotObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnSlotSelected(slotButton));
        }

        private void OnSlotSelected(SaveSlotButton slot)
        {
            if (selectedSlot != null)
            {
                selectedSlot.GetComponent<Image>().color = Color.white;
            }

            selectedSlot = slot;
            slot.GetComponent<Image>().color = selectedSlotColor;

            UpdateButtonStates();
        }

        private void OnSaveClicked()
        {
            string slotName = string.IsNullOrEmpty(newSlotInput.text) ? 
                             $"Save_{System.DateTime.Now:yyyyMMdd_HHmmss}" : 
                             newSlotInput.text;

            saveSystem.SaveGame(slotName);
            newSlotInput.text = "";
        }

        private void OnLoadClicked()
        {
            if (selectedSlot != null)
            {
                saveSystem.LoadGame(selectedSlot.SlotInfo.name);
            }
        }

        private void OnDeleteClicked()
        {
            if (selectedSlot != null)
            {
                saveSystem.DeleteSave(selectedSlot.SlotInfo.name);
                RefreshSlotList();
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = selectedSlot != null;
            loadButton.interactable = hasSelection;
            deleteButton.interactable = hasSelection;
        }

        private void ShowMessage(string message, bool isError = false)
        {
            messageText.text = message;
            messageText.color = isError ? Color.red : Color.white;
            messageTimer = messageDisplayTime;
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {suffixes[order]}";
        }

        #region Save System Event Handlers

        private void OnSaveStarted(string slotName)
        {
            ShowMessage($"Saving game to {slotName}...");
        }

        private void OnSaveCompleted(string slotName)
        {
            ShowMessage($"Game saved to {slotName}");
            RefreshSlotList();
        }

        private void OnLoadStarted(string slotName)
        {
            ShowMessage($"Loading game from {slotName}...");
        }

        private void OnLoadCompleted(string slotName)
        {
            ShowMessage($"Game loaded from {slotName}");
            saveLoadPanel.SetActive(false);
        }

        private void OnSaveError(string error)
        {
            ShowMessage($"Save Error: {error}", true);
        }

        private void OnLoadError(string error)
        {
            ShowMessage($"Load Error: {error}", true);
        }

        #endregion

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (saveSystem != null)
            {
                saveSystem.OnSaveStarted -= OnSaveStarted;
                saveSystem.OnSaveCompleted -= OnSaveCompleted;
                saveSystem.OnLoadStarted -= OnLoadStarted;
                saveSystem.OnLoadCompleted -= OnLoadCompleted;
                saveSystem.OnSaveError -= OnSaveError;
                saveSystem.OnLoadError -= OnLoadError;
            }
        }
    }

    public class SaveSlotButton : MonoBehaviour
    {
        public SaveSlotInfo SlotInfo { get; private set; }

        public void Initialize(SaveSlotInfo slotInfo)
        {
            SlotInfo = slotInfo;
        }
    }
}
