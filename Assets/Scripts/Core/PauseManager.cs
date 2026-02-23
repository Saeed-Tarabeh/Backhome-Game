using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    private float timeScaleBeforePause = 1f;

    // Prevents input leak on resume
    private bool resumeInputBlock;

    public bool InputBlocked => IsPaused || resumeInputBlock;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void Resume()
    {
        if (!IsPaused) return;
        StartCoroutine(ResumeSafe());
    }

    private IEnumerator ResumeSafe()
    {
        resumeInputBlock = true;

        // Unpause time
        IsPaused = false;
        Time.timeScale = timeScaleBeforePause;
        AudioListener.pause = false;

        // Wait until mouse buttons are released
        while (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            yield return null;

        // Extra frame safety
        yield return null;

        resumeInputBlock = false;
    }
}