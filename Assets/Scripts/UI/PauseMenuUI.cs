using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Selectable firstSelected; // drag Resume button here

    [Header("Scene")]
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

        // Optional: pause AudioSources globally (UI audio can ignore this)
        AudioListener.pause = isPaused;

        if (isPaused)
        {
            StartCoroutine(ForceHighlightFirstButton());
        }
        else
        {
            // Clear selection when closing so we start fresh next time
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private IEnumerator ForceHighlightFirstButton()
    {
        // Wait until the panel & buttons are fully enabled
        yield return null;

        var es = EventSystem.current;
        if (es == null || firstSelected == null)
            yield break;

        // Rebuild UI so transitions are ready
        Canvas.ForceUpdateCanvases();

        // Clear then select
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(firstSelected.gameObject);

        // Force "Selected" visuals (keyboard/controller highlight)
        firstSelected.Select();

        // Also force hover visuals refresh even if mouse didn't move
        var ped = new PointerEventData(es);
        ExecuteEvents.Execute(firstSelected.gameObject, ped, ExecuteEvents.pointerExitHandler);
        ExecuteEvents.Execute(firstSelected.gameObject, ped, ExecuteEvents.pointerEnterHandler);
    }

    // Button events
    public void Resume()
    {
        SetPaused(false);
    }

    public void RestartLevel()
    {
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