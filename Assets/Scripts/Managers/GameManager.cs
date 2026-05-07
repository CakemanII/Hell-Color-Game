using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject playerPrefab;

    private Transform player;
    public Transform Player => player;

    void Awake()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        // Instantiate Player
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        // Initialize PlayerInput
        playerInput.Init(player.GetComponent<PlayerController>());
    }
}
