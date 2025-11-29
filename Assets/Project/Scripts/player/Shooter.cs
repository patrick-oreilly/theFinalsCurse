using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    public Basketball basketballPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public Vector2 straightThrow = new Vector2(15f, 5f);
    public Vector2 archedThrow = new Vector2(8f, 15f);
    
    // Ammo Mechanic
    public int maxAmmo = 1;
    public int currentAmmo;
    
    // Track the active ball for recall
    private Basketball activeBall;
    
    // Event for UI to listen to
    public event System.Action<int> OnAmmoChanged;

    private bool isFacingRight = true; // Track facing direction
    private player_movement playerMovement;

    [Header("Animation")]
    public Animator animator;

    [Header("Audio")]
    public AudioClip throwSound;
    public AudioClip pickupSound;
    private AudioSource audioSource;

    private void Awake()
    {
        currentAmmo = maxAmmo; // Start with full ammo
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // If animator is not assigned manually, try to find it on the parent (Player)
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }
        playerMovement = GetComponentInParent<player_movement>();
    }
    
    private void Start()
    {
        // Notify UI at start
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    [Header("Aiming")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public Vector2 minThrowForce = new Vector2(10f, 2f); // Low arc
    public Vector2 maxThrowForce = new Vector2(5f, 15f); // High arc
    [Range(0, 1)] public float currentAimT = 0.5f; // 0 = min (straight), 1 = max (high arc)
    public float aimAdjustSpeed = 2f;
    public LayerMask trajectoryCollisionMask; // Layers that stop the line (Ground, Walls)

    private void Update()
    {
        // Simple check to see which way the player is facing based on localScale
        if (transform.parent != null)
        {
            isFacingRight = transform.parent.localScale.x > 0;
        }
        else
        {
             isFacingRight = transform.localScale.x > 0;
        }

        // Aim Adjustment (W = Up, S = Down)
        if (playerMovement != null)
        {
            float verticalInput = playerMovement.currentInput.y;
            if (verticalInput > 0.1f)
            {
                currentAimT += aimAdjustSpeed * Time.deltaTime;
            }
            else if (verticalInput < -0.1f)
            {
                currentAimT -= aimAdjustSpeed * Time.deltaTime;
            }
            currentAimT = Mathf.Clamp01(currentAimT);
        }

        // Toggle Aim Line (Press 'T')
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            isAimingVisible = !isAimingVisible;
            Debug.Log($"Aiming Toggled: {isAimingVisible}");
        }

        // Draw Trajectory if we have ammo AND aiming is visible
        if (currentAmmo > 0 && isAimingVisible)
        {
            DrawTrajectory();
        }
        else
        {
            if (trajectoryLine != null) trajectoryLine.positionCount = 0;
        }
    }

    private bool isAimingVisible = true; // Default to visible

    private void DrawTrajectory()
    {
        if (trajectoryLine == null) return;

        Vector2 force = GetCurrentThrowForce();
        Vector2 startPos = firePoint.position;
        Vector2[] points = new Vector2[trajectoryResolution];
        
        Vector2 previousPos = startPos;
        int validPointCount = 0;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float t = i * 0.1f; // Time step
            // Physics formula: p(t) = p0 + v0*t + 0.5*g*t^2
            Vector2 pos = startPos + force * t + 0.5f * Physics2D.gravity * (t * t);

            // Check for collision between previous point and new point
            if (i > 0) // Skip first point check or check from startPos
            {
                RaycastHit2D hit = Physics2D.Linecast(previousPos, pos, trajectoryCollisionMask);
                if (hit.collider != null)
                {
                    // We hit something! Stop the line here.
                    points[i] = hit.point;
                    validPointCount = i + 1;
                    break; 
                }
            }

            points[i] = pos;
            validPointCount = i + 1;
            previousPos = pos;
        }

        trajectoryLine.positionCount = validPointCount;
        for (int i = 0; i < validPointCount; i++)
        {
            trajectoryLine.SetPosition(i, points[i]);
        }
        
        // Debugging: Print once per second to avoid spam
        if (Time.frameCount % 60 == 0) 
        {
            Debug.Log($"Drawing Line: {validPointCount} points. Start: {points[0]}, End: {points[validPointCount-1]}");
        }
    }

    private Vector2 GetCurrentThrowForce()
    {
        // Lerp between straight and high arc based on aim
        Vector2 baseForce = Vector2.Lerp(minThrowForce, maxThrowForce, currentAimT);
        
        // Flip X based on facing direction
        float xForce = isFacingRight ? baseForce.x : -baseForce.x;
        return new Vector2(xForce, baseForce.y);
    }

    // This method is called by the Input System (PlayerInput component)
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else if (activeBall != null)
            {
                activeBall.StartRecall(transform);
            }
        }
        else if (context.canceled)
        {
            if (activeBall != null)
            {
                activeBall.StopRecall();
            }
        }
    }

    public void OnToggleAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isAimingVisible = !isAimingVisible;
            Debug.Log($"Aiming Toggled: {isAimingVisible}");
        }
    }

    private void Shoot()
    {
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        else
        {
            // Fallback if no animator: spawn immediately
            SpawnBall();
        }
    }

    [Header("Visuals")]
    public Sprite regularBallSprite;
    public Sprite goldenBallSprite;

    private bool isGoldenMode = false;

    public void EnableGoldenMode()
    {
        isGoldenMode = true;
        Debug.Log("Golden Mode Activated!");
    }
    
    public void CollectBall()
    {
        if (currentAmmo < maxAmmo)
        {
            currentAmmo++;
            activeBall = null; // Clear reference since it's destroyed
            OnAmmoChanged?.Invoke(currentAmmo);
            
            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            
            Debug.Log($"Ball Collected! Ammo: {currentAmmo}");
        }
    }

    // This method will be called by the Animation Event
    public void SpawnBall()
    {
        if (currentAmmo <= 0) return; // Double check

        if (throwSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(throwSound);
        }

        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo);

        Basketball ball = Instantiate(basketballPrefab, firePoint.position, Quaternion.identity);
        activeBall = ball; // Track the new ball
        
        ball.SetGolden(isGoldenMode); // Apply golden state logic
        
        // Apply sprite
        if (isGoldenMode && goldenBallSprite != null)
        {
            ball.SetSprite(goldenBallSprite);
        }
        else if (regularBallSprite != null)
        {
            ball.SetSprite(regularBallSprite);
        }

        // Use the calculated aim force
        ball.Throw(GetCurrentThrowForce());
    }
    public void ResetAmmo()
    {
        // Destroy the active ball if it exists in the world
        if (activeBall != null)
        {
            Destroy(activeBall.gameObject);
            activeBall = null;
        }

        // Restore full ammo
        currentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo);
    }
}
