using UnityEngine;

public class Hoop : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object is a basketball
        Basketball ball = collision.GetComponent<Basketball>();
        
        if (ball != null)
        {
            if (ball.IsGolden)
            {
                Debug.Log("HOOP! YOU WIN!");
                
                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake(1.0f); // Big shake for winning!
                }

                // Notify Game Manager
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.LevelComplete();
                }
            }
            else
            {
                Debug.Log("Ball is not Golden yet! Collect all coins first.");
            }
        }
    }
}
