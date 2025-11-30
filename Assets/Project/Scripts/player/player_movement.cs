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
    public float fallDamageThreshold = 15f; // Lowered from 20f to make it easier to trigger
    public int minFallDamage = 10;
    public float damageMultiplier = 2f;
    private Health health;
    // Track velocity while airborne to detect impact speed even if physics zeroes velocity on landing
    private float airVelocity;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 30f; // Increased to allow for faster, dangerous falls
    public float fallSpeedMultiplier = 2.5f;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip footstepSound;
    public float footstepInterval = 0.3f;
    private float footstepTimer;
    private AudioSource audioSource;

    void Start()
    {
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Capture velocity while airborne
        // IMPORTANT: We ignore 0 velocity here because if the physics engine stops the player
        // on the same frame we run this, we don't want to overwrite our high fall speed with 0
        // before the GroundCheck triggers the damage.
        if (!isGrounded && rb.linearVelocity.y < -0.1f)
        {
            airVelocity = rb.linearVelocity.y;
        }

        GroundCheck();
        processWallSlide();
        processWallJump();
        Gravity();
        
        if (!isWallJumping && !isKnockedBack)
        {
            rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
            flip();
            
            // Footstep Logic
            if (isGrounded && Mathf.Abs(moveX) > 0.1f)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0)
                {
                    if (footstepSound != null && audioSource != null)
                    {
                        // Randomize pitch slightly for variety
                        audioSource.pitch = Random.Range(0.9f, 1.1f);
                        audioSource.PlayOneShot(footstepSound, 0.5f); // Lower volume
                        audioSource.pitch = 1f; // Reset
                    }
                    footstepTimer = footstepInterval;
                }
            }
            else
            {
                footstepTimer = 0.1f; // Reset so it plays immediately when starting to move
            }
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

    public Vector2 currentInput;

    public void Move(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
        moveX = currentInput.x;
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
            else if (context.canceled && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                // jumpsRemaining--; // Wait, do we consume a jump on cancel? Usually not.
                // JumpEffects(); // REMOVED: Don't play sound/effects on release
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
        
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        
        if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer))
        {
            // Fix for Triple Jump Bug:
            // Only reset jumps if we are NOT moving upwards.
            // This prevents the game from resetting our jumps immediately after we press jump
            // but before we physically leave the ground trigger.
            if (rb.linearVelocity.y < 0.1f)
            {
                jumpsRemaining = maxJumps;
            }
            isGrounded = true;

            // Check for Fall Damage upon landing using tracked airVelocity
            if (!wasGrounded)
            {
                // Debug.Log($"Landed! Impact Speed: {airVelocity}"); // Uncomment to debug impact speeds
                if (airVelocity < -fallDamageThreshold)
                {
                    ApplyFallDamage(Mathf.Abs(airVelocity));
                }
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
            
            // Debug.Log($"Hard Landing! Speed: {impactSpeed}, Damage: {damage}");
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
