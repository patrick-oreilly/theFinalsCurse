using System.Collections;
using UnityEngine;
using TMPro; // Updated to TMP
using UnityEngine.SceneManagement;

public class OutroManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup backgroundFade; // The black background panel
    public TextMeshProUGUI textComponent; // Updated to TMP
    public GameObject pressToContinueObj; // The prompt at the end

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float backspaceSpeed = 0.03f;
    public float lineDisplayTime = 2.5f;
    public float fadeDuration = 1.0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip crowdCheerSound; // For the "Waking up" moment

    [Header("Narrative Content")]
    [TextArea(2, 5)]
    public string[] narrativeLines = new string[] {
        "The echo of the swish fades into pure silence...",
        "LeBron: \"You did it. You played through every moment.\"",
        "\"The curse only showed what you never wanted to see.\"",
        "The arena fades. Blinding light returns.",
        "\"He's waking up! The rookie is back!\"",
        "LeBron: \"One more shot.\"",
        "SWISH.",
        "\"The curse was never meant to break you...\"",
        "\"...it was meant to teach you how to play through it.\"",
        "Sometimes, you don't beat the curse — you outplay it."
    };

    private void Start()
    {
        if (backgroundFade) backgroundFade.alpha = 0;
        if (textComponent) textComponent.text = "";
        if (pressToContinueObj) pressToContinueObj.SetActive(false);
        
        // Auto-play for testing or if loaded as a scene
        PlayOutro();
    }

    public void PlayOutro()
    {
        StartCoroutine(OutroSequence());
    }

    private IEnumerator OutroSequence()
    {
        // 1. Fade everything to black (Only if panel is assigned)
        if (backgroundFade != null)
        {
            yield return StartCoroutine(FadeCanvas(backgroundFade, 0, 1, fadeDuration));
        }
        
        // 2. Loop through each narrative line
        foreach (string line in narrativeLines)
        {
            // Type In
            yield return StartCoroutine(TypeMessage(line));

            // Wait
            yield return new WaitForSeconds(lineDisplayTime);

            // Erase (Left to Right effect)
            yield return StartCoroutine(UntypeMessage());
            
            yield return new WaitForSeconds(0.5f);
        }

        // 3. Epilogue / Continue Prompt
        if (crowdCheerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(crowdCheerSound);
        }

        if (pressToContinueObj) pressToContinueObj.SetActive(true);

        // Wait for input
        bool waiting = true;
        while(waiting)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                waiting = false;
            }
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                waiting = false;
            }
            yield return null;
        }

        StartNewGamePlus();
    }

    private IEnumerator TypeMessage(string message)
    {
        textComponent.text = "";
        foreach (char letter in message)
        {
            textComponent.text += letter;
            if (audioSource != null && typingSound != null) audioSource.PlayOneShot(typingSound);
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator UntypeMessage()
    {
        string originalText = textComponent.text;
        int length = originalText.Length;

        for (int i = 0; i <= length; i++)
        {
            // Create two parts: Invisible (eaten) and Visible (remaining)
            string invisiblePart = originalText.Substring(0, i);
            string visiblePart = originalText.Substring(i);

            // Use Rich Text to make the first part transparent but keep its width
            textComponent.text = $"<color=#00000000>{invisiblePart}</color>{visiblePart}";
            
            yield return new WaitForSeconds(backspaceSpeed);
        }
        textComponent.text = "";
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null) yield break;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        cg.alpha = end;
    }

    private void StartNewGamePlus()
    {
        // Debug.Log("Starting New Game+");
        PlayerPrefs.SetInt("NewGamePlus", 1);
        PlayerPrefs.Save();
        
        // Return to Main Menu (Scene 0)
        SceneManager.LoadScene(0); 
    }
}
