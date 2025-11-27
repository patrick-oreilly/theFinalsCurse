using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [Header("UI Configuration")]
    public Image[] heartSlots; // Drag your 3 Heart Image objects here

    [Header("Sprites")]
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    public void UpdateLives(int currentLives)
    {
        // Loop through all UI slots
        for (int i = 0; i < heartSlots.Length; i++)
        {
            if (i < currentLives)
            {
                // Heart is Full
                if (fullHeartSprite != null) heartSlots[i].sprite = fullHeartSprite;
                heartSlots[i].color = Color.white;
            }
            else
            {
                // Heart is Empty (Lost Life)
                if (emptyHeartSprite != null)
                {
                    heartSlots[i].sprite = emptyHeartSprite;
                    heartSlots[i].color = Color.white;
                }
                else
                {
                    // Fallback: Just darken/fade the heart
                    if (fullHeartSprite != null) heartSlots[i].sprite = fullHeartSprite;
                    heartSlots[i].color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Dark Grey + Transparent
                }
            }
        }
    }
}
