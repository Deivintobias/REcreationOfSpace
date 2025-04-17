using UnityEngine;
using System.Collections;

namespace REcreationOfSpace.World
{
    public class RespawnManager : MonoBehaviour
    {
        [Header("Respawn Settings")]
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField] private Vector3 respawnOffset = Vector3.up;
        [SerializeField] private bool useCheckpoints = true;

        [Header("Effects")]
        [SerializeField] private GameObject respawnEffect;
        [SerializeField] private AudioClip respawnSound;

        private Vector3 lastCheckpoint;
        private Health health;
        private AudioSource audioSource;
        private ScreenFade screenFade;

        private void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && respawnSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Find screen fade component
            screenFade = FindObjectOfType<ScreenFade>();

            // Set initial checkpoint
            lastCheckpoint = transform.position;
        }

        public void OnDeath()
        {
            StartCoroutine(RespawnSequence());
        }

        private IEnumerator RespawnSequence()
        {
            // Fade out
            if (screenFade != null)
            {
                yield return screenFade.FadeOut();
            }

            // Wait for delay
            yield return new WaitForSeconds(respawnDelay);

            // Reset position
            Vector3 respawnPosition = useCheckpoints ? lastCheckpoint : transform.position;
            respawnPosition += respawnOffset;
            transform.position = respawnPosition;

            // Reset health
            if (health != null)
            {
                health.Heal(999); // Full heal
            }

            // Play effects
            if (respawnEffect != null)
            {
                Instantiate(respawnEffect, transform.position, Quaternion.identity);
            }

            if (audioSource != null && respawnSound != null)
            {
                audioSource.PlayOneShot(respawnSound);
            }

            // Fade in
            if (screenFade != null)
            {
                yield return screenFade.FadeIn();
            }
        }

        public void SetCheckpoint(Vector3 position)
        {
            if (useCheckpoints)
            {
                lastCheckpoint = position;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check for checkpoint triggers
            if (useCheckpoints && other.CompareTag("Checkpoint"))
            {
                SetCheckpoint(other.transform.position);
            }
        }
    }
}
