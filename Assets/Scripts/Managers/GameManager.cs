using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private GameUI gameUI;
    [SerializeField] private CheatsManager cheatsManager;
    [SerializeField] private CollectablesManager collectablesManager;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private LevelGenerationManager levelGenerationManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private EnemySpawnerManager enemySpawnerManager;


    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private CinemachineCamera cinemachineCamera;

    [Header("Game Settings")]
    [SerializeField] private int wavesPerLevel = 2;

    private Transform player;
    public Transform Player => player;

    private bool isGamePaused = false;
    private int currentWave = 0;
    private int currentLevel = -1;

    private bool gameIsActive = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;
    }

    private void Update()
    {
        if (currentLevel == -1)
        {
            currentLevel = 1;
            currentWave = 1;
            gameIsActive = false;
            CreateLevel();
        }
        else
        if (gameIsActive && enemySpawnerManager.GetEnemyCount() == 0)
        {
            currentWave++;
            if (currentWave >= wavesPerLevel)
            {
                Debug.Log("Finished level!");
                currentLevel++;
                currentWave = 0;
                gameIsActive = false;
                QuestManager.Instance.AddScore(currentWave * 150 * currentLevel);
                CreateLevel();
            }
            else
            {
                QuestManager.Instance.AddScore(currentWave * 100 * currentLevel);
                SpawnWave();
            }
        }
    }

    private void CreateLevel()
    {
        gameUI.SetLoadingScreen(true);
        playerInput.SetPlayerInputEnabled(false);

        StartCoroutine(MakeLevelSequence());
    }

    private IEnumerator MakeLevelSequence()
    {
        yield return new WaitForSeconds(1f); // Simulate loading time
        
        // Generate level
        levelGenerationManager.OnGenerationComplete += OnLevelReady;

        // Determine room amount
        int roomCount = 10 + (currentLevel * 5); // Example: Increase room count by 5 each level
        levelGenerationManager.GenerateLevel(roomCount, Mathf.Min(currentLevel, 10), Vector3.zero);
    }

    private void SpawnWave()
    {
        // Spawn enemies for the first wave
        enemySpawnerManager.SpawnEnemies(1 + (currentLevel * 5)); // Example: Increase enemy count by 5 each level
    }

    private void OnLevelReady()
    {
        // Spawn the player
        if (player == null)
            SpawnPlayer();
        else
        {
            player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            player.position = Vector3.zero;
        }
        levelGenerationManager.OnGenerationComplete -= OnLevelReady;

        // Stop loading
        playerInput.SetPlayerInputEnabled(true);
        gameUI.SetLoadingScreen(false);

        // Start first wave
        SpawnWave();
        gameIsActive = true;
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
        isGamePaused = true;
        audioManager.PauseAudios();
        Time.timeScale = 0f;
        gameUI.SetUIBeingDisplayed(UIType.Settings);
    }

    private void UnpauseGame()
    {
        isGamePaused = false;
        audioManager.PlayMusic();
        Time.timeScale = 1f;
        gameUI.SetUIBeingDisplayed(UIType.MainHUD);
    }
}
