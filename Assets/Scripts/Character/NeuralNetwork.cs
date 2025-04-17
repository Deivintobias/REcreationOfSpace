using UnityEngine;
using System.Collections.Generic;

namespace REcreationOfSpace.Character
{
    public class NeuralNetwork : MonoBehaviour
    {
        [System.Serializable]
        public class Neuron
        {
            public string id;
            public float value;
            public float bias;
            public List<string> connections = new List<string>();
            public List<float> weights = new List<float>();
        }

        [Header("Network Settings")]
        [SerializeField] private int inputNeurons = 8;
        [SerializeField] private int hiddenNeurons = 12;
        [SerializeField] private int outputNeurons = 4;
        [SerializeField] private float learningRate = 0.1f;
        [SerializeField] private float mutationRate = 0.1f;

        private List<Neuron> neurons = new List<Neuron>();
        private Dictionary<string, int> neuronIndices = new Dictionary<string, int>();

        private void Awake()
        {
            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            neurons.Clear();
            neuronIndices.Clear();

            // Create input neurons
            for (int i = 0; i < inputNeurons; i++)
            {
                CreateNeuron($"input_{i}");
            }

            // Create hidden neurons
            for (int i = 0; i < hiddenNeurons; i++)
            {
                var neuron = CreateNeuron($"hidden_{i}");
                
                // Connect to all input neurons
                for (int j = 0; j < inputNeurons; j++)
                {
                    ConnectNeurons($"input_{j}", neuron.id);
                }
            }

            // Create output neurons
            for (int i = 0; i < outputNeurons; i++)
            {
                var neuron = CreateNeuron($"output_{i}");
                
                // Connect to all hidden neurons
                for (int j = 0; j < hiddenNeurons; j++)
                {
                    ConnectNeurons($"hidden_{j}", neuron.id);
                }
            }
        }

        private Neuron CreateNeuron(string id)
        {
            var neuron = new Neuron
            {
                id = id,
                value = 0f,
                bias = Random.Range(-1f, 1f)
            };

            neurons.Add(neuron);
            neuronIndices[id] = neurons.Count - 1;
            return neuron;
        }

        private void ConnectNeurons(string fromId, string toId)
        {
            if (!neuronIndices.ContainsKey(fromId) || !neuronIndices.ContainsKey(toId))
                return;

            var toNeuron = neurons[neuronIndices[toId]];
            toNeuron.connections.Add(fromId);
            toNeuron.weights.Add(Random.Range(-1f, 1f));
        }

        public float[] ProcessInputs(float[] inputs)
        {
            if (inputs.Length != inputNeurons)
                return null;

            // Set input values
            for (int i = 0; i < inputs.Length; i++)
            {
                neurons[i].value = inputs[i];
            }

            // Process hidden and output neurons
            for (int i = inputNeurons; i < neurons.Count; i++)
            {
                var neuron = neurons[i];
                float sum = neuron.bias;

                for (int j = 0; j < neuron.connections.Count; j++)
                {
                    var fromNeuron = neurons[neuronIndices[neuron.connections[j]]];
                    sum += fromNeuron.value * neuron.weights[j];
                }

                neuron.value = Activate(sum);
            }

            // Get output values
            float[] outputs = new float[outputNeurons];
            int outputStart = neurons.Count - outputNeurons;
            for (int i = 0; i < outputNeurons; i++)
            {
                outputs[i] = neurons[outputStart + i].value;
            }

            return outputs;
        }

        private float Activate(float x)
        {
            // Hyperbolic tangent activation function
            return Mathf.Tanh(x);
        }

        public void Mutate()
        {
            foreach (var neuron in neurons)
            {
                if (Random.value < mutationRate)
                {
                    neuron.bias += Random.Range(-0.5f, 0.5f);
                }

                for (int i = 0; i < neuron.weights.Count; i++)
                {
                    if (Random.value < mutationRate)
                    {
                        neuron.weights[i] += Random.Range(-0.5f, 0.5f);
                    }
                }
            }
        }

        public void Learn(float[] expectedOutputs)
        {
            if (expectedOutputs.Length != outputNeurons)
                return;

            int outputStart = neurons.Count - outputNeurons;
            
            // Update output neurons
            for (int i = 0; i < outputNeurons; i++)
            {
                var neuron = neurons[outputStart + i];
                float error = expectedOutputs[i] - neuron.value;

                // Update weights
                for (int j = 0; j < neuron.connections.Count; j++)
                {
                    var fromNeuron = neurons[neuronIndices[neuron.connections[j]]];
                    float delta = learningRate * error * fromNeuron.value;
                    neuron.weights[j] += delta;
                }

                // Update bias
                neuron.bias += learningRate * error;
            }
        }

        public NeuralNetworkData GetNetworkData()
        {
            return new NeuralNetworkData
            {
                neurons = new List<Neuron>(neurons)
            };
        }

        public void LoadNetworkData(NeuralNetworkData data)
        {
            neurons = new List<Neuron>(data.neurons);
            neuronIndices.Clear();
            for (int i = 0; i < neurons.Count; i++)
            {
                neuronIndices[neurons[i].id] = i;
            }
        }
    }

    [System.Serializable]
    public class NeuralNetworkData
    {
        public List<Neuron> neurons;
    }
}
