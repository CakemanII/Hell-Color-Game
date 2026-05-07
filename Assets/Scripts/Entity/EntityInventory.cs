using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;
using UnityEngine.Events;

public class SlotContent
{
    public ItemSO item; // Will convert to gameobject
    public int quantity;
}

public enum InventoryType
{
    Primary,
    Secondary
}

public class EntityInventory : MonoBehaviour
{
    [SerializeField] private int maxPrimaryInventorySlots = 8;
    public int MaxPrimaryInventorySlots => maxPrimaryInventorySlots;
    [SerializeField] private int maxSecondaryInventorySlots = 30;
    public int MaxSecondaryInventorySlots => maxSecondaryInventorySlots;

    private List<SlotContent> primaryInventorySlots = new List<SlotContent>();
    private List<SlotContent> secondaryInventorySlots = new List<SlotContent>();

    // This is used to store content that is being moved around, EX: When the mouse picks something up.
    private SlotContent floatingSlotContent;

    private UnityEvent onInventoryEquippedChangeSubscribers = new UnityEvent();
    private UnityEvent onInventoryChangeSubscribers = new UnityEvent();
    private UnityEvent onInventoryFloatingSlotChangeSubscribers = new UnityEvent();

    #region Primary Inventory
    private int activeSlotIndex = 0;
    public int GetActivePrimarySlotIndex() { return activeSlotIndex; }
    public int GetPreviousPrimarySlotIndex() { return (activeSlotIndex - 1 + primaryInventorySlots.Count) % primaryInventorySlots.Count; }
    public int GetNextPrimarySlotIndex() { return (activeSlotIndex + 1) % primaryInventorySlots.Count; }

    public void SetActivePrimarySlot(int slotIndex)
    {
        // Ensure the slot index is within the valid range
        if (slotIndex < 0 || slotIndex >= primaryInventorySlots.Count)
            return;

        // If previous slot is different than new slot
        bool changedEquippedItem = activeSlotIndex != slotIndex;

        // Set the active slot index
        activeSlotIndex = slotIndex;

        // Invoke the change equipped item event
        if (changedEquippedItem) onInventoryEquippedChangeSubscribers.Invoke();
    }
    #endregion

    private void DropItem(ItemSO item, int amount)
    {

    }

    #region Essential Inventory Functions
    /// <summary>
    /// Sets the amount of inventory slots for the specified inventory type.
    /// </summary>
    private void SetInventorySlotsAmount(int amount, InventoryType inventoryType)
    {
        // Ensure amount is valid
        if (amount < 0) return;

        // Ensure the amount does not exceed max bounds
        if (amount > (inventoryType == InventoryType.Primary ? maxPrimaryInventorySlots : maxSecondaryInventorySlots)) return;

        // Get the current amount of slots
        List<SlotContent> currentInventorySlots = inventoryType == InventoryType.Primary ? primaryInventorySlots : secondaryInventorySlots; // Linked List

        int inventorySlotsAmount = currentInventorySlots.Count;
        if (amount == currentInventorySlots.Count) return;

        // Add slots if increasing the amount, remove slots if decreasing the amount
        if (amount > inventorySlotsAmount)
       {
            for (int _ = 0; _ < inventorySlotsAmount - amount; _++)
                currentInventorySlots.Add(new SlotContent { item = null, quantity = 0 });
       }
       else
       {
            // Remove extra slots if decreasing the amount and keep track of removed items.
            List<SlotContent> removedSlots = new List<SlotContent>();
            for (int i = inventorySlotsAmount - 1; i >= amount; i--)
            {
                removedSlots.Add(currentInventorySlots[i]);
                currentInventorySlots.RemoveAt(i);
            }

            // Attempt to stack removed items in existing slots or empty slots, drop the remaining.
            foreach (SlotContent removedSlot in removedSlots)
            {
                if (removedSlot.item != null)
                {
                    int amountRemaining = AttemptAddToBothInventory(removedSlot.item, removedSlot.quantity, inventoryType).quantity;
                    if (amountRemaining > 0)
                    {
                        DropItem(removedSlot.item, amountRemaining);
                    }
                }
            }
       }

        onInventoryChangeSubscribers.Invoke();
    }

    /// <summary>
    /// Adds an item to the first available slot, if possible.
    /// Stacks if autoStack is enabled and there are existing stacks of the same item.
    /// Returns a number of remaining items.
    /// </summary>
    private SlotContent AppendItemToAvailableSlot(ItemSO item, int amount, bool stackInExistingStacks, InventoryType inventoryType)
    {
        // Stop if item or amount is invalid
        if (item == null || amount <= 0) return new SlotContent() {item=item, quantity=amount};

        // Get the inventory slots and amount based on the inventory type
        List<SlotContent> currentInventorySlots = inventoryType == InventoryType.Primary ? primaryInventorySlots : this.secondaryInventorySlots; // Linked List
        int currentInventorySlotsAmount = currentInventorySlots.Count;

        // Stop if secondary inventory is not enabled or has no slots
        if (currentInventorySlotsAmount == 0) return new SlotContent() { item = item, quantity = amount }; ;

        int amountRemaining = amount;

        // If autoStack is enabled, try to stack the item in existing stacks first
        if (stackInExistingStacks)
        {
            // Iterate through each slot, and see if it can stack the item.
            for (int i = 0; i < currentInventorySlotsAmount; i++)
            {
                if (currentInventorySlots[i].item == item && currentInventorySlots[i].quantity < item.maxStackSize)
                {
                    int amountCanBeAddedToStack = item.maxStackSize - currentInventorySlots[i].quantity;
                    int amountToAddToStack = Mathf.Min(amountRemaining, amountCanBeAddedToStack);
                    currentInventorySlots[i].quantity += amountToAddToStack;
                    amountRemaining -= amountToAddToStack;

                    if (amountRemaining <= 0) break;
                }
            }
        }
        
        while (amountRemaining > 0)
        {
            // Check if there is room in the inventory
            int? firstAvailableSlotIndex = GetFirstAvailableSlot(inventoryType);

            // If there is no room, return
            if (firstAvailableSlotIndex == null) break;

            // Add the item to the first available slot
            currentInventorySlots[firstAvailableSlotIndex.Value].item = item;

            // Calculate the amount to add to this slot (either the remaining amount or the max stack size, whichever is smaller)
            int amountToAdd = Mathf.Min(amountRemaining, item.maxStackSize);
            currentInventorySlots[firstAvailableSlotIndex.Value].quantity = amountToAdd;
            amountRemaining = Mathf.Max(0, amountRemaining - amountToAdd);
        }

        if (amountRemaining != amount) onInventoryChangeSubscribers.Invoke();
        return new SlotContent() { item = item, quantity = 0 };
    }

    /// <summary>
    /// Finds the first avaiable slot in an inventory and returns its index. Returns null if there are no available slots.
    /// </summary>
    private int? GetFirstAvailableSlot(InventoryType inventoryType)
    {
        List<SlotContent> currentInventorySlots = inventoryType == InventoryType.Primary ? primaryInventorySlots : this.secondaryInventorySlots; // Linked List

        for (int i = 0; i < currentInventorySlots.Count; i++)
        {
            if (currentInventorySlots[i].item == null)
            {
                return i;
            }
        }

        return null;
    }

    /// <summary>
    /// Sets the item of an inventory slot directly, replacing the existing item and quantity. Completely removing it.
    /// Returns amount of item remaining (if obeying the stacking limimt) or 0 if all items were added to the slot.
    /// </summary>
    private SlotContent SetItemInSlot(ItemSO item, int amount, int slotIndex, bool ignoreStackingLimit, InventoryType inventoryType)
    {
        // Stop if item or amount is invalid
        if (amount < 0) return new SlotContent() { item = item, quantity = amount };

        // Get the inventory slots and amount based on the inventory type
        List<SlotContent> currentInventorySlots = inventoryType == InventoryType.Primary ? primaryInventorySlots : this.secondaryInventorySlots; // Linked List
        int currentInventorySlotsAmount = currentInventorySlots.Count;

        // Stop if secondary inventory is not enabled or has no slots
        if (currentInventorySlotsAmount == 0) return new SlotContent() { item = item, quantity = amount };

        // Stop if slot index is out of range
        if (slotIndex < 0 || slotIndex >= currentInventorySlotsAmount) return new SlotContent() { item = item, quantity = amount };

        // Set the item and quantity in the specified slot
        currentInventorySlots[slotIndex].item = item;

        int amountToAdd = ignoreStackingLimit ? amount : Mathf.Min(amount, item.maxStackSize);
        currentInventorySlots[slotIndex].quantity = amountToAdd;

        // Return any remaining items that couldn't fit in the slot
        onInventoryChangeSubscribers.Invoke();
        return new SlotContent() { item = item, quantity = amount - amountToAdd };
    }
    
    /// <summary>
    /// Clear the item of an inventory slot, replacing the existing item and quantity with null and 0. Completely removing it.
    /// Returns the item contents that it removed.
    /// </summary>
    private SlotContent ClearItemInSlot(int slotIndex, InventoryType inventoryType)
    {
        // Get the inventory slots and amount based on the inventory type
        List<SlotContent> currentInventorySlots = inventoryType == InventoryType.Primary ? primaryInventorySlots : this.secondaryInventorySlots; // Linked List
        int currentInventorySlotsAmount = currentInventorySlots.Count;

        // Stop if secondary inventory is not enabled or has no slots
        if (currentInventorySlotsAmount == 0) return new SlotContent() { item = null, quantity = 0 };

        // Stop if slot index is out of range
        if (slotIndex < 0 || slotIndex >= currentInventorySlotsAmount) return new SlotContent() { item = null, quantity = 0 };

        // Store the item and quantity that was in the slot before clearing it
        ItemSO removedItem = currentInventorySlots[slotIndex].item;
        int removedQuantity = currentInventorySlots[slotIndex].quantity;

        // Clear the item and quantity in the specified slot
        currentInventorySlots[slotIndex].item = null;
        currentInventorySlots[slotIndex].quantity = 0;

        // Return the item and quantity that was removed from the slot
        onInventoryChangeSubscribers.Invoke();
        return new SlotContent() { item = removedItem, quantity = removedQuantity };
    }

    /// <summary>
    /// Returns the slot contents of a specific slot.
    /// </summary>
    private SlotContent GetSlotContents(InventoryType inventoryType, int slotIndex)
    {
        return inventoryType == InventoryType.Primary ? primaryInventorySlots[slotIndex] : secondaryInventorySlots[slotIndex];
    }
    
    /// <summary>
    /// Returns the size of an inventory.
    /// </summary>
    private int GetInventorySize(InventoryType inventoryType)
    {
        return inventoryType == InventoryType.Primary ? primaryInventorySlots.Count : secondaryInventorySlots.Count;
    }
    
    /// <summary>
    /// Searches an inventory for a specific item and returns a list of indexes and a total quantity.
    /// </summary>
    private (List<int>, int) FindItemInInventory(ItemSO item, InventoryType inventoryType)
    {
        List<SlotContent> currentInventorySlots = inventoryType == InventoryType.Primary ? primaryInventorySlots : secondaryInventorySlots; // Linked List

        List<int> foundIndexes = new List<int>();
        int totalQuantity = 0;

        for (int i = 0; i < currentInventorySlots.Count; i++)
        {
            if (currentInventorySlots[i].item == item)
            {
                foundIndexes.Add(i);
                totalQuantity += currentInventorySlots[i].quantity;
            }
        }

        return (foundIndexes, totalQuantity);
    }
    #endregion

    #region Specified Inventory Functions
    /// <summary>
    /// Insert an item to a specific slot, stacking the item if the slot already contains the same item and stacking is allowed. Returns the amount of item that couldn't be added to the slot.
    /// </summary>
    private SlotContent InsertItemSlotWithStacking(ItemSO item, int amount, int slotIndex, InventoryType inventoryType)
    {
        // Stop if item or amount is invalid
        if (item == null || amount <= 0) return new SlotContent() { item = item, quantity = amount };

        // Get the inventory slots and amount based on the inventory type
        List<SlotContent> currentInventorySlots = inventoryType == InventoryType.Primary ? primaryInventorySlots : secondaryInventorySlots; // Linked List

        // Stop if secondary inventory is not enabled or has no slots
        if (currentInventorySlots.Count == 0) return new SlotContent() { item = item, quantity = amount };

        // Stop if slot index is out of range
        if (slotIndex < 0 || slotIndex >= currentInventorySlots.Count) return new SlotContent() { item = item, quantity = amount };

        // If the slot already contains the same item, try to stack it. Otherwise, do not stack.
        if (currentInventorySlots[slotIndex].item == item)
        {
            int amountCanBeAddedToStack = item.maxStackSize - currentInventorySlots[slotIndex].quantity;
            int amountToAddToStack = Mathf.Min(amount, amountCanBeAddedToStack);
            currentInventorySlots[slotIndex].quantity += amountToAddToStack;
            onInventoryChangeSubscribers.Invoke();
            return new SlotContent() { item = item, quantity = amount - amountToAddToStack };
        }
        else
        {
            return new SlotContent() { item = item, quantity = amount };
        }
    }

    /// <summary>
    /// Swap an item in a specific slot, replacing the existing item and quantity with the new item and quantity. Returns the item and quantity that was in the slot before the swap.
    /// This will obey stacking limits, and will not swap a stack that is more than full with an item.
    /// </summary>
    private SlotContent SwapItemInSlot(ItemSO item, int amount, int slotIndex, InventoryType inventoryType)
    {
        // Stop if amount is invalid
        if (amount < 0) return new SlotContent() { item = item, quantity = amount };

        // Get the inventory slots and amount based on the inventory type
        List<SlotContent> currentInventorySlots = inventoryType == InventoryType.Primary ? primaryInventorySlots : this.secondaryInventorySlots; // Linked List

        // Stop if secondary inventory is not enabled or has no slots
        if (currentInventorySlots.Count == 0) return new SlotContent() { item = item, quantity = amount };

        // Stop if the new item's quantity exceeds the stacking limit of the item in the slot (if they are the same item), and do not swap if that is the case.
        if (currentInventorySlots[slotIndex].item == item && amount > item.maxStackSize) return new SlotContent() { item = item, quantity = amount };

        // Stop if slot index is out of range
        if (slotIndex < 0 || slotIndex >= currentInventorySlots.Count) return new SlotContent() { item = item, quantity = amount };

        // Store the item and quantity that was in the slot before swapping it
        ItemSO removedItem = currentInventorySlots[slotIndex].item;
        int removedQuantity = currentInventorySlots[slotIndex].quantity;

        // Set the new item and quantity in the specified slot
        currentInventorySlots[slotIndex].item = item;
        currentInventorySlots[slotIndex].quantity = amount;

        // Return the item and quantity that was removed from the slot
        onInventoryChangeSubscribers.Invoke();
        return new SlotContent() { item = removedItem, quantity = removedQuantity };
    }
    
    /// <summary>
    /// Appends an item to the inventory, first checking the primary inventory, then the secondary inventory.
    /// Returns the item and amount that couldn't be added to either inventory.
    /// </summary>
    private SlotContent AttemptAddToBothInventory(ItemSO item, int amount, InventoryType firstInventory)
    {
        // First check and add to the primary inventory.
        int amountRemainingAfterFirst = AppendItemToAvailableSlot(item, amount, true, firstInventory).quantity;

        // If there is any amount remaining, check and add to the secondary inventory.
        return AppendItemToAvailableSlot(item, amountRemainingAfterFirst, true,
            firstInventory == InventoryType.Primary ? InventoryType.Secondary : InventoryType.Primary
        );
    }
    #endregion

    #region Main Inventory Slot Functions
    public void SetPrimaryInventorySlotsAmount(int amount)
    { SetInventorySlotsAmount(amount, InventoryType.Primary); }
    public void SetSecondaryInventorySlotsAmount(int amount)
    { SetInventorySlotsAmount(amount, InventoryType.Secondary); }

    public void AppendItemToPrimaryInventory(ItemSO item, int amount)
    { AttemptAddToBothInventory(item, amount, InventoryType.Primary); }
    public void AppendItemToSecondaryInventory(ItemSO item, int amount)
    { AttemptAddToBothInventory(item, amount, InventoryType.Secondary); }

    public void OverrideSetItemAtIndexPrimaryInventory(ItemSO item, int amount, int slotIndex, bool ignoreStackingLimit = false)
    { SetItemInSlot(item, amount, slotIndex, ignoreStackingLimit, InventoryType.Primary); }
    public void OverrideSetItemAtIndexSecondaryInventory(ItemSO item, int amount, int slotIndex, bool ignoreStackingLimit = false)
    { SetItemInSlot(item, amount, slotIndex, ignoreStackingLimit, InventoryType.Secondary); }

    public void ClearItemAtIndexPrimaryInventory(int slotIndex)
    { ClearItemInSlot(slotIndex, InventoryType.Primary); }
    public void ClearItemAtIndexSecondaryInventory(int slotIndex)
    { ClearItemInSlot(slotIndex, InventoryType.Secondary); }

    public void InsertItemAtIndexWithStackingPrimaryInventory(ItemSO item, int amount, int slotIndex)
    { InsertItemSlotWithStacking(item, amount, slotIndex, InventoryType.Primary); }
    public void InsertItemAtIndexWithStackingSecondaryInventory(ItemSO item, int amount, int slotIndex)
    { InsertItemSlotWithStacking(item, amount, slotIndex, InventoryType.Secondary); }

    public void SwapItemAtIndexPrimaryInventory(ItemSO item, int amount, int slotIndex)
    { SwapItemInSlot(item, amount, slotIndex, InventoryType.Primary); }
    public void SwapItemAtIndexSecondaryInventory(ItemSO item, int amount, int slotIndex)
    { SwapItemInSlot(item, amount, slotIndex, InventoryType.Secondary); }

    public SlotContent GetActivePrimarySlotContent()
    {
        return GetSlotContents(InventoryType.Primary, activeSlotIndex);
    }
    public SlotContent GetPrimarySlotContent(int activeSlotIndex)
    {
        return GetSlotContents(InventoryType.Primary, activeSlotIndex);
    }
    public SlotContent GetSecondarySlotContent(int activeSlotIndex)
    {
        return GetSlotContents(InventoryType.Secondary, activeSlotIndex);
    }

    /// <summary>
    /// Searches for an item in both inventories, first checking the primary inventory, then the secondary inventory. Returns a list of indexes and a total quantity.
    /// </summary>
    public (List<int>, int) FindItemInBothInventories(ItemSO item)
    {
        // First check the primary inventory.
        (List<int> primaryIndexes, int primaryTotalQuantity) = FindItemInInventory(item, InventoryType.Primary);

        // Then check the secondary inventory.
        (List<int> secondaryIndexes, int secondaryTotalQuantity) = FindItemInInventory(item, InventoryType.Secondary);

        // Combine the results and return them.
        List<int> combinedIndexes = new List<int>();
        combinedIndexes.AddRange(primaryIndexes);
        combinedIndexes.AddRange(secondaryIndexes);
        int combinedTotalQuantity = primaryTotalQuantity + secondaryTotalQuantity;

        return (combinedIndexes, combinedTotalQuantity);
    }
    #endregion

    #region Inventory Floating Slot Functions
    /// <summary>
    /// Returns the current floating slot contents
    /// </summary>
    public SlotContent GetFloatingSlotContents()
    { return floatingSlotContent; }

    /// <summary>
    /// Sets the floating slot content, which is used to store content that is being moved around, EX: When the mouse picks something up.
    /// Will not override current floating slot content, unless autoDrop is enabled, in which it will drop the current floating slot content and set the new floating slot content.
    /// Returns true if successful, false if the floating slot already has content and autoDrop is not enabled.
    /// </summary>
    public bool SetInventoryFloatingSlot(ItemSO item, int amount, bool autoDrop)
    {
        SlotContent currentSlot = GetFloatingSlotContents();
        if (currentSlot.item != null && currentSlot.quantity > 0)
        {
            if (autoDrop)
                DropInventoryFloatingSlot();
            else
                return false;
        }

        floatingSlotContent = new SlotContent() { item = item, quantity = amount };
        return true;
    }
    
    /// <summary>
    /// Drop current floating slot content and clear the floating slot.
    /// </summary>
    public void DropInventoryFloatingSlot()
    {
        SlotContent currentSlot = GetFloatingSlotContents();
        DropItem(currentSlot.item, currentSlot.quantity);
        ClearInventoryFloatingSlot();
    }
    
    /// <summary>
    /// Clear the floating slot, erasing content if there is any.
    /// </summary>
    private void ClearInventoryFloatingSlot()
    { floatingSlotContent = new SlotContent() { item = null, quantity = 0 }; }
    #endregion

    #region Main Inventory Floating Slot Functions
    /// <summary>
    /// Swap floating inventory contents with the contents of a specific inventory slot.
    /// </summary>
    private void SwapFloatingContentsAndInventorySlot(int slotIndex, InventoryType inventoryType)
    {
        // Get slot contents
        SlotContent slotContent = GetSlotContents(inventoryType, slotIndex);
        if (slotContent == null) return;

        // Get floating slot contents
        SlotContent floatingSlotContent = GetFloatingSlotContents();
        if (slotContent == null) return;

        // If there is already content in the floating slot, ensure it does not exceed stack size.
        if (slotContent.item && slotContent.quantity > slotContent.item.maxStackSize) return;

        // Swap the contents of the floating slot and the inventory slot
        SlotContent newFloatingSlotContent = SwapItemInSlot(floatingSlotContent.item, floatingSlotContent.quantity, slotIndex, inventoryType);
        this.floatingSlotContent = newFloatingSlotContent;

        if (floatingSlotContent != newFloatingSlotContent) onInventoryFloatingSlotChangeSubscribers.Invoke();
    }

    public void SwapFloatingContentsAndPrimaryInventorySlot(int slotIndex)
    { SwapFloatingContentsAndInventorySlot(slotIndex, InventoryType.Primary); }
    public void SwapFloatingContentsAndSecondaryInventorySlot(int slotIndex)
    { SwapFloatingContentsAndInventorySlot(slotIndex, InventoryType.Secondary); }
    #endregion

    public void SubscribeToInventoryEquippedChange(UnityAction listener)
    { onInventoryEquippedChangeSubscribers.AddListener(listener); }
    public void UnsubscribeFromInventoryEquippedChange(UnityAction listener)
    { onInventoryEquippedChangeSubscribers.RemoveListener(listener); }

    public void SubscribeToInventoryChange(UnityAction listener)
    { onInventoryChangeSubscribers.AddListener(listener); }
    public void UnsubscribeToInventoryChange(UnityAction listener)
    { onInventoryChangeSubscribers.RemoveListener(listener); }

    public void SubscribeToInventoryFloatingSlotChange(UnityAction listener)
    { onInventoryFloatingSlotChangeSubscribers.AddListener(listener); }
    public void UnsubscribeToInventoryFloatingSlotChange(UnityAction listener)
    { onInventoryFloatingSlotChangeSubscribers.RemoveListener(listener); }
}
