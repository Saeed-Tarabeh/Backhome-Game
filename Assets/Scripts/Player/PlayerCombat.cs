using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.9f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackCooldown = 0.4f;

    private float lastAttackTime;
    private PlayerAnimator playerAnim;

    private Collider2D attackHitbox;  // <-- the trigger collider on AttackPoint

    public bool CanReflect { get; private set; }

    private void Awake()
    {
        playerAnim = GetComponent<PlayerAnimator>();

        if (attackPoint != null)
        {
            attackHitbox = attackPoint.GetComponent<Collider2D>();
            if (attackHitbox != null) attackHitbox.enabled = false; // off by default
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            playerAnim?.TriggerAttack1();
            lastAttackTime = Time.time;
        }
    }

    // Animation events:
    public void AnimEvent_ReflectOn()
    {
        CanReflect = true;
        if (attackHitbox != null) attackHitbox.enabled = true;
    }

    public void AnimEvent_ReflectOff()
    {
        CanReflect = false;
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    public void AnimEvent_AttackHit()
    {
        Attack();
    }

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (var enemy in hitEnemies)
            enemy.GetComponent<EnemyHealth>()?.Die();
    }
}