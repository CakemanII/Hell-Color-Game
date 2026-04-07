using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject player;

    [SerializeField] private ItemSO tempItemSO;

    void Awake()
    {
        // Initialize player input.
        playerInput.Init(player.GetComponent<PlayerController>());
    }

    private void Start()
    {
        // Temp
        player.GetComponent<EntityInventory>().AddItemToAvailablePrimarySlot(tempItemSO, 1);
    }
}
