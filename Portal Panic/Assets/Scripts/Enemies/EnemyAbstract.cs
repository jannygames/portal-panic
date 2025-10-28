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
        player = GameObject.FindGameObjectWithTag("Player");

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

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(this.gameObject);
    }
}
