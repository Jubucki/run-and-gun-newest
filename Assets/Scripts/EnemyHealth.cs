using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f;

    [Header("Status (readonly)")]
    [SerializeField] private float currentHealth;

    public Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
