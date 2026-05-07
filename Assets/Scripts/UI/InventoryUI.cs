using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEditor.ShaderKeywordFilter;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryIconPrefab;
    [Space()]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform primaryInventoryParentLeft;
    [SerializeField] private Transform primaryInventoryParentRight;
    [SerializeField] private Transform secondaryInventoryParent;
    [Space()]
    [SerializeField] private Transform floatingSlot;
    [SerializeField] private InventorySlot floatingInventorySlot;

    private EntityInventory playerInventory;

    private List<InventorySlot> primaryInventorySlots = new List<InventorySlot>();
    private List<InventorySlot> secondaryInventorySlots = new List<InventorySlot>();

    private bool beingDisplayed;

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager reference is missing in InventoryUI.");
        }

        InitializeInventoryUI();
    }

    private void InitializeInventoryUI()
    {
        // Initialize the primary inventory slots
        for (int i = 0; i < Mathf.Min(4, playerInventory.MaxPrimaryInventorySlots); i++ ) 
        {
            InventorySlot inventorySlot = Instantiate(inventoryIconPrefab, primaryInventoryParentLeft).GetComponent<InventorySlot>();
            inventorySlot.Init(this, i, InventoryType.Primary);
            primaryInventorySlots.Add(inventorySlot); 
        }
        for (int i = 0; i < Mathf.Min(4, Mathf.Max(4, playerInventory.MaxPrimaryInventorySlots - 4)); i++ ) 
        {
            InventorySlot inventorySlot = Instantiate(inventoryIconPrefab, primaryInventoryParentRight).GetComponent<InventorySlot>();
            inventorySlot.Init(this, i, InventoryType.Primary);
            primaryInventorySlots.Add(inventorySlot);
        }

        // Initialize the secondary inventory slots
        for (int i = 0; i < playerInventory.MaxSecondaryInventorySlots; i++)
        {
            InventorySlot inventorySlot = Instantiate(inventoryIconPrefab, secondaryInventoryParent).GetComponent<InventorySlot>();
            inventorySlot.Init(this, i, InventoryType.Secondary);
            secondaryInventorySlots.Add(inventorySlot);
        }
    }

    public void UpdateAllInventorySlots()
    {
        if (!beingDisplayed) return;
        for (int i = 0; i < primaryInventorySlots.Count; i++)
        {
            UpdateInventoryIconAtDisplay(i, InventoryType.Primary);
        }

        for (int i = 0; i < secondaryInventorySlots.Count; i++)
        {
            UpdateInventoryIconAtDisplay(i, InventoryType.Secondary);
        }
    }

    public void UpdateFloatingInventorySlot()
    {
        if (!beingDisplayed) return;
        SlotContent floatingSlotContent = playerInventory.GetFloatingSlotContents();
        floatingInventorySlot.SetData(
            floatingSlotContent.item != null ? floatingSlotContent.item.itemIcon : null, 
            floatingSlotContent.quantity
        );
    }

    private void UpdateInventoryIconAtDisplay(int slotIndex, InventoryType inventoryType)
    {
        if (!beingDisplayed) return;
        // Get current content in the slot.
        SlotContent content = inventoryType == InventoryType.Primary ? 
            playerInventory.GetPrimarySlotContent(slotIndex) : playerInventory.GetSecondarySlotContent(slotIndex);

        // Get the item icon and count from content.
        ItemSO itemSO = content.item;
        Sprite itemIcon = itemSO != null ? itemSO.itemIcon : null;
        int quantity = content.quantity;

        // Update the icon and quantity text in the UI.
        if (inventoryType == InventoryType.Primary)
            primaryInventorySlots[slotIndex].SetData(itemIcon, quantity);
        else
            secondaryInventorySlots[slotIndex].SetData(itemIcon, quantity);
    }

    private void Update()
    {
        // Update the reference
        UpdateInventoryReference();

        // Update the position of the floating slot to follow the mouse cursor
        if (beingDisplayed)
            floatingSlot.position = Input.mousePosition;
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

    private void InitializeListeners()
    {
        playerInventory.SubscribeToInventoryChange(UpdateAllInventorySlots);
        playerInventory.SubscribeToInventoryFloatingSlotChange(UpdateFloatingInventorySlot);
    }

    public void ToggleMove(int slotIndex, InventoryType inventoryType)
    {
        // Get the current slot content
        SlotContent content = inventoryType == InventoryType.Primary ? 
            playerInventory.GetPrimarySlotContent(slotIndex) : playerInventory.GetSecondarySlotContent(slotIndex);

        // Attempt to switch the slot
        if (inventoryType == InventoryType.Primary)
        {
            playerInventory.SwapFloatingContentsAndPrimaryInventorySlot(slotIndex);
        }
        else
        {
            playerInventory.SwapFloatingContentsAndSecondaryInventorySlot(slotIndex);
        }
    }
}
