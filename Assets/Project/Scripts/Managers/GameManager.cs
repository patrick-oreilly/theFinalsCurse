using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    int progress;
    public Shooter playerShooter; // Reference to the player's Shooter script
    public Slider progressBar;
    private int maxProgress;

    [Header("Lives & Respawn")]
    public int maxLives = 3;
    private int currentLives;
    public Transform currentCheckpoint;
    public GameObject player;

    void Start()
    {
        currentLives = maxLives;
        progress = 0;
        
        // Auto-find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        // Set initial checkpoint to player's start position
        if (player != null && currentCheckpoint == null)
        {
            GameObject startPoint = new GameObject("StartCheckpoint");
            startPoint.transform.position = player.transform.position;
            currentCheckpoint = startPoint.transform;
        }

        // Auto-find shooter if not assigned
        if (playerShooter == null)
        {
            playerShooter = FindFirstObjectByType<Shooter>();
        }
        
        // 1. Find all coins in the level
        Coin[] allCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        
        // 2. Calculate total worth
        maxProgress = 0;
        foreach (Coin coin in allCoins)
        {
            maxProgress += coin.worth;
        }

        // 3. Configure the Slider
        progressBar.minValue = 0;
        progressBar.maxValue = maxProgress;
        progressBar.value = 0;

        Coin.OnCoinCollect += IncreaseProgress;
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void IncreaseProgress(int amount)
    {
        progress += amount;
        progressBar.value = progress;
        
        // Check if we reached the calculated max
        if (progress >= maxProgress)
        {
            Debug.Log("Level Complete! Golden Ball Unlocked!");
            
            // Enable Golden Mode on Player
            if (playerShooter != null)
            {
                playerShooter.EnableGoldenMode();
            }

            // Update UI to show Golden Balls
            AmmoUI ammoUI = FindFirstObjectByType<AmmoUI>();
            if (ammoUI != null)
            {
                ammoUI.EnableGoldenUI();
            }
        }
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
        Debug.Log("Checkpoint Updated!");
    }

    public void PlayerDied()
    {
        currentLives--;
        Debug.Log($"Player Died! Lives remaining: {currentLives}");

        // Update Lives UI
        LivesUI livesUI = FindFirstObjectByType<LivesUI>();
        if (livesUI != null)
        {
            livesUI.UpdateLives(currentLives);
        }

        if (currentLives > 0)
        {
            RespawnPlayer();
        }
        else
        {
            GameOver();
        }
    }

    private void RespawnPlayer()
    {
        StartCoroutine(RespawnRoutine());
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        if (player != null)
        {
            // 1. Disable Player Visuals/Control temporarily
            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            Collider2D col = player.GetComponent<Collider2D>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Shooter shooter = player.GetComponentInChildren<Shooter>();
            
            if (sr != null) sr.enabled = false;
            if (col != null) col.enabled = false;
            if (rb != null) rb.simulated = false; // Stop physics
            if (shooter != null) shooter.enabled = false;

            // 2. Wait for "Flash" or Death Animation time
            yield return new WaitForSeconds(1.5f); // Longer delay as requested

            // 3. Reset Position
            if (currentCheckpoint != null)
            {
                player.transform.position = currentCheckpoint.position;
            }

            // 4. Re-enable everything
            if (sr != null) sr.enabled = true;
            if (col != null) col.enabled = true;
            if (rb != null) 
            {
                rb.simulated = true;
                rb.linearVelocity = Vector2.zero;
            }
            if (shooter != null) shooter.enabled = true;

            // 5. Reset Health
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Heal(playerHealth.maxHealth); 
            }
        }
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER");
        // TODO: Show Game Over Screen
        // Time.timeScale = 0; // Pause game
    }

    public void LevelComplete()
    {
        Debug.Log("CONGRATULATIONS! LEVEL FINISHED!");
        
        WinScreenUI winScreen = FindFirstObjectByType<WinScreenUI>();
        if (winScreen != null)
        {
            winScreen.ShowWinScreen();
        }
        else
        {
            Debug.LogWarning("WinScreenUI not found in the scene!");
        }
    }
}
