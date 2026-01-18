using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 2f;
    private float nextSpawn = 0f;

    void Update()
    {
        if (Time.time > nextSpawn)
        {
            nextSpawn = Time.time + spawnRate;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // Crea un punto casuale fuori dallo schermo (per ora semplificato)
        Vector2 spawnPos = new Vector2(Random.Range(-10, 10), 6); 
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
