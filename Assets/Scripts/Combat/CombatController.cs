using UnityEngine;

namespace REcreationOfSpace.Combat
{
    public class CombatController : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private LayerMask attackableLayers;

        [Header("Effects")]
        [SerializeField] private ParticleSystem attackEffect;
        [SerializeField] private AudioClip attackSound;

        private float lastAttackTime;
        private AudioSource audioSource;
        private Health health;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && attackSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            health = GetComponent<Health>();
        }

        public void Attack()
        {
            if (Time.time - lastAttackTime < attackCooldown)
                return;

            lastAttackTime = Time.time;

            // Play effects
            if (attackEffect != null)
                attackEffect.Play();

            if (audioSource != null && attackSound != null)
                audioSource.PlayOneShot(attackSound);

            // Check for hits
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, attackableLayers);
            foreach (var hit in hits)
            {
                // Don't damage self
                if (hit.gameObject == gameObject)
                    continue;

                var targetHealth = hit.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(attackDamage);
                }

                // Handle enemy AI response
                var enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.OnAttacked(gameObject);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize attack range in editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
