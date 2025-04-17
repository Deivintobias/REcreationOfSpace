using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace REcreationOfSpace.UI
{
    public class NeuralNetworkUI : MonoBehaviour
    {
        [System.Serializable]
        private class NeuronUI
        {
            public string id;
            public RectTransform rect;
            public Image background;
            public TextMeshProUGUI valueText;
            public List<LineRenderer> connections = new List<LineRenderer>();
        }

        [Header("UI Elements")]
        [SerializeField] private GameObject neuronPrefab;
        [SerializeField] private GameObject connectionPrefab;
        [SerializeField] private RectTransform networkContainer;
        [SerializeField] private float updateInterval = 0.1f;

        [Header("Layout")]
        [SerializeField] private float layerSpacing = 200f;
        [SerializeField] private float neuronSpacing = 100f;
        [SerializeField] private Vector2 neuronSize = new Vector2(80f, 80f);

        [Header("Visuals")]
        [SerializeField] private Color positiveColor = Color.green;
        [SerializeField] private Color negativeColor = Color.red;
        [SerializeField] private Color neutralColor = Color.gray;
        [SerializeField] private bool showValues = true;
        [SerializeField] private string valueFormat = "F2";

        private Dictionary<string, NeuronUI> neuronUIs = new Dictionary<string, NeuronUI>();
        private NeuralNetwork targetNetwork;
        private float updateTimer;

        private void Start()
        {
            // Find neural network
            targetNetwork = FindObjectOfType<NeuralNetwork>();
            if (targetNetwork != null)
            {
                CreateNetworkVisualization();
            }
        }

        private void Update()
        {
            if (targetNetwork == null)
                return;

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateVisualization();
            }
        }

        private void CreateNetworkVisualization()
        {
            ClearVisualization();

            var networkData = targetNetwork.GetNetworkData();
            if (networkData == null || networkData.neurons == null)
                return;

            // Group neurons by layer
            Dictionary<string, List<NeuralNetwork.Neuron>> layers = new Dictionary<string, List<NeuralNetwork.Neuron>>();
            foreach (var neuron in networkData.neurons)
            {
                string layer = neuron.id.Split('_')[0];
                if (!layers.ContainsKey(layer))
                {
                    layers[layer] = new List<NeuralNetwork.Neuron>();
                }
                layers[layer].Add(neuron);
            }

            // Create neuron UIs
            float xOffset = 0f;
            foreach (var layer in new[] { "input", "hidden", "output" })
            {
                if (!layers.ContainsKey(layer))
                    continue;

                var neurons = layers[layer];
                float yOffset = -(neurons.Count - 1) * neuronSpacing * 0.5f;

                foreach (var neuron in neurons)
                {
                    CreateNeuronUI(neuron, new Vector2(xOffset, yOffset));
                    yOffset += neuronSpacing;
                }

                xOffset += layerSpacing;
            }

            // Create connections
            foreach (var neuron in networkData.neurons)
            {
                for (int i = 0; i < neuron.connections.Count; i++)
                {
                    CreateConnectionUI(neuron.connections[i], neuron.id, neuron.weights[i]);
                }
            }
        }

        private void CreateNeuronUI(NeuralNetwork.Neuron neuron, Vector2 position)
        {
            if (neuronPrefab == null || networkContainer == null)
                return;

            GameObject neuronObj = Instantiate(neuronPrefab, networkContainer);
            RectTransform rect = neuronObj.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = neuronSize;

            NeuronUI neuronUI = new NeuronUI
            {
                id = neuron.id,
                rect = rect,
                background = neuronObj.GetComponent<Image>(),
                valueText = neuronObj.GetComponentInChildren<TextMeshProUGUI>()
            };

            if (neuronUI.valueText != null)
            {
                neuronUI.valueText.gameObject.SetActive(showValues);
            }

            neuronUIs[neuron.id] = neuronUI;
        }

        private void CreateConnectionUI(string fromId, string toId, float weight)
        {
            if (!neuronUIs.ContainsKey(fromId) || !neuronUIs.ContainsKey(toId))
                return;

            GameObject connectionObj = Instantiate(connectionPrefab, networkContainer);
            LineRenderer line = connectionObj.GetComponent<LineRenderer>();
            if (line != null)
            {
                // Set line color based on weight
                Color color = weight > 0 ? positiveColor : negativeColor;
                color.a = Mathf.Abs(weight);
                line.startColor = line.endColor = color;

                // Add to from neuron's connections
                neuronUIs[toId].connections.Add(line);
            }
        }

        private void UpdateVisualization()
        {
            var networkData = targetNetwork.GetNetworkData();
            if (networkData == null || networkData.neurons == null)
                return;

            foreach (var neuron in networkData.neurons)
            {
                if (!neuronUIs.ContainsKey(neuron.id))
                    continue;

                var neuronUI = neuronUIs[neuron.id];

                // Update color based on value
                if (neuronUI.background != null)
                {
                    Color color = neuron.value > 0 ? positiveColor : (neuron.value < 0 ? negativeColor : neutralColor);
                    color.a = Mathf.Abs(neuron.value);
                    neuronUI.background.color = color;
                }

                // Update value text
                if (neuronUI.valueText != null && showValues)
                {
                    neuronUI.valueText.text = neuron.value.ToString(valueFormat);
                }

                // Update connection positions
                foreach (var line in neuronUI.connections)
                {
                    UpdateConnectionPosition(line, neuronUI.rect);
                }
            }
        }

        private void UpdateConnectionPosition(LineRenderer line, RectTransform toRect)
        {
            if (line == null)
                return;

            Vector3[] positions = new Vector3[2];
            line.GetPositions(positions);
            positions[1] = toRect.position;
            line.SetPositions(positions);
        }

        private void ClearVisualization()
        {
            foreach (var neuronUI in neuronUIs.Values)
            {
                foreach (var connection in neuronUI.connections)
                {
                    if (connection != null)
                        Destroy(connection.gameObject);
                }

                if (neuronUI.rect != null)
                    Destroy(neuronUI.rect.gameObject);
            }

            neuronUIs.Clear();
        }

        private void OnDestroy()
        {
            ClearVisualization();
        }
    }
}
