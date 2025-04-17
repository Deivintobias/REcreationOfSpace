using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 360f;
    
    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public LayerMask interactionMask;

    private CharacterController controller;
    private Camera mainCamera;
    private Vector3 moveDirection;
    private bool isInteracting;

    // Farming variables
    private bool isFarming = false;
    private float farmingProgress = 0f;
    private float farmingTime = 2f;

    // Homestead variables
    private bool isBuilding = false;
    private float buildProgress = 0f;
    private float buildTime = 3f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }
    }

    void Update()
    {
        HandleMovement();
        HandleInteraction();
        HandleFarming();
        HandleBuilding();
    }

    void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement direction relative to camera
        Vector3 forward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        
        moveDirection = (forward * vertical + right * horizontal).normalized;

        // Move character
        if (moveDirection.magnitude > 0.1f && !isInteracting)
        {
            // Rotate character to face movement direction
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                rotationSpeed * Time.deltaTime
            );

            // Apply movement
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // Apply gravity
        controller.Move(Vector3.down * 9.81f * Time.deltaTime);
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Check for interactable objects
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, interactionMask);
            
            foreach (Collider collider in colliders)
            {
                // Check for different interaction types
                if (collider.CompareTag("FarmPlot"))
                {
                    StartFarming(collider.gameObject);
                }
                else if (collider.CompareTag("Homestead"))
                {
                    StartBuilding(collider.gameObject);
                }
                else if (collider.CompareTag("FirstGuide"))
                {
                    InteractWithGuide(collider.gameObject);
                }
            }
        }
    }

    void HandleFarming()
    {
        if (isFarming)
        {
            farmingProgress += Time.deltaTime;
            
            if (farmingProgress >= farmingTime)
            {
                CompleteFarming();
            }

            // Cancel farming if moved
            if (moveDirection.magnitude > 0.1f)
            {
                CancelFarming();
            }
        }
    }

    void HandleBuilding()
    {
        if (isBuilding)
        {
            buildProgress += Time.deltaTime;
            
            if (buildProgress >= buildTime)
            {
                CompleteBuilding();
            }

            // Cancel building if moved
            if (moveDirection.magnitude > 0.1f)
            {
                CancelBuilding();
            }
        }
    }

    void StartFarming(GameObject farmPlot)
    {
        isFarming = true;
        isInteracting = true;
        farmingProgress = 0f;
        
        // Notify UI or show progress bar
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Tending to crops...");
        }
    }

    void CompleteFarming()
    {
        isFarming = false;
        isInteracting = false;
        
        // Grant neural network experience for farming
        var neuralNetwork = GetComponent<NeuralNetwork>();
        if (neuralNetwork != null)
        {
            neuralNetwork.GainExperience(5f); // Farming contributes to consciousness
        }

        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Farming complete!");
        }
    }

    void CancelFarming()
    {
        isFarming = false;
        isInteracting = false;
        farmingProgress = 0f;
    }

    void StartBuilding(GameObject homestead)
    {
        isBuilding = true;
        isInteracting = true;
        buildProgress = 0f;
        
        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Building homestead...");
        }
    }

    void CompleteBuilding()
    {
        isBuilding = false;
        isInteracting = false;
        
        // Grant neural network experience for building
        var neuralNetwork = GetComponent<NeuralNetwork>();
        if (neuralNetwork != null)
        {
            neuralNetwork.GainExperience(10f); // Building contributes more to consciousness
        }

        if (GuiderMessageUI.Instance != null)
        {
            GuiderMessageUI.Instance.ShowMessage("Construction complete!");
        }
    }

    void CancelBuilding()
    {
        isBuilding = false;
        isInteracting = false;
        buildProgress = 0f;
    }

    void InteractWithGuide(GameObject guide)
    {
        FirstGuide firstGuide = guide.GetComponent<FirstGuide>();
        if (firstGuide != null)
        {
            firstGuide.OnPlayerInteraction(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    // Called when player dies
    public void OnDeath()
    {
        // Find respawn manager
        RespawnManager respawnManager = FindObjectOfType<RespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.RespawnPlayer(gameObject);
        }
    }
}
