using System.Collections;
using UnityEngine;

/// <summary>
/// One-time tutorial trigger:
/// - Player enters trigger zone
/// - Slow time + play whisper + fade UI in
/// - Player holds E to confirm
/// - On success: consumes 1 photo checkpoint, fades UI out, restores time
/// - waits for E to be RELEASED before re-enabling PhotoCheckpoint input
///   (prevents "holding E" from immediately consuming another photo after tutorial)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CheckpointTutorialTrigger : MonoBehaviour
{
    [Header("Time Slow")]
    [SerializeField] private float slowedTimeScale = 0.8f;

    [Header("UI (CanvasGroup on tutorial panel root)")]
    [SerializeField] private CanvasGroup tutorialUI;
    [SerializeField] private float fadeDuration = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioClip whisperClip;
    [SerializeField] private float whisperVolume = 0.6f;

    [Header("Hold To Confirm")]
    [SerializeField] private KeyCode key = KeyCode.E;
    [SerializeField] private float holdSeconds = 0.6f;

    // State
    private bool tutorialActive;               // tutorial currently running (time slowed, UI visible)
    private bool completed;                   // tutorial successfully completed once
    private float holdTimer;                  // uses unscaled time so it feels consistent during time slow
    private float prevTimeScale = 1f;

    private PhotoCheckpoint photoCheckpoint;   // grabbed from player
    private Coroutine fadeRoutine;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // Start UI hidden.
        SetUIInstant(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed) return;

        if (!other.CompareTag("Player")) return;

        // Find PhotoCheckpoint (root or parent).
        photoCheckpoint = other.GetComponent<PhotoCheckpoint>() ?? other.GetComponentInParent<PhotoCheckpoint>();
        if (photoCheckpoint == null)
        {
            Debug.LogWarning("[CheckpointTutorialTrigger] Player has no PhotoCheckpoint component.");
            return;
        }

        StartTutorial();
    }

    private void Update()
    {
        if (!tutorialActive) return;

        // Hold E (use unscaled time so slow-motion doesn't change how long the hold feels).
        if (Input.GetKey(key))
        {
            holdTimer += Time.unscaledDeltaTime;
            if (holdTimer >= holdSeconds)
            {
                TryConfirm();
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void StartTutorial()
    {
        if (tutorialActive) return;

        tutorialActive = true;
        holdTimer = 0f;

        // Disable the player's normal PhotoCheckpoint input so ONLY this tutorial consumes the hold.
        photoCheckpoint.SetInputEnabled(false);

        // Slow time (remember previous).
        prevTimeScale = Time.timeScale;
        Time.timeScale = slowedTimeScale;

        // Play whisper.
        // if (SFXManager.Instance != null)
        //     SFXManager.Instance.Play(whisperClip, whisperVolume, 0.95f, 1.05f);

        // Fade UI in.
        FadeUI(1f);
    }

    private void TryConfirm()
    {
        if (completed) return;

        // Attempt to use a checkpoint photo for real.
        // If it fails (no photos / not allowed), keep tutorial active and keep UI up.
        if (!photoCheckpoint.TryUseCheckpointNow())
            return;

        // Success: lock completion so it can't fire twice.
        completed = true;
        tutorialActive = false;
        holdTimer = 0f;

        // Restore time immediately (feels responsive).
        Time.timeScale = prevTimeScale;

        // Fade UI out, then:
        // - wait for E RELEASE
        // - re-enable normal PhotoCheckpoint input
        // - disable this trigger
        FadeUI(0f);
        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        // Let the fade complete.
        yield return new WaitForSecondsRealtime(fadeDuration);

        // If the player keeps holding E, re-enabling PhotoCheckpoint input can cause
        // an immediate second photo consumption. So we wait for E to be released once.
        while (Input.GetKey(key))
            yield return null;

        // Now it's safe to restore normal input.
        if (photoCheckpoint != null)
            photoCheckpoint.SetInputEnabled(true);

        // Safety: force-hide UI.
        SetUIInstant(false);

        // Disable this trigger so it never runs again.
        gameObject.SetActive(false);
    }

    private void FadeUI(float toAlpha)
    {
        if (tutorialUI == null) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(toAlpha));
    }

    private IEnumerator FadeRoutine(float toAlpha)
    {
        float fromAlpha = tutorialUI.alpha;
        float t = 0f;

        // If visible, allow raycasts/interact. If not, block them.
        tutorialUI.blocksRaycasts = toAlpha > 0.01f;
        tutorialUI.interactable = toAlpha > 0.01f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            tutorialUI.alpha = Mathf.Lerp(fromAlpha, toAlpha, t / fadeDuration);
            yield return null;
        }

        tutorialUI.alpha = toAlpha;
        fadeRoutine = null;
    }

    private void SetUIInstant(bool visible)
    {
        if (tutorialUI == null) return;

        tutorialUI.alpha = visible ? 1f : 0f;
        tutorialUI.interactable = visible;
        tutorialUI.blocksRaycasts = visible;
    }

    private void OnDisable()
    {
        // Safety: if disabled mid-tutorial, restore time & hide UI.
        if (tutorialActive)
            Time.timeScale = prevTimeScale;

        tutorialActive = false;
        holdTimer = 0f;

        SetUIInstant(false);

        // Also ensure we don't leave input disabled if something disables this unexpectedly.
        if (photoCheckpoint != null)
            photoCheckpoint.SetInputEnabled(true);
    }
}