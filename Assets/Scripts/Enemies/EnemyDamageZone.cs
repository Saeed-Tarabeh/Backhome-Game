using UnityEngine;

public class EnemyDamageZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Tooltip("Seconds between damage hits while overlapping.")]
    [SerializeField] private float hitCooldown = 0.5f;

    [Tooltip("If set, only objects on this layer can be damaged (recommended).")]
    [SerializeField] private LayerMask playerLayer;

    private float nextHitTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextHitTime) return;

        // Optional: layer filter (if you set playerLayer in inspector)
        if (playerLayer.value != 0)
        {
            int otherLayerBit = 1 << other.gameObject.layer;
            if ((playerLayer.value & otherLayerBit) == 0) return;
        }

        // Try getting PlayerHealth from the collider or its parents (common if collider is on a child)
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(damage);
        nextHitTime = Time.time + hitCooldown;
    }
}