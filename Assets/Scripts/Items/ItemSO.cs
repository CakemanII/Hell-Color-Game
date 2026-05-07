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
    public Sprite itemIcon;

    [Header("Item Settings")]
    public int maxStackSize = 1;
}
