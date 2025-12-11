using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a single portal - handles spawning enemies from this specific location
/// </summary>
public class EnemyPortal : MonoBehaviour
{
	[Header("Portal Settings")]
	[SerializeField] private float fadeInDuration = 1f;
	[SerializeField] private float fadeOutDuration = 1f;
	[SerializeField] private float spawnRadius = 0f;

	private Renderer portalRenderer;
	private Material portalMaterial;
	private PortalManager portalManager;
	private int enemiesToSpawn = 0;
	private bool isActive = false;

	void Start()
	{
		// Get the renderer component
		portalRenderer = GetComponent<Renderer>();
		if (portalRenderer != null)
		{
			portalMaterial = new Material(portalRenderer.material);
			portalRenderer.material = portalMaterial;
		}

		// Disable the portal initially
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
		{
			return;
		}

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

	public IEnumerator FadeInPortal()
	{
		isActive = true;
		float elapsed = 0f;

		while (elapsed < fadeInDuration)
		{
			elapsed += Time.deltaTime;
			float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
			SetPortalAlpha(alpha);
			yield return null;
		}

		SetPortalAlpha(1f);
	}

	public IEnumerator FadeOutPortal()
	{
		isActive = false;
		float elapsed = 0f;

		while (elapsed < fadeOutDuration)
		{
			elapsed += Time.deltaTime;
			float alpha = Mathf.Clamp01(1f - (elapsed / fadeOutDuration));
			SetPortalAlpha(alpha);
			yield return null;
		}

		SetPortalAlpha(0f);
	}

	public void DisablePortalInstantly()
	{
		SetPortalAlpha(0f);
		isActive = false;
	}

	private void SetPortalAlpha(float alpha)
	{
		if (portalMaterial != null)
		{
			Color color = portalMaterial.color;
			color.a = alpha;
			portalMaterial.color = color;
		}

		// Also hide all child renderers
		Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in childRenderers)
		{
			if (renderer == portalRenderer) continue;

			Material mat = new Material(renderer.material);
			Color c = mat.color;
			c.a = alpha;
			mat.color = c;
			renderer.material = mat;
		}
	}

	public bool IsActive()
	{
		return isActive;
	}
}