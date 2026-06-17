using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            // updatet die Healthbar
            UpdateHealthBar(playerHealth.GetCurrentHealth(), playerHealth.maxHealth);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}