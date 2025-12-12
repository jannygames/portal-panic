using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a single portal - handles spawning enemies from this specific location
/// </summary>
public class EnemyPortal : MonoBehaviour
{
	[Header("Portal Settings")]
	[SerializeField] private float spawnRadius = 0f;

	private Renderer portalRenderer;
	private PortalManager portalManager;
	private int enemiesToSpawn = 0;
	private bool isActive = false;

	void Start()
	{
		portalRenderer = GetComponent<Renderer>();
		DisablePortalInstantly();
	}

	public void Initialize(PortalManager manager)
	{
		portalManager = manager;
	}

	public void SetEnemiesToSpawn(int count)
	{
		enemiesToSpawn = count;
	}

	public int GetEnemiesToSpawn()
	{
		return enemiesToSpawn;
	}

	public void SpawnEnemy()
	{
		if (enemiesToSpawn <= 0 || portalManager == null)
			return;

		GameObject enemyToSpawn = portalManager.SelectEnemyType();
		if (enemyToSpawn == null)
		{
			Debug.LogWarning("EnemyPortal: No valid enemy selected to spawn!");
			return;
		}

		Vector3 spawnPosition = transform.position;
		if (spawnRadius > 0)
		{
			Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
			spawnPosition += new Vector3(randomCircle.x, 0, randomCircle.y);
		}

		GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnPosition, transform.rotation);

		// Pass the player reference to the enemy
		EnemyAbstract enemyScript = spawnedEnemy.GetComponent<EnemyAbstract>();
		if (enemyScript != null)
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			enemyScript.SetPlayer(player);
		}

		// Attach the EnemyTracker to notify when the enemy is destroyed
		EnemyTracker tracker = spawnedEnemy.AddComponent<EnemyTracker>();
		tracker.Initialize(portalManager);

		enemiesToSpawn--;
		portalManager.RegisterEnemySpawned();
	}

	public void ShowPortal()
	{
		isActive = true;
		SetPortalVisible(true);
	}

	public void HidePortal()
	{
		isActive = false;
		SetPortalVisible(false);
	}

	public void DisablePortalInstantly()
	{
		isActive = false;
		SetPortalVisible(false);
	}

	private void SetPortalVisible(bool visible)
	{
		if (portalRenderer != null)
			portalRenderer.enabled = visible;

		// Toggle all child renderers
		Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in childRenderers)
		{
			renderer.enabled = visible;
		}
	}

	public bool IsActive()
	{
		return isActive;
	}
}
