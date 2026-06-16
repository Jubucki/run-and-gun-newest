using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f;

    [Header("Audio")]
    public AudioClip deathSound;

    [Header("Status (readonly)")]
    [SerializeField] private float currentHealth = 100f;

    public Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"{gameObject.name} taking {amount} damage. Current health: {currentHealth}");
        if (currentHealth <= 0f) 
        {
            Debug.Log($"{gameObject.name} is already dead (health: {currentHealth})");
            return;
        }

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} new health: {currentHealth}");

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Debug.Log($"{gameObject.name} health reached 0. Dying.");
            Die();
        }
    }

    void Die()
    {
        OnDeath?.Invoke();

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10); // Add 10 points per enemy kill
        }
        Destroy(gameObject);
    }
}
