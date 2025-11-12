using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private int maxHearts = 5;
    private int currentHearts;

    [Header("Enemy Detection")]
    [SerializeField] private float enemyDetectionRadius = 2.0f; // Radius to detect nearby enemies
    [SerializeField] private LayerMask enemyLayer;

    private float damageCooldown = 1.0f; // Time between health loss
    private float damageTimer = 0.0f;

    void Start()
    {
        currentHearts = maxHearts; // Initialize player health
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
        return enemies.Length > 0;
    }

    public void TakeDamage(int damage)
    {
        currentHearts -= damage;
        Debug.Log($"Player took damage! Current hearts: {currentHearts}");

        if (currentHearts <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died! Returning to MainMenu...");
        SceneManager.LoadScene("MainMenu"); // Load the MainMenu scene
    }
}