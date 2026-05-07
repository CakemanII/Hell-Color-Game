using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private bool IsMoveableSlot;

    [SerializeField] private RawImage iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    private InventoryUI inventoryUI;
    private int slotIndex;
    private InventoryType inventoryType;

    public void Init(InventoryUI inventoryUI, int slotIndex, InventoryType inventoryType)
    {
        this.inventoryUI = inventoryUI;
        this.slotIndex = slotIndex;
        this.inventoryType = inventoryType;
    }

    public void SetData(Sprite icon, int quantity)
    {
        if (icon != null)
        {
            iconImage.texture = icon.texture;
            iconImage.color = Color.white; // Ensure the icon is visible
        }
        else
        {
            iconImage.texture = null;
            iconImage.color = Color.clear; // Hide the icon if there's no item
        }

        quantityText.text = quantity > 1 ? quantity.ToString() : string.Empty; // Show quantity only if more than 1
    }

    private void OnMouseDown()
    {
        if (!IsMoveableSlot) return;
        inventoryUI.ToggleMove(slotIndex, inventoryType);
    }
}
