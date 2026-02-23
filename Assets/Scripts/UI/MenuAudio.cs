using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuAudio : MonoBehaviour
{
    public static MenuAudio Instance;

    [Header("Clips")]
    [SerializeField] private AudioClip highlightClip;
    [SerializeField] private AudioClip clickClip;

    [Header("Volume")]
    [SerializeField] private float volume = 0.8f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.ignoreListenerPause = true;
    }

    public void PlayHighlight()
    {
        if (highlightClip != null)
            audioSource.PlayOneShot(highlightClip, volume);
    }

    public void PlayClick()
    {
        if (clickClip != null)
            audioSource.PlayOneShot(clickClip, volume);
    }
}