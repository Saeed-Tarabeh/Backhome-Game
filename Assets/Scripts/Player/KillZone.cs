using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Try on the same object, or on parent (common if collider is on a child)
        var health = collision.GetComponent<PlayerHealth>() 
                     ?? collision.GetComponentInParent<PlayerHealth>();

        if (health != null)
            health.TakeDamage(1);
        else
            Debug.LogError("Player entered KillZone but no PlayerHealth found on it or its parents.");
    }
}