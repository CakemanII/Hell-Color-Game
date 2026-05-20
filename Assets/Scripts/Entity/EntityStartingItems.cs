using UnityEngine;

public class EntityStartingItems : MonoBehaviour
{
    [SerializeField] private SlotContent[] primaryInventoryStartingSlotContent;
    [SerializeField] private SlotContent[] secondaryInventoryStartingSlotContent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EntityInventory entityInventory = GetComponent<EntityInventory>();

        foreach (SlotContent secondaryInventoryStartingSlotContent in secondaryInventoryStartingSlotContent)
        { entityInventory.AppendItemToSecondaryInventory(secondaryInventoryStartingSlotContent, false); }

        foreach (SlotContent primaryInventoryStartingSlotContent in primaryInventoryStartingSlotContent)
        { entityInventory.AppendItemToPrimaryInventory(primaryInventoryStartingSlotContent, false); }

        Destroy(this);
    }
}
