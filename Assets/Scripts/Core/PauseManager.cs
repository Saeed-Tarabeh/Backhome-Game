using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    private float timeScaleBeforePause = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        timeScaleBeforePause = Time.timeScale;   // could be 1 or 0.6 etc.
        Time.timeScale = 0f;
        AudioListener.pause = true;              // optional: pauses AudioSources (not Mixer effects)
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = timeScaleBeforePause;  // restores slow-mo if it was active
        AudioListener.pause = false;
    }
}