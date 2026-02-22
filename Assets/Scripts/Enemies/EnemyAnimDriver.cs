using UnityEngine;

public class EnemyAnimDriver : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyHealth health;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    private void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!health) health = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (!anim) return;

        bool dead = health != null && health.IsDead;
        anim.SetBool(IsDead, dead);

        if (dead) return;

        // moving if horizontal speed is not ~0
        float vx = rb ? Mathf.Abs(rb.linearVelocity.x) : 0f;
        anim.SetBool(IsMoving, vx > 0.05f);
    }
}