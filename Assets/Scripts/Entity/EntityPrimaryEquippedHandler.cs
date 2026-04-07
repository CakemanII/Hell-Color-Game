using UnityEngine;

public class EntityPrimaryEquippedHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] EntityInventory entityInventory;
    [SerializeField] Transform equippedItemParent; // Parent transform for equipped items (optional)

    private GameObject currentlyEquippedObject;

    public GameObject GetEquippedObject() {  return currentlyEquippedObject; }

    void Awake()
    {
        // Subscribe to the entity inventory equipped change event
        entityInventory.SubscribeToInventoryEquippedChange(OnEquippedItemChange);
    }

    private void OnEquippedItemChange()
    {
        Debug.Log("Swithcing equipped item in EntityPrimaryEquippedHandler");

        // Get the currently equipped item from the inventory
        InventorySlot activeSlot = entityInventory.GetActivePrimarySlot();

        // Destroy current equipped item if exists
        Destroy(currentlyEquippedObject);

        // Instantiate the new equipped item if it exists
        if (activeSlot.item != null)
        {
            // Instantiate the equipped item prefab as a child of this GameObject
            currentlyEquippedObject = Instantiate(activeSlot.item.itemEquipedPrefab, transform);

            // Set the parent of the object to the equipped item parent if it exists
            if (equippedItemParent != null)
            {
                currentlyEquippedObject.transform.SetParent(equippedItemParent, false);
                currentlyEquippedObject.transform.localPosition = Vector3.zero; // Reset local position
                currentlyEquippedObject.transform.localRotation = Quaternion.identity; // Reset local rotation
            }
        }
    }

    public void ToggleUseEquippedItem(bool use)
    {
        if (currentlyEquippedObject)
        {
            // (temp)
            currentlyEquippedObject.GetComponent<Weapon_Pistol>().UseWeapon(use);
        }
    }
}
