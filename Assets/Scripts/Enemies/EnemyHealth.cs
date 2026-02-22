using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Death Feel")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float popScale = 1.18f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] deathClips;

    [SerializeField] private float hitStopDuration = 0.08f;
    [SerializeField] private float hitStopScale = 0.05f;

    private bool dead;

    private Rigidbody2D rb;
    private Collider2D col;
    private EnemyPatrol patrol;
    private Animator anim;

    private SpriteRenderer[] renderers;
    private Vector3 originalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        patrol = GetComponent<EnemyPatrol>();
        anim = GetComponentInChildren<Animator>();

        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalScale = transform.localScale;
    }

    public bool IsDead => dead;

    public void Die()
    {
        if (dead) return;
        dead = true;

        if (deathClips.Length > 0 && SFXManager.Instance != null)
        {
            AudioClip clip = deathClips[Random.Range(0, deathClips.Length)];
            SFXManager.Instance.Play(clip, 2f, 0.95f, 1.25f);
        }

        // register death (only once)
        var eid = GetComponent<EnemyID>();
        if (eid != null && CheckpointManager.Instance != null)
            CheckpointManager.Instance.RegisterEnemyDeath(eid.id);

        // stop movement scripts
        if (patrol) patrol.enabled = false;

        // STOP animator so it doesn't override color/scale
        if (anim) anim.enabled = false;

        // stop physics
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;   // if your Unity version doesn't have this, use rb.velocity
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }
        if (col) col.enabled = false;

        if (HitStop.Instance != null)
            HitStop.Instance.Stop(hitStopDuration, hitStopScale);

        StartCoroutine(PopThenFadeRoutine());
    }

    private IEnumerator PopThenFadeRoutine()
    {
        // --- POP (scale up then back) ---
        float signX = Mathf.Sign(transform.localScale.x);
        Vector3 baseScale = new Vector3(Mathf.Abs(originalScale.x) * signX, originalScale.y, originalScale.z);

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / popDuration;

            // simple "punch": up quickly then settle
            float s = (k < 0.5f)
                ? Mathf.Lerp(1f, popScale, k / 0.5f)
                : Mathf.Lerp(popScale, 1f, (k - 0.5f) / 0.5f);

            transform.localScale = new Vector3(baseScale.x * s, baseScale.y * (2f - s), baseScale.z);
            yield return null;
        }

        // --- FADE OUT (all sprite renderers) ---
        t = 0f;

        // cache starting colors
        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            startColors[i] = renderers[i] ? renderers[i].color : Color.white;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (!r) continue;

                Color c = startColors[i];
                r.color = new Color(c.r, c.g, c.b, a);
            }

            yield return null;
        }

        // hide at end
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i]) renderers[i].enabled = false;
    }

    public void ResetEnemy()
    {
        dead = false;

        // restore visuals
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i]) continue;
                var c = renderers[i].color;
                renderers[i].color = new Color(c.r, c.g, c.b, 1f);
                renderers[i].enabled = true;
            }
        }

        // restore physics / scripts
        if (rb)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero; // or rb.velocity
            rb.angularVelocity = 0f;
        }
        if (col) col.enabled = true;
        if (patrol) patrol.enabled = true;
        if (anim) anim.enabled = true;

        // restore scale with facing sign
        float signX = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * signX, originalScale.y, originalScale.z);
    }

    public void ForceDeadState()
    {
        dead = true;

        if (rb) rb.simulated = false;
        if (col) col.enabled = false;
        if (patrol) patrol.enabled = false;
        if (anim) anim.enabled = false;

        if (renderers != null)
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i]) renderers[i].enabled = false;
    }
}