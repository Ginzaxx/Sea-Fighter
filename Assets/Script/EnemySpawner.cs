using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Enemy[] enemyPrefabs;
    private int poolSizePerPrefab = 2;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float initialSpawnInterval = 3f;
    [SerializeField] private float minSpawnInterval = 1.5f;
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
        // Berhenti spawn jika game belum mulai atau sudah selesai
        if (GameOverManager.Instance != null && (!GameOverManager.Instance.IsGameStarted || GameOverManager.Instance.IsGameFinished)) 
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
        // Membalik urutan loop agar prefab diinstansiasi bergantian
        for (int i = 0; i < poolSizePerPrefab; i++)
        {
            foreach (Enemy prefab in enemyPrefabs)
            {
                Enemy enemy = Instantiate(prefab);
                enemy.gameObject.SetActive(false);
                enemyPool.Add(enemy);
            }
        }
    }

    private void SpawnEnemy()
    {
        Enemy enemy = GetRandomEnemyFromPool();
        if (enemy != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            enemy.transform.position = spawnPoint.position;
            enemy.gameObject.SetActive(true);
            enemy.Init(this, enemySpeed);
        }
    }

    private Enemy GetRandomEnemyFromPool()
    {
        // Mencari musuh yang tidak aktif secara acak agar variasi lebih terasa
        List<Enemy> availableEnemies = new List<Enemy>();
        foreach (Enemy enemy in enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                availableEnemies.Add(enemy);
            }
        }

        if (availableEnemies.Count > 0)
        {
            return availableEnemies[Random.Range(0, availableEnemies.Count)];
        }
        return null;
    }
}
