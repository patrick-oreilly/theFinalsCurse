using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string firstLevelSceneName = "Level_01"; // Name of your game scene

    [Header("UI References")]
    public UnityEngine.UI.Text playButtonText; // Assign this in Inspector

    private void Start()
    {
        if (PlayerPrefs.GetInt("NewGamePlus", 0) == 1)
        {
            if (playButtonText != null)
            {
                playButtonText.text = "The League Beyond";
                playButtonText.color = Color.red; // Or gold
            }
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void QuitGame()
    {
        // Debug.Log("Quit Game");
        Application.Quit();
    }
}
