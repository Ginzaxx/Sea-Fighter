using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int poolSize = 10;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float enemySpeed = 5f;

    private List<Enemy> enemyPool;
    private float timer;

    void Start()
    {
        InitializePool();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0;
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
        foreach (Enemy enemy in enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                return enemy;
            }
        }
        return null; // Bisa dikembangkan untuk menambah pool jika kurang
    }
}
