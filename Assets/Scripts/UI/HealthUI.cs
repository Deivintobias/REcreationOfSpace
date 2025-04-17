using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace REcreationOfSpace.UI
{
    public class HealthUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image healthBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image damageFlashOverlay;

        [Header("Settings")]
        [SerializeField] private float damageFlashDuration = 0.2f;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private bool showNumericValue = true;

        private Health playerHealth;
        private float flashTimer;

        private void Start()
        {
            // Find player health component
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    // Subscribe to health change events
                    playerHealth.onHealthChanged.AddListener(UpdateHealth);
                }
            }

            // Initialize UI elements
            if (damageFlashOverlay != null)
            {
                damageFlashOverlay.color = Color.clear;
            }
        }

        private void Update()
        {
            // Handle damage flash effect
            if (flashTimer > 0)
            {
                flashTimer -= Time.deltaTime;
                if (damageFlashOverlay != null)
                {
                    float alpha = (flashTimer / damageFlashDuration) * damageFlashColor.a;
                    damageFlashOverlay.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, alpha);
                }
            }
        }

        private void UpdateHealth(int currentHealth, int maxHealth)
        {
            // Update health bar
            if (healthBar != null)
            {
                healthBar.fillAmount = (float)currentHealth / maxHealth;
            }

            // Update health text
            if (healthText != null && showNumericValue)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }

            // Trigger damage flash
            if (damageFlashOverlay != null)
            {
                flashTimer = damageFlashDuration;
                damageFlashOverlay.color = damageFlashColor;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (playerHealth != null)
            {
                playerHealth.onHealthChanged.RemoveListener(UpdateHealth);
            }
        }
    }
}
