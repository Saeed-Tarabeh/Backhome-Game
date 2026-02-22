using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;
    private float timeScaleBeforePause = 1f;

    private void Start()
    {
        SetPaused(false);
    }

    private void Update()
    {
        if (GameOverUI.Instance != null && GameOverUI.Instance.IsShowing) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        SetPaused(!isPaused);
    }

    private void SetPaused(bool value)
    {
        // If we're switching to paused, remember the current timeScale (1, 0.6, etc.)
        if (value && !isPaused)
            timeScaleBeforePause = Time.timeScale;

        isPaused = value;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        // Pause forces 0. Resume restores whatever it was before pause.
        Time.timeScale = isPaused ? 0f : timeScaleBeforePause;

        // Optional: pause AudioSources globally (Mixer snapshots still apply)
        AudioListener.pause = isPaused;
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void RestartLevel()
    {
        // Always normalize before reload
        Time.timeScale = 1f;
        AudioListener.pause = false;

        FindFirstObjectByType<AudioTimeReset>()?.ResetNow();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        FindFirstObjectByType<AudioTimeReset>()?.ResetNow();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}