using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 10;
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnAttack;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Trigger Attack Event (Animation)
            OnAttack?.Invoke();

            // Deal Damage
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }

            // Apply Knockback
            player_movement playerMove = collision.gameObject.GetComponent<player_movement>();
            if (playerMove != null)
            {
                // Calculate direction: away from the enemy
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                // Add a bit of upward force for a nice arc
                direction += Vector2.up * 0.5f; 
                
                playerMove.ApplyKnockback(direction * knockbackForce, knockbackDuration);
            }
        }
    }
}
