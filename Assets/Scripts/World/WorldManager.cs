using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    [Header("World Layers")]
    public Transform sionLayer; // The surface world
    public Transform sinaiLayer; // The underground world
    
    [Header("Layer Settings")]
    public float layerTransitionHeight = 100f; // Height between layers
    public float transitionSpeed = 2f;

    public enum WorldLayer
    {
        Sion,  // Surface layer
        Sinai  // Underground layer
    }

    private WorldLayer currentLayer = WorldLayer.Sion;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Always start in Sion layer
        currentLayer = WorldLayer.Sion;
        
        // Initialize layer positions
        if (sinaiLayer != null)
        {
            // Position Sinai layer below Sion
            Vector3 sinaiPos = sinaiLayer.position;
            sinaiPos.y -= layerTransitionHeight;
            sinaiLayer.position = sinaiPos;
        }

        // Ensure Sion layer is at default height
        if (sionLayer != null)
        {
            Vector3 sionPos = sionLayer.position;
            sionPos.y = 0f;
            sionLayer.position = sionPos;
        }
    }

    public void EnsureSionLayer()
    {
        if (currentLayer != WorldLayer.Sion)
        {
            TransitionToLayer(WorldLayer.Sion);
        }
    }

    public void ResetLayerPositions()
    {
        if (sionLayer != null)
        {
            sionLayer.position = new Vector3(sionLayer.position.x, 0f, sionLayer.position.z);
        }
        
        if (sinaiLayer != null)
        {
            sinaiLayer.position = new Vector3(sinaiLayer.position.x, -layerTransitionHeight, sinaiLayer.position.z);
        }
    }

    public void TransitionToLayer(WorldLayer targetLayer)
    {
        if (isTransitioning || currentLayer == targetLayer)
            return;

        isTransitioning = true;
        float targetY = (targetLayer == WorldLayer.Sion) ? 0 : -layerTransitionHeight;

        // Start transition coroutine for smooth movement
        StartCoroutine(TransitionLayerCoroutine(targetY));
        currentLayer = targetLayer;
    }

    private System.Collections.IEnumerator TransitionLayerCoroutine(float targetY)
    {
        Transform activeLayer = (currentLayer == WorldLayer.Sion) ? sionLayer : sinaiLayer;
        Vector3 startPos = activeLayer.position;
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);
        float elapsedTime = 0f;

        while (elapsedTime < transitionSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionSpeed;
            
            // Use smooth step for more natural transition
            t = Mathf.SmoothStep(0, 1, t);
            
            activeLayer.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        activeLayer.position = targetPos;
        isTransitioning = false;
    }

    public WorldLayer GetCurrentLayer()
    {
        return currentLayer;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}
