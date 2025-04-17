using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        onDeath?.Invoke();

        // Handle player death differently from other entities
        if (gameObject.CompareTag("Player"))
        {
            // Disable player controls during death sequence
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Handle respawn through RespawnManager
            RespawnManager.Instance?.HandlePlayerDeath(gameObject);
        }
        else
        {
            // Non-player entities are destroyed
            Destroy(gameObject);
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
