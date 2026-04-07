using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventorySlot
{
    public ItemSO item; // Will convert to gameobject
    public int quantity;
}

public class EntityInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int primaryInventorySlotsAmount;

    private InventorySlot[] primaryInventorySlots;
    private List<InventorySlot> secondaryInventorySlots = new List<InventorySlot>();

    private UnityEvent onInventoryEquippedChangeSubscribers = new UnityEvent();

    private int activeSlotIndex = 0;
    public int GetActiveSlotIndex() { return activeSlotIndex; }
    public int GetPreviousSlotIndex() { return (activeSlotIndex - 1 + primaryInventorySlotsAmount) % primaryInventorySlotsAmount; }
    public int GetNextSlotIndex() { return (activeSlotIndex + 1) % primaryInventorySlotsAmount; }


    void Awake()
    {
        // Initialize the inventory items list with empty inventory slots
        primaryInventorySlots = new InventorySlot[primaryInventorySlotsAmount];
        for (int i = 0; i < primaryInventorySlotsAmount; i++)
            primaryInventorySlots[i] = new InventorySlot { item = null, quantity = 0 };
    }

    public void AddItemToAvailablePrimarySlot(ItemSO item, int amount)
    {
        if (item == null) return;

        // Check if there is room in the inventory
        if (primaryInventorySlotsAmount == 0) return;

        bool isRoomInInventory = false;
        int? firstAvailableSlotIndex = null;
        for (int i = 0; i < primaryInventorySlotsAmount; i++)
        {
            if (primaryInventorySlots[i].item == null)
            {
                isRoomInInventory = true;
                firstAvailableSlotIndex = i;
                break;
            }
        }

        // If there is no room, return
        if (!isRoomInInventory) return;

        // Add the item to the first available slot
        primaryInventorySlots[firstAvailableSlotIndex.Value].item = item;
        primaryInventorySlots[firstAvailableSlotIndex.Value].quantity = amount;

        // If the active slot is the one we just added to, invoke the change equipped item event
        if (activeSlotIndex == firstAvailableSlotIndex.Value)
            onInventoryEquippedChangeSubscribers.Invoke();
        // If the active slot is empty, set the active slot to the one we just added to
        else if (primaryInventorySlots[activeSlotIndex].item == null)
            SetActivePrimarySlot(firstAvailableSlotIndex.Value);
    }

    public void SetActivePrimarySlot(int slotIndex)
    {
        // Ensure the slot index is within the valid range
        if (slotIndex < 0 || slotIndex >= primaryInventorySlotsAmount)
            return;

        // If previous slot is different than new slot
        bool changedEquippedItem = activeSlotIndex != slotIndex;

        // Set the active slot index
        activeSlotIndex = slotIndex;

        // Invoke the change equipped item event
        if (changedEquippedItem) onInventoryEquippedChangeSubscribers.Invoke();
    }

    public InventorySlot GetActivePrimarySlot()
    {
        return primaryInventorySlots[activeSlotIndex];
    }

    public void SubscribeToInventoryEquippedChange(UnityAction listener)
    { onInventoryEquippedChangeSubscribers.AddListener(listener); }

    public void UnsubscribeFromInventoryEquippedChange(UnityAction listener)
    { onInventoryEquippedChangeSubscribers.RemoveListener(listener); }
}
