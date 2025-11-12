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

    private float damageCooldown = 1.0f; // Time between health loss
    private float damageTimer = 0.0f;

    void Start()
    {
        currentHearts = maxHearts; // Initialize player health
        UpdateHUD(); // Update the HUD at the start
    }

    void Update()
    {
        damageTimer += Time.deltaTime;

        // Check for nearby enemies every second
        if (damageTimer >= damageCooldown)
        {
            damageTimer = 0.0f;

            if (IsEnemyNearby())
            {
                TakeDamage(1);
            }
        }
    }

    private bool IsEnemyNearby()
    {
        // Check for enemies within the detection radius
        Collider[] enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, enemyLayer);

        // Debugging: Log detected enemies
        if (enemies.Length > 0)
        {
            Debug.Log($"Detected {enemies.Length} enemies nearby.");
        }

        return enemies.Length > 0; // Return true if at least one enemy is nearby
    }

    public void TakeDamage(int damage)
    {
        currentHearts -= damage;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts); // Ensure health doesn't go below 0
        UpdateHUD();

        Debug.Log($"Player took {damage} damage. Current hearts: {currentHearts}");

        if (currentHearts <= 0)
        {
            Die();
        }
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
}