using UnityEngine;

public class Bullet : MonoBehaviour
{
	[Header("Bullet Settings")]
	[SerializeField] private float speed = 50f;
	[SerializeField] private float lifetime = 5f; // Auto-destroy after this time
	//[SerializeField] private int damage = 3;
	[SerializeField] private LayerMask enemyLayer;
	[Tooltip("Layers to ignore (e.g., Player, UI). Bullet will pass through these.")]
	[SerializeField] private LayerMask ignoreLayers;

	[Header("Effects")]
	[SerializeField] private GameObject hitEffectPrefab;
	[SerializeField] private bool destroyOnHit = true;
	[Tooltip("Sound effect to play when bullet hits something")]
	[SerializeField] private AudioClip impactSound;
	[SerializeField][Range(0, 1)] private float impactSoundVolume = 1f;

	private Rigidbody rb;
	private Vector3 direction;
	private float spawnTime;
	private bool hasHit = false;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
		}

		rb.isKinematic = false;
		rb.useGravity = false;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		rb.interpolation = RigidbodyInterpolation.Interpolate;

		spawnTime = Time.time;

		if (direction != Vector3.zero)
		{
			rb.linearVelocity = direction * speed;
		}
		else
		{
			rb.linearVelocity = transform.forward * speed;
		}
	}

	void Update()
	{
		if (Time.time - spawnTime > lifetime)
		{
			DestroyBullet();
		}
	}

	public void Initialize(Vector3 shootDirection, float bulletSpeed, LayerMask enemyLayerMask, LayerMask ignoreLayerMask)
	{
		direction = shootDirection.normalized;
		speed = bulletSpeed;
		enemyLayer = enemyLayerMask;
		ignoreLayers = ignoreLayerMask;

		if (rb != null)
		{
			rb.linearVelocity = direction * speed;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (hasHit) return;
		if (((1 << other.gameObject.layer) & ignoreLayers.value) != 0) return;

		bool isEnemy = ((1 << other.gameObject.layer) & enemyLayer.value) != 0;
		if (isEnemy) HandleEnemyHit(other);
		else HandleObstacleHit(other);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (hasHit) return;
		if (((1 << collision.gameObject.layer) & ignoreLayers.value) != 0) return;

		bool isEnemy = ((1 << collision.gameObject.layer) & enemyLayer.value) != 0;
		if (isEnemy) HandleEnemyHit(collision.collider);
		else HandleObstacleHit(collision.collider);
	}

	private void HandleEnemyHit(Collider hitCollider)
	{
		Debug.Log($"Bullet hit collider: {hitCollider.name}, Tag: {hitCollider.tag}, Layer: {LayerMask.LayerToName(hitCollider.gameObject.layer)}");

		hasHit = true;

		EnemyAbstract enemy = hitCollider.GetComponent<EnemyAbstract>();
		if (enemy == null) enemy = hitCollider.GetComponentInParent<EnemyAbstract>();
		if (enemy == null) enemy = hitCollider.GetComponentInChildren<EnemyAbstract>();

		if (enemy != null)
		{
			bool isHeadshot = hitCollider.CompareTag("Head");
			int instanceID = enemy.GetInstanceID();
			int healthBefore = enemy.health;

			Debug.Log($"Bullet: Hit enemy '{enemy.gameObject.name}' (ID: {instanceID}). Health before: {healthBefore}. Headshot: {isHeadshot}");

			enemy.TakeDamageFromGun(isHeadshot);

			if (enemy != null && enemy.gameObject != null)
			{
				int healthAfter = enemy.health;
				Debug.Log($"Bullet: ✓ Dealt damage to '{enemy.gameObject.name}' (ID: {instanceID}). Health: {healthBefore} → {healthAfter}");
			}
			else
			{
				Debug.Log($"Bullet: ✓ Enemy destroyed (ID: {instanceID}). Health was {healthBefore}.");
			}
		}
		else
		{
			Debug.LogWarning($"Bullet: Hit object '{hitCollider.gameObject.name}' on enemy layer but EnemyAbstract not found!");
		}

		if (impactSound != null)
		{
			AudioSource.PlayClipAtPoint(impactSound, transform.position, impactSoundVolume);
		}

		if (hitEffectPrefab != null)
		{
			Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
		}

		if (destroyOnHit) DestroyBullet();
	}

	private void HandleObstacleHit(Collider hitCollider)
	{
		hasHit = true;
		Debug.Log($"Bullet: Hit obstacle '{hitCollider.gameObject.name}' (layer: {LayerMask.LayerToName(hitCollider.gameObject.layer)})");

		if (impactSound != null)
		{
			AudioSource.PlayClipAtPoint(impactSound, transform.position, impactSoundVolume);
		}

		if (hitEffectPrefab != null)
		{
			Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
		}

		if (destroyOnHit) DestroyBullet();
	}

	private void DestroyBullet()
	{
		if (rb != null) rb.linearVelocity = Vector3.zero;
		Destroy(gameObject);
	}
}
