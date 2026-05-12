using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelCollectionSO defaultTestLevelCollection;
    public static LevelManager instance { private set; get; }

    private LevelCollectionSO activeLevelCollection;

    private int currentLevelIndex = 0;

    public void SetActiveLevelCollection(LevelCollectionSO levelCollection)
    { activeLevelCollection = levelCollection; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Implement singleton pattern
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // For testing individual levels:
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            activeLevelCollection = defaultTestLevelCollection;
            GameManager.instance.OnLevelLoaded(true);
        }
    }

    /// <summary>
    /// Loads the current level based on the active level collection and current level index.
    /// </summary>
    private void LoadLevel(bool firstLevel = false)
    {
        transform.parent = null;
        // Load the current level from the active level collection
        if (activeLevelCollection != null && activeLevelCollection.levels.Length > 0)
        {
            // Get the scene to load
            string levelToLoad = activeLevelCollection.levels[currentLevelIndex].sceneName;

            // Setup the listener for scene loaded event
            // DO NOT refactor this, this line is very dangerous if changed incorrectly
            UnityAction<Scene, LoadSceneMode> handler = null;
            handler = (scene, mode) =>
            {
                GameManager.instance.OnLevelLoaded(firstLevel);
                SceneManager.sceneLoaded -= handler; // removes the exact same delegate
            };
            SceneManager.sceneLoaded += handler;

            // Load the scene
            SceneManager.LoadScene(levelToLoad);

        }
        else
            Debug.LogError("Cannot load level: Active level collection is null or has no levels.");
    }

    /// <summary>
    /// Loads the next level in the active level collection.
    /// </summary>
    public void LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= activeLevelCollection.levels.Length)
        {
            currentLevelIndex = 0; // Loop back to the first level
        }
        LoadLevel();
    }

    /// <summary>
    /// Reloads the current level.
    /// </summary>
    public void ReloadCurrentLevel()
    { LoadLevel(); }

    /// <summary>
    /// Load first level in the active level collection.
    /// </summary>
    public void LoadFirstLevel()
    {
        currentLevelIndex = 0;
        LoadLevel(true);
    }

    /// <summary>
    /// Return to the main menu.
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }


    #region Getters
    public LevelSO GetCurrentLevelSO()
    { return activeLevelCollection.levels[currentLevelIndex]; }
    public LevelCollectionSO GetCurrentLevelCollection()
    { return activeLevelCollection; }
    #endregion
}
