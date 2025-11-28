using UnityEngine;

public class Hoop : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip swishSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object is a basketball
        Basketball ball = collision.GetComponent<Basketball>();
        
        if (ball != null)
        {
            // Play Swish Sound
            if (swishSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(swishSound);
            }

            if (ball.IsGolden)
            {
                Debug.Log("HOOP! YOU WIN!");
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayWinSound();
                }

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
