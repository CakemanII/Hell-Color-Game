using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { private set; get; }

    [Header("Game Configuration")]
    [SerializeField] private GameObject playerPrefab;
    [Tooltip("Name of the GameObject that marks the player's starting position in the level")]
    [SerializeField] private string playerStartPointGameObjectName = "PlayerStart";
    [Space()]
    [SerializeField] private float scorePerSecondRemaining = 0.1f;

    public int score { get; private set; }
    //public int bananasCollected { get; private set; }
    public float timeRemaining { get; private set; }
    public int livesRemaining { get; private set; }

    private bool timerActive = false;

    private bool levelComplete = false;
    private bool gameOver = false;

    public PlayerController playerController { get; private set; }

    void Awake()
    {
        Debug.Log("GameManager Awake called.");
        // Implement singleton pattern
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // Do not destroy on load
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Called when a level is loaded.
    /// </summary>
    public void OnLevelLoaded(bool firstLevel)
    {
        // Attempt to duck tape fix to stop double listeners.
        if (GameObject.FindWithTag("Player")) return;

        Debug.Log("Level Loaded");
        // Reset game values.
        ResetValues(firstLevel);

        // Instantiate the player.
        Transform playerStartingTrans = GameObject.Find(playerStartPointGameObjectName).transform;
        playerController = Instantiate(playerPrefab, playerStartingTrans.position, playerStartingTrans.rotation).GetComponent<PlayerController>();
    }

    void ResetValues(bool firstLevel)
    {
        // Initialize game values
        score = 0;
        //bananasCollected = 0;
        timeRemaining = LevelManager.instance.GetCurrentLevelSO().levelTime;

        // Get previous level lives if available, else use starting lives
        livesRemaining = firstLevel ? LevelManager.instance.GetCurrentLevelCollection().startingLives : livesRemaining;

        // Start the game timer
        StartTimer();
    }

    void Update()
    {
        Countdown();
    }

    /// <summary>
    /// Handles the countdown of the game timer.
    /// </summary>
    private void Countdown()
    {
        if (!timerActive) return;
        if (timeRemaining > 0)
            // Decrease time remaining
            timeRemaining -= Time.deltaTime;
        else if (timeRemaining <= 0)
        {
            // Time's up - handle game over or level failure
            timeRemaining = 0;
            OnGameOver();
        }
    }
    private void StartTimer() { timerActive = true; }

    /// <summary>
    /// Triggered when the player makes it to the end of the level.
    /// </summary>
    public void OnLevelComplete()
    {
        if (levelComplete || gameOver) return; // Prevent multiple triggers
        // Disable timer and player controls
        // Stop wind sfx
        //SFXHandler.instance.PlayAmbientWind(false);
        timerActive = false;
        playerController.DisablePlayerControls();

        // Convert the remaining seconds into score.
        //AddScoreFromTimeRemaining();

        // Start level complete sequence
        StartCoroutine(LevelCompleteSequence());
    }

    /// <summary>
    /// Triggered when the player dies or runs out of time.
    /// </summary>
    public void OnGameOver()
    {
        // Prevent multiple triggers
        if (gameOver || levelComplete) return;

        // Stop wind sfx
        //SFXHandler.instance.PlayAmbientWind(false);

        // Disable timer and player controls
        timerActive = false;
        playerController.DisablePlayerControls();

        // Decrement lives
        livesRemaining--;
        gameOver = true;

        // Handle game over logic (e.g., restart level or go to main menu)
        if (livesRemaining > 0)
        {
            // Restart the current level
            LevelManager.instance.ReloadCurrentLevel();
        }
        else
        {
            // No lives left - go to main menu or game over screen
            LevelManager.instance.LoadMainMenu();
        }

        // Reset game over flag for next level
        gameOver = false;
    }

    /*#region (Incr/Decr)ement Methods
    public void AddScore(int amount) { score += amount; }
    public void AddBanana() { bananasCollected += 1; }
    #endregion*/

    private IEnumerator LevelCompleteSequence()
    {
        yield return new WaitForSeconds(2.0f);
        LevelManager.instance.LoadNextLevel();
    }

    public void PauseGame()
    {
        Debug.Log("Game Paused");
        timerActive = false;
        playerController.DisablePlayerControls();
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
    }

    public void ResumeGame()
    {
        Debug.Log("Game Resumed");
        timerActive = true;
        playerController.EnablePlayerControls();
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public void QuitToMainMenu()
    {
        // Stop wind sfx
        //SFXHandler.instance.PlayAmbientWind(false);

        Time.timeScale = 1f; // Ensure time scale is reset
        Time.fixedDeltaTime = 0.02f;
        LevelManager.instance.LoadMainMenu();
    }

    /*private void AddScoreFromTimeRemaining()
    {
        float remainingSeconds = timeRemaining;
        float scoreFromSeconds = timeRemaining * scorePerSecondRemaining;
        int intScoreFromSeconds = ((int)scoreFromSeconds);
        AddScore(intScoreFromSeconds);
    }*/
}