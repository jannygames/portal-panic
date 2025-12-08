using UnityEngine;
using UnityEngine.AI;

public class EnemyAbstract : MonoBehaviour
{
    [Range(0, 100)] public int health = 10;
    [Range(0, 100)] public int speed = 10;
    [SerializeField] [Range(0, 50)] private int damageToPlayer = 1;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] [Range(0, 10)] private float stoppingDistance = 1.5f;
    [SerializeField] [Range(0, 5)] private float attackRange = 2.0f; // Range at which enemy can attack
    [SerializeField] private float attackCooldown = 1.0f; // Time between attacks

    private Rigidbody rb;
    private Animator animator;
    private GameObject player;
    private float lastAttackTime = 0f;
    private bool isDead = false;

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
        // Don't update if dead
        if (isDead)
        {
            return;
        }

        // Always follow the player if they exist
        if (player != null && agent != null && agent.enabled)
        {
            agent.SetDestination(player.transform.position);
            
            // Check if enemy is close enough to attack
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                TriggerAttack();
                lastAttackTime = Time.time;
            }
        }
    }
    
    private void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void TakeDamageFromGun()
    {
        TakeDamage(3); // Each gun hit deals 3 damage
    }

    public void Die()
    {
        // Don't die twice
        if (isDead)
        {
            return;
        }
        
        isDead = true;
        
        // Set death animation parameter
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        
        // Disable agent movement
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // Notify the kill counter manager
        if (KillCounterManager.Instance != null)
        {
            KillCounterManager.Instance.AddKill();
        }

        // Destroy after a short delay to allow death animation to play
        Destroy(this.gameObject, 2.0f);
    }
}
