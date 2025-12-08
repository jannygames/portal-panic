using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifetime = 5f; // Auto-destroy after this time
    [SerializeField] private int damage = 3;
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("Layers to ignore (e.g., Player, UI). Bullet will pass through these.")]
    [SerializeField] private LayerMask ignoreLayers;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private bool destroyOnHit = true;
    [Tooltip("Sound effect to play when bullet hits something")]
    [SerializeField] private AudioClip impactSound;
    [Tooltip("Volume for the impact sound (0-1)")]
    [SerializeField] [Range(0, 1)] private float impactSoundVolume = 1f;
    
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
        
        // Configure rigidbody for bullet physics
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        spawnTime = Time.time;
        
        // Set initial velocity if direction was set
        if (direction != Vector3.zero)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            // Fallback: use forward direction
            rb.linearVelocity = transform.forward * speed;
        }
    }
    
    void Update()
    {
        // Auto-destroy after lifetime
        if (Time.time - spawnTime > lifetime)
        {
            DestroyBullet();
        }
    }
    
    /// <summary>
    /// Initialize the bullet with direction and settings
    /// </summary>
    public void Initialize(Vector3 shootDirection, float bulletSpeed, LayerMask enemyLayerMask, LayerMask ignoreLayerMask)
    {
        direction = shootDirection.normalized;
        speed = bulletSpeed;
        enemyLayer = enemyLayerMask;
        ignoreLayers = ignoreLayerMask;
        
        // Set velocity immediately if rigidbody exists
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return; // Prevent multiple hits
        
        // Ignore collisions with specified layers
        if (((1 << other.gameObject.layer) & ignoreLayers.value) != 0)
        {
            return;
        }
        
        // Check if we hit an enemy
        bool isEnemy = ((1 << other.gameObject.layer) & enemyLayer.value) != 0;
        
        if (isEnemy)
        {
            HandleEnemyHit(other);
        }
        else
        {
            // Hit something that's not an enemy (wall, obstacle, etc.)
            HandleObstacleHit(other);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return; // Prevent multiple hits

        if(collision.gameObject.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision.collider);
            return;
        }
        
        // Ignore collisions with specified layers
        if (((1 << collision.gameObject.layer) & ignoreLayers.value) != 0)
        {
            return;
        }
        
        // Check if we hit an enemy
        bool isEnemy = ((1 << collision.gameObject.layer) & enemyLayer.value) != 0;
        
        if (isEnemy)
        {
            HandleEnemyHit(collision.collider);
        }
        else
        {
            // Hit something that's not an enemy (wall, obstacle, etc.)
            HandleObstacleHit(collision.collider);
        }
    }
    
    private void HandleEnemyHit(Collider hitCollider)
    {
        hasHit = true;
        
        // Try to find EnemyAbstract component on the hit object, its parent, or children
        EnemyAbstract enemy = hitCollider.GetComponent<EnemyAbstract>();
        
        if (enemy == null)
        {
            enemy = hitCollider.GetComponentInParent<EnemyAbstract>();
        }
        
        if (enemy == null)
        {
            enemy = hitCollider.GetComponentInChildren<EnemyAbstract>();
        }
        
        if (enemy != null)
        {
            int instanceID = enemy.GetInstanceID();
            int healthBefore = enemy.health;
            
            Debug.Log($"Bullet: Hit enemy '{enemy.gameObject.name}' (Instance ID: {instanceID}). Current health: {healthBefore}");
            
            enemy.TakeDamageFromGun();
            
            if (enemy != null && enemy.gameObject != null)
            {
                int healthAfter = enemy.health;
                Debug.Log($"Bullet: ✓ Dealt {damage} damage to '{enemy.gameObject.name}' (Instance ID: {instanceID}). Health: {healthBefore} → {healthAfter}");
            }
            else
            {
                Debug.Log($"Bullet: ✓ Dealt {damage} damage to enemy (Instance ID: {instanceID}). Enemy destroyed (health was {healthBefore}).");
            }
        }
        else
        {
            Debug.LogWarning($"Bullet: Hit object '{hitCollider.gameObject.name}' on enemy layer but EnemyAbstract component not found!");
        }
        
        // Play impact sound effect
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position, impactSoundVolume);
        }
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (destroyOnHit)
        {
            DestroyBullet();
        }
    }
    
    private void HandleObstacleHit(Collider hitCollider)
    {
        hasHit = true;
        Debug.Log($"Bullet: Hit obstacle '{hitCollider.gameObject.name}' (layer: {LayerMask.LayerToName(hitCollider.gameObject.layer)})");
        
        // Play impact sound effect
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position, impactSoundVolume);
        }
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (destroyOnHit)
        {
            DestroyBullet();
        }
    }
    
    private void DestroyBullet()
    {
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        Destroy(gameObject);
    }
}

