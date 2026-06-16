using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public float damage = 100f;
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bullet hit: {other.name} (Object: {other.gameObject.name}) on layer {LayerMask.LayerToName(other.gameObject.layer)}");
        
        // Check if we hit an enemy
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            // Try searching root
            enemyHealth = other.transform.root.GetComponentInChildren<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            Debug.Log($"EnemyHealth found on {enemyHealth.gameObject.name}, dealing {damage} damage.");
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        else
        {
            Debug.Log($"No EnemyHealth found for {other.name}");
        }

        // Destroy on hit with anything else (like walls)
        if (!other.isTrigger && !other.CompareTag("Player"))
        {
            Debug.Log($"Bullet destroyed by hitting non-enemy: {other.name}");
            Destroy(gameObject);
        }
    }
}
