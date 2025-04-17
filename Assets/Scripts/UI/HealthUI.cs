using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Image healthBarFill;
    public Text healthText;

    private Health playerHealth;

    private void Start()
    {
        // Find the player's health component
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        
        if (playerHealth != null)
        {
            // Subscribe to health changes
            playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
            
            // Initial UI update
            UpdateHealthUI(playerHealth.currentHealth);
        }
        else
        {
            Debug.LogError("Player Health component not found!");
        }
    }

    private void UpdateHealthUI(float currentHealth)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = playerHealth.GetHealthPercentage();
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)} / {playerHealth.maxHealth}";
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            // Unsubscribe from health changes
            playerHealth.onHealthChanged.RemoveListener(UpdateHealthUI);
        }
    }
}
