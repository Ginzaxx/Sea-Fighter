using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Enemy enemyPrefab;
    private int poolSize = 4;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float initialSpawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.75f;
    [SerializeField] private float intervalDecreasePerSecond = 0.02f; // Berkurang per detik
    [SerializeField] private float enemySpeed = 5f;

    [Header("Debug/Monitor")]
    [SerializeField] private float currentSpawnInterval;
    private float spawnTimer;

    private List<Enemy> enemyPool;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        InitializePool();
    }

    void Update()
    {
        // Berhenti spawn jika game sudah selesai (menang/kalah)
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished) 
            return;

        // Update timer spawn
        spawnTimer += Time.deltaTime;
        
        // Kurangi interval spawn seiring berjalannya waktu (per detik)
        if (currentSpawnInterval > minSpawnInterval)
        {
            currentSpawnInterval -= intervalDecreasePerSecond * Time.deltaTime;
            currentSpawnInterval = Mathf.Max(currentSpawnInterval, minSpawnInterval);
        }

        if (spawnTimer >= currentSpawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0;
        }
    }

    private void InitializePool()
    {
        enemyPool = new List<Enemy>();
        for (int i = 0; i < poolSize; i++)
        {
            Enemy enemy = Instantiate(enemyPrefab);
            enemy.gameObject.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    private void SpawnEnemy()
    {
        Enemy enemy = GetEnemyFromPool();
        if (enemy != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            enemy.transform.position = spawnPoint.position;
            enemy.gameObject.SetActive(true);
            enemy.Init(this, enemySpeed);
        }
    }

    private Enemy GetEnemyFromPool()
    {
        // Perbaikan syntax error pada foreach
        foreach (Enemy enemy in enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                return enemy;
            }
        }
        return null;
    }
}
