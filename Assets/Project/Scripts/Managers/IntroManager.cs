using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Configuration")]
    [TextArea(3, 10)] public string[] introLines;
    public string nextSceneName = "Level_01"; 
    public float typingSpeed = 0.05f;
    public float backspaceSpeed = 0.03f;
    public float lineDelay = 2.0f; // Time to read

    [Header("UI References")]
    public TextMeshProUGUI displayText;
    public GameObject titleCardPanel; // Drag your Title Card Panel here
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip titleCardAudio;

    private void Start()
    {
        if (titleCardPanel != null) titleCardPanel.SetActive(false);
        displayText.text = "";
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // Ensure Title Card is hidden initially
        if (titleCardPanel != null) titleCardPanel.SetActive(false);
        
        yield return new WaitForSeconds(1f); // Initial delay

        foreach (string line in introLines)
        {
            // 1. Type In
            yield return StartCoroutine(TypeMessage(line));
            
            // 2. Wait
            yield return new WaitForSeconds(lineDelay);

            // 3. Type Out (Backspace/Space replace)
            yield return StartCoroutine(UntypeMessage());
            
            // Short pause between lines
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(0.5f);
        
        // Show Title Card
        if (titleCardPanel != null)
        {
            titleCardPanel.SetActive(true);
            
            if (audioSource != null && titleCardAudio != null)
            {
                audioSource.PlayOneShot(titleCardAudio);
            }

            yield return new WaitForSeconds(4f); // Show title for 4 seconds
        }

        LoadNextLevel();
    }

    private IEnumerator TypeMessage(string message)
    {
        displayText.text = "";
        foreach (char letter in message.ToCharArray())
        {
            displayText.text += letter;
            if (audioSource != null && typingSound != null) audioSource.PlayOneShot(typingSound);
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator UntypeMessage()
    {
        string originalText = displayText.text;
        int length = originalText.Length;

        for (int i = 0; i <= length; i++)
        {
            // Create two parts: Invisible (eaten) and Visible (remaining)
            string invisiblePart = originalText.Substring(0, i);
            string visiblePart = originalText.Substring(i);

            // Use Rich Text to make the first part transparent but keep its width
            displayText.text = $"<color=#00000000>{invisiblePart}</color>{visiblePart}";
            
            yield return new WaitForSeconds(backspaceSpeed);
        }
        
        displayText.text = ""; // Ensure fully empty at the end
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    private void Update()
    {
        // Skip with Space
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LoadNextLevel();
        }
    }
}
