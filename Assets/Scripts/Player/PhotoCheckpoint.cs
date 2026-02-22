using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhotoCheckpoint : MonoBehaviour
{
    [Header("Checkpoint")]
    [SerializeField] private int maxPhotos = 3;
    [SerializeField] private float holdTime = 0.5f;
    [SerializeField] private float reflectFreezeTime = 0.3f;

    [Header("Optional Slow Time (only while holding AND photos > 0)")]
    [SerializeField] private bool useSlowTime = true;
    [SerializeField] private float slowTimeScale = 0.6f;

    [Header("UI (Single Icon + Count)")]
    [SerializeField] private Image photoIcon;          // single photo image
    [SerializeField] private TMP_Text countText;       // shows "xN"
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioClip photoUsedClip;
    [SerializeField] private AudioClip noPhotosClip;

    [SerializeField] private float usedVolume = 1f;
    [SerializeField] private float emptyVolume = 0.8f;
    private bool inputEnabled = true;

    private int photosRemaining;
    private float holdCounter;
    private bool isHolding;
    private bool isSlowingTime;

    private PlayerHealth playerHealth;
    private PlayerMovement movement;
    private Rigidbody2D rb;

    private Coroutine iconFadeRoutine;

    private void Start()
    {
        photosRemaining = maxPhotos;

        playerHealth = GetComponent<PlayerHealth>();
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();

        RefreshUI(forceShowIcon: true);
    }

    private void Update()
    {
        if (!inputEnabled) return;
        // No photos left -> no slow time, no hold, no checkpoint.
        if (photosRemaining <= 0)
        {
            if (Input.GetKeyDown(KeyCode.E)){
                if (SFXManager.Instance != null)
                    SFXManager.Instance.Play(noPhotosClip, emptyVolume, 0.95f, 1.05f);
                Debug.Log("No photos remaining.");
            }

            StopSlowTimeIfNeeded();
            holdCounter = 0f;
            isHolding = false;
            return;
        }

        // Prevent mid-air checkpoint
        if (movement != null && !movement.IsGrounded)
        {
            StopSlowTimeIfNeeded();
            return;
        }
        if (Input.GetKey(KeyCode.E))
        {
            holdCounter += Time.unscaledDeltaTime;

            if (useSlowTime && !isSlowingTime)
            {
                Time.timeScale = slowTimeScale;
                isSlowingTime = true;
            }

            if (holdCounter >= holdTime && !isHolding)
            {
                CreateCheckpoint();
                isHolding = true;
            }
        }
        else
        {
            holdCounter = 0f;
            isHolding = false;
            StopSlowTimeIfNeeded();
        }
    }

    private void CreateCheckpoint()
    {
        if (photosRemaining <= 0) return;

        photosRemaining--;
        if (SFXManager.Instance != null) SFXManager.Instance.Play(photoUsedClip, usedVolume, 0.95f, 1.05f);
        playerHealth.SetRespawnPoint(transform.position);

        if (movement != null) movement.enabled = false;
        Invoke(nameof(ReenableMovement), reflectFreezeTime);

        StopSlowTimeIfNeeded();

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.SaveCheckpointSnapshot();

        // Update UI:
        // - If now 1 -> hide the xN text
        // - If now 0 -> fade the icon away
        RefreshUI(forceShowIcon: true);

        if (photosRemaining == 0)
        {
            FadeOutIcon();
            Debug.Log("Last photo used.");
        }
    }

    private void RefreshUI(bool forceShowIcon)
    {
        // Icon visible while we still have photos (or if you want it visible before fade finishes)
        if (photoIcon != null)
        {
            if (forceShowIcon && photosRemaining > 0)
                SetImageAlpha(photoIcon, 1f);
        }

        // Count text logic:
        // show only if > 1, hide if == 1 or 0
        if (countText != null)
        {
            if (photosRemaining > 1)
            {
                countText.gameObject.SetActive(true);
                countText.text = $"x{photosRemaining}";
            }
            else
            {
                countText.gameObject.SetActive(false);
            }
        }
    }

    private void FadeOutIcon()
    {
        if (photoIcon == null) return;

        if (iconFadeRoutine != null)
            StopCoroutine(iconFadeRoutine);

        iconFadeRoutine = StartCoroutine(FadeImageAlpha(photoIcon, 0f, fadeDuration));
    }

    private IEnumerator FadeImageAlpha(Image img, float targetAlpha, float duration)
    {
        Color c = img.color;
        float startAlpha = c.a;

        if (duration <= 0.0001f)
        {
            c.a = targetAlpha;
            img.color = c;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);

            c.a = Mathf.Lerp(startAlpha, targetAlpha, u);
            img.color = c;

            yield return null;
        }

        c.a = targetAlpha;
        img.color = c;
    }

    private void SetImageAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    private void ReenableMovement()
    {
        if (movement != null) movement.enabled = true;
    }

    private void StopSlowTimeIfNeeded()
    {
        if (isSlowingTime)
        {
            Time.timeScale = 1f;
            isSlowingTime = false;
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        // reset holding state so it doesn't instantly trigger when re-enabled
        holdCounter = 0f;
        isHolding = false;
        StopSlowTimeIfNeeded();
    }

    // Called by tutorial trigger (or any script) to force a checkpoint use
    public bool TryUseCheckpointNow()
    {
        if (photosRemaining <= 0) return false;

        // Use the same grounded rule as the normal checkpoint logic
        if (movement != null && !movement.IsGrounded) return false;

        CreateCheckpoint();
        return true;
    }
}