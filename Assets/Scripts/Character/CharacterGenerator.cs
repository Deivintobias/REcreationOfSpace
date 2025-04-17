using UnityEngine;

namespace REcreationOfSpace.Character
{
    public class CharacterGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject guidePrefab;
        [SerializeField] private Vector3 playerSpawnPoint = Vector3.zero;

        private CharacterVisualGenerator visualGenerator;

        public void Initialize()
        {
            // Get or add the visual generator component
            visualGenerator = GetComponent<CharacterVisualGenerator>();
            if (visualGenerator == null)
                visualGenerator = gameObject.AddComponent<CharacterVisualGenerator>();

            // Generate initial characters
            SpawnPlayer();
            SetupCharacterSystems();
        }

        private void SpawnPlayer()
        {
            if (playerPrefab != null)
            {
                var player = Instantiate(playerPrefab, playerSpawnPoint, Quaternion.identity);
                player.transform.parent = transform;

                // Set up player components
                var controller = player.GetComponent<PlayerController>();
                if (controller != null)
                    controller.enabled = true;

                // Set up combat system
                var combat = player.GetComponent<CombatController>();
                if (combat != null)
                    combat.enabled = true;

                // Set up experience system
                var exp = player.GetComponent<ExperienceManager>();
                if (exp != null)
                    exp.enabled = true;

                // Apply visual customization
                if (visualGenerator != null)
                    visualGenerator.ApplyVisuals(player);
            }
        }

        private void SetupCharacterSystems()
        {
            // Initialize trading system
            var trading = GetComponent<TradingSystem>();
            if (trading != null)
                trading.enabled = true;

            // Initialize team system
            var team = GetComponent<TeamSystem>();
            if (team != null)
                team.enabled = true;

            // Initialize neural network for AI
            var neuralNet = GetComponent<NeuralNetwork>();
            if (neuralNet != null)
                neuralNet.enabled = true;

            // Initialize guide system
            var guide = GetComponent<GuiderSystem>();
            if (guide != null)
                guide.enabled = true;
        }
    }
}
