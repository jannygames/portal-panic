using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    [Range(0, 100)] public float spawnChance = 50f;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [SerializeField] private EnemySpawnData[] enemyTypes;
    
    [Header("Wave Settings")]
    [SerializeField] private int initialEnemiesPerWave = 5;
    [SerializeField] [Range(1f, 5f)] private float enemyMultiplier = 1.5f;
    [SerializeField] private float timeBetweenWaves = 5f; // Fixed 5-second break between waves
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float maxSpawnInterval = 1.0f;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 0f;

    [Header("HUD")]
    [SerializeField] private HUDController hudController; // Reference to the HUDController

    private int currentWave = 1;
    private int enemiesToSpawn;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    private List<GameObject> activeEnemies = new List<GameObject>(); // Track active enemies

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Validate enemy types array
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: No enemy types assigned!");
            return;
        }
        
        // Start the wave spawning system
        StartWave();
    }

    void StartWave()
    {
        if (isSpawning) return;

        // Update the round text using HUDController
        if (hudController != null)
        {
            hudController.UpdateHUDText($"Round {currentWave}");
        }

        // Calculate enemies for this wave
        enemiesToSpawn = Mathf.RoundToInt(initialEnemiesPerWave * Mathf.Pow(enemyMultiplier, currentWave - 1));
        Debug.Log($"Starting Wave {currentWave} - Spawning {enemiesToSpawn} enemies");

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        isSpawning = true;

        // Spawn all enemies for this wave
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            float randomInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(randomInterval);
        }

        isSpawning = false;

        // Wait until all enemies are killed
        yield return new WaitUntil(() => activeEnemies.Count == 0);

        Debug.Log($"Wave {currentWave} complete!");

        // Wait for the break time before starting the next wave
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;
        StartWave();
    }

    void SpawnEnemy()
    {
        GameObject enemyToSpawn = SelectEnemyType();

        if (enemyToSpawn == null)
        {
            Debug.LogWarning("EnemySpawner: No valid enemy selected to spawn!");
            return;
        }

        Vector3 spawnPosition = transform.position;
        if (spawnRadius > 0)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            spawnPosition += new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnPosition, transform.rotation);

        // Add the spawned enemy to the activeEnemies list
        activeEnemies.Add(spawnedEnemy);
        Debug.Log($"Enemy spawned. Active enemies: {activeEnemies.Count}");

        // Pass the player reference to the enemy
        EnemyAbstract enemyScript = spawnedEnemy.GetComponent<EnemyAbstract>();
        if (enemyScript != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            enemyScript.SetPlayer(player);
        }

        // Attach the EnemyTracker to notify when the enemy is destroyed
        EnemyTracker tracker = spawnedEnemy.AddComponent<EnemyTracker>();
        tracker.Initialize(this);
    }

    GameObject SelectEnemyType()
    {
        float totalChance = 0f;
        foreach (var enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null)
            {
                totalChance += enemyData.spawnChance;
            }
        }

        if (totalChance <= 0f)
        {
            return null;
        }

        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;

        foreach (var enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null)
            {
                currentChance += enemyData.spawnChance;
                if (randomValue <= currentChance)
                {
                    return enemyData.enemyPrefab;
                }
            }
        }

        foreach (var enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null)
            {
                return enemyData.enemyPrefab;
            }
        }

        return null;
    }

    public void NotifyEnemyDestroyed(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        Debug.Log($"Enemy destroyed. Active enemies: {activeEnemies.Count}");
    }

    public void PauseSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            isSpawning = false;
        }
    }

    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            StartWave();
        }
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetEnemiesInWave()
    {
        return enemiesToSpawn;
    }
}

public class EnemyTracker : MonoBehaviour
{
    private EnemySpawner spawner;

    public void Initialize(EnemySpawner spawner)
    {
        this.spawner = spawner;
    }

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.NotifyEnemyDestroyed(gameObject);
        }
    }
}
