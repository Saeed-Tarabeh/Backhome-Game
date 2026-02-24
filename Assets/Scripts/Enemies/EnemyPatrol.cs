using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool startMovingRight = false;
    [SerializeField] private float turnPauseSeconds = 1f;

    [Header("Edge / Wall Detection")]
    [SerializeField] private Transform groundCheck; // front-foot position
    [SerializeField] private Vector2 groundBoxSize = new Vector2(0.22f, 0.12f); // tweak
    [SerializeField] private float groundBoxForwardOffset = 0.15f; // push box slightly forward
    [SerializeField] private float groundBoxDownOffset = 0.1f;
    [SerializeField] private Transform wallCheck; // front side position
    [SerializeField] private Vector2 wallBoxSize = new Vector2(0.12f, 0.35f); // tweak
    [SerializeField] private float wallBoxForwardOffset = 0.06f; // push box slightly forward

    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private EnemyHealth health;

    private bool movingRight;
    private bool isTurning;
    private Coroutine turnRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();

        movingRight = startMovingRight;

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");

        if (groundCheck == null)
        {
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0.35f, -0.5f, 0f);
            groundCheck = gc.transform;
        }

        if (wallCheck == null)
        {
            var wc = new GameObject("WallCheck");
            wc.transform.SetParent(transform);
            wc.transform.localPosition = new Vector3(0.45f, -0.15f, 0f);
            wallCheck = wc.transform;
        }

        ApplyFacing();
        AlignChecksToFacing();
    }

    private void FixedUpdate()
    {
        if (rb == null || !rb.simulated) return;

        if (isTurning)
        {
            // Keep vertical motion (gravity), but stop horizontal during pause
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dir = movingRight ? 1f : -1f;

        bool hasGroundAhead = CheckGroundAhead();
        bool hitWallAhead = CheckWallAhead(dir);

        if (!hasGroundAhead || hitWallAhead)
        {
            StartTurnPause();
            return;
        }

        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    private bool CheckGroundAhead()
    {
        float dir = movingRight ? 1f : -1f;

        Vector2 center =
            (Vector2)groundCheck.position
            + Vector2.right * dir * groundBoxForwardOffset
            + Vector2.down * groundBoxDownOffset;

        Collider2D hit = Physics2D.OverlapBox(center, groundBoxSize, 0f, groundLayer);
        return hit != null;
    }

    private bool CheckWallAhead(float dir)
    {
        // Box centered slightly forward from the wallCheck, in facing direction
        Vector2 center = (Vector2)wallCheck.position + Vector2.right * dir * wallBoxForwardOffset;
        Collider2D hit = Physics2D.OverlapBox(center, wallBoxSize, 0f, groundLayer);
        return hit != null;
    }

    private void StartTurnPause()
    {
        if (turnRoutine != null) return;
        turnRoutine = StartCoroutine(TurnPauseThenFlip());
    }

    private IEnumerator TurnPauseThenFlip()
    {
        isTurning = true;

        // immediately stop pushing into wall/edge
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // wait
        yield return new WaitForSeconds(turnPauseSeconds);

        // flip + resume
        movingRight = !movingRight;
        ApplyFacing();
        AlignChecksToFacing();

        isTurning = false;
        turnRoutine = null;
    }

    private void ApplyFacing()
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (movingRight ? -1f : 1f);
        transform.localScale = s;
    }

    private void AlignChecksToFacing()
    {
        if (groundCheck != null)
        {
            var p = groundCheck.localPosition;
            p.x = Mathf.Abs(p.x) * (movingRight ? 1f : -1f);
            groundCheck.localPosition = p;
        }

        if (wallCheck != null)
        {
            var p = wallCheck.localPosition;
            p.x = Mathf.Abs(p.x) * (movingRight ? 1f : -1f);
            wallCheck.localPosition = p;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            float dir = (transform.localScale.x >= 0f) ? 1f : -1f;

            Vector3 center =
                groundCheck.position
                + Vector3.right * dir * groundBoxForwardOffset
                + Vector3.down * groundBoxDownOffset;

            Gizmos.DrawWireCube(center, groundBoxSize);
        }

        if (wallCheck != null)
        {
            float dir = (transform.localScale.x >= 0f) ? 1f : -1f;
            Vector3 center = wallCheck.position + Vector3.right * dir * wallBoxForwardOffset;
            Gizmos.DrawWireCube(center, wallBoxSize);
        }
    }
}