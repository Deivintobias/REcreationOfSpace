using UnityEngine;
using UnityEngine.Events;

namespace REcreationOfSpace.Character
{
    public class ExperienceManager : MonoBehaviour
    {
        [Header("Experience Settings")]
        [SerializeField] private int experienceToLevel = 100;
        [SerializeField] private float experienceMultiplier = 1.5f;
        [SerializeField] private int maxLevel = 50;

        [Header("Level Up Effects")]
        [SerializeField] private ParticleSystem levelUpEffect;
        [SerializeField] private AudioClip levelUpSound;

        public UnityEvent<int> onLevelUp; // Current level
        public UnityEvent<int, int> onExperienceGained; // Current XP, XP needed for next level

        private int currentLevel = 1;
        private int currentExperience = 0;
        private AudioSource audioSource;
        private Health health;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && levelUpSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            health = GetComponent<Health>();
        }

        public void GainExperience(int amount)
        {
            if (currentLevel >= maxLevel)
                return;

            currentExperience += amount;
            int experienceNeeded = GetExperienceForNextLevel();

            // Notify UI
            onExperienceGained?.Invoke(currentExperience, experienceNeeded);

            // Check for level up
            while (currentExperience >= experienceNeeded && currentLevel < maxLevel)
            {
                LevelUp();
                experienceNeeded = GetExperienceForNextLevel();
            }
        }

        private void LevelUp()
        {
            currentLevel++;
            currentExperience -= GetExperienceForNextLevel();

            // Play effects
            if (levelUpEffect != null)
            {
                levelUpEffect.Play();
            }

            if (audioSource != null && levelUpSound != null)
            {
                audioSource.PlayOneShot(levelUpSound);
            }

            // Increase max health
            if (health != null)
            {
                // Heal to full and increase max health
                health.Heal(999);
            }

            // Notify listeners
            onLevelUp?.Invoke(currentLevel);
        }

        private int GetExperienceForNextLevel()
        {
            return Mathf.RoundToInt(experienceToLevel * Mathf.Pow(experienceMultiplier, currentLevel - 1));
        }

        public int GetCurrentLevel()
        {
            return currentLevel;
        }

        public float GetLevelProgress()
        {
            return (float)currentExperience / GetExperienceForNextLevel();
        }

        public bool IsMaxLevel()
        {
            return currentLevel >= maxLevel;
        }
    }
}
