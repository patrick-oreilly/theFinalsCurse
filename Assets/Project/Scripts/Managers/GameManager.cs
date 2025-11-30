using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    
    [Header("UI References")]
    public GameMenus gameMenus; // Drag 'menus' object here

    [Header("Tutorial & Narrative")]
    [TextArea] public string introLine1 = "Whoa—easy! You're not one of those... things. Thank god. I thought I was the only one down here.";
    [TextArea] public string introLine2 = "One minute I'm lacing up for Game 7, the next... I'm falling. What is this place? And why is everything trying to kill us?";
    [TextArea] public string controlsHint = "Listen up. A and D to move. W or Space to jump. E to shoot. Use Up and Down arrows to aim your shot. Press T to toggle the aim line. Don't miss.";
    
    private static bool hasShownIntro = false; // Static so it persists across scene reloads if needed, but for now just prevents respawn repeats

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

        Coin.OnCoinCollect += IncreaseProgress;
    }

    private void OnDestroy()
    {
        Coin.OnCoinCollect -= IncreaseProgress;
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
            // Debug.Log("Level Complete! Golden Ball Unlocked!");
            
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
        // Debug.Log("Checkpoint Updated!");
    }

    [Header("Death Hints")]
    public Sprite lebronPortrait; // Drag the LeBron sprite here in the Inspector
    public string[] deathHints = {
        "Watch the drop... fall damage is real.",
        "Pro Tip: Throw the ball OVER enemies, then recall it through them to hit multiple at once!"
    };

    public void PlayerDied()
    {
        currentLives--;
        // Debug.Log($"Player Died! Lives remaining: {currentLives}");

        // Update Lives UI
        LivesUI livesUI = FindFirstObjectByType<LivesUI>();
        if (livesUI != null)
        {
            livesUI.UpdateLives(currentLives);
        }

        if (currentLives > 0)
        {
            // Show Random Death Hint
            if (DialogueManager.Instance != null)
            {
                string randomHint = deathHints[Random.Range(0, deathHints.Length)];
                DialogueManager.Instance.ShowDialogue("LeBron", randomHint, lebronPortrait, 4f);
            }
            RespawnPlayer();
        }
        else
        {
            // Final Death - Special Message
            StartCoroutine(GameOverRoutine());
        }
    }

    private System.Collections.IEnumerator GameOverRoutine()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue("LeBron", "I really believed in you, kid...", lebronPortrait, 3f);
        }
        
        // Wait for the message to be read before showing the Game Over screen
        yield return new WaitForSeconds(3f);
        
        GameOver();
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
            if (shooter != null) 
            {
                shooter.enabled = true;
                shooter.ResetAmmo(); // Ensure player has the ball back
            }

            // 5. Reset Health
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Heal(playerHealth.maxHealth); 
            }

            // 6. Notify Listeners (Traps, etc.)
            OnPlayerRespawn?.Invoke();
        }
    }

    public static event System.Action OnPlayerRespawn;

    private void GameOver()
    {
        // Debug.Log("GAME OVER");
        
        if (gameMenus != null)
        {
            gameMenus.ShowGameOver();
        }
        else
        {
            // Fallback if not assigned
            GameMenus menus = FindFirstObjectByType<GameMenus>();
            if (menus != null)
            {
                menus.ShowGameOver();
            }
            else
            {
                Debug.LogWarning("GameMenus script not found in scene!");
            }
        }
    }

    [Header("Outro")]
    public bool isFinalLevel;
    public string outroSceneName = "Outro"; // Type the name of your scene here

    public void LevelComplete()
    {
        // Debug.Log("CONGRATULATIONS! LEVEL FINISHED!");
        
        if (isFinalLevel)
        {
            SceneManager.LoadScene(outroSceneName);
        }
        else
        {
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
}
