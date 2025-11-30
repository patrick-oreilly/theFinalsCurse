using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f; // Faster when chasing
    public float checkRadius = 0.2f; // Size of the check bubble
    public float wallCheckDistance = 1.0f; // Distance to check for walls
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

    private void FixedUpdate()
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

    private void Update()
    {
        // Keep empty or use for non-physics logic like timers if needed
    }

    private void DetectPlayer()
    {
        // 1. Initial Range Check
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        
        if (player != null)
        {
            // Strict Check: Only chase objects tagged "Player"
            if (!player.CompareTag("Player")) return;

            // 2. Line of Sight Check (Prevent seeing through walls)
            Vector2 directionToPlayer = player.transform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;
            
            // Raycast from enemy to player. We use 'groundLayer' to check for walls/obstacles.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, groundLayer);

            if (hit.collider == null)
            {
                // No wall in the way!
                playerTransform = player.transform;
                isChasing = true;
                return;
            }
        }

        // If player null OR wall in the way
        isChasing = false;
        playerTransform = null;
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
        // 1. Edge Check (Cliff)
        // Raycast DOWN from the groundDetection point (which should be IN FRONT of the enemy)
        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, checkRadius + 0.5f, groundLayer);
        
        // 2. Wall Check
        // Raycast FORWARD from the BODY CENTER (transform.position)
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallInfo = Physics2D.Raycast(transform.position, direction, wallCheckDistance, groundLayer);

        // If NO ground detected ahead (Cliff) OR Wall detected
        if (groundInfo.collider == null || wallInfo.collider != null)
        {
            if (wallInfo.collider != null)
            {
                Debug.Log($"Wall Detected: {wallInfo.collider.name}. IsChasing: {isChasing}");
            }

            if (isChasing)
            {
                // Stop if chasing to avoid falling
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                Flip();
            }
        }
    }

    private float flipCooldown = 0.5f;
    private float lastFlipTime;

    private void Flip()
    {
        if (Time.time < lastFlipTime + flipCooldown) return; // Prevent rapid flipping

        Debug.Log("Flipping Direction!");
        movingRight = !movingRight;
        lastFlipTime = Time.time;
        
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
            Gizmos.DrawLine(groundDetection.position, groundDetection.position + Vector3.down * (checkRadius + 0.5f));

            // Draw Wall Check (Line) from BODY CENTER
            Vector3 direction = movingRight ? Vector3.right : Vector3.left;
            if (!Application.isPlaying) direction = transform.right; // Approximation in editor
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + direction * wallCheckDistance);
        }
    }
}
