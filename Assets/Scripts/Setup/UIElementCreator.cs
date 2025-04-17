using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

public class UIElementCreator : MonoBehaviour
{
    public static void CreateNeuralNodePrefab()
    {
        // Previous neural node creation code remains the same
        // ... (keeping existing code)
    }

    public static void CreateNeuralNetworkPanel()
    {
        // Previous neural network panel creation code remains the same
        // ... (keeping existing code)
    }

    public static void CreateTeamUI()
    {
        // Previous team UI creation code remains the same
        // ... (keeping existing code)
    }

    public static void CreateResourceUI()
    {
        // Create resource display prefab first
        GameObject resourceDisplayPrefab = CreateResourceDisplayPrefab();

        // Create main resource panel
        GameObject resourcePanel = new GameObject("ResourcePanel");
        RectTransform panelRect = resourcePanel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(300, 400);

        // Add panel components
        Image panelImage = resourcePanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(resourcePanel.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Resources";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 24;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(0, 40);

        // Create resource container
        GameObject containerObj = new GameObject("ResourceContainer");
        containerObj.transform.SetParent(resourcePanel.transform);
        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0.1f);
        containerRect.anchorMax = new Vector2(1, 0.9f);
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;

        // Create gather progress bar
        GameObject progressObj = new GameObject("GatherProgress");
        progressObj.transform.SetParent(resourcePanel.transform);
        RectTransform progressRect = progressObj.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0, 0);
        progressRect.anchorMax = new Vector2(1, 0.1f);
        progressRect.sizeDelta = new Vector2(-20, -10);
        progressRect.anchoredPosition = Vector2.zero;

        Image progressBg = progressObj.AddComponent<Image>();
        progressBg.color = new Color(0.2f, 0.2f, 0.2f);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(progressObj.transform);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        GameObject progressTextObj = new GameObject("ProgressText");
        progressTextObj.transform.SetParent(progressObj.transform);
        Text progressText = progressTextObj.AddComponent<Text>();
        progressText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        progressText.fontSize = 14;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.color = Color.white;
        RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
        progressTextRect.anchorMin = Vector2.zero;
        progressTextRect.anchorMax = Vector2.one;
        progressTextRect.sizeDelta = Vector2.zero;

        // Add ResourceUI component
        ResourceUI resourceUI = resourcePanel.AddComponent<ResourceUI>();
        resourceUI.resourcePanel = resourcePanel;
        resourceUI.resourceDisplayPrefab = resourceDisplayPrefab;
        resourceUI.resourceContainer = containerRect;
        resourceUI.gatherProgressBar = fillImage;
        resourceUI.gatherText = progressText;

        // Save prefabs
        string panelPath = "Assets/Prefabs/UI/ResourcePanel.prefab";
        string displayPath = "Assets/Prefabs/UI/ResourceDisplay.prefab";
        
        // Ensure directory exists
        string directory = System.IO.Path.GetDirectoryName(panelPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Save prefabs
        PrefabUtility.SaveAsPrefabAsset(resourceDisplayPrefab, displayPath);
        PrefabUtility.SaveAsPrefabAsset(resourcePanel, panelPath);
        
        DestroyImmediate(resourceDisplayPrefab);
        DestroyImmediate(resourcePanel);

        Debug.Log("Resource UI prefabs created successfully!");
    }

    private static GameObject CreateResourceDisplayPrefab()
    {
        GameObject displayObj = new GameObject("ResourceDisplay");
        RectTransform displayRect = displayObj.AddComponent<RectTransform>();
        displayRect.sizeDelta = new Vector2(280, 40);

        // Background
        Image bgImage = displayObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Resource icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(displayObj.transform);
        Image iconImage = iconObj.AddComponent<Image>();
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.sizeDelta = new Vector2(30, 30);
        iconRect.anchoredPosition = new Vector2(20, 0);

        // Resource name and quantity
        GameObject textObj = new GameObject("QuantityText");
        textObj.transform.SetParent(displayObj.transform);
        Text quantityText = textObj.AddComponent<Text>();
        quantityText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        quantityText.fontSize = 16;
        quantityText.alignment = TextAnchor.MiddleLeft;
        quantityText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(0.7f, 1);
        textRect.offsetMin = new Vector2(60, 0);
        textRect.offsetMax = new Vector2(0, 0);

        // Progress bar
        GameObject progressObj = new GameObject("ProgressBar");
        progressObj.transform.SetParent(displayObj.transform);
        Image progressBg = progressObj.AddComponent<Image>();
        progressBg.color = new Color(0.2f, 0.2f, 0.2f);
        RectTransform progressRect = progressObj.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.7f, 0.2f);
        progressRect.anchorMax = new Vector2(0.95f, 0.8f);
        progressRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(progressObj.transform);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.white;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        return displayObj;
    }
}
#endif
