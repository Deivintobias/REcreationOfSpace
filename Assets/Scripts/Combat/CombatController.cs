using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 20f;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;

    private float nextAttackTime;
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // Check for attack input (left mouse button)
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        nextAttackTime = Time.time + attackCooldown;

        // Create a sphere cast to detect enemies in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (var hitCollider in hitColliders)
        {
            // Check if the hit object has a Health component
            Health enemyHealth = hitCollider.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Apply damage
                enemyHealth.TakeDamage(attackDamage);
            }
        }

        // Here you could trigger attack animation
        // animator.SetTrigger("Attack");
    }

    // Helper method to visualize attack range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
