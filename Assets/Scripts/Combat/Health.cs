using UnityEngine;
using UnityEngine.Events;

namespace REcreationOfSpace.Combat
{
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private bool isInvulnerable = false;

        [Header("Effects")]
        [SerializeField] private ParticleSystem damageEffect;
        [SerializeField] private AudioClip damageSound;
        [SerializeField] private AudioClip deathSound;

        [Header("Events")]
        public UnityEvent onDeath;
        public UnityEvent<int, int> onHealthChanged; // current, max

        private int currentHealth;
        private AudioSource audioSource;
        private bool isDead = false;

        private void Awake()
        {
            currentHealth = maxHealth;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (damageSound != null || deathSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Notify UI of initial health
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            if (isInvulnerable || isDead || damage <= 0)
                return;

            currentHealth = Mathf.Max(0, currentHealth - damage);

            // Play effects
            if (damageEffect != null)
                damageEffect.Play();

            if (audioSource != null && damageSound != null)
                audioSource.PlayOneShot(damageSound);

            // Notify UI of health change
            onHealthChanged?.Invoke(currentHealth, maxHealth);

            // Check for death
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (isDead || amount <= 0)
                return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            isDead = true;

            // Play death sound
            if (audioSource != null && deathSound != null)
                audioSource.PlayOneShot(deathSound);

            // Notify listeners
            onDeath?.Invoke();

            // Handle respawn if applicable
            var respawnManager = GetComponent<RespawnManager>();
            if (respawnManager != null)
            {
                respawnManager.OnDeath();
            }
            else
            {
                // If no respawn manager, destroy the object
                Destroy(gameObject, deathSound != null ? deathSound.length : 0f);
            }
        }

        public float GetHealthPercentage()
        {
            return (float)currentHealth / maxHealth;
        }

        public bool IsDead()
        {
            return isDead;
        }
    }
}
