using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    private Transform player;
    private NavMeshAgent agent;
    private Health health;
    private float nextAttackTime;

    private enum EnemyState
    {
        Idle,
        Chasing,
        Attacking
    }

    private EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Subscribe to death event
        health.onDeath.AddListener(OnDeath);
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Update state based on distance to player
        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chasing;
        }
        else
        {
            currentState = EnemyState.Idle;
        }

        // Handle behavior based on state
        switch (currentState)
        {
            case EnemyState.Idle:
                // Could add patrol behavior here
                agent.isStopped = true;
                break;

            case EnemyState.Chasing:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                break;

            case EnemyState.Attacking:
                agent.isStopped = true;
                if (Time.time >= nextAttackTime)
                {
                    AttackPlayer();
                }
                break;
        }
    }

    private void AttackPlayer()
    {
        // Reset attack cooldown
        nextAttackTime = Time.time + attackCooldown;

        // Get player's health component and apply damage
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        // Here you could trigger attack animation
        // animator.SetTrigger("Attack");
    }

    private void OnDeath()
    {
        // Handle enemy death
        agent.isStopped = true;
        enabled = false;
        
        // You might want to play death animation here before destroying
        Destroy(gameObject, 2f); // Destroy after 2 seconds
    }

    // Visualize detection and attack ranges in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
