using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    public Basketball basketballPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public Vector2 throwForce = new Vector2(10f, 10f);
    
    // Ammo Mechanic
    public int maxAmmo = 1;
    public int currentAmmo;
    
    // Track the active ball for recall
    private Basketball activeBall;
    
    // Event for UI to listen to
    public event System.Action<int> OnAmmoChanged;

    private bool isFacingRight = true; // Track facing direction

    [Header("Animation")]
    public Animator animator;

    private void Awake()
    {
        currentAmmo = maxAmmo; // Start with full ammo
        
        // If animator is not assigned manually, try to find it on the parent (Player)
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }
    }
    
    private void Start()
    {
        // Notify UI at start
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    private void Update()
    {
        // Simple check to see which way the player is facing based on localScale
        // This assumes the Player object (parent) flips scale.x when turning.
        if (transform.parent != null)
        {
            isFacingRight = transform.parent.localScale.x > 0;
        }
        else
        {
             isFacingRight = transform.localScale.x > 0;
        }
    }

    // This method is called by the Input System (PlayerInput component)
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
            // Optional: Play catch sound
            Debug.Log($"Ball Collected! Ammo: {currentAmmo}");
        }
    }

    // This method will be called by the Animation Event
    public void SpawnBall()
    {
        if (currentAmmo <= 0) return; // Double check

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

        // Calculate direction based on facing direction
        float xForce = isFacingRight ? throwForce.x : -throwForce.x;
        Vector2 finalForce = new Vector2(xForce, throwForce.y);
        
        ball.Throw(finalForce);
    }
}
