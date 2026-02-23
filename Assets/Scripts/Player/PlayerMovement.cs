using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float deceleration = 45f;
    [SerializeField] private float airControlMultiplier = 0.55f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBuffer = 0.12f;

    [Header("Amount of Jumps")]
    [SerializeField] private int maxJumps = 2;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask groundLayer;

    private float airTimer;
    private Rigidbody2D rb;
    private Collider2D col;

    private float moveInput;
    private bool isGrounded;
    
    private PlayerAnimator playerAnim;

    private float coyoteCounter;
    private float jumpBufferCounter;

    private int jumpsRemaining;
    private bool wasGrounded;



    private readonly Collider2D[] groundHits = new Collider2D[8];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerAnim = GetComponent<PlayerAnimator>();

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");

        // Create ground check
        if (groundCheck == null)
        {
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            groundCheck = gc.transform;
        }

        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0) { transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * moveInput, transform.localScale.y, transform.localScale.z); }

        float feetY = col.bounds.min.y - 0.02f;
        groundCheck.position = new Vector3(transform.position.x, feetY, transform.position.z);

        isGrounded = CheckGrounded();

        // Reset jumps when landed
        if (isGrounded && !wasGrounded)
        {
            if(rb.linearVelocity.y < -20f)GetComponent<PlayerAudio>()?.PlayLand();
            else if(airTimer > 0.35f)GetComponent<PlayerAudio>()?.PlayLand(Mathf.Min(0.7f,Mathf.Max(0.1f,Mathf.Abs(rb.linearVelocity.y)/10f)));
            airTimer = 0f;
            jumpsRemaining = maxJumps;
        }
        wasGrounded = isGrounded;

        // Coyote time
        if (isGrounded){
            coyoteCounter = coyoteTime;
        }
        else{
            coyoteCounter -= Time.deltaTime;
            airTimer += Time.deltaTime;
        }

        // Jump buffer
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBuffer;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Jump
        if (jumpBufferCounter > 0f)
        {
            bool canGroundJump = coyoteCounter > 0f;                 // normal jump (ground/coyote)
            bool canAirJump = !canGroundJump && jumpsRemaining > 0;  // extra jump in air

            // Note: ground jump uses 1 "jump", and air jump uses 1 "jump"
            if ((canGroundJump && jumpsRemaining > 0) || canAirJump)
            {
                GetComponent<PlayerAudio>()?.PlayJump();
                jumpBufferCounter = 0f;

                // If it was a ground/coyote jump, consume coyote so we don't "double count"
                if (canGroundJump) coyoteCounter = 0f;

                jumpsRemaining--;

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                playerAnim?.TriggerJump();
            }
        }

        // Variable jump height
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.55f);
        }
        if (playerAnim != null)
        {
            // Speed normalized 0..1 based on maxSpeed
            float speed01 = Mathf.Clamp01(Mathf.Abs(rb.linearVelocity.x) / maxSpeed);
            playerAnim.SetSpeed01(speed01);
            playerAnim.SetGrounded(isGrounded);
        }
    }

    private void FixedUpdate()
    {
        float targetSpeed = moveInput * maxSpeed;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        if (!isGrounded)
            accelRate *= airControlMultiplier;

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = speedDiff * accelRate;
        rb.AddForce(Vector2.right * movement);

        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
    }

    public bool IsGrounded => isGrounded;
    private bool CheckGrounded()
    {
        // Use a circle to check if we're on the ground
        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundLayer,
            useTriggers = false
        };

        // Check for hits
        int count = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, filter, groundHits);

        // If any of the hits are not our own collider or null, we're grounded
        for (int i = 0; i < count; i++)
        {
            var hit = groundHits[i];
            if (hit != null && hit != col)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}