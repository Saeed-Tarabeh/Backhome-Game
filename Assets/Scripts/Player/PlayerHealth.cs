using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [SerializeField] private bool isWounded = true;

    private bool isDead;
    private Vector3 respawnPoint;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerAnimator playerAnim;
    private bool invulnerable;
    private Animator animator;
    private bool gameOverPending;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        respawnPoint = transform.position;

        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        playerAnim = GetComponent<PlayerAnimator>();
        animator = GetComponent<Animator>();
    }

    public void SetRespawnPoint(Vector3 point)
    {
        respawnPoint = point;
    }

    public void TakeDamage(int amount)
    {
        if (invulnerable) return;
        if (isDead) return;
        GetComponent<PlayerAudio>()?.PlayHurt();

        if (isWounded)
            Die();
        else
        {
            // Later for boss phase
        }
    }

    private void Die()
    {
        isDead = true;

        // NEW: decide now whether this death leads to respawn or game over
        if (LevelLives.Instance != null)
        {
            bool canRespawn = LevelLives.Instance.SpendLife();
            gameOverPending = !canRespawn;
        }
        else
        {
            // If you forgot to place LevelLives in the scene,
            // default to "respawn allowed" so the game keeps working.
            gameOverPending = false;
        }

        // Trigger death animation/state
        playerAnim?.SetDead(true);

        // Disable movement so player doesn't move during death anim
        if (movement != null) movement.enabled = false;
    }

    public void AnimEvent_Respawn()
    {
        if (gameOverPending)
        {
            if (GameOverUI.Instance != null)
                GameOverUI.Instance.Show();
            else if (LevelLives.Instance != null)
                LevelLives.Instance.RestartLevel(); // fallback

            return;
        }

        RespawnInternal();
    }

    private void RespawnInternal()
    {
        // Teleport
        transform.position = respawnPoint;

        // Reset physics so you don't keep falling/sliding
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Clear "dead" so animator can return to locomotion
        playerAnim?.SetDead(false);

        // Force animator to refresh immediately (prevents sticking on death frame)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // Re-enable movement
        if (movement != null) movement.enabled = true;

        isDead = false;

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.RestoreToCheckpointSnapshot();
    }

    public void SetInvulnerable(bool value)
    {
        invulnerable = value;
    }
}