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

        // Determine Shot Type based on Input
        Vector2 selectedForce = straightThrow;
        if (playerMovement != null && playerMovement.currentInput.y > 0.5f)
        {
            selectedForce = archedThrow;
        }

        // Calculate direction based on facing direction
        float xForce = isFacingRight ? selectedForce.x : -selectedForce.x;
        Vector2 finalForce = new Vector2(xForce, selectedForce.y);
        
        ball.Throw(finalForce);
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
