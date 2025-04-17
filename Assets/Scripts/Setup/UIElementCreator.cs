using UnityEngine;

namespace REcreationOfSpace.Setup
{
    public class UIElementCreator : MonoBehaviour
    {
        [SerializeField] private GameObject healthUIPrefab;
        [SerializeField] private GameObject resourceUIPrefab;
        [SerializeField] private GameObject teamUIPrefab;
        [SerializeField] private GameObject neuralNetworkUIPrefab;
        [SerializeField] private GameObject portalPromptPrefab;
        [SerializeField] private GameObject guiderMessagePrefab;
        [SerializeField] private Canvas mainCanvas;

        public void Initialize()
        {
            // Create main canvas if not present
            if (mainCanvas == null)
            {
                var canvasObj = new GameObject("Main Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.transform.parent = transform;
            }

            // Create UI elements
            CreateHealthUI();
            CreateResourceUI();
            CreateTeamUI();
            CreateNeuralNetworkUI();
            CreatePromptUI();
        }

        private void CreateHealthUI()
        {
            if (healthUIPrefab != null)
            {
                var healthUI = Instantiate(healthUIPrefab, mainCanvas.transform);
                var healthComponent = healthUI.GetComponent<HealthUI>();
                if (healthComponent != null)
                    healthComponent.enabled = true;
            }
        }

        private void CreateResourceUI()
        {
            if (resourceUIPrefab != null)
            {
                var resourceUI = Instantiate(resourceUIPrefab, mainCanvas.transform);
                var resourceComponent = resourceUI.GetComponent<ResourceUI>();
                if (resourceComponent != null)
                    resourceComponent.enabled = true;
            }
        }

        private void CreateTeamUI()
        {
            if (teamUIPrefab != null)
            {
                var teamUI = Instantiate(teamUIPrefab, mainCanvas.transform);
                var teamComponent = teamUI.GetComponent<TeamUI>();
                if (teamComponent != null)
                    teamComponent.enabled = true;
            }
        }

        private void CreateNeuralNetworkUI()
        {
            if (neuralNetworkUIPrefab != null)
            {
                var nnUI = Instantiate(neuralNetworkUIPrefab, mainCanvas.transform);
                var nnComponent = nnUI.GetComponent<NeuralNetworkUI>();
                if (nnComponent != null)
                    nnComponent.enabled = true;
            }
        }

        private void CreatePromptUI()
        {
            if (portalPromptPrefab != null)
            {
                var promptUI = Instantiate(portalPromptPrefab, mainCanvas.transform);
                var promptComponent = promptUI.GetComponent<PortalPromptUI>();
                if (promptComponent != null)
                    promptComponent.enabled = true;
            }

            if (guiderMessagePrefab != null)
            {
                var messageUI = Instantiate(guiderMessagePrefab, mainCanvas.transform);
                var messageComponent = messageUI.GetComponent<GuiderMessageUI>();
                if (messageComponent != null)
                    messageComponent.enabled = true;
            }
        }
    }
}
