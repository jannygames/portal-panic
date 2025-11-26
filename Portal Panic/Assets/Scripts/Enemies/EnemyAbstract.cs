using UnityEngine;
using UnityEngine.AI;

public class EnemyAbstract : MonoBehaviour
{
    [Range(0, 100)] public int health = 10;
    [Range(0, 100)] public int speed = 10;
    [SerializeField] [Range(0, 50)] private int damageToPlayer = 1;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] [Range(0, 10)] private float stoppingDistance = 1.5f;

    private Rigidbody rb;
    private Animator animator;
    private GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Initialize agent if not assigned in inspector
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Set the vars of agent
        if (agent != null)
        {
            agent.speed = speed;
            agent.stoppingDistance = stoppingDistance;
            agent.updateRotation = true;
            agent.updateUpAxis = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Always follow the player if they exist
        if (player != null && agent != null && agent.enabled)
        {
            agent.SetDestination(player.transform.position);
        }
    }

    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    public void TakeDamage(int damage)
    {
        int healthBefore = health;
        health -= damage;
        
        // Clamp health to minimum 0 (shouldn't be negative)
        if (health < 0)
        {
            health = 0;
        }
        
        Debug.Log($"[EnemyAbstract] {gameObject.name}: Taking {damage} damage. Health: {healthBefore} → {health}");
        
        if (health <= 0)
        {
            Debug.Log($"[EnemyAbstract] {gameObject.name}: Health reached 0 or below ({health}). Dying...");
            Die();
        }
        else
        {
            Debug.Log($"[EnemyAbstract] {gameObject.name}: Health remaining: {health}. Still alive.");
        }
    }

    public void Die()
    {
        Debug.Log($"[EnemyAbstract] {gameObject.name}: Destroying enemy object.");
        Destroy(this.gameObject);
    }

    public void TakeDamageFromGun()
    {
        TakeDamage(3); // Each gun hit deals 3 damage
    }
}
