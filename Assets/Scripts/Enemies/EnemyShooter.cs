using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform target; // set to player transform, or auto-find

    [Header("Shooting")]
    [SerializeField] private float shootInterval = 1.2f;
    [SerializeField] private float shootRange = 8f;
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private LayerMask losBlockers; // Ground layer usually

    [SerializeField] private AudioClip shootClip;
    [SerializeField] private float shootVolume = 0.8f;

    private float nextShootTime;
    private EnemyHealth health;

    private void Awake()
    {
        if (!firePoint) firePoint = transform;
        if (!target && PlayerHealth.Instance) target = PlayerHealth.Instance.transform;
        health = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (health != null && health.IsDead) return;
        if (!projectilePrefab || !target) return;
        if (Time.time < nextShootTime) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > shootRange) return;

        if (requireLineOfSight)
        {
            Vector2 from = firePoint.position;
            Vector2 to = target.position;
            Vector2 dir = (to - from).normalized;
            float len = Vector2.Distance(from, to);

            RaycastHit2D hit = Physics2D.Raycast(from, dir, len, losBlockers);
            if (hit.collider != null) return; // blocked
        }

        Shoot();
        nextShootTime = Time.time + shootInterval;
    }

    private void Shoot()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.Play(shootClip, 0.8f, 0.95f, 1.05f);
        Vector2 dir = (target.position - firePoint.position).normalized;

        EnemyProjectile p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        p.Launch(dir);
    }
}