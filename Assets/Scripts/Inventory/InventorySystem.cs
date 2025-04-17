using UnityEngine;
using System.Collections.Generic;
using System;

namespace REcreationOfSpace.Inventory
{
    [Serializable]
    public class Item
    {
        public string id;
        public string name;
        public string description;
        public ItemType type;
        public ItemRarity rarity;
        public Sprite icon;
        public GameObject prefab;
        public int maxStackSize = 99;
        public float weight = 1f;
        public Dictionary<string, float> stats = new Dictionary<string, float>();
        public bool isQuestItem = false;
        public string[] tags;

        [NonSerialized] public Action<Item> onUse;
        [NonSerialized] public Action<Item> onEquip;
        [NonSerialized] public Action<Item> onUnequip;
    }

    public enum ItemType
    {
        Resource,
        Tool,
        Weapon,
        Armor,
        Consumable,
        QuestItem,
        Seed,
        Crafting
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Divine
    }

    [Serializable]
    public class InventorySlot
    {
        public Item item;
        public int quantity;
        public bool isLocked;

        public InventorySlot(Item item = null, int quantity = 0)
        {
            this.item = item;
            this.quantity = quantity;
            this.isLocked = false;
        }

        public bool CanAddItem(Item newItem, int amount = 1)
        {
            if (isLocked) return false;
            if (item == null) return true;
            if (item.id != newItem.id) return false;
            return quantity + amount <= item.maxStackSize;
        }
    }

    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 40;
        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private bool useWeightLimit = true;

        [Header("Equipment Slots")]
        [SerializeField] private int weaponSlots = 2;
        [SerializeField] private int armorSlots = 4;
        [SerializeField] private int accessorySlots = 3;
        [SerializeField] private int toolSlots = 4;

        private InventorySlot[] slots;
        private InventorySlot[] equipmentSlots;
        private float currentWeight;
        private Dictionary<string, Item> itemDatabase = new Dictionary<string, Item>();

        public event Action<Item, int, int> OnItemAdded;
        public event Action<Item, int, int> OnItemRemoved;
        public event Action<Item, int> OnItemEquipped;
        public event Action<Item, int> OnItemUnequipped;
        public event Action OnInventoryChanged;
        public event Action<float> OnWeightChanged;

        private void Awake()
        {
            InitializeInventory();
            LoadItemDatabase();
        }

        private void InitializeInventory()
        {
            // Initialize main inventory
            slots = new InventorySlot[maxSlots];
            for (int i = 0; i < maxSlots; i++)
            {
                slots[i] = new InventorySlot();
            }

            // Initialize equipment slots
            int totalEquipmentSlots = weaponSlots + armorSlots + accessorySlots + toolSlots;
            equipmentSlots = new InventorySlot[totalEquipmentSlots];
            for (int i = 0; i < totalEquipmentSlots; i++)
            {
                equipmentSlots[i] = new InventorySlot();
            }
        }

        private void LoadItemDatabase()
        {
            // Load items from Resources folder
            var items = Resources.LoadAll<ScriptableObject>("Items");
            foreach (var item in items)
            {
                if (item is ItemData itemData)
                {
                    var newItem = new Item
                    {
                        id = itemData.id,
                        name = itemData.name,
                        description = itemData.description,
                        type = itemData.type,
                        rarity = itemData.rarity,
                        icon = itemData.icon,
                        prefab = itemData.prefab,
                        maxStackSize = itemData.maxStackSize,
                        weight = itemData.weight,
                        isQuestItem = itemData.isQuestItem,
                        tags = itemData.tags
                    };

                    itemDatabase[itemData.id] = newItem;
                }
            }
        }

        public bool AddItem(string itemId, int amount = 1)
        {
            if (!itemDatabase.TryGetValue(itemId, out Item item))
                return false;

            return AddItem(item, amount);
        }

        public bool AddItem(Item item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            // Check weight limit
            if (useWeightLimit && currentWeight + (item.weight * amount) > maxWeight)
                return false;

            int remainingAmount = amount;
            List<int> affectedSlots = new List<int>();

            // First try to stack with existing items
            for (int i = 0; i < slots.Length && remainingAmount > 0; i++)
            {
                if (slots[i].item?.id == item.id && slots[i].CanAddItem(item, remainingAmount))
                {
                    int spaceInSlot = item.maxStackSize - slots[i].quantity;
                    int amountToAdd = Mathf.Min(remainingAmount, spaceInSlot);
                    
                    slots[i].quantity += amountToAdd;
                    remainingAmount -= amountToAdd;
                    affectedSlots.Add(i);
                }
            }

            // Then try to find empty slots for remaining items
            for (int i = 0; i < slots.Length && remainingAmount > 0; i++)
            {
                if (slots[i].item == null)
                {
                    int amountToAdd = Mathf.Min(remainingAmount, item.maxStackSize);
                    slots[i].item = item;
                    slots[i].quantity = amountToAdd;
                    remainingAmount -= amountToAdd;
                    affectedSlots.Add(i);
                }
            }

            if (remainingAmount < amount)
            {
                int addedAmount = amount - remainingAmount;
                currentWeight += item.weight * addedAmount;
                
                foreach (int slot in affectedSlots)
                {
                    OnItemAdded?.Invoke(item, slot, slots[slot].quantity);
                }
                
                OnInventoryChanged?.Invoke();
                OnWeightChanged?.Invoke(currentWeight);
                return remainingAmount == 0;
            }

            return false;
        }

        public bool RemoveItem(string itemId, int amount = 1)
        {
            if (!itemDatabase.TryGetValue(itemId, out Item item))
                return false;

            return RemoveItem(item, amount);
        }

        public bool RemoveItem(Item item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            int remainingAmount = amount;
            List<int> affectedSlots = new List<int>();

            // Remove items from slots
            for (int i = slots.Length - 1; i >= 0 && remainingAmount > 0; i--)
            {
                if (slots[i].item?.id == item.id)
                {
                    int amountToRemove = Mathf.Min(remainingAmount, slots[i].quantity);
                    slots[i].quantity -= amountToRemove;
                    remainingAmount -= amountToRemove;

                    if (slots[i].quantity <= 0)
                        slots[i].item = null;

                    affectedSlots.Add(i);
                }
            }

            if (remainingAmount < amount)
            {
                int removedAmount = amount - remainingAmount;
                currentWeight -= item.weight * removedAmount;

                foreach (int slot in affectedSlots)
                {
                    OnItemRemoved?.Invoke(item, slot, slots[slot].quantity);
                }

                OnInventoryChanged?.Invoke();
                OnWeightChanged?.Invoke(currentWeight);
                return remainingAmount == 0;
            }

            return false;
        }

        public bool EquipItem(int inventorySlot, int equipmentSlot)
        {
            if (inventorySlot < 0 || inventorySlot >= slots.Length ||
                equipmentSlot < 0 || equipmentSlot >= equipmentSlots.Length)
                return false;

            var sourceSlot = slots[inventorySlot];
            var targetSlot = equipmentSlots[equipmentSlot];

            if (sourceSlot.item == null || !CanEquipInSlot(sourceSlot.item, equipmentSlot))
                return false;

            // Unequip current item if any
            if (targetSlot.item != null)
            {
                UnequipItem(equipmentSlot);
            }

            // Move item to equipment slot
            targetSlot.item = sourceSlot.item;
            targetSlot.quantity = 1;
            
            // Remove one item from inventory
            sourceSlot.quantity--;
            if (sourceSlot.quantity <= 0)
                sourceSlot.item = null;

            // Trigger events
            OnItemEquipped?.Invoke(targetSlot.item, equipmentSlot);
            targetSlot.item.onEquip?.Invoke(targetSlot.item);
            OnInventoryChanged?.Invoke();

            return true;
        }

        public bool UnequipItem(int equipmentSlot)
        {
            if (equipmentSlot < 0 || equipmentSlot >= equipmentSlots.Length)
                return false;

            var slot = equipmentSlots[equipmentSlot];
            if (slot.item == null)
                return false;

            // Try to add item back to inventory
            if (AddItem(slot.item, slot.quantity))
            {
                var item = slot.item;
                slot.item = null;
                slot.quantity = 0;

                OnItemUnequipped?.Invoke(item, equipmentSlot);
                item.onUnequip?.Invoke(item);
                OnInventoryChanged?.Invoke();

                return true;
            }

            return false;
        }

        private bool CanEquipInSlot(Item item, int equipmentSlot)
        {
            int weaponEnd = weaponSlots;
            int armorEnd = weaponEnd + armorSlots;
            int accessoryEnd = armorEnd + accessorySlots;
            int toolEnd = accessoryEnd + toolSlots;

            if (equipmentSlot < weaponEnd)
                return item.type == ItemType.Weapon;
            else if (equipmentSlot < armorEnd)
                return item.type == ItemType.Armor;
            else if (equipmentSlot < accessoryEnd)
                return item.type == ItemType.Consumable;
            else if (equipmentSlot < toolEnd)
                return item.type == ItemType.Tool;

            return false;
        }

        public InventorySlot GetSlot(int index)
        {
            return (index >= 0 && index < slots.Length) ? slots[index] : null;
        }

        public InventorySlot GetEquipmentSlot(int index)
        {
            return (index >= 0 && index < equipmentSlots.Length) ? equipmentSlots[index] : null;
        }

        public int GetItemCount(string itemId)
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.item?.id == itemId)
                    count += slot.quantity;
            }
            return count;
        }

        public float GetCurrentWeight()
        {
            return currentWeight;
        }

        public float GetMaxWeight()
        {
            return maxWeight;
        }

        public void SetSlotLock(int slotIndex, bool locked)
        {
            if (slotIndex >= 0 && slotIndex < slots.Length)
            {
                slots[slotIndex].isLocked = locked;
            }
        }

        public Item GetItemFromDatabase(string itemId)
        {
            return itemDatabase.TryGetValue(itemId, out Item item) ? item : null;
        }
    }
}
