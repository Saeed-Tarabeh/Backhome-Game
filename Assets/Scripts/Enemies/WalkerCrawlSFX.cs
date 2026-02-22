using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WalkerCrawlSFX : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform player;                 // drag Player here (or auto-find by tag)
    [SerializeField] private Rigidbody2D rb;                   // optional: if you want velocity-based check

    [Header("When to play")]
    [SerializeField] private float minMoveSpeed = 0.05f;       // how fast counts as "moving"
    [SerializeField] private bool useVelocityCheck = true;     // true = rb velocity; false = animator param / custom

    [Header("Distance volume")]
    [SerializeField] private float maxHearingDistance = 12f;   // at this distance -> volume 0
    [SerializeField] private float minHearingDistance = 2f;    // inside this -> full volume
    [SerializeField] private float maxVolume = 0.9f;           // volume when close
    [SerializeField] private float volumeSmooth = 10f;         // smoothing speed

    private AudioSource src;
    private float targetVolume;

    private void Awake()
    {
        src = GetComponent<AudioSource>();

        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Make sure looping behavior is correct
        src.loop = true;
        src.playOnAwake = false;
    }

    private void Update()
    {
        bool isMoving = IsWalkerMoving();

        // Start/stop clip based on movement
        if (isMoving)
        {
            if (!src.isPlaying)
                src.Play();
        }
        else
        {
            if (src.isPlaying)
                src.Stop();
        }

        // Distance-based volume (even while playing)
        targetVolume = ComputeDistanceVolume();
        src.volume = Mathf.MoveTowards(src.volume, targetVolume, volumeSmooth * Time.deltaTime);
    }

    private bool IsWalkerMoving()
    {
        if (useVelocityCheck && rb != null)
        {
            // mostly care about horizontal motion for a crawler
            return Mathf.Abs(rb.linearVelocity.x) > minMoveSpeed;
        }

        // If you prefer: return your own “isWalking” boolean from EnemyPatrol, Animator param, etc.
        return false;
    }

    private float ComputeDistanceVolume()
    {
        if (player == null) return 0f;

        float d = Vector2.Distance(transform.position, player.position);

        // Full volume when close, fade out to 0 by max distance
        float t = Mathf.InverseLerp(maxHearingDistance, minHearingDistance, d); // 0 far -> 1 close
        float v = Mathf.Lerp(0f, maxVolume, t);

        return v;
    }
}