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
        
        // Überprüft ob ein gegner getroffen wurde
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            // sucht den root
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

        // kugel verschwindet bei collision mit der wand
        if (!other.isTrigger && !other.CompareTag("Player"))
        {
            Debug.Log($"Bullet destroyed by hitting non-enemy: {other.name}");
            Destroy(gameObject);
        }
    }
}
