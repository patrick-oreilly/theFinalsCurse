using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenUI : MonoBehaviour
{
    public GameObject winScreenPanel;

    private void Start()
    {
        // Ensure it's hidden at start
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }
    }

    public void ShowWinScreen()
    {
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
            Time.timeScale = 0f; // Pause the game
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // Unpause
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        // Debug.Log("Quit Game");
    }
}
