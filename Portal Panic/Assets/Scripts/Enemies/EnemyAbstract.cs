using UnityEngine;
using UnityEngine.AI;

public class EnemyAbstract : MonoBehaviour
{
    [Range(0, 100)] public int health = 10;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] [Range(0, 5)] private float attackRange = 2.0f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] [Range(0, 100)] private float lookAheadDistance = 10f;
    [SerializeField] [Range(0, 100)] private float minDistanceForLookAhead = 5f;
    [SerializeField] private float attackAnimationDuration = 0.8f; // Duration of attack animation in seconds
    [SerializeField] private float damageAnimationDuration = 0.6f; // Duration of damage animation in seconds
    
    private Animator animator;
    private GameObject player;
    private PlayerHealth playerHealth;
    private float timePassed;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isTakingDamage = false;
    private bool hasDealtDamage = false;
    private float attackAnimationTimer = 0f;
    private float damageAnimationTimer = 0f;

    private float newDestinationCooldown = 0.5f;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    void Update()
    {
        if (isDead || player == null || agent == null)
        {
            return;
        }

        // Handle attack animation timing
        if (isAttacking)
        {
            attackAnimationTimer -= Time.deltaTime;

            // Deal damage at ~40% through the animation (adjust as needed)
            if (!hasDealtDamage && attackAnimationTimer < (attackAnimationDuration * 0.7f))
            {
                DealDamage();
            }

            // End attack when animation duration is done
            if (attackAnimationTimer <= 0)
            {
                OnAttackEnd();
            }
        }

        // Handle damage animation timing
        if (isTakingDamage)
        {
            damageAnimationTimer -= Time.deltaTime;

            // End damage animation when duration is done
            if (damageAnimationTimer <= 0)
            {
                OnDamageEnd();
            }
        }

        // Update speed parameter - set to 0 when attacking or taking damage
        float currentSpeed = (!isAttacking && !isTakingDamage) ? (agent.velocity.magnitude / agent.speed) : 0f;
        animator.SetFloat("speed", currentSpeed);

        // Attack logic
        if (timePassed >= attackCooldown && !isAttacking && !isTakingDamage)
        {
            if (Vector3.Distance(player.transform.position, transform.position) <= attackRange)
            {
                TriggerAttack();
                timePassed = 0;
            }
        }

        timePassed += Time.deltaTime;

        // Movement logic - only move when not attacking or taking damage
        if (!isAttacking && !isTakingDamage)
        {
            // Ensure agent has a path when not in animation
            if (agent.enabled && !agent.hasPath)
            {
                agent.SetDestination(player.transform.position);
            }

            if (newDestinationCooldown <= 0)
            {
                newDestinationCooldown = 0.5f;
                agent.SetDestination(player.transform.position);
            }

            newDestinationCooldown -= Time.deltaTime;
        }
        else
        {
            // Stop movement while attacking or taking damage
            if (agent.enabled && agent.hasPath)
            {
                agent.ResetPath();
            }
        }

        // Smart look direction
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer < minDistanceForLookAhead)
        {
            transform.LookAt(player.transform);
        }
        else if (distanceToPlayer < lookAheadDistance && agent.velocity.magnitude > 0.1f)
        {
            Vector3 lookDirection = agent.velocity.normalized;
            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }

    private void TriggerAttack()
    {
        if (animator != null && !isAttacking && !isTakingDamage)
        {
            isAttacking = true;
            hasDealtDamage = false;
            attackAnimationTimer = attackAnimationDuration;
            animator.SetTrigger("attack");
            Debug.Log($"{gameObject.name} is attacking!");
        }
    }

    // Called automatically based on animation timing
    public void DealDamage()
    {
        if (!hasDealtDamage && player != null && playerHealth != null)
        {
            hasDealtDamage = true;
            Debug.Log($"Enemy {gameObject.name} dealing damage to player!");
            playerHealth.TakeDamage(1, transform.position);
        }
        else if (!hasDealtDamage)
        {
            Debug.LogWarning($"DealDamage called but: player={player}, playerHealth={playerHealth}");
        }
    }

    // Called automatically when attack animation duration ends
    public void OnAttackEnd()
    {
        isAttacking = false;
        attackAnimationTimer = 0;
        Debug.Log($"Attack ended for {gameObject.name}");
    }

    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            Debug.Log($"Player set for {gameObject.name}, PlayerHealth found: {playerHealth != null}");
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (animator != null && !isTakingDamage)
        {
            isTakingDamage = true;
            damageAnimationTimer = damageAnimationDuration;
            animator.SetTrigger("damage");
            Debug.Log($"{gameObject.name} taking damage! Health: {health}");
        }
        if (health <= 0)
        {
            Die();
        }
    }

    // Called automatically when damage animation duration ends
    public void OnDamageEnd()
    {
        isTakingDamage = false;
        damageAnimationTimer = 0;
        Debug.Log($"Damage animation ended for {gameObject.name}");
    }

    public void TakeDamageFromGun()
    {
        TakeDamage(3);
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }

        if (agent != null)
        {
            agent.enabled = false;
        }

        if (KillCounterManager.Instance != null)
        {
            KillCounterManager.Instance.AddKill();
        }

        Destroy(gameObject);
    }
}
