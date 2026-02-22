using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    static readonly int Speed = Animator.StringToHash("Speed");
    static readonly int Grounded = Animator.StringToHash("Grounded");
    static readonly int Defending = Animator.StringToHash("Defending");
    static readonly int Dead = Animator.StringToHash("Dead");

    static readonly int Jump = Animator.StringToHash("Jump");
    static readonly int Attack1 = Animator.StringToHash("Attack1");
    static readonly int Hurt = Animator.StringToHash("Hurt");

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    public void SetSpeed01(float speed01) => animator.SetFloat(Speed, speed01);
    public void SetGrounded(bool grounded) => animator.SetBool(Grounded, grounded);
    public void SetDefending(bool defending) => animator.SetBool(Defending, defending);

    public void TriggerJump() => animator.SetTrigger(Jump);
    public void TriggerAttack1() => animator.SetTrigger(Attack1);
    public void TriggerHurt() => animator.SetTrigger(Hurt);

    public void SetDead(bool dead) => animator.SetBool(Dead, dead);
}