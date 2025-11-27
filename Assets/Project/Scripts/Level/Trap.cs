using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Trap Settings")]
    public int damage = 20;
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Deal Damage
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Apply Knockback
            player_movement playerMove = collision.GetComponent<player_movement>();
            if (playerMove != null)
            {
                // Calculate direction: away from the trap center
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                // Ensure we always knock them UP a bit so they don't get stuck in the floor
                direction.y = Mathf.Abs(direction.y) + 0.5f; 
                direction = direction.normalized;

                playerMove.ApplyKnockback(direction * knockbackForce, knockbackDuration);
            }
        }
    }
}
