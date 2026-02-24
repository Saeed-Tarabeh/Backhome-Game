using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDefend : MonoBehaviour
{
    [SerializeField] private PlayerAnimator playerAnim;

    [Header("Behavior")]
    [SerializeField] private bool freezeMovement = true;
    [SerializeField] private bool makeInvulnerable = true;

    [Header("Visual")]
    [SerializeField, Range(0f, 1f)] private float defendAlpha = 0.25f;

    [Header("Layers")]
    [SerializeField] private string normalPlayerLayerName = "Player";
    [SerializeField] private string defendingLayerName = "PlayerDefending";

    private int normalLayer;
    private int defendingLayer;

    private bool isDefending;
    private bool defendHeld; 

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerHealth playerHealth;

    private SpriteRenderer[] spriteRenderers;
    private float[] originalAlphas;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();

        normalLayer = LayerMask.NameToLayer(normalPlayerLayerName);
        defendingLayer = LayerMask.NameToLayer(defendingLayerName);

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalAlphas = new float[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            originalAlphas[i] = spriteRenderers[i].color.a;

        if (!playerAnim) playerAnim = GetComponent<PlayerAnimator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            defendHeld = true;
            playerAnim?.SetDefending(true);
        }

        if (Input.GetMouseButtonUp(1))
        {
            defendHeld = false;
            playerAnim?.SetDefending(false);

            // Always restore immediately on release
            StopDefendLogic();
        }
    }

    public bool IsDefending => isDefending;

    // Called by animation event on frame 3
    public void Anim_DefendActivate()
    {
        // ignore the event if the player already released RMB
        if (!defendHeld) return;
        // cancel if not grounded
        if (movement != null && !movement.IsGrounded)
        {
            defendHeld = false;
            playerAnim?.SetDefending(false);
            return;
        }

        StartDefendLogic();
    }

    public void StartDefendLogic()
    {
        if (isDefending) return;
        isDefending = true;

        if (defendingLayer != -1)
            gameObject.layer = defendingLayer;

        if (freezeMovement)
        {
            if (movement != null) movement.enabled = false;
            rb.linearVelocity = Vector2.zero; 
        }

        if (makeInvulnerable && playerHealth != null)
            playerHealth.SetInvulnerable(true);

        SetAllSpriteAlpha(defendAlpha);
    }

    public void StopDefendLogic()
    {
        if (!isDefending) return;
        isDefending = false;

        if (normalLayer != -1)
            gameObject.layer = normalLayer;

        if (freezeMovement && movement != null)
            movement.enabled = true;

        if (makeInvulnerable && playerHealth != null)
            playerHealth.SetInvulnerable(false);

        RestoreSpriteAlphas();
    }

    private void SetAllSpriteAlpha(float a)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (!spriteRenderers[i]) continue;
            var c = spriteRenderers[i].color;
            c.a = a;
            spriteRenderers[i].color = c;
        }
    }

    private void RestoreSpriteAlphas()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (!spriteRenderers[i]) continue;
            var c = spriteRenderers[i].color;
            c.a = originalAlphas[i];
            spriteRenderers[i].color = c;
        }
    }

    private void OnDisable()
    {
        defendHeld = false;
        StopDefendLogic();
    }
}