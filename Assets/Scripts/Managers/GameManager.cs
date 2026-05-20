using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private CinemachineCamera cinemachineCamera;

    private Transform player;
    public Transform Player => player;

    private bool isGamePaused = false;

    void Awake()
    {
        // Ensure singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        // Instantiate Player
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        this.player = player.transform;
        cinemachineCamera.Target.TrackingTarget = this.player.Find("CameraPosition").transform;

        // Initialize PlayerInput
        playerInput.Init(player.GetComponent<PlayerController>());
    }

    public void TogglePause()
    {
        if (isGamePaused)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        // Stop music
        // Stop SFX
        // Stop physics time
    }

    private void UnpauseGame()
    {
        // Resume music
        // Resume SFX
        // Resume physics time
    }
}
