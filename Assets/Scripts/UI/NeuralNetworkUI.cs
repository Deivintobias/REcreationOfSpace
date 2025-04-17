using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NeuralNetworkUI : MonoBehaviour
{
    [System.Serializable]
    public class NodeUI
    {
        public string nodeName;
        public RectTransform nodeRect;
        public Image fillImage;
        public Text developmentText;
        public Image lockIcon;
    }

    [Header("UI References")]
    public GameObject nodeUIPrefab;
    public RectTransform nodesContainer;
    public Text totalDevelopmentText;
    public Image freedomProgressBar;

    [Header("Visual Settings")]
    public float nodeSpacing = 100f;
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color unlockedColor = new Color(1f, 1f, 1f, 1f);
    public Color completedColor = new Color(0f, 1f, 0f, 1f);

    private Dictionary<string, NodeUI> nodeUIs = new Dictionary<string, NodeUI>();
    private NeuralNetwork neuralNetwork;
    private LineRenderer[] connectionLines;

    void Start()
    {
        neuralNetwork = FindObjectOfType<NeuralNetwork>();
        if (neuralNetwork != null)
        {
            InitializeUI();
        }
    }

    void InitializeUI()
    {
        // Create node UIs
        var nodes = neuralNetwork.GetUnlockedNodes();
        var developments = neuralNetwork.GetNodeDevelopments();

        float startY = 200f;
        float currentY = startY;

        foreach (var nodeName in nodes)
        {
            CreateNodeUI(nodeName, developments[nodeName], new Vector2(0, currentY));
            currentY -= nodeSpacing;
        }

        // Create connection lines
        CreateConnectionLines();

        // Initial update
        UpdateDisplay();
    }

    void CreateNodeUI(string nodeName, float development, Vector2 position)
    {
        if (nodeUIPrefab == null || nodesContainer == null) return;

        // Instantiate node UI
        GameObject nodeObj = Instantiate(nodeUIPrefab, nodesContainer);
        RectTransform rect = nodeObj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;

        // Setup node components
        NodeUI nodeUI = new NodeUI();
        nodeUI.nodeName = nodeName;
        nodeUI.nodeRect = rect;
        nodeUI.fillImage = nodeObj.transform.Find("FillImage")?.GetComponent<Image>();
        nodeUI.developmentText = nodeObj.transform.Find("DevelopmentText")?.GetComponent<Text>();
        nodeUI.lockIcon = nodeObj.transform.Find("LockIcon")?.GetComponent<Image>();

        // Set node name
        Text nameText = nodeObj.transform.Find("NameText")?.GetComponent<Text>();
        if (nameText != null)
        {
            nameText.text = nodeName;
        }

        nodeUIs[nodeName] = nodeUI;
    }

    void CreateConnectionLines()
    {
        // Create lines between dependent nodes
        List<LineRenderer> lines = new List<LineRenderer>();
        
        foreach (var node in neuralNetwork.nodes)
        {
            if (node.dependencies != null)
            {
                foreach (var dependency in node.dependencies)
                {
                    if (nodeUIs.ContainsKey(node.name) && nodeUIs.ContainsKey(dependency))
                    {
                        GameObject lineObj = new GameObject("ConnectionLine");
                        lineObj.transform.SetParent(transform);
                        
                        LineRenderer line = lineObj.AddComponent<LineRenderer>();
                        line.material = new Material(Shader.Find("Sprites/Default"));
                        line.startWidth = 2f;
                        line.endWidth = 2f;
                        line.positionCount = 2;
                        
                        lines.Add(line);
                    }
                }
            }
        }

        connectionLines = lines.ToArray();
        UpdateConnectionLines();
    }

    void UpdateConnectionLines()
    {
        if (connectionLines == null) return;

        int lineIndex = 0;
        foreach (var node in neuralNetwork.nodes)
        {
            if (node.dependencies != null)
            {
                foreach (var dependency in node.dependencies)
                {
                    if (nodeUIs.ContainsKey(node.name) && nodeUIs.ContainsKey(dependency) && lineIndex < connectionLines.Length)
                    {
                        Vector3 start = nodeUIs[dependency].nodeRect.position;
                        Vector3 end = nodeUIs[node.name].nodeRect.position;
                        
                        connectionLines[lineIndex].SetPosition(0, start);
                        connectionLines[lineIndex].SetPosition(1, end);
                        
                        // Color based on development
                        float progress = neuralNetwork.GetNodeDevelopment(dependency) / 100f;
                        connectionLines[lineIndex].startColor = Color.Lerp(lockedColor, completedColor, progress);
                        connectionLines[lineIndex].endColor = connectionLines[lineIndex].startColor;
                        
                        lineIndex++;
                    }
                }
            }
        }
    }

    public void UpdateDisplay()
    {
        if (neuralNetwork == null) return;

        foreach (var nodeUI in nodeUIs.Values)
        {
            float development = neuralNetwork.GetNodeDevelopment(nodeUI.nodeName);
            bool isUnlocked = neuralNetwork.IsNodeUnlocked(nodeUI.nodeName);

            // Update fill
            if (nodeUI.fillImage != null)
            {
                nodeUI.fillImage.fillAmount = development / 100f;
                nodeUI.fillImage.color = Color.Lerp(
                    isUnlocked ? unlockedColor : lockedColor,
                    completedColor,
                    development / 100f
                );
            }

            // Update text
            if (nodeUI.developmentText != null)
            {
                nodeUI.developmentText.text = $"{Mathf.Round(development)}%";
            }

            // Update lock icon
            if (nodeUI.lockIcon != null)
            {
                nodeUI.lockIcon.enabled = !isUnlocked;
            }
        }

        // Update total development
        if (totalDevelopmentText != null)
        {
            float total = neuralNetwork.GetTotalDevelopment();
            totalDevelopmentText.text = $"Total Development: {Mathf.Round(total)}%";
        }

        // Update freedom progress
        if (freedomProgressBar != null)
        {
            float freedom = neuralNetwork.GetFreedomLevel();
            freedomProgressBar.fillAmount = freedom / 100f;
        }

        // Update connection lines
        UpdateConnectionLines();
    }

    void OnDestroy()
    {
        // Cleanup connection lines
        if (connectionLines != null)
        {
            foreach (var line in connectionLines)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
        }
    }
}
