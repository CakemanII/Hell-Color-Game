using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Mono.Cecil;

public class MainHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private Transform activeInventorySlotHighlight;
    [SerializeField] private Transform primaryInventoryParentLeft;
    [SerializeField] private Transform primaryInventoryParentRight;
    [SerializeField] private TextMeshProUGUI itemStateDisplayText;
    [Space()]
    [SerializeField] private GameObject inventorySlotPrefab;

    private EntityHealth playerHealth;
    private EntityInventory playerInventory;
    private EntityPrimaryEquippedHandler entityEquippedHandler;

    private List<InventorySlot> primaryInventorySlots = new List<InventorySlot>();

    private void Start()
    {
        UpdatePlayerReferences();
        if (playerInventory == null) { Debug.LogError("Player inventory is null in MainHUD."); return; }

        InitializeInventoryPrimaryUI();

        playerInventory.SubscribeToInventoryChange(UpdatePrimaryInventorySlots);
        playerInventory.SubscribeToInventoryEquippedChange(UpdateActiveInventorySlot);
        UpdateActiveInventorySlot();
        UpdateAllInventorySlots();
    }

    private void Update()
    {
        if (GameManager.Instance.Player == null) return;
        UpdatePlayerReferences();

        // Update health display
        UpdateHealthDisplay();

        // Update state display
        UpdateStateDisplay();
    }

    private void UpdatePlayerReferences()
    {
        // Return if no player
        if (GameManager.Instance.Player == null) return;

        // Ensure the references are assigned
        if (playerHealth == null || playerInventory == null || entityEquippedHandler == null)
        {
            playerHealth = GameManager.Instance.Player.GetComponent<EntityHealth>();
            playerInventory = GameManager.Instance.Player.GetComponent<EntityInventory>();
            entityEquippedHandler = GameManager.Instance.Player.GetComponent<EntityPrimaryEquippedHandler>();
        }
    }

    private void UpdateHealthDisplay()
    {
        healthBar.right = new Vector3(
            healthBar.right.x - (healthBar.rect.width * (1 - playerHealth.CurrentHealth / playerHealth.MaxHealth)),
            healthBar.right.y, healthBar.right.z);
    }

    private void UpdateStateDisplay()
    {
        TypeEquipped typeEquipped = entityEquippedHandler.GetTypeEquipped();
        if (typeEquipped == TypeEquipped.None || typeEquipped == TypeEquipped.Misc)
        {
            itemStateDisplayText.text = "";
            return;
        }
        if (typeEquipped == TypeEquipped.StackableItem)
        {
            SlotContent content = entityEquippedHandler.GetEntityInventory().GetActivePrimarySlotContent();
            int amount = content.quantity;

            itemStateDisplayText.text = $"{amount}";
        }
        else if (typeEquipped == TypeEquipped.LoadableWeapon)
        {
            if (entityEquippedHandler.GetEquippedObjectItem() is not ProjectileWeapon weapon) return;
            int ammoInInventory = weapon.AmmoType == AmmoType.Medium ? entityEquippedHandler.GetEntityInventory().CurrentMediumAmmo : entityEquippedHandler.GetEntityInventory().CurrentHeavyAmmo;
            itemStateDisplayText.text = $"{weapon.CurrentAmmoInMagazine} / {weapon.MagazineSize}  {ammoInInventory}";
        }
    }

    private void InitializeInventoryPrimaryUI()
    {
        // Initialize the primary inventory slots
        if (playerInventory == null) { Debug.LogWarning("Player inventory reference is missing in MainHUD."); return; }

        for (int i = 0; i < Mathf.Min(4, playerInventory.MaxPrimaryInventorySlots); i++)
        {
            InventorySlot inventorySlot = Instantiate(inventorySlotPrefab, primaryInventoryParentLeft).GetComponent<InventorySlot>();
            inventorySlot.Init(null, i, InventoryType.Primary);
            primaryInventorySlots.Add(inventorySlot);
        }
        for (int i = 0; i < Mathf.Min(4, Mathf.Max(4, playerInventory.MaxPrimaryInventorySlots - 4)); i++)
        {
            InventorySlot inventorySlot = Instantiate(inventorySlotPrefab, primaryInventoryParentRight).GetComponent<InventorySlot>();
            inventorySlot.Init(null, i, InventoryType.Primary);
            primaryInventorySlots.Add(inventorySlot);
        }
    }

    private void UpdateAllInventorySlots()
    {
        for (int i = 0; i < primaryInventorySlots.Count; i++)
            UpdateInventoryIconAtDisplay(i);
    }

    private void UpdateInventoryIconAtDisplay(int slotIndex)
    {
        // Get current content in the slot.
        SlotContent content = playerInventory.GetPrimarySlotContent(slotIndex);

        // Get the item icon and count from content.
        ItemSO itemSO = content.item;
        Sprite itemIcon = itemSO != null ? itemSO.itemIcon : null;
        int quantity = content.quantity;

        // Update the icon and quantity text in the UI.
        primaryInventorySlots[slotIndex].SetData(itemIcon, quantity);
    }

    private void UpdatePrimaryInventorySlots()
    {
        UpdateAllInventorySlots();
    }

    private void UpdateActiveInventorySlot()
    {
        int activeSlotIndex = playerInventory.GetActivePrimarySlotIndex();

        if (activeSlotIndex >= 4)
        {
            // Right side
            activeInventorySlotHighlight.SetParent(primaryInventoryParentRight.GetChild(activeSlotIndex - 4));
            activeInventorySlotHighlight.localPosition = Vector3.zero;
        }
        else
        {
            // Left side
            activeInventorySlotHighlight.SetParent(primaryInventoryParentLeft.GetChild(activeSlotIndex));
            activeInventorySlotHighlight.localPosition = Vector3.zero;
        }
    }
}
