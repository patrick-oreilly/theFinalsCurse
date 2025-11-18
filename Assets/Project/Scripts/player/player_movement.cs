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

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2.5f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        processWallSlide();
        processWallJump();
        Gravity();
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
            flip();

        }
        animator.SetFloat("yVelocity", rb.linearVelocityY);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        animator.SetBool("isWallSliding", isWallSliding);
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
        if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
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
        return Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);

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
