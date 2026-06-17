    using UnityEngine;
    using System;
    using System.Threading.Tasks;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public event Action<int, int> OnHealthChanged;

    [Header("Audio")]
    public AudioClip deathSound;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log("Player took damage. Current Health: " + currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        Debug.Log("Player Died.");
        if (ScoreManager.Instance != null)
        {
            LeaderboardManager.SubmitScore(ScoreManager.Instance.GetScore());
        }
        
        SkipToMainMenu();
           
    }

    public async void SkipToMainMenu()
        {
            await Task.Delay(10000);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu#");
        }

    public int GetCurrentHealth() => currentHealth;
}