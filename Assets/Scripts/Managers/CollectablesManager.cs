using UnityEngine;

public class CollectablesManager : MonoBehaviour
{
    public static CollectablesManager Instance { private set; get; }

    [Header("Spawn Settings")]
    [SerializeField] float standardDropForce = 5f;
    [SerializeField] SpecializedGameObjectPool weaponPool;
    public SpecializedGameObjectPool WeaponPool => weaponPool;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of CollectablesManager detected. Destroying the new one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool PickupCollectable(SlotContent content, EntityInventory playerInventory, GameObject collectable)
    {
        SlotContent leftOverContent;
        if (content.item.importantItem)
            leftOverContent = playerInventory.AppendItemToPrimaryInventory(content, false);
        else
            leftOverContent = playerInventory.AppendItemToSecondaryInventory(content, false);

        // Nothing was added — inventory full
        if (leftOverContent != null && leftOverContent.quantity == content.quantity) return false;

        // Spawn any items that didn't fit
        if (leftOverContent != null && leftOverContent.quantity > 0)
        {
            Quaternion rotation = playerInventory.transform.Find("CameraPosition").transform.rotation;
            SpawnCollectable(leftOverContent.item, leftOverContent.quantity, leftOverContent.essentialItemReference, playerInventory.transform.position + rotation * (Vector3.one / 2), rotation, standardDropForce);
        }

        QuestManager.Instance.IncrementCollectItems();

        return true;
    }

    public void SpawnCollectable(ItemSO item, int quantity, GameObject essentialItemReference, Vector3 position, Quaternion direction, float? standardDropForce = null)
    {
        if (standardDropForce == null) standardDropForce = this.standardDropForce;
        GameObject collectable = Instantiate(item.itemCollectablePrefab, position, Quaternion.Euler(Vector3.up));
        collectable.GetComponent<Collectable>().Init(quantity, (float)standardDropForce, direction, essentialItemReference);
    }
}
