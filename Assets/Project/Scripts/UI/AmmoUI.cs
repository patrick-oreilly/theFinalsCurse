using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public Shooter playerShooter;

    [Header("UI Configuration")]
    public Image[] ammoSlots; // Drag your 3 Image objects here

    [Header("Sprites")]
    public Sprite activeBallSprite; // The colored ball (Normal)
    public Sprite goldenBallSprite; // The Golden Ball
    public Sprite emptySlotSprite;  // The grey circle

    private Sprite currentActiveSprite;

    private void Start()
    {
        currentActiveSprite = activeBallSprite; // Default to normal

        if (playerShooter == null)
        {
            playerShooter = FindFirstObjectByType<Shooter>();
        }

        if (playerShooter != null)
        {
            playerShooter.OnAmmoChanged += UpdateAmmoDisplay;
            // Initialize display
            UpdateAmmoDisplay(playerShooter.currentAmmo);
        }
    }

    public void EnableGoldenUI()
    {
        currentActiveSprite = goldenBallSprite;
        if (playerShooter != null)
        {
            UpdateAmmoDisplay(playerShooter.currentAmmo); // Refresh immediately
        }
    }

    private void OnDestroy()
    {
        if (playerShooter != null)
        {
            playerShooter.OnAmmoChanged -= UpdateAmmoDisplay;
        }
    }

    private void UpdateAmmoDisplay(int currentAmmo)
    {
        // Loop through all UI slots
        for (int i = 0; i < ammoSlots.Length; i++)
        {
            if (i < currentAmmo)
            {
                // Slot is Full
                if (currentActiveSprite != null) ammoSlots[i].sprite = currentActiveSprite;
                ammoSlots[i].color = Color.white; // Full visibility
            }
            else
            {
                // Slot is Empty
                if (emptySlotSprite != null)
                {
                    ammoSlots[i].sprite = emptySlotSprite;
                    ammoSlots[i].color = Color.white;
                }
                else
                {
                    // Fallback: If no empty sprite is assigned, just darken the active one
                    if (currentActiveSprite != null) ammoSlots[i].sprite = currentActiveSprite;
                    ammoSlots[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Dark Grey + Transparent
                }
            }
        }
    }
}
