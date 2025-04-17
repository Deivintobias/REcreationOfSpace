using UnityEngine;
using UnityEngine.AI;

namespace REcreationOfSpace.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float patrolRadius = 10f;
        [SerializeField] private float minPatrolWaitTime = 2f;
        [SerializeField] private float maxPatrolWaitTime = 5f;

        private NavMeshAgent agent;
        private GameObject target;
        private Vector3 startPosition;
        private CombatController combat;
        private Health health;
        private float nextPatrolTime;

        private enum State { Patrol, Chase, Attack }
        private State currentState;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            combat = GetComponent<CombatController>();
            health = GetComponent<Health>();
            startPosition = transform.position;
            currentState = State.Patrol;
        }

        private void Start()
        {
            SetNewPatrolDestination();
        }

        private void Update()
        {
            if (health != null && health.IsDead())
                return;

            switch (currentState)
            {
                case State.Patrol:
                    Patrol();
                    break;
                case State.Chase:
                    ChaseTarget();
                    break;
                case State.Attack:
                    AttackTarget();
                    break;
            }
        }

        private void Patrol()
        {
            // Look for potential targets
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    target = collider.gameObject;
                    currentState = State.Chase;
                    return;
                }
            }

            // Handle patrol movement
            if (Time.time >= nextPatrolTime && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                SetNewPatrolDestination();
            }
        }

        private void ChaseTarget()
        {
            if (target == null)
            {
                currentState = State.Patrol;
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget <= attackRange)
            {
                currentState = State.Attack;
            }
            else if (distanceToTarget > detectionRange)
            {
                target = null;
                currentState = State.Patrol;
            }
            else
            {
                agent.SetDestination(target.transform.position);
            }
        }

        private void AttackTarget()
        {
            if (target == null)
            {
                currentState = State.Patrol;
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget > attackRange)
            {
                currentState = State.Chase;
                return;
            }

            // Face the target
            Vector3 direction = (target.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            // Attack
            if (combat != null)
            {
                combat.Attack();
            }
        }

        private void SetNewPatrolDestination()
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += startPosition;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
            {
                agent.SetDestination(hit.position);
                nextPatrolTime = Time.time + Random.Range(minPatrolWaitTime, maxPatrolWaitTime);
            }
        }

        public void OnAttacked(GameObject attacker)
        {
            if (currentState == State.Patrol)
            {
                target = attacker;
                currentState = State.Chase;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw patrol radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, patrolRadius);
        }
    }
}
