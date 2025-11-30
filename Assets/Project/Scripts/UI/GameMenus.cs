using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameMenus : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuUI;
    public GameObject gameOverUI;

    [Header("Configuration")]
    public string mainMenuSceneName = "MainMenu";
    
    private bool isPaused = false;

    private void Start()
    {
        // Ensure menus are hidden at start
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }



    // Call this from PlayerInput "Pause" action
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Pause Input Received!");
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null) 
        {
            pauseMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Pause Menu UI is NOT assigned in the Inspector!");
        }
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        if (pauseMenuUI != null) 
        {
            pauseMenuUI.SetActive(true);
            Debug.Log("Pause Menu Activated");
        }
        else
        {
            Debug.LogError("Pause Menu UI is NOT assigned in the Inspector!");
        }
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ShowGameOver()
    {
        if (gameOverUI != null) 
        {
            gameOverUI.SetActive(true);
            Debug.Log("Game Over Menu Activated");
        }
        else
        {
            Debug.LogError("Game Over UI is NOT assigned in the Inspector!");
        }
        Time.timeScale = 0f; // Stop the game
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // Reset time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        // Debug.Log("Quit Game");
        Application.Quit();
    }
}
