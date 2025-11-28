using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class Basketball : MonoBehaviour
{
    private Rigidbody2D _rb;

    [Header("Settings")]
    public int damage = 1;
    public int goldenDamage = 50; // Configurable damage for golden ball
    public float pickupDelay = 0.5f; // Time before player can pick it up again
    private bool canBePickedUp = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }



    private void EnablePickup()
    {
        canBePickedUp = true;
    }

    public void Throw(Vector2 force)
    {
        _rb.linearVelocity = Vector2.zero; // Reset previous velocity
        _rb.angularVelocity = 0f;
        _rb.AddForce(force, ForceMode2D.Impulse);
    }

    [Header("Recall")]
    public float recallSpeed = 20f;
    private bool isRecalling = false;
    private Transform recallTarget;
    private float defaultGravity;

    public void StartRecall(Transform target)
    {
        isRecalling = true;
        recallTarget = target;
        defaultGravity = _rb.gravityScale;
        _rb.gravityScale = 0f;
        
        // Optional: Reset velocity so it doesn't fight the recall immediately
        // _rb.linearVelocity = Vector2.zero; 
    }

    public void StopRecall()
    {
        isRecalling = false;
        recallTarget = null;
        _rb.gravityScale = defaultGravity > 0 ? defaultGravity : 1f;
    }

    private void FixedUpdate()
    {
        if (isRecalling && recallTarget != null)
        {
            Vector2 direction = (recallTarget.position - transform.position).normalized;
            _rb.linearVelocity = direction * recallSpeed;
        }
    }

    private bool isGolden = false;
    public bool IsGolden => isGolden; // Public getter for the Hoop to check

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer; // Assign in Inspector or find automatically
    public TrailRenderer trailRenderer;

    public void SetGolden(bool state)
    {
        isGolden = state;
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = isGolden ? Color.yellow : Color.white;
        }
        
        if (trailRenderer != null)
        {
            trailRenderer.startColor = isGolden ? Color.yellow : Color.white;
            trailRenderer.endColor = isGolden ? new Color(1, 0.92f, 0.016f, 0f) : new Color(1, 1, 1, 0f);
        }
    }

    public void SetSprite(Sprite newSprite)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }

    [Header("Effects")]
    public GameObject bounceEffectPrefab; // Assign a particle system prefab
    public AudioClip bounceSound;
    private AudioSource audioSource;

    private void Start()
    {
        // Allow pickup after a short delay so we don't catch it immediately upon throwing
        Invoke(nameof(EnablePickup), pickupDelay);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Play Sound
        if (bounceSound != null && audioSource != null)
        {
            // Adjust volume based on impact speed?
            float speed = collision.relativeVelocity.magnitude;
            audioSource.PlayOneShot(bounceSound, Mathf.Clamp01(speed / 10f));
        }

        // 2. Spawn Particle Effect
        if (bounceEffectPrefab != null)
        {
            // Spawn at the contact point
            ContactPoint2D contact = collision.contacts[0];
            Instantiate(bounceEffectPrefab, contact.point, Quaternion.identity);
        }

        // Check for Player Pickup
        if (canBePickedUp && collision.gameObject.CompareTag("Player"))
        {
            Shooter shooter = collision.gameObject.GetComponentInChildren<Shooter>();
            if (shooter != null)
            {
                shooter.CollectBall();
                Destroy(gameObject); // Remove this ball from the world
                return;
            }
        }

        // If we hit an enemy, deal damage but DO NOT disappear
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Health enemyHealth = collision.gameObject.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Golden ball could do more damage
                int finalDamage = isGolden ? goldenDamage : damage;
                enemyHealth.TakeDamage(finalDamage);
            }
        }
    }
}
