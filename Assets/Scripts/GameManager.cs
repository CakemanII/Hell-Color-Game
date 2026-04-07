using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject player;

    void Awake()
    {
        // Initialize player input.
        playerInput.Init(player.GetComponent<PlayerController>());
    }
}
