using UnityEngine;

[CreateAssetMenu(
    fileName = "Item",
    menuName = "Items/Item",
    order = 1
)]

public class ItemSO : ScriptableObject
{
    [Header("References")]
    public GameObject itemCollectablePrefab;
    public GameObject itemEquipedPrefab;
    [Space()]
    public GameObject itemIcon;

    [Header("Item Settings")]
    public bool isStackable = false;
    [Tooltip("Only applies if isStackable is true")]
    public int maxStackSize = 1;
}
