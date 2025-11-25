using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private int maxHearts = 5;
    private int currentHearts;

    [Header("HUD")]
    [SerializeField] private HUDController hudController; // Reference to the HUDController

    [Header("Enemy Detection")]
    [SerializeField] private float enemyDetectionRadius = 2.0f; // Radius to detect nearby enemies
    [SerializeField] private LayerMask enemyLayer; // Layer mask for enemies
    [SerializeField] private string enemyTag = "Enemy"; // Tag to identify enemies (fallback method)

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5.0f; // Horizontal knockback force
    [SerializeField] private float upwardForce = 3.0f; // Upward knockback force
    [SerializeField] private float knockbackDuration = 0.5f; // How long knockback lasts

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1.0f; // Time player is invincible after taking damage

    private float damageCooldown = 1.0f; // Time between health loss
    private float damageTimer = 0.0f;
    private float invincibilityTimer = 0.0f;
    private bool isInvincible = false;

    private CharacterController characterController;
    private Vector3 knockbackVelocity = Vector3.zero;
    private bool isKnockedBack = false;

    void Start()
    {
        currentHearts = maxHearts; // Initialize player health
        UpdateHUD(); // Update the HUD at the start

        // Get Character Controller component
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("PlayerHealth: CharacterController component not found!");
        }

        // Validate setup
        if (enemyLayer.value == 0)
        {
            Debug.LogWarning("PlayerHealth: Enemy layer mask is not set. Will use tag-based detection only.");
        }
    }

    void Update()
    {
        // Update invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0.0f)
            {
                isInvincible = false;
            }
        }

        // Handle knockback
        if (isKnockedBack)
        {
            // Apply knockback movement
            if (characterController != null)
            {
                characterController.Move(knockbackVelocity * Time.deltaTime);
            }

            // Gradually reduce knockback velocity
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime / knockbackDuration);

            // Check if knockback is finished
            if (knockbackVelocity.magnitude < 0.1f)
            {
                knockbackVelocity = Vector3.zero;
                isKnockedBack = false;
            }
        }

        // Only check for damage if not invincible
        if (!isInvincible)
        {
            damageTimer += Time.deltaTime;

            // Check for nearby enemies every second
            if (damageTimer >= damageCooldown)
            {
                damageTimer = 0.0f;

                GameObject nearestEnemy = GetNearestEnemy();
                if (nearestEnemy != null)
                {
                    TakeDamage(1, nearestEnemy.transform.position);
                }
            }
        }
    }

    private GameObject GetNearestEnemy()
    {
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        // Method 1: Check using Physics.OverlapSphere (requires colliders)
        // Note: Works with both trigger and non-trigger colliders
        if (enemyLayer.value != 0)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, enemyLayer);
            if (enemies.Length > 0)
            {
                foreach (Collider enemyCollider in enemies)
                {
                    if (enemyCollider == null || enemyCollider.gameObject == null) continue;

                    float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
                    if (distance < nearestDistance && distance <= enemyDetectionRadius)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemyCollider.gameObject;
                    }
                }
            }
        }

        // Method 2: Fallback - Find all enemies by tag and check distance (works without colliders)
        if (nearestEnemy == null && !string.IsNullOrEmpty(enemyTag))
        {
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag(enemyTag);

            foreach (GameObject enemy in allEnemies)
            {
                if (enemy == null) continue; // Skip destroyed enemies

                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance && distance <= enemyDetectionRadius)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }

        if (nearestEnemy != null)
        {
            Debug.Log($"Detected nearest enemy at distance: {nearestDistance}");
        }

        return nearestEnemy;
    }

    public void TakeDamage(int damage)
    {
        // If no enemy position provided, try to find nearest enemy
        GameObject nearestEnemy = GetNearestEnemy();
        Vector3 enemyPosition = nearestEnemy != null ? nearestEnemy.transform.position : transform.position + Vector3.forward;
        TakeDamage(damage, enemyPosition);
    }

    public void TakeDamage(int damage, Vector3 enemyPosition)
    {
        // Don't take damage if invincible
        if (isInvincible)
        {
            return;
        }

        currentHearts -= damage;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts); // Ensure health doesn't go below 0
        UpdateHUD();

        Debug.Log($"Player took {damage} damage. Current hearts: {currentHearts}");

        // Apply knockback
        ApplyKnockback(enemyPosition);

        // Start invincibility
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        if (currentHearts <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback(Vector3 enemyPosition)
    {
        if (characterController == null)
        {
            Debug.LogWarning("PlayerHealth: Cannot apply knockback - CharacterController not found!");
            return;
        }

        // Calculate direction from enemy to player (opposite direction)
        Vector3 direction = (transform.position - enemyPosition).normalized;
        
        // Remove Y component for horizontal direction, then normalize
        direction.y = 0;
        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();
        }
        else
        {
            // If enemy is directly above/below, push backward (away from forward)
            direction = -transform.forward;
        }

        // Apply knockback force: backward horizontal + upward
        knockbackVelocity = direction * knockbackForce + Vector3.up * upwardForce;
        isKnockedBack = true;

        Debug.Log($"Knockback applied! Direction: {direction}, Force: {knockbackVelocity}");
    }

    private void UpdateHUD()
    {
        if (hudController != null)
        {
            hudController.UpdateHearts(currentHearts);
        }
    }

    private void Die()
    {
        Debug.Log("Player died! Returning to MainMenu...");
        SceneManager.LoadScene("MainMenu"); // Load the MainMenu scene
    }

    // Visualize the detection radius in the Scene view (for debugging)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);
    }
}