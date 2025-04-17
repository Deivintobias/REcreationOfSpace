using UnityEngine;

public class MaterialCreator : MonoBehaviour
{
    public static void CreateBasicMaterials()
    {
        // Create Sion character material
        Material sionMaterial = new Material(Shader.Find("Standard"));
        sionMaterial.color = new Color(0.8f, 0.8f, 0.9f); // Light blue-grey
        sionMaterial.EnableKeyword("_EMISSION");
        sionMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 1f, 0.5f));
        AssetDatabase.CreateAsset(sionMaterial, "Assets/Materials/SionCharacter.mat");

        // Create Sinai character material
        Material sinaiMaterial = new Material(Shader.Find("Standard"));
        sinaiMaterial.color = new Color(0.9f, 0.8f, 0.7f); // Warm beige
        sinaiMaterial.EnableKeyword("_EMISSION");
        sinaiMaterial.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.5f, 0.5f));
        AssetDatabase.CreateAsset(sinaiMaterial, "Assets/Materials/SinaiCharacter.mat");

        // Create ground material
        Material groundMaterial = new Material(Shader.Find("Standard"));
        groundMaterial.color = new Color(0.5f, 0.5f, 0.5f); // Grey
        AssetDatabase.CreateAsset(groundMaterial, "Assets/Materials/Ground.mat");

        // Create mountain material
        Material mountainMaterial = new Material(Shader.Find("Standard"));
        mountainMaterial.color = new Color(0.7f, 0.7f, 0.8f); // Light grey with blue tint
        AssetDatabase.CreateAsset(mountainMaterial, "Assets/Materials/Mountain.mat");

        // Create glow material
        Material glowMaterial = new Material(Shader.Find("Standard"));
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetColor("_EmissionColor", Color.white);
        AssetDatabase.CreateAsset(glowMaterial, "Assets/Materials/Glow.mat");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
