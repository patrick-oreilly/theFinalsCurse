using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OutroManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup fadeOverlay; // For white/black flashes
    public Image fadeImage; // To change color of fade
    public GameObject dialogueBox;
    public Text speakerNameText;
    public Text dialogueText;
    public GameObject epiloguePanel;
    public Text epilogueQuote;
    public GameObject pressToContinueText;

    [Header("Scene Objects")]
    public GameObject player;
    public GameObject leBronSpectral;
    public GameObject realWorldEnvironment; // The "Game 7" visuals
    public GameObject lockerRoomEnvironment;
    public GameObject cursedRealmEnvironment;
    public GameObject basketball; // The ball to glow/shoot

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip silenceClip; // Or just stop audio
    public AudioClip commentaryClip;
    public AudioClip crowdEruptClip;
    public AudioClip swishClip;
    public AudioClip leBronVoice1;
    public AudioClip leBronVoice2;
    public AudioClip leBronVoice3;

    private bool waitingForInput = false;

    private void Start()
    {
        // For testing, you might want to call PlayOutro() directly
        // PlayOutro();
        
        // Ensure UI is hidden at start
        if(dialogueBox) dialogueBox.SetActive(false);
        if(epiloguePanel) epiloguePanel.SetActive(false);
        if(leBronSpectral) leBronSpectral.SetActive(false);
    }

    public void PlayOutro()
    {
        StartCoroutine(OutroSequence());
    }

    private IEnumerator OutroSequence()
    {
        // --- Scene 1: The Silence After the Shot ---
        yield return StartCoroutine(Scene1_Silence());

        // --- Scene 2: The Return ---
        yield return StartCoroutine(Scene2_TheReturn());

        // --- Scene 3: The Locker Room ---
        yield return StartCoroutine(Scene3_LockerRoom());

        // --- Scene 4: Epilogue ---
        yield return StartCoroutine(Scene4_Epilogue());
    }

    private IEnumerator Scene1_Silence()
    {
        // 1. Flash White
        fadeImage.color = Color.white;
        yield return StartCoroutine(Fade(0, 1, 0.1f)); // Fast fade out to white
        
        // 2. Setup Scene
        // Hide crowd/enemies, stop music
        AudioManager.Instance.musicSource.Stop();
        if(realWorldEnvironment) realWorldEnvironment.SetActive(false);
        if(lockerRoomEnvironment) lockerRoomEnvironment.SetActive(false);
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(Fade(1, 0, 2f)); // Slow fade in to scene

        // 3. LeBron Appears
        if(leBronSpectral) leBronSpectral.SetActive(true);
        
        // 4. Dialogue
        yield return StartCoroutine(ShowDialogue("LeBron", "You did it. You played through every moment the curse twisted. You faced yourself.", leBronVoice1));
        yield return StartCoroutine(ShowDialogue("Player", "But… this wasn’t just the Finals, was it?", null));
        yield return StartCoroutine(ShowDialogue("LeBron", "No. This was your life — your fears, your pride, your doubt. The curse only showed what you never wanted to see.", leBronVoice2));

        // 5. Toss Ball (Animation or just enable glowing ball)
        // Assume animation plays here
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator Scene2_TheReturn()
    {
        // 1. Fade to White (Blinding light)
        fadeImage.color = Color.white;
        yield return StartCoroutine(Fade(0, 1, 1f));

        // 2. Setup Real World Scene
        if(leBronSpectral) leBronSpectral.SetActive(false);
        if(realWorldEnvironment) realWorldEnvironment.SetActive(true);
        
        // Play Commentary
        audioSource.PlayOneShot(commentaryClip);
        
        yield return new WaitForSeconds(2f); // Wait for commentary to build
        
        yield return StartCoroutine(Fade(1, 0, 0.5f)); // Fade in

        // 3. The Shot
        // Slow motion effect
        Time.timeScale = 0.3f;
        
        // Wait for "One more shot"
        yield return new WaitForSecondsRealtime(2f);
        audioSource.PlayOneShot(leBronVoice3); // "One more shot"
        
        // Simulate shot (or wait for player input if interactive)
        // For narrative flow, we'll automate it or prompt simple input
        
        yield return new WaitForSecondsRealtime(1f);
        
        // SWISH
        audioSource.PlayOneShot(swishClip);
        Time.timeScale = 1f; // Restore time
        
        yield return new WaitForSeconds(0.2f);
        audioSource.PlayOneShot(crowdEruptClip);
        
        yield return new WaitForSeconds(3f); // Let the victory soak in
    }

    private IEnumerator Scene3_LockerRoom()
    {
        // 1. Fade to Black (Time passing)
        fadeImage.color = Color.black;
        yield return StartCoroutine(Fade(0, 1, 1f));
        
        if(realWorldEnvironment) realWorldEnvironment.SetActive(false);
        if(lockerRoomEnvironment) lockerRoomEnvironment.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(Fade(1, 0, 1f));

        // 2. Reflection Dialogue
        yield return StartCoroutine(ShowDialogue("LeBron", "The curse was never meant to break you. It was meant to teach you how to play through it.", null));
        
        // 3. Fade out LeBron (if visible in reflection)
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator Scene4_Epilogue()
    {
        // 1. Fade to Black
        fadeImage.color = Color.black;
        yield return StartCoroutine(Fade(0, 1, 2f));
        
        if(lockerRoomEnvironment) lockerRoomEnvironment.SetActive(false);
        if(cursedRealmEnvironment) cursedRealmEnvironment.SetActive(true); // Empty court

        // 2. Show Text
        epiloguePanel.SetActive(true);
        epilogueQuote.text = "“Sometimes, you don’t beat the curse — you outplay it.”";
        
        // Set initial alpha to 0
        Color textColor = epilogueQuote.color;
        textColor.a = 0;
        epilogueQuote.color = textColor;
        
        // Fade text in
        float t = 0;
        while(t < 1)
        {
            t += Time.deltaTime;
            textColor.a = t;
            epilogueQuote.color = textColor;
            yield return null;
        }
        
        yield return new WaitForSeconds(3f);
        
        // 3. Show "Press [Shoot] to Continue"
        pressToContinueText.SetActive(true);
        waitingForInput = true;
        
        while(waitingForInput)
        {
            if(Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space)) // Assuming Fire1 is shoot
            {
                waitingForInput = false;
            }
            yield return null;
        }
        
        // 4. Trigger New Game+
        StartNewGamePlus();
    }

    private IEnumerator ShowDialogue(string speaker, string text, AudioClip voiceClip)
    {
        dialogueBox.SetActive(true);
        speakerNameText.text = speaker;
        dialogueText.text = "";
        
        if(voiceClip) audioSource.PlayOneShot(voiceClip);

        // Typewriter effect
        foreach(char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }
        
        // Wait for player to advance
        while(!Input.GetButtonDown("Fire1") && !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        
        dialogueBox.SetActive(false);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float t = 0;
        while(t < 1)
        {
            t += Time.deltaTime / duration;
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        fadeOverlay.alpha = endAlpha;
    }

    private void StartNewGamePlus()
    {
        Debug.Log("Starting New Game+");
        // Set difficulty flag
        PlayerPrefs.SetInt("NewGamePlus", 1);
        PlayerPrefs.Save();
        
        // Reload first level or Main Menu
        SceneManager.LoadScene(0); // Assuming 0 is Main Menu or First Level
    }
}
