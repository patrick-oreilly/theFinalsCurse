using UnityEngine;
using UnityEngine.InputSystem;

public class player_movement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    bool isFacingRight = true;
    public ParticleSystem smokeFX;

    [Header("Movement")]
    public float speed = 5f;
    float moveX;

    [Header("Jumping")]
    public float jumpForce = 10f;
    public int maxJumps = 2;
    private int jumpsRemaining = 0;

    [Header("GroundCheck")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    private bool isGrounded = false;

    [Header("WallCheck")]
    public Transform wallCheck;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;

    [Header("WallSlide")]
    public float wallSlideSpeed = 2f;
    bool isWallSliding = false;
    bool isWallJumping = false;

    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;

    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    [Header("Fall Damage")]
    public float fallDamageThreshold = 20f; // Increased to prevent damage from normal jumps
    public int minFallDamage = 10;
    public float damageMultiplier = 2f;
    private float lastYVelocity;
    private Health health;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 30f; // Increased to allow for faster, dangerous falls
    public float fallSpeedMultiplier = 2.5f;

    void Start()
    {
        health = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        // Track velocity BEFORE physics updates change it (for landing detection)
        // We do this at the start of Update or FixedUpdate, but since we check Grounding in Update, let's track it here.
        // Actually, checking just before GroundCheck is safer.
        
        lastYVelocity = rb.linearVelocity.y;

        GroundCheck();
        processWallSlide();
        processWallJump();
        Gravity();
        if (!isWallJumping && !isKnockedBack)
        {
            rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
            flip();

        }
        animator.SetFloat("yVelocity", rb.linearVelocityY);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        animator.SetBool("isWallSliding", isWallSliding);
    }

    private bool isKnockedBack = false;

    public void ApplyKnockback(Vector2 force, float duration)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero; // Stop current movement
        rb.AddForce(force, ForceMode2D.Impulse);
        Invoke(nameof(StopKnockback), duration);
    }

    private void StopKnockback()
    {
        isKnockedBack = false;
    }

    private void Gravity()
    {
        if (rb.linearVelocity.y < 0 && !isWallSliding)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
        }
        else
        {
            rb.gravityScale = baseGravity;
        }

        // D. Apply the Max Fall Speed Cap (This is the only line of manual velocity setting needed)
        // We cap the velocity after gravity has had a chance to accelerate the player
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>();
        moveX = movementInput.x;
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0)
        {
            if (context.performed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
                JumpEffects();

            }
            else if (context.canceled && rb.linearVelocityY > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                jumpsRemaining--;
                JumpEffects();

            }
        }
        // Wall jump section
        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y); // jump away from the wall
            wallJumpTimer = 0;
            JumpEffects();

            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(cancelWallJump), wallJumpTime + 0.1f); // wall jump should last 0.5 seconds, jump again in 0.6
        }
    }

    private void JumpEffects()
    {
        animator.SetTrigger("jump");
        smokeFX.Play();
    }

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        
        if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;

            // Check for Fall Damage upon landing
            if (!wasGrounded && lastYVelocity < -fallDamageThreshold)
            {
                ApplyFallDamage(Mathf.Abs(lastYVelocity));
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    private void ApplyFallDamage(float impactSpeed)
    {
        if (health != null)
        {
            // Calculate damage: (Impact Speed - Threshold) * Multiplier
            float excessSpeed = impactSpeed - fallDamageThreshold;
            int damage = minFallDamage + Mathf.RoundToInt(excessSpeed * damageMultiplier);
            
            Debug.Log($"Hard Landing! Speed: {impactSpeed}, Damage: {damage}");
            health.TakeDamage(damage);
        }
    }

    private void flip()
    {
        if (isFacingRight && moveX < 0 || !isFacingRight && moveX > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;

            if (rb.linearVelocityY == 0)
            {
                smokeFX.Play();
            }
        }
    }

    private bool WallCheck()
    {
        bool hit = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);
        // Debug.Log($"WallCheck: {hit}"); // Uncomment to debug
        return hit;
    }

    private void processWallSlide()
    {
        if (!isGrounded && WallCheck() && moveX != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void processWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(cancelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }
    private void cancelWallJump()
    {
        isWallJumping = false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);

    }
}
