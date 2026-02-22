using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class JumpHintTrigger : MonoBehaviour
{
    [Header("UI Animator")]
    [SerializeField] private SlideHintSequence hintSequence;

    [Header("Timing")]
    [SerializeField] private float delayAfterStep = 2.0f;

    [Header("Behavior")]
    [SerializeField] private bool triggerOnce = true;

    [Header("Time Control")]
    [SerializeField] private float slowedTimeScale = 0.6f;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerSnapshot normalSnapshot;
    [SerializeField] private AudioMixerSnapshot slowMoSnapshot;
    [SerializeField] private float transitionTime = 0.2f;

    private float previousTimeScale;
    private PlayerMovement playerMovement;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Immediately slow time
        previousTimeScale = Time.timeScale;
        Time.timeScale = slowedTimeScale;
        slowMoSnapshot.TransitionTo(transitionTime);

        // Immediately lock movement
        if (PlayerHealth.Instance != null)
        {
            playerMovement = PlayerHealth.Instance.GetComponent<PlayerMovement>();
            if (playerMovement != null)
                playerMovement.enabled = false;
        }

        StartCoroutine(RunAfterDelay());
    }

    private IEnumerator RunAfterDelay()
    {
        // Use REALTIME because time is slowed
        yield return new WaitForSecondsRealtime(delayAfterStep);

        if (hintSequence != null)
            hintSequence.Play();

        yield return new WaitForSecondsRealtime(
            hintSequence.TotalSequenceTime()
        );

        RestoreTimeAndPlayer();
    }

    private void RestoreTimeAndPlayer()
    {
        if (Time.timeScale != 0f)
            Time.timeScale = previousTimeScale;
        normalSnapshot.TransitionTo(transitionTime);

        if (playerMovement != null)
            playerMovement.enabled = true;
    }
}