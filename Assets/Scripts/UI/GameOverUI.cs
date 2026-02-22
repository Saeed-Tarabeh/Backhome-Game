using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsShowing { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        HideImmediate();
    }

    public void Show()
    {
        IsShowing = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        FindFirstObjectByType<AudioTimeReset>()?.ResetNow();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        FindFirstObjectByType<AudioTimeReset>()?.ResetNow();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void HideImmediate()
    {
        IsShowing = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}