using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string levelSceneName = "Level";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void LoadMainMenu() => LoadScene(mainMenuSceneName);
    public void LoadLevel() => LoadScene(levelSceneName);
    public void ReloadCurrentScene() => LoadScene(SceneManager.GetActiveScene().name);
    public void QuitGame() => Application.Quit();

    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
