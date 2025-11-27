using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    [Header("References")]
    public Door linkedDoor; // Drag the Door object here
    public Sprite pressedSprite; // Optional: Sprite to show when pressed
    public Sprite unpressedSprite; // Optional: Sprite to show when unpressed
    
    private bool isPressed = false;
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (unpressedSprite == null && sr != null)
        {
            unpressedSprite = sr.sprite; // Save original sprite as unpressed
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision.gameObject);
    }

    private void CheckCollision(GameObject obj)
    {
        // Check if the object hitting us is the Basketball
        if (obj.GetComponent<Basketball>() != null)
        {
            ToggleButton();
        }
    }

    private void ToggleButton()
    {
        isPressed = !isPressed;
        
        // Toggle the door
        if (linkedDoor != null)
        {
            linkedDoor.Toggle();
        }

        // Change visual state
        if (sr != null)
        {
            if (isPressed)
            {
                if (pressedSprite != null) sr.sprite = pressedSprite;
                else sr.color = Color.gray;
            }
            else
            {
                if (unpressedSprite != null) sr.sprite = unpressedSprite;
                else sr.color = Color.white;
            }
        }
        
        Debug.Log(isPressed ? "Button Pressed!" : "Button Released!");
    }
}
