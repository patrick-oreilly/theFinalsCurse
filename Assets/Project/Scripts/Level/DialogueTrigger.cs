using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    public string message = "You've been cursed, youngblood...";
    public string speakerName = "Spectral LeBron";
    public bool oneTimeOnly = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (oneTimeOnly && hasTriggered) return;

            DialogueManager.Instance.ShowDialogue(speakerName, message);
            hasTriggered = true;
        }
    }
}
