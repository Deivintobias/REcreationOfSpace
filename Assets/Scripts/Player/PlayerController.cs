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

        [Header("Farming Tools")]
        [SerializeField] private KeyCode plowKey = KeyCode.Q;
        [SerializeField] private KeyCode waterKey = KeyCode.R;
        [SerializeField] private KeyCode plantKey = KeyCode.F;
        [SerializeField] private KeyCode harvestKey = KeyCode.G;

        [Header("Menu Keys")]
        [SerializeField] private KeyCode characterMenuKey = KeyCode.C;
        [SerializeField] private KeyCode inventoryKey = KeyCode.I;
        [SerializeField] private KeyCode mapKey = KeyCode.M;
        [SerializeField] private KeyCode timelineKey = KeyCode.T;

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
            // We'll read moveAction directly in Update for continuous movement
        }

        private void OnDisable()
        {
            if (moveToPointAction != null) moveToPointAction.performed -= OnMoveToPointPerformed;
            if (attackAction != null) attackAction.performed -= OnAttackPerformed;
        }

        private void Update()
        {
            // Handle menu inputs first (still using old input for menus for now)
            HandleLegacyMenuInput();

            // Only process gameplay inputs if no menu is open
            if (!isMenuOpen)
            {
                HandleWASDMovement();
                HandleRotationWithMouse(); // Renamed for clarity
                HandleInteractions(); // Still uses old input for E, Q, R, F, G
                // Attack is handled by OnAttackPerformed
                UpdateAgentAndRigidbodyState();
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

        private void HandleLegacyMenuInput()
        {
            // Character menu
            if (Input.GetKeyDown(characterMenuKey))
            {
                ToggleCharacterMenu();
            }

            // Inventory menu (placeholder)
            if (Input.GetKeyDown(inventoryKey))
            {
                // TODO: Implement inventory menu
                Debug.Log("Inventory not implemented yet");
            }

            // Map menu (placeholder)
            if (Input.GetKeyDown(mapKey))
            {
                // TODO: Implement map menu
                Debug.Log("Map not implemented yet");
            }

            // Timeline menu
            if (Input.GetKeyDown(timelineKey))
            {
                if (timelineUI != null)
                {
                    timelineUI.Toggle();
                    SetMenuOpen(timelineUI.gameObject.activeSelf);
                }
            }
        }

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

        private void HandleInteractions()
        {
            // Check for interactable objects
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange))
            {
                // Handle farm plot interactions
                FarmPlot farmPlot = hit.collider.GetComponent<FarmPlot>();
                if (farmPlot != null)
                {
                    currentFarmPlot = farmPlot;
                    HandleFarmingInput();
                }
                else
                {
                    currentFarmPlot = null;
                }

                // Handle workbench interactions
                Workbench workbench = hit.collider.GetComponent<Workbench>();
                if (workbench != null)
                {
                    currentWorkbench = workbench;
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        workbench.OnInteractionRangeEntered(gameObject);
                        SetMenuOpen(true);
                    }
                }
                else if (currentWorkbench != null)
                {
                    currentWorkbench.OnInteractionRangeExited(gameObject);
                    currentWorkbench = null;
                }

                // Handle resource node interactions
                ResourceNode resourceNode = hit.collider.GetComponent<ResourceNode>();
                if (resourceNode != null)
                {
                    currentResourceNode = resourceNode;
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        resourceNode.Interact();
                    }
                }
                else
                {
                    currentResourceNode = null;
                }
            }
            else
            {
                currentFarmPlot = null;
                if (currentWorkbench != null)
                {
                    currentWorkbench.OnInteractionRangeExited(gameObject);
                    currentWorkbench = null;
                }
                currentResourceNode = null;
            }
        }

        private void HandleFarmingInput()
        {
            if (currentFarmPlot == null)
                return;

            if (Input.GetKeyDown(plowKey) && currentFarmPlot.CanPlow())
            {
                currentFarmPlot.Plow();
            }
            else if (Input.GetKeyDown(waterKey) && currentFarmPlot.CanWater())
            {
                currentFarmPlot.Water();
            }
            else if (Input.GetKeyDown(plantKey))
            {
                // For now, just try to plant a basic crop
                // You could add a crop selection UI later
                currentFarmPlot.Plant("Wheat");
            }
            else if (Input.GetKeyDown(harvestKey) && currentFarmPlot.CanHarvest())
            {
                currentFarmPlot.Harvest();
            }
        }

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
