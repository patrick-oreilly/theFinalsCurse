using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Settings")]
    public Vector3 openOffset = new Vector3(0, 4, 0); // Direction and distance to move (default: Up 4 units)
    public float openSpeed = 2f;
    
    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 targetPosition;

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition + openOffset;
    }

    private void Update()
    {
        Vector3 destination = isOpen ? targetPosition : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, destination, openSpeed * Time.deltaTime);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        PlaySound(isOpen ? openSound : closeSound);
    }

    public void Open()
    {
        if (!isOpen) PlaySound(openSound);
        isOpen = true;
    }
    
    public void Close()
    {
        if (isOpen) PlaySound(closeSound);
        isOpen = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
