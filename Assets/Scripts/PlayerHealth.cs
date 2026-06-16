using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public event Action<int, int> OnHealthChanged;

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
        Debug.Log("Player Died.");
        if (ScoreManager.Instance != null)
        {
            LeaderboardManager.SubmitScore(ScoreManager.Instance.GetScore());
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu#");
    }

    public int GetCurrentHealth() => currentHealth;
}