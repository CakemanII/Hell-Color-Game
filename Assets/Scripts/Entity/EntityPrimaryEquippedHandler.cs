using UnityEngine;

public enum TypeEquipped
{
    None,
    LoadableWeapon,
    StackableItem,
    Misc
}

public class EntityPrimaryEquippedHandler : MonoBehaviour
{
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private string weaponLayerName = "Weapon";

    [Header("References")]
    [SerializeField] EntityInventory entityInventory;
    [SerializeField] Transform equippedItemParent;

    public EntityInventory GetEntityInventory() => entityInventory;

    private GameObject currentlyEquippedObject;
    private TypeEquipped typeEquipped;
    private int previousSlotIndex;
    private ItemSO previousEquippedItem;

    public TypeEquipped GetTypeEquipped() => typeEquipped;
    public GameObject GetEquippedObject() => currentlyEquippedObject;
    public Item GetEquippedObjectItem() => currentlyEquippedObject?.GetComponent<Item>();
    public bool IsEquippedWeaponReloading() =>
        currentlyEquippedObject != null &&
        currentlyEquippedObject.GetComponent<ProjectileWeapon>() is ProjectileWeapon pw &&
        pw.IsReloading;

    void Awake()
    {
        entityInventory.SubscribeToInventoryEquippedChange(OnEquippedItemChange);
    }

    private void OnEquippedItemChange()
    {
        int currentSlotIndex = entityInventory.GetActivePrimarySlotIndex();
        SlotContent activeSlot = entityInventory.GetActivePrimarySlotContent();

        if (currentSlotIndex == previousSlotIndex && activeSlot.item == previousEquippedItem) return;

        UnequipCurrentItem();

        currentlyEquippedObject = null;

        if (activeSlot.item != null)
            EquipItem(activeSlot);

        SetEquippedType(activeSlot);
        previousSlotIndex = currentSlotIndex;
        previousEquippedItem = activeSlot.item;
    }

    private void UnequipCurrentItem()
    {
        if (currentlyEquippedObject == null) return;

        if (typeEquipped == TypeEquipped.LoadableWeapon)
        {
            // Write the live weapon object back into the old slot so it can be re-equipped later
            entityInventory.SetEssentialReferenceAtPrimarySlot(previousSlotIndex, currentlyEquippedObject);
            CollectablesManager.Instance.WeaponPool.Give(currentlyEquippedObject);
        }
        else
        {
            Destroy(currentlyEquippedObject);
        }
    }

    private void EquipItem(SlotContent activeSlot)
    {
        if (activeSlot.essentialItemReference != null)
        {
            CollectablesManager.Instance.WeaponPool.Take(activeSlot.essentialItemReference);
            currentlyEquippedObject = activeSlot.essentialItemReference;
            currentlyEquippedObject.SetActive(true);
        }
        else
        {
            currentlyEquippedObject = Instantiate(activeSlot.item.itemEquipedPrefab, transform);
        }

        if (equippedItemParent != null)
        {
            currentlyEquippedObject.transform.SetParent(equippedItemParent, false);
            currentlyEquippedObject.transform.localPosition = Vector3.zero;
            currentlyEquippedObject.transform.localRotation = Quaternion.identity;
            SetLayerOfEquippedObject();
        }

        float v = isPlayer ? 1f : 0.01719806f;
        currentlyEquippedObject.transform.localScale = Vector3.one * v;
    }

    private void SetEquippedType(SlotContent activeSlot)
    {
        if (currentlyEquippedObject == null)
            typeEquipped = TypeEquipped.None;
        else if (currentlyEquippedObject.GetComponent<ProjectileWeapon>() != null)
            typeEquipped = TypeEquipped.LoadableWeapon;
        else if (activeSlot.quantity > 1)
            typeEquipped = TypeEquipped.StackableItem;
        else
            typeEquipped = TypeEquipped.Misc;
    }

    public void ToggleUseEquippedItem(bool use)
    {
        GetEquippedObjectItem()?.UseItemInput(use);
    }

    public void AttemptSetReload(bool input)
    {
        if (typeEquipped != TypeEquipped.LoadableWeapon) return;
        if (currentlyEquippedObject?.GetComponent<ProjectileWeapon>() is ProjectileWeapon weapon)
            weapon.SetReloadInput(input);
    }

    private void SetLayerOfEquippedObject()
    {
        if (!isPlayer) return;
        int layerIndex = LayerMask.NameToLayer(weaponLayerName);
        if (layerIndex < 0)
        {
            Debug.LogError($"[EntityPrimaryEquippedHandler] Layer '{weaponLayerName}' not found. Add it in Edit > Project Settings > Tags and Layers.");
            return;
        }
        SetLayerRecursively(currentlyEquippedObject, layerIndex);
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
