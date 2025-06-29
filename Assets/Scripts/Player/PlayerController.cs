using UnityEngine;
using UnityEngine.AI; // Added for NavMeshAgent
using UnityEngine.InputSystem; // Added for New Input System
using REcreationOfSpace.Farming;
using REcreationOfSpace.Crafting;
using REcreationOfSpace.UI;

namespace REcreationOfSpace.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 360f; // Speed for mouse rotation & WASD turn
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask groundLayerMask; // For click-to-move raycast
        [SerializeField] private GameObject moveIndicatorPrefab; // Prefab for click-to-move visual feedback

        // Obsolete KeyCode fields have been removed.

        // Component References
        private Rigidbody rb;
        private NavMeshAgent agent;
        private CombatController combat;
        private PlayerInput playerInput; // Added for New Input System
        private Camera mainCamera;

        // Input Actions
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction moveToPointAction;
        private InputAction primaryPointerPositionAction;
        private InputAction toggleCharacterMenuAction;
        private InputAction toggleInventoryMenuAction;
        private InputAction toggleMapMenuAction;
        private InputAction toggleTimelineMenuAction;
        private InputAction interactAlternateAction;
        private InputAction plowAction;
        private InputAction waterAction;
        private InputAction plantAction;
        private InputAction harvestAction;

        // State
        private FarmPlot currentFarmPlot;
        private Workbench currentWorkbench;
        private ResourceNode currentResourceNode;

        private CharacterMenu characterMenu;
        private GameMenu gameMenu;
        private TimelineUI timelineUI;
        private bool isMenuOpen = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
            combat = GetComponent<CombatController>();
            playerInput = GetComponent<PlayerInput>();
            mainCamera = Camera.main;

            if (agent != null)
            {
                agent.speed = moveSpeed;
                // agent.acceleration = moveSpeed * 2; // Example: tie acceleration to speed
                // Consider exposing NavMeshAgent's angularSpeed and stoppingDistance as well if needed
            }

            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on player. Please add it.");
                enabled = false;
                return;
            }

            // Initialize Actions
            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];
            attackAction = playerInput.actions["Attack"];
            moveToPointAction = playerInput.actions["MoveToPoint"];
            primaryPointerPositionAction = playerInput.actions["PrimaryPointerPosition"];

            // Initialize new menu and interaction actions
            toggleCharacterMenuAction = playerInput.actions["ToggleCharacterMenu"];
            toggleInventoryMenuAction = playerInput.actions["ToggleInventoryMenu"];
            toggleMapMenuAction = playerInput.actions["ToggleMapMenu"];
            toggleTimelineMenuAction = playerInput.actions["ToggleTimelineMenu"];
            interactAlternateAction = playerInput.actions["InteractAlternate"];
            plowAction = playerInput.actions["Plow"];
            waterAction = playerInput.actions["Water"];
            plantAction = playerInput.actions["Plant"];
            harvestAction = playerInput.actions["Harvest"];

            // Find menu references
            characterMenu = FindObjectOfType<CharacterMenu>();
            gameMenu = FindObjectOfType<GameMenu>();
            timelineUI = FindObjectOfType<TimelineUI>();

            // Lock cursor for combat
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            if (moveToPointAction != null) moveToPointAction.performed += OnMoveToPointPerformed;
            if (attackAction != null) attackAction.performed += OnAttackPerformed;

            // Subscribe to new actions
            if (toggleCharacterMenuAction != null) toggleCharacterMenuAction.performed += OnToggleCharacterMenuPerformed;
            if (toggleInventoryMenuAction != null) toggleInventoryMenuAction.performed += OnToggleInventoryMenuPerformed;
            if (toggleMapMenuAction != null) toggleMapMenuAction.performed += OnToggleMapMenuPerformed;
            if (toggleTimelineMenuAction != null) toggleTimelineMenuAction.performed += OnToggleTimelineMenuPerformed;
            if (interactAlternateAction != null) interactAlternateAction.performed += OnInteractAlternatePerformed;
            if (plowAction != null) plowAction.performed += OnPlowActionPerformed;
            if (waterAction != null) waterAction.performed += OnWaterActionPerformed;
            if (plantAction != null) plantAction.performed += OnPlantActionPerformed;
            if (harvestAction != null) harvestAction.performed += OnHarvestActionPerformed;
        }

        private void OnDisable()
        {
            if (moveToPointAction != null) moveToPointAction.performed -= OnMoveToPointPerformed;
            if (attackAction != null) attackAction.performed -= OnAttackPerformed;

            // Unsubscribe from new actions
            if (toggleCharacterMenuAction != null) toggleCharacterMenuAction.performed -= OnToggleCharacterMenuPerformed;
            if (toggleInventoryMenuAction != null) toggleInventoryMenuAction.performed -= OnToggleInventoryMenuPerformed;
            if (toggleMapMenuAction != null) toggleMapMenuAction.performed -= OnToggleMapMenuPerformed;
            if (toggleTimelineMenuAction != null) toggleTimelineMenuAction.performed -= OnToggleTimelineMenuPerformed;
            if (interactAlternateAction != null) interactAlternateAction.performed -= OnInteractAlternatePerformed;
            if (plowAction != null) plowAction.performed -= OnPlowActionPerformed;
            if (waterAction != null) waterAction.performed -= OnWaterActionPerformed;
            if (plantAction != null) plantAction.performed -= OnPlantActionPerformed;
            if (harvestAction != null) harvestAction.performed -= OnHarvestActionPerformed;
        }

        private void Update()
        {
            // Legacy input methods HandleLegacyMenuInput() and HandleInteractions() (which calls HandleFarmingInput())
            // will be removed or their contents gutted as logic moves to event handlers.
            // The raycasting logic from HandleInteractions for finding interactables will still be needed
            // perhaps called from OnInteractAlternatePerformed or periodically in Update to know what is in range.

            // Only process continuous gameplay updates if no menu is open
            if (!isMenuOpen)
            {
                HandleWASDMovement(); // Reads continuous input, so stays in Update
                HandleRotationWithMouse(); // Reads continuous input, so stays in Update
                UpdateAgentAndRigidbodyState();

                // Update current interactable context (needed for context-sensitive actions like farming)
                UpdateInteractableContext();
            }
        }

        private void UpdateAgentAndRigidbodyState()
        {
            if (agent == null || rb == null) return;

            bool isWASDInputActive = moveAction != null && moveAction.ReadValue<Vector2>().sqrMagnitude > 0.01f;

            // If agent has reached destination or has no path, and no WASD input is active
            if (!isWASDInputActive && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    // Reached destination or path is invalid/complete
                    if (rb.isKinematic) // If it was kinematic due to NavMeshAgent
                    {
                        rb.isKinematic = false;
                        agent.updatePosition = false;
                        agent.updateRotation = false;
                        // No need to explicitly call agent.isStopped = true here if ResetPath() was used or it completed.
                        // However, if it just reached destination, setting isStopped might be good.
                        if (!agent.isStopped) agent.isStopped = true;
                    }
                }
            }
            // If agent is supposed to be moving but Rigidbody is not kinematic, make it kinematic.
            // This can happen if WASD was pressed then released, and we want agent to resume a path that wasn't reset.
            // However, current logic resets path on WASD, so this case might be less relevant
            // unless we change WASD to only temporarily interrupt.
            // For now, the primary control flow is: click -> agent moves (kinematic=true). WASD -> rb moves (kinematic=false), agent stops.
        }

        // New Input System Event Handlers for Menus
        private void OnToggleCharacterMenuPerformed(InputAction.CallbackContext context)
        {
            if (context.performed) ToggleCharacterMenu();
        }

        private void OnToggleInventoryMenuPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // TODO: Implement inventory menu toggle logic here
                Debug.Log("Inventory toggle action performed (Not Implemented)");
                // Example: if (inventoryUI != null) { inventoryUI.Toggle(); SetMenuOpen(inventoryUI.isActive); }
            }
        }

        private void OnToggleMapMenuPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // TODO: Implement map menu toggle logic here
                Debug.Log("Map toggle action performed (Not Implemented)");
                // Example: if (mapUI != null) { mapUI.Toggle(); SetMenuOpen(mapUI.isActive); }
            }
        }

        private void OnToggleTimelineMenuPerformed(InputAction.CallbackContext context)
        {
            if (context.performed && timelineUI != null)
            {
                timelineUI.Toggle();
                SetMenuOpen(timelineUI.gameObject.activeSelf);
            }
        }

        // This method will be removed or its contents moved
        // private void HandleLegacyMenuInput()
        // {
        //     // Contents moved to individual OnToggle...Performed methods
        // }

        private void OnMoveToPointPerformed(InputAction.CallbackContext context)
        {
            if (isMenuOpen || mainCamera == null || agent == null) return;

            Vector2 screenPosition = primaryPointerPositionAction.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            // Use the groundLayerMask in the Raycast
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, groundLayerMask))
            {
                // Check if the hit point is on the NavMesh
                if (NavMesh.SamplePosition(hitInfo.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(navHit.position);

                    // Instantiate the move indicator
                    if (moveIndicatorPrefab != null)
                    {
                        // Instantiate slightly above the hit point to avoid z-fighting, adjust y-offset as needed
                        Instantiate(moveIndicatorPrefab, navHit.position + Vector3.up * 0.05f, Quaternion.identity);
                        // Consider Quaternion.LookRotation(navHit.normal) if indicator should align with ground normal
                        // or Quaternion.Euler(90,0,0) for a decal-like sprite that's setup to be flat.
                    }
                }
                else
                {
                    // Optional: Provide feedback if clicked point is not on NavMesh, e.g., a sound or visual cue
                    Debug.Log("Clicked point is not on a NavMesh. Cannot move there.");
                    return; // Do not proceed if not on NavMesh
                }

                if (agent.isStopped) agent.isStopped = false;
                rb.isKinematic = true; // Let NavMeshAgent control movement
                agent.updatePosition = true;
                agent.updateRotation = true; // Let NavMeshAgent also control rotation towards path
            }
        }

        private void HandleWASDMovement()
        {
            if (moveAction == null) return;

            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0.01f) // If there's significant WASD input
            {
                if (agent != null && (agent.hasPath || !agent.isStopped))
                {
                    agent.ResetPath();
                    agent.isStopped = true; // Stop NavMeshAgent movement
                    rb.isKinematic = false; // Give Rigidbody control back
                    agent.updatePosition = false;
                    agent.updateRotation = false;
                }

                Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
                rb.MovePosition(transform.position + movement * moveSpeed * Time.deltaTime);

                // Optional: Rotate player to face movement direction during WASD
                if (movement != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(movement);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else // No WASD input
            {
                // If agent is not pathfinding, ensure Rigidbody is not kinematic (unless some other system makes it so)
                if (agent == null || (!agent.hasPath && agent.isStopped))
                {
                     rb.isKinematic = false; // Or handle based on other states
                }
            }
             // If NavMeshAgent is moving the character, it should handle rotation.
            if (agent != null && agent.hasPath && !agent.isStopped)
            {
                // NavMeshAgent handles rotation if updateRotation is true.
            }
        }

        private void HandleRotationWithMouse()
        {
            // Only apply mouse rotation if not actively pathfinding with NavMeshAgent
            // or if specific game design allows it (e.g. strafing while pathfinding)
            if (agent != null && agent.hasPath && !agent.isStopped && agent.updateRotation)
            {
                return; // NavMeshAgent is handling rotation
            }

            if (lookAction == null) return;
            float mouseX = lookAction.ReadValue<Vector2>().x; // Assuming Look action is Vector2 for delta
            transform.Rotate(Vector3.up * mouseX * rotationSpeed * Time.deltaTime);
        }

        private void UpdateInteractableContext()
        {
            // Raycast to find interactable objects in front of the player
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward); // Or from player's eyes/center
            RaycastHit hit;

            // Reset current interactables
            FarmPlot detectedFarmPlot = null;
            Workbench detectedWorkbench = null;
            ResourceNode detectedResourceNode = null;

            if (Physics.Raycast(ray, out hit, interactionRange)) // Consider a layer mask for interactables
            {
                detectedFarmPlot = hit.collider.GetComponent<FarmPlot>();
                detectedWorkbench = hit.collider.GetComponent<Workbench>();
                detectedResourceNode = hit.collider.GetComponent<ResourceNode>();
            }

            // Update FarmPlot context
            if (currentFarmPlot != detectedFarmPlot)
            {
                currentFarmPlot = detectedFarmPlot;
                // Potentially show/hide UI prompts for farm plot
            }

            // Update Workbench context
            if (currentWorkbench != detectedWorkbench)
            {
                if (currentWorkbench != null)
                {
                    // currentWorkbench.OnInteractionRangeExited(gameObject); // This might still be needed if UI closes on exit
                }
                currentWorkbench = detectedWorkbench;
                if (currentWorkbench != null)
                {
                    // currentWorkbench.OnInteractionRangeEntered(gameObject); // This might be too aggressive, E key will handle open
                }
                // Potentially show/hide UI prompts for workbench
            }

            // Update ResourceNode context
            if (currentResourceNode != detectedResourceNode)
            {
                currentResourceNode = detectedResourceNode;
                // Potentially show/hide UI prompts for resource node
            }
        }

        private void OnInteractAlternatePerformed(InputAction.CallbackContext context)
        {
            if (!context.performed || isMenuOpen) return;

            // Prioritize interactions: Workbench > ResourceNode (or define your own priority)
            if (currentWorkbench != null)
            {
                currentWorkbench.OnInteractionRangeEntered(gameObject); // Assuming this opens the UI
                SetMenuOpen(true); // Assuming workbench interaction opens a menu
                return;
            }

            if (currentResourceNode != null)
            {
                currentResourceNode.Interact();
                return;
            }
            // Add other generic interactions here if any
        }

        private void OnPlowActionPerformed(InputAction.CallbackContext context)
        {
            if (!context.performed || isMenuOpen || currentFarmPlot == null) return;
            if (currentFarmPlot.CanPlow()) currentFarmPlot.Plow();
        }

        private void OnWaterActionPerformed(InputAction.CallbackContext context)
        {
            if (!context.performed || isMenuOpen || currentFarmPlot == null) return;
            if (currentFarmPlot.CanWater()) currentFarmPlot.Water();
        }

        private void OnPlantActionPerformed(InputAction.CallbackContext context)
        {
            if (!context.performed || isMenuOpen || currentFarmPlot == null) return;
            // For now, just try to plant a basic crop
            // You could add a crop selection UI later
            currentFarmPlot.Plant("Wheat");
        }

        private void OnHarvestActionPerformed(InputAction.CallbackContext context)
        {
            if (!context.performed || isMenuOpen || currentFarmPlot == null) return;
            if (currentFarmPlot.CanHarvest()) currentFarmPlot.Harvest();
        }

        // Legacy methods HandleInteractions() and HandleFarmingInput() are now removed.
        // Their logic has been integrated into UpdateInteractableContext() and the
        // OnInteractAlternatePerformed, OnPlowActionPerformed, etc., methods.

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (isMenuOpen || combat == null) return;
            combat.Attack();
        }

        private void ToggleCharacterMenu()
        {
            if (characterMenu != null)
            {
                if (characterMenu.gameObject.activeSelf)
                {
                    characterMenu.Hide();
                    SetMenuOpen(false);
                }
                else
                {
                    characterMenu.Show();
                    SetMenuOpen(true);
                }
            }
        }

        public void SetMenuOpen(bool open)
        {
            isMenuOpen = open;
            ToggleCursor(open);
        }

        public void ToggleCursor(bool show)
        {
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = show;
        }

        public bool IsMenuOpen()
        {
            return isMenuOpen;
        }

        private void OnDrawGizmos()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        public FarmPlot GetCurrentFarmPlot()
        {
            return currentFarmPlot;
        }

        public Workbench GetCurrentWorkbench()
        {
            return currentWorkbench;
        }

        public ResourceNode GetCurrentResourceNode()
        {
            return currentResourceNode;
        }
    }
}
