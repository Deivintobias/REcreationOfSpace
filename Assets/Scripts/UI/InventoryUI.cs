using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using REcreationOfSpace.Inventory;

namespace REcreationOfSpace.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform inventoryContent;
        [SerializeField] private Transform equipmentContent;
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private TextMeshProUGUI weightText;
        [SerializeField] private Image weightBar;

        [Header("UI Settings")]
        [SerializeField] private Color normalSlotColor = Color.white;
        [SerializeField] private Color lockedSlotColor = Color.gray;
        [SerializeField] private Color selectedSlotColor = Color.yellow;
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private float tooltipOffset = 10f;

        private InventorySystem inventory;
        private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
        private List<InventorySlotUI> equipmentSlots = new List<InventorySlotUI>();
        private InventorySlotUI selectedSlot;
        private InventorySlotUI hoveredSlot;

        private void Start()
        {
            inventory = FindObjectOfType<InventorySystem>();
            if (inventory == null)
            {
                Debug.LogError("No InventorySystem found!");
                return;
            }

            InitializeUI();
            SubscribeToEvents();
            UpdateUI();

            // Hide UI initially
            inventoryPanel.SetActive(false);
            tooltipPanel.SetActive(false);
        }

        private void InitializeUI()
        {
            // Create inventory slots
            for (int i = 0; i < inventory.GetType().GetField("maxSlots").GetValue(inventory); i++)
            {
                CreateInventorySlot(i);
            }

            // Create equipment slots
            int weaponSlots = (int)inventory.GetType().GetField("weaponSlots").GetValue(inventory);
            int armorSlots = (int)inventory.GetType().GetField("armorSlots").GetValue(inventory);
            int accessorySlots = (int)inventory.GetType().GetField("accessorySlots").GetValue(inventory);
            int toolSlots = (int)inventory.GetType().GetField("toolSlots").GetValue(inventory);

            CreateEquipmentSection("Weapons", weaponSlots, 0);
            CreateEquipmentSection("Armor", armorSlots, weaponSlots);
            CreateEquipmentSection("Accessories", accessorySlots, weaponSlots + armorSlots);
            CreateEquipmentSection("Tools", toolSlots, weaponSlots + armorSlots + accessorySlots);
        }

        private void CreateInventorySlot(int index)
        {
            GameObject slotObj = Instantiate(slotPrefab, inventoryContent);
            var slotUI = slotObj.AddComponent<InventorySlotUI>();
            slotUI.Initialize(index, false);
            
            // Add event listeners
            var button = slotObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnSlotClicked(slotUI));

            var eventTrigger = slotObj.AddComponent<EventTrigger>();
            var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            pointerEnter.callback.AddListener((data) => OnSlotHovered(slotUI));
            eventTrigger.triggers.Add(pointerEnter);

            var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            pointerExit.callback.AddListener((data) => OnSlotUnhovered(slotUI));
            eventTrigger.triggers.Add(pointerExit);

            inventorySlots.Add(slotUI);
        }

        private void CreateEquipmentSection(string title, int slotCount, int startIndex)
        {
            // Create section header
            var headerObj = new GameObject(title + "Header");
            headerObj.transform.SetParent(equipmentContent, false);
            var headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = title;
            headerText.fontSize = 16;
            headerText.alignment = TextAlignmentOptions.Center;

            // Create slots
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, equipmentContent);
                var slotUI = slotObj.AddComponent<InventorySlotUI>();
                slotUI.Initialize(startIndex + i, true);

                var button = slotObj.GetComponent<Button>();
                button.onClick.AddListener(() => OnSlotClicked(slotUI));

                var eventTrigger = slotObj.AddComponent<EventTrigger>();
                var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                pointerEnter.callback.AddListener((data) => OnSlotHovered(slotUI));
                eventTrigger.triggers.Add(pointerEnter);

                var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                pointerExit.callback.AddListener((data) => OnSlotUnhovered(slotUI));
                eventTrigger.triggers.Add(pointerExit);

                equipmentSlots.Add(slotUI);
            }
        }

        private void SubscribeToEvents()
        {
            inventory.OnItemAdded += (item, slot, quantity) => UpdateUI();
            inventory.OnItemRemoved += (item, slot, quantity) => UpdateUI();
            inventory.OnItemEquipped += (item, slot) => UpdateUI();
            inventory.OnItemUnequipped += (item, slot) => UpdateUI();
            inventory.OnWeightChanged += UpdateWeight;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleInventory();
            }

            if (tooltipPanel.activeSelf)
            {
                UpdateTooltipPosition();
            }
        }

        private void UpdateUI()
        {
            // Update inventory slots
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                var slot = inventory.GetSlot(i);
                UpdateSlotUI(inventorySlots[i], slot);
            }

            // Update equipment slots
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                var slot = inventory.GetEquipmentSlot(i);
                UpdateSlotUI(equipmentSlots[i], slot);
            }

            UpdateWeight(inventory.GetCurrentWeight());
        }

        private void UpdateSlotUI(InventorySlotUI slotUI, InventorySlot slot)
        {
            if (slot == null) return;

            var image = slotUI.GetComponent<Image>();
            var iconImage = slotUI.transform.Find("Icon")?.GetComponent<Image>();
            var quantityText = slotUI.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();

            // Update slot appearance
            image.color = slot.isLocked ? lockedSlotColor : 
                         (slotUI == selectedSlot ? selectedSlotColor : normalSlotColor);

            // Update item display
            if (slot.item != null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = slot.item.icon;
                    iconImage.enabled = true;
                }
                if (quantityText != null)
                {
                    quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
                    quantityText.enabled = true;
                }
            }
            else
            {
                if (iconImage != null)
                    iconImage.enabled = false;
                if (quantityText != null)
                    quantityText.enabled = false;
            }
        }

        private void UpdateWeight(float currentWeight)
        {
            float maxWeight = inventory.GetMaxWeight();
            float weightPercentage = currentWeight / maxWeight;

            weightText.text = $"Weight: {currentWeight:F1} / {maxWeight:F1}";
            weightBar.fillAmount = weightPercentage;
            weightBar.color = Color.Lerp(Color.green, Color.red, weightPercentage);
        }

        private void OnSlotClicked(InventorySlotUI clickedSlot)
        {
            if (selectedSlot == null)
            {
                // Select slot if it has an item
                if (GetSlotItem(clickedSlot) != null)
                {
                    selectedSlot = clickedSlot;
                    UpdateUI();
                }
            }
            else
            {
                // Try to move/equip item
                if (clickedSlot.isEquipmentSlot)
                {
                    inventory.EquipItem(selectedSlot.index, clickedSlot.index);
                }
                else
                {
                    // Implement item stacking/swapping logic
                }

                // Deselect
                selectedSlot = null;
                UpdateUI();
            }
        }

        private void OnSlotHovered(InventorySlotUI slot)
        {
            hoveredSlot = slot;
            var item = GetSlotItem(slot);
            if (item != null)
            {
                tooltipText.text = item.GetTooltipText();
                tooltipPanel.SetActive(true);
                UpdateTooltipPosition();
            }
        }

        private void OnSlotUnhovered(InventorySlotUI slot)
        {
            if (hoveredSlot == slot)
            {
                hoveredSlot = null;
                tooltipPanel.SetActive(false);
            }
        }

        private void UpdateTooltipPosition()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 tooltipPos = mousePos + Vector2.right * tooltipOffset;

            // Keep tooltip on screen
            var tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            var canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            if (tooltipPos.x + tooltipRect.rect.width > canvasRect.rect.width)
                tooltipPos.x = mousePos.x - tooltipRect.rect.width - tooltipOffset;
            if (tooltipPos.y + tooltipRect.rect.height > canvasRect.rect.height)
                tooltipPos.y = mousePos.y - tooltipRect.rect.height;

            tooltipPanel.transform.position = tooltipPos;
        }

        private Item GetSlotItem(InventorySlotUI slotUI)
        {
            if (slotUI.isEquipmentSlot)
                return inventory.GetEquipmentSlot(slotUI.index)?.item;
            else
                return inventory.GetSlot(slotUI.index)?.item;
        }

        public void ToggleInventory()
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            if (!inventoryPanel.activeSelf)
            {
                selectedSlot = null;
                tooltipPanel.SetActive(false);
            }
            UpdateUI();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (inventory != null)
            {
                var type = inventory.GetType();
                var eventField = type.GetField("OnItemAdded");
                if (eventField != null)
                    eventField.SetValue(inventory, null);

                eventField = type.GetField("OnItemRemoved");
                if (eventField != null)
                    eventField.SetValue(inventory, null);

                eventField = type.GetField("OnItemEquipped");
                if (eventField != null)
                    eventField.SetValue(inventory, null);

                eventField = type.GetField("OnItemUnequipped");
                if (eventField != null)
                    eventField.SetValue(inventory, null);

                eventField = type.GetField("OnWeightChanged");
                if (eventField != null)
                    eventField.SetValue(inventory, null);
            }
        }
    }

    public class InventorySlotUI : MonoBehaviour
    {
        public int index;
        public bool isEquipmentSlot;

        public void Initialize(int slotIndex, bool isEquipment)
        {
            index = slotIndex;
            isEquipmentSlot = isEquipment;
        }
    }
}
