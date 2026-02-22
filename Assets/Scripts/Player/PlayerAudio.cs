using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip[] hurtClips;

    [Header("Volume")]
    [SerializeField] private float footstepVolume = 0.6f;
    [SerializeField] private float jumpVolume = 0.8f;
    [SerializeField] private float attackVolume = 0.9f;
    [SerializeField] private float hurtVolume = 1f;

    // Called from animation event
    public void PlayFootstep()
    {
        // block footsteps while defending (or while movement is disabled)
        var defend = GetComponentInParent<PlayerDefend>();
        if (defend != null && defend.IsDefending) return;

        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        SFXManager.Instance?.Play(clip, footstepVolume, 0.95f, 1.05f);
    }

    public void PlayJump()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.Play(jumpClip, jumpVolume, 0.95f, 1.05f);
    }

    public void PlayLand(float landVolume = 0.8f)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.Play(landClip, landVolume, 0.95f, 1.05f);
    }
    public void PlayAttack()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.Play(attackClip, attackVolume, 0.95f, 1.05f);
    }

    public void PlayHurt()
    {
        if (hurtClips.Length == 0) return;

        AudioClip clip = hurtClips[Random.Range(0, hurtClips.Length)];

        if (SFXManager.Instance != null)
            SFXManager.Instance.Play(clip, hurtVolume, 0.95f, 1.05f);
    }
}