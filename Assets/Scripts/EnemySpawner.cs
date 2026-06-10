using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave 1";
        public int enemyCount = 5;
        public float spawnInterval = 1.5f;   // Zeit zwischen jedem Spawn
        public float timeBetweenWaves = 5f;  // Pause nach der Welle
    }

    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("Waves")]
    public Wave[] waves;
    public bool loopWaves = false; // Nach letzter Welle von vorne?

    [Header("Spawn Punkte")]
    public Transform[] fixedSpawnPoints; // Feste Punkte (optional)
    public float randomSpawnRadius = 10f; // Radius für zufälligen NavMesh-Spawn

    [Header("Status (readonly)")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private int enemiesAlive = 0;
    [SerializeField] private bool spawning = false;

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        while (true)
        {
            if (currentWaveIndex >= waves.Length)
            {
                if (loopWaves)
                    currentWaveIndex = 0;
                else
                {
                    Debug.Log("Alle Wellen abgeschlossen!");
                    yield break;
                }
            }

            Wave wave = waves[currentWaveIndex];
            Debug.Log($"Starte {wave.waveName} ({wave.enemyCount} Gegner)");

            yield return StartCoroutine(SpawnWave(wave));

            // Warten bis alle Gegner tot sind
            yield return new WaitUntil(() => AllEnemiesDead());

            Debug.Log($"{wave.waveName} abgeschlossen! Nächste Welle in {wave.timeBetweenWaves}s");
            yield return new WaitForSeconds(wave.timeBetweenWaves);

            currentWaveIndex++;
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        spawning = true;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        spawning = false;
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = GetSpawnPosition();
        if (spawnPos == Vector3.zero) return;
        
      

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);
        enemiesAlive++;

        // Automatisch aus Liste entfernen wenn Gegner stirbt
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
            health.OnDeath += () => OnEnemyDied(enemy);
    }

    void OnEnemyDied(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    bool AllEnemiesDead()
    {
        // Entferne null-Einträge (zerstörte Objekte)
        activeEnemies.RemoveAll(e => e == null);
        enemiesAlive = activeEnemies.Count;
        return !spawning && activeEnemies.Count == 0;
    }

    Vector3 GetSpawnPosition()
    {
        // Zufällig: fester Punkt oder NavMesh?
        bool useFixed = fixedSpawnPoints != null
                        && fixedSpawnPoints.Length > 0
                        && Random.value > 0.5f;

        if (useFixed)
        {
            Transform point = fixedSpawnPoints[Random.Range(0, fixedSpawnPoints.Length)];
            return GetNavMeshPosition(point.position, 5f);
        }
        else
        {
            Vector3 randomPos = Random.insideUnitSphere * randomSpawnRadius + transform.position;
            randomPos.y = transform.position.y;
            return GetNavMeshPosition(randomPos, randomSpawnRadius);
        }
    }

    Vector3 GetNavMeshPosition(Vector3 origin, float range)
    {
        if (NavMesh.SamplePosition(origin, out NavMeshHit hit, range, NavMesh.AllAreas))
            return hit.position;

        Debug.LogWarning("Kein NavMesh-Punkt gefunden!");
        return Vector3.zero;
    }

    // Gizmos: Spawn-Radius in Scene-View anzeigen
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, randomSpawnRadius);

        if (fixedSpawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var p in fixedSpawnPoints)
                if (p != null) Gizmos.DrawSphere(p.position, 0.5f);
        }
    }
}
