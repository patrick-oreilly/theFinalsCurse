using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;
    private Health health;
    private EnemyCombat combat;
    private EnemyPatrol patrol;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        combat = GetComponent<EnemyCombat>();
        patrol = GetComponent<EnemyPatrol>();
    }

    private void Start()
    {
        if (health != null)
        {
            health.OnHealthChanged.AddListener(OnHurt);
            health.OnDeath.AddListener(OnDie);
        }

        if (combat != null)
        {
            combat.OnAttack.AddListener(OnAttack);
        }
    }

    private void OnHurt(int currentHealth)
    {
        // Don't play hurt animation if dead (0 health)
        if (currentHealth > 0 && animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    private void OnDie()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Disable movement
        if (patrol != null)
        {
            patrol.StopPatrol();
        }

        // Disable collider so player can walk through dead body
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Disable physics (so it doesn't fall through floor if we disable collider, 
        // actually we probably want to keep collider for floor but ignore player... 
        // simpler to just make it kinematic)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
}
