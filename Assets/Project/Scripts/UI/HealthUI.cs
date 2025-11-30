using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Slider healthSlider;
    public Health playerHealth;

    [Header("Damage Flash")]
    public Image damageFlashImage;
    public float flashDuration = 0.5f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);
    private Coroutine flashCoroutine;

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        if (playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.currentHealth;
            
            // Subscribe to the event
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
        }

        if (damageFlashImage != null)
        {
            damageFlashImage.color = Color.clear;
        }
    }

    public void UpdateHealthBar(int currentHealth)
    {
        // Check if damage was taken (current health is less than what the slider shows)
        if (currentHealth < healthSlider.value)
        {
            TriggerDamageFlash();
        }

        healthSlider.value = currentHealth;
    }

    private void TriggerDamageFlash()
    {
        if (damageFlashImage != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        // Flash in
        damageFlashImage.color = flashColor;
        
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            // Fade out over duration
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
            damageFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        damageFlashImage.color = Color.clear;
    }
}
