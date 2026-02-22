using UnityEngine;

public class HazardKill : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        var health = collision.collider.GetComponent<PlayerHealth>()
                  ?? collision.collider.GetComponentInParent<PlayerHealth>();

        if (health != null)
            health.TakeDamage(1);
    }
}