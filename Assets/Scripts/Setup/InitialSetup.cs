using UnityEngine;

namespace REcreationOfSpace.Setup
{
    public class InitialSetup : MonoBehaviour
    {
        private void Awake()
        {
            // Initialize game systems
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            // Initialize world generation
            var worldGen = GetComponent<WorldGenerator>();
            if (worldGen != null)
                worldGen.Initialize();

            // Initialize character systems
            var charGen = GetComponent<CharacterGenerator>();
            if (charGen != null)
                charGen.Initialize();

            // Initialize UI
            var uiCreator = GetComponent<UIElementCreator>();
            if (uiCreator != null)
                uiCreator.Initialize();

            // Initialize game setup
            var gameSetup = GetComponent<GameSetup>();
            if (gameSetup != null)
                gameSetup.Initialize();
        }
    }
}
