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

    private void Start()
    {
        if (gameManager == null) { Debug.LogError("GameManager reference is missing in HotbarUI."); return; }

        playerInventory = gameManager.Player?.GetComponent<EntityInventory>();
        if (playerInventory == null) { Debug.LogError("Player inventory is null in HotbarUI."); return; }

        InitializePrimarySlots();
        InitializeListeners();
        UpdatePrimarySlots();
    }

    private void InitializePrimarySlots()
    {
        foreach (Transform child in primaryInventoryParentLeft)
        {
            InventorySlot slot = child.GetComponent<InventorySlot>();
            if (slot != null) primaryInventorySlots.Add(slot);
        }
        foreach (Transform child in primaryInventoryParentRight)
        {
            InventorySlot slot = child.GetComponent<InventorySlot>();
            if (slot != null) primaryInventorySlots.Add(slot);
        }
    }

    private void InitializeListeners()
    {
        playerInventory.SubscribeToInventoryChange(UpdatePrimarySlots);
        playerInventory.SubscribeToInventoryEquippedChange(UpdateActiveSlotSelect);
    }

    private void UpdatePrimarySlots()
    {
        for (int i = 0; i < primaryInventorySlots.Count; i++)
            UpdatePrimaryIconAtDisplay(i);
    }

    private void UpdateActiveSlotSelect()
    {

    }

    private void UpdatePrimaryIconAtDisplay(int slotIndex)
    {
        SlotContent content = playerInventory.GetPrimarySlotContent(slotIndex);

        ItemSO itemSO = content.item;
        Sprite itemIcon = itemSO != null ? itemSO.itemIcon : null;
        int quantity = content.quantity;

        primaryInventorySlots[slotIndex].SetData(itemIcon, quantity);
    }
}
