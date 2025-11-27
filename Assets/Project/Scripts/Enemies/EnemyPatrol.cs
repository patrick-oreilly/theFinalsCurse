using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f; // Faster when chasing
    public float checkRadius = 0.2f; // Size of the check bubble
    public Transform groundDetection;
    public LayerMask groundLayer; 
    
    [Header("Player Detection")]
    public float detectionRange = 5f;
    public LayerMask playerLayer;
    private Transform playerTransform;
    private bool isChasing = false;
    
    private bool movingRight = true;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private float chaseStopTimer;
    public float chaseStopDelay = 1.0f; // Keep chasing for 1 second after losing player

    private void Update()
    {
        // 1. Check for Player
        DetectPlayer();

        if (isChasing && playerTransform != null)
        {
            ChasePlayer();
            chaseStopTimer = chaseStopDelay; // Reset timer while seeing player
        }
        else if (chaseStopTimer > 0)
        {
            // Keep moving in the last direction for a bit
            chaseStopTimer -= Time.deltaTime;
            
            // Ensure we keep moving in the current facing direction
            float moveDirection = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(moveDirection * chaseSpeed, rb.linearVelocity.y);
            
            CheckForEdges();
        }
        else
        {
            Patrol();
        }
    }

    private void DetectPlayer()
    {
        // Simple distance check or OverlapCircle
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        
        if (player != null)
        {
            playerTransform = player.transform;
            isChasing = true;
        }
        else
        {
            isChasing = false;
            playerTransform = null;
        }
    }

    private void ChasePlayer()
    {
        // Determine direction to player
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        
        // Move towards player
        float moveDirection = directionToPlayer > 0 ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveDirection * chaseSpeed, rb.linearVelocity.y);

        // Face the player
        if (moveDirection > 0 && !movingRight) Flip();
        else if (moveDirection < 0 && movingRight) Flip();
        
        // Optional: Stop chasing if about to fall off a cliff (Safety Check)
        // You can copy the cliff check here if you want them to be smart, 
        // or leave it out if you want them to recklessly jump off ledges for you.
        CheckForEdges(); 
    }

    private void Patrol()
    {
        // Move the enemy
        float moveDirection = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveDirection * patrolSpeed, rb.linearVelocity.y);

        CheckForEdges();
    }

    private void CheckForEdges()
    {
        // Check for edges (Ground Check)
        bool isGrounded = Physics2D.OverlapCircle(groundDetection.position, checkRadius, groundLayer);
        
        // Check for walls (Wall Check)
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallInfo = Physics2D.Raycast(groundDetection.position, direction, checkRadius * 2f, groundLayer);

        // If NO ground detected (Cliff) OR Wall detected
        if (!isGrounded || wallInfo.collider != null)
        {
            // If we are chasing, we might want to be braver, but for now let's stick to safety.
            // The issue might be that we flip, then immediately detect a cliff on the other side if the collider is weird.
            
            // Only flip if we are actually moving into the danger zone
            if (isChasing)
            {
                // If chasing, stop moving instead of flipping endlessly
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        
        if (movingRight)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
        }
    }

    public void StopPatrol()
    {
        enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        // Draw Detection Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (groundDetection != null)
        {
            // Draw Ground Check (Sphere)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundDetection.position, checkRadius);

            // Draw Wall Check (Line)
            Vector3 direction = movingRight ? Vector3.right : Vector3.left;
            if (!Application.isPlaying) direction = transform.right;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundDetection.position, groundDetection.position + direction * (checkRadius * 2f));
        }
    }
}
