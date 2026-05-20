using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RawImage iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    private InventoryUI inventoryUI;
    private int slotIndex;
    private InventoryType inventoryType;
    private bool isMoveable;

    public void Init(InventoryUI inventoryUI, int slotIndex, InventoryType inventoryType)
    {
        this.inventoryUI  = inventoryUI;
        this.slotIndex    = slotIndex;
        this.inventoryType = inventoryType;
        isMoveable = inventoryUI != null;
    }

    public void SetData(Sprite icon, int quantity)
    {
        if (icon != null)
        {
            iconImage.texture = icon.texture;
            iconImage.color   = Color.white;
        }
        else
        {
            iconImage.texture = null;
            iconImage.color   = Color.clear;
        }

        quantityText.text = quantity > 1 ? quantity.ToString() : string.Empty;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isMoveable || inventoryUI == null) return;
        inventoryUI.ToggleMove(slotIndex, inventoryType);
    }
}
