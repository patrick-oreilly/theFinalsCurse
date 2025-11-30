using UnityEngine;
using System.Collections;

public class IntroTrigger : MonoBehaviour
{
    [Header("Narrative")]
    [TextArea] public string introLine1 = "Whoa—easy! You're not one of those... things. Thank god. I thought I was the only one down here.";
    [TextArea] public string introLine2 = "One minute I'm lacing up for Game 7, the next... I'm falling. What is this place? And why is everything trying to kill us?";
    [TextArea] public string controlsHint = "Listen up. A and D to move. W or Space to jump. E to shoot. Use Up and Down arrows to aim your shot. Press T to toggle the aim line. Don't miss.";
    
    [Header("Settings")]
    public Sprite speakerPortrait;
    public string speakerName = "LeBron";

    // Static flag to ensure this ONLY plays once per game session, even if you respawn
    private static bool hasPlayedIntro = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayedIntro) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(PlaySequence());
        }
    }

    private IEnumerator PlaySequence()
    {
        hasPlayedIntro = true; // Mark as played immediately

        if (DialogueManager.Instance != null)
        {
            // Line 1
            DialogueManager.Instance.ShowDialogue(speakerName, introLine1, speakerPortrait, 7f);
            yield return new WaitForSeconds(7.5f);

            // Line 2
            DialogueManager.Instance.ShowDialogue(speakerName, introLine2, speakerPortrait, 8.5f);
            yield return new WaitForSeconds(9f);

            // Controls
            DialogueManager.Instance.ShowDialogue(speakerName, controlsHint, speakerPortrait, 10f);
        }
    }
}
