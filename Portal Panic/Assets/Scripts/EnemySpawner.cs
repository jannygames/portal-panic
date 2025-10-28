using UnityEngine;
using System.Collections;

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
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float minSpawnInterval = 0.3f; // Minimum time between individual enemy spawns
    [SerializeField] private float maxSpawnInterval = 1.0f; // Maximum time between individual enemy spawns
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 0f; // Spawn exactly at portal if 0, or in a small radius
    
    private int currentWave = 1;
    private int enemiesToSpawn;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    
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
        
        // Calculate enemies for this wave
        enemiesToSpawn = Mathf.RoundToInt(initialEnemiesPerWave * Mathf.Pow(enemyMultiplier, currentWave - 1));
        
        Debug.Log($"Starting Wave {currentWave} - Spawning {enemiesToSpawn} enemies");
        
        // Start spawning enemies
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
            // Random interval between spawns
            float randomInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(randomInterval);
        }
        
        isSpawning = false;
        Debug.Log($"Wave {currentWave} complete!");
        
        // Wait before starting next wave
        yield return new WaitForSeconds(timeBetweenWaves);
        
        // Start next wave
        currentWave++;
        StartWave();
    }
    
    void SpawnEnemy()
    {
        // Select enemy type based on spawn chances
        GameObject enemyToSpawn = SelectEnemyType();
        
        if (enemyToSpawn == null)
        {
            Debug.LogWarning("EnemySpawner: No valid enemy selected to spawn!");
            return;
        }
        
        // Calculate spawn position (at portal or small radius around it)
        Vector3 spawnPosition = transform.position;
        if (spawnRadius > 0)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            spawnPosition += new Vector3(randomCircle.x, 0, randomCircle.y);
        }
        
        // Instantiate enemy
        GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnPosition, transform.rotation);
        
        // Optional: Add a spawn effect or portal animation trigger here
    }
    
    GameObject SelectEnemyType()
    {
        // Calculate total chance for normalization
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
        
        // Generate random value
        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;
        
        // Select enemy based on weighted chance
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
        
        // Fallback to first valid enemy
        foreach (var enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null)
            {
                return enemyData.enemyPrefab;
            }
        }
        
        return null;
    }
    
    // Public methods for external control
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
