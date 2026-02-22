using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Play(AudioClip clip, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (clip == null) return;

        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, volume);
        sfxSource.pitch = 1f;
    }
}