using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Transform primaryInventoryParentLeft;
    [SerializeField] private Transform primaryInventoryParentRight;

    private List<InventorySlot> primaryInventorySlots = new List<InventorySlot>();

    private EntityInventory playerInventory;
    private bool beingDisplayed;

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager reference is missing in HotbarUI.");
        }

        UpdateInventoryReference();
    }

    private void UpdateInventoryReference()
    {
        // Implement
        if (playerInventory == null && gameManager != null)
        {
            playerInventory = gameManager.Player?.GetComponent<EntityInventory>();
            InitializeListeners();
        }
    }

    private void UpdatePrimarySlots()
    {
        if (!beingDisplayed) return;
        for (int i = 0; i < primaryInventorySlots.Count; i++)
        {
            UpdatePrimaryIconAtDisplay(i);
        }
    }

    private void UpdateActiveSlotSelect()
    {

    }

    private void InitializeListeners()
    {
        playerInventory.SubscribeToInventoryChange(UpdatePrimarySlots);
        playerInventory.SubscribeToInventoryEquippedChange(UpdateActiveSlotSelect);
    }

    private void UpdatePrimaryIconAtDisplay(int slotIndex)
    {
        if (!beingDisplayed) return;
        // Get current content in the slot.
        SlotContent content = playerInventory.GetPrimarySlotContent(slotIndex);

        // Get the item icon and count from content.
        ItemSO itemSO = content.item;
        Sprite itemIcon = itemSO != null ? itemSO.itemIcon : null;
        int quantity = content.quantity;

        // Update the icon and quantity text in the UI.
        primaryInventorySlots[slotIndex].SetData(itemIcon, quantity);
    }
}
