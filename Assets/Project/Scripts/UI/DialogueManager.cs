using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public UnityEngine.UI.Image portraitImage; // Reference to the UI Image
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;

    [Header("Settings")]
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideDialogue();
    }

    public void ShowDialogue(string speaker, string message, Sprite portrait = null, float duration = 3f)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            
            if (speakerNameText != null) speakerNameText.text = speaker;
            
            // Set the portrait if provided
            if (portraitImage != null)
            {
                if (portrait != null)
                {
                    portraitImage.sprite = portrait;
                    portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    // Hide image if no sprite is provided
                    portraitImage.gameObject.SetActive(false); 
                }
            }
            
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeMessage(message, duration));
        }
    }

    private IEnumerator TypeMessage(string message, float duration)
    {
        dialogueText.text = "";
        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (duration > 0)
        {
            yield return new WaitForSeconds(duration);
            HideDialogue();
        }
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}
