using UnityEngine;

public class TrapRoom : MonoBehaviour
{
    [Header("Door Settings")]
    public GameObject door; // Assign the door object
    public bool closeDoorOnEnter = true;

    [Header("Lava Settings")]
    public Transform lava; // Assign the lava object
    public float riseSpeed = 1f;
    public float stopHeightY = 0f; // The Y position where lava stops rising
    
    private bool isTrapActive = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTrapActive)
        {
            ActivateTrap();
        }
    }

    [Header("Shake Settings")]
    public float shakeIntensity = 2f; // High value for violent shake

    private void ActivateTrap()
    {
        isTrapActive = true;
        // Debug.Log("Trap Activated! Run!");

        if (door != null && closeDoorOnEnter)
        {
            door.SetActive(true); // Close the door (ensure it starts inactive/open)
        }

        // Trigger violent camera shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeIntensity);
        }
    }

    private Vector3 initialLavaPos;

    private void Start()
    {
        if (lava != null)
        {
            initialLavaPos = lava.position;
        }
    }

    private void OnEnable()
    {
        GameManager.OnPlayerRespawn += ResetTrap;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerRespawn -= ResetTrap;
    }

    private void ResetTrap()
    {
        isTrapActive = false;
        Debug.Log("Resetting Trap Room...");

        if (lava != null)
        {
            lava.position = initialLavaPos;
        }

        if (door != null && closeDoorOnEnter)
        {
            door.SetActive(false); // Open the door again
        }
    }

    private void Update()
    {
        if (isTrapActive && lava != null)
        {
            // Move lava up
            if (lava.position.y < stopHeightY)
            {
                lava.position += Vector3.up * riseSpeed * Time.deltaTime;
            }
        }
    }
}
