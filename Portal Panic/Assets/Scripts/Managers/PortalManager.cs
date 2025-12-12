using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnData
{
	public GameObject enemyPrefab;
	[Range(0, 100)] public float spawnChance = 50f;
}

/// <summary>
/// Manages all portals and coordinates the spawning system
/// </summary>
public class PortalManager : MonoBehaviour
{
	[Header("Enemy Configuration")]
	[SerializeField] private EnemySpawnData[] enemyTypes;

	[Header("Wave Settings")]
	[SerializeField] private int initialEnemiesPerWave = 5;
	[SerializeField][Range(1f, 5f)] private float enemyMultiplier = 1.5f;
	[SerializeField] private float timeBetweenWaves = 5f;
	[SerializeField] private float spawnInterval = 1f; // Time between each enemy spawn

	[Header("HUD")]
	[SerializeField] private HUDController hudController;

	private List<EnemyPortal> allPortals = new List<EnemyPortal>();
	private List<EnemyPortal> activePortals = new List<EnemyPortal>();
	private List<GameObject> activeEnemies = new List<GameObject>();

	private int currentWave = 1;
	private int totalEnemiesForWave = 0;
	private int enemiesSpawned = 0;
	private bool isSpawning = false;
	private Coroutine spawnCoroutine;
	private Coroutine waveCoroutine;

	void Start()
	{
		// Find all portals in the scene
		EnemyPortal[] portalsInScene = FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None);

		if (portalsInScene.Length == 0)
		{
			Debug.LogError("PortalManager: No EnemyPortal components found in scene!");
			return;
		}

		// Initialize all portals
		foreach (EnemyPortal portal in portalsInScene)
		{
			allPortals.Add(portal);
			portal.Initialize(this);
			portal.DisablePortalInstantly();
		}

		Debug.Log($"PortalManager: Found {allPortals.Count} portals");

		// Validate enemy types
		if (enemyTypes == null || enemyTypes.Length == 0)
		{
			Debug.LogWarning("PortalManager: No enemy types assigned!");
			return;
		}

		// Start the wave system
		StartWave();
	}

	void StartWave()
	{
		if (isSpawning)
			return;

		// Update HUD
		if (hudController != null)
		{
			hudController.UpdateRoundText($"Round {currentWave}");
		}

		// Notify kill counter
		if (KillCounterManager.Instance != null)
		{
			KillCounterManager.Instance.SetCurrentRound(currentWave);
		}

		// Calculate total enemies for this wave
		totalEnemiesForWave = Mathf.RoundToInt(initialEnemiesPerWave * Mathf.Pow(enemyMultiplier, currentWave - 1));
		Debug.Log($"Starting Wave {currentWave} - Total enemies: {totalEnemiesForWave}");

		waveCoroutine = StartCoroutine(RunWave());
	}

	IEnumerator RunWave()
	{
		// Select 1 or 2 random active portals
		SelectActivePortals();

		// Distribute enemies among active portals
		DistributeEnemies();

		// Show all active portals instantly
		foreach (EnemyPortal portal in activePortals)
		{
			portal.ShowPortal();
		}

		// Start spawning enemies alternately between portals
		isSpawning = true;
		spawnCoroutine = StartCoroutine(SpawnEnemiesAlternately());

		// Wait until all enemies are spawned
		yield return new WaitUntil(() => enemiesSpawned >= totalEnemiesForWave);

		// Wait until all enemies are killed
		yield return new WaitUntil(() => activeEnemies.Count == 0);

		Debug.Log($"Wave {currentWave} complete!");

		// Hide portals instantly
		foreach (EnemyPortal portal in activePortals)
		{
			portal.HidePortal();
		}

		isSpawning = false;

		// Start countdown timer
		yield return StartCoroutine(RoundTimerCountdown());

		// Prepare for next wave
		currentWave++;
		enemiesSpawned = 0;
		StartWave();
	}

	void SelectActivePortals()
	{
		activePortals.Clear();

		if (allPortals.Count == 1)
		{
			activePortals.Add(allPortals[0]);
		}
		else if (allPortals.Count == 2)
		{
			activePortals.Add(allPortals[0]);
			activePortals.Add(allPortals[1]);
		}
		else
		{
			List<EnemyPortal> available = new List<EnemyPortal>(allPortals);

			EnemyPortal portal1 = available[Random.Range(0, available.Count)];
			activePortals.Add(portal1);
			available.Remove(portal1);

			EnemyPortal portal2 = available[Random.Range(0, available.Count)];
			activePortals.Add(portal2);
		}

		Debug.Log($"Selected {activePortals.Count} active portal(s) for this wave");
	}

	void DistributeEnemies()
	{
		int enemiesPerPortal = totalEnemiesForWave / activePortals.Count;
		int extraEnemies = totalEnemiesForWave % activePortals.Count;

		for (int i = 0; i < activePortals.Count; i++)
		{
			int count = enemiesPerPortal + (i < extraEnemies ? 1 : 0);
			activePortals[i].SetEnemiesToSpawn(count);
			Debug.Log($"Portal {i} will spawn {count} enemies");
		}
	}

	IEnumerator SpawnEnemiesAlternately()
	{
		int portalIndex = 0;

		while (enemiesSpawned < totalEnemiesForWave)
		{
			EnemyPortal currentPortal = activePortals[portalIndex];

			if (currentPortal.GetEnemiesToSpawn() > 0)
			{
				currentPortal.SpawnEnemy();
				enemiesSpawned++;
			}

			portalIndex = (portalIndex + 1) % activePortals.Count;
			yield return new WaitForSeconds(spawnInterval);
		}
	}

	IEnumerator RoundTimerCountdown()
	{
		float countdownTime = timeBetweenWaves;

		while (countdownTime > 0)
		{
			int displayTime = Mathf.CeilToInt(countdownTime);
			if (hudController != null)
			{
				hudController.UpdateNextRoundText($"Next Round in {displayTime}");
			}

			yield return new WaitForSeconds(1f);
			countdownTime -= 1f;
		}

		if (hudController != null)
		{
			hudController.UpdateNextRoundText("");
		}
	}

	public GameObject SelectEnemyType()
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
			return null;

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

		return null;
	}

	public void RegisterEnemySpawned()
	{
		// Called by EnemyTracker when enemy is created
	}

	public void NotifyEnemyDestroyed(GameObject enemy)
	{
		activeEnemies.Remove(enemy);
		Debug.Log($"Enemy destroyed. Remaining: {activeEnemies.Count}");
	}

	public void RegisterEnemyAlive(GameObject enemy)
	{
		if (!activeEnemies.Contains(enemy))
		{
			activeEnemies.Add(enemy);
			Debug.Log($"Enemy registered. Active: {activeEnemies.Count}");
		}
	}
}

/// <summary>
/// Tracks individual enemy lifecycle
/// </summary>
public class EnemyTracker : MonoBehaviour
{
	private PortalManager portalManager;

	public void Initialize(PortalManager manager)
	{
		portalManager = manager;
		portalManager.RegisterEnemyAlive(gameObject);
	}

	void OnDestroy()
	{
		if (portalManager != null)
		{
			portalManager.NotifyEnemyDestroyed(gameObject);
		}
	}
}