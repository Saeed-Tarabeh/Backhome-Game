using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WalkerCrawlSFX : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform player;            
    [SerializeField] private Rigidbody2D rb;                   

    [Header("When to play")]
    [SerializeField] private float minMoveSpeed = 0.05f;      
    [SerializeField] private bool useVelocityCheck = true;     

    [Header("Distance volume")]
    [SerializeField] private float maxHearingDistance = 12f;   
    [SerializeField] private float minHearingDistance = 2f;    
    [SerializeField] private float maxVolume = 0.9f;           
    [SerializeField] private float volumeSmooth = 10f;         

    private AudioSource src;
    private float targetVolume;

    private void Awake()
    {
        src = GetComponent<AudioSource>();

        if (rb == null) rb = GetComponent<Rigidbody2D>();

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

        // Distance-based volume
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