using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color reflectedColor = Color.blue;

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private bool rotateToDirection = true;

    [Header("Damage")]
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private int damageToEnemy = 1;

    [Header("Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Reflection")]
    [Tooltip("Tag on the player's attack hitbox collider (ex: 'PlayerAttack').")]
    [SerializeField] private string playerAttackTag = "PlayerAttack";
    [Tooltip("Small boost after reflection so it feels snappy.")]
    [SerializeField] private float reflectSpeedMultiplier = 1.15f;

    [Header("SFX")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private float hitVolume = 1f;

    [Header("Animation")]
    [Tooltip("Animator Trigger name that switches from Fly -> Impact")]
    [SerializeField] private string hitTriggerName = "Hit";

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;

    private Vector2 moveDir;
    private bool reflected;
    private SpriteRenderer sr;

    // Prevents multiple hits / multiple destroys / multiple sounds
    private bool impactPlayed;

    private int hitTriggerHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        hitTriggerHash = Animator.StringToHash(hitTriggerName);

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        col.isTrigger = true;

        if (sr != null)
            sr.color = normalColor;

        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 dir) => SetDirection(dir);

    private void SetDirection(Vector2 dir)
    {
        moveDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        rb.linearVelocity = moveDir * speed;

        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void ReflectFrom(Vector2 reflectSourcePosition)
    {
        // Send it away from the attack hitbox position
        Vector2 newDir = (Vector2)transform.position - reflectSourcePosition;
        if (newDir.sqrMagnitude < 0.0001f) newDir = -moveDir;

        reflected = true;
        speed *= reflectSpeedMultiplier;

        // Change color when reflected
        if (sr != null)
        sr.color = reflectedColor;

        SetDirection(newDir);
    }

    private void PlayImpactAndStop()
    {
        if (impactPlayed) return;
        impactPlayed = true;

        // stop moving and stop future collisions
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        col.enabled = false;

        // trigger impact animation
        if (anim != null) anim.SetTrigger(hitTriggerHash);

        // sound once
        if (SFXManager.Instance != null)
            SFXManager.Instance.Play(hitClip, 0.8f, 0.95f, 1.05f);
    }

    // Add an Animation Event at the LAST frame of the Impact clip calling this:
    public void OnImpactAnimFinished()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (impactPlayed) return;

        // 1) Reflect if hit by player attack hitbox
        if (!reflected && other.CompareTag(playerAttackTag))
        {
            var combat = other.GetComponentInParent<PlayerCombat>();
            if (combat != null && combat.CanReflect)
            {
                ReflectFrom(other.bounds.center);
                return; // IMPORTANT: no impact anim on reflection
            }
        }

        int bit = 1 << other.gameObject.layer;

        // 2) Ground/walls: impact + destroy after anim
        if (groundLayer.value != 0 && (groundLayer.value & bit) != 0)
        {
            PlayImpactAndStop();
            return;
        }

        // 3) If NOT reflected: hurt player + impact
        if (!reflected)
        {
            if (playerLayer.value != 0 && (playerLayer.value & bit) != 0)
            {
                var health = other.GetComponentInParent<PlayerHealth>();
                if (health != null) health.TakeDamage(damageToPlayer);

                PlayImpactAndStop();
            }
            return;
        }

        // 4) If reflected: hurt enemies + impact
        if (enemyLayer.value != 0 && (enemyLayer.value & bit) != 0)
        {
            var eh = other.GetComponentInParent<EnemyHealth>();
            if (eh != null) eh.Die(); // or take damage later

            PlayImpactAndStop();
        }
    }
}