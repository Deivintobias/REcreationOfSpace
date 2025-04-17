using UnityEngine;
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
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private float interactionRange = 2f;

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

        private Rigidbody rb;
        private CombatController combat;
        private FarmPlot currentFarmPlot;
        private Workbench currentWorkbench;
        private ResourceNode currentResourceNode;

        private CharacterMenu characterMenu;
        private GameMenu gameMenu;
        private TimelineUI timelineUI;
        private bool isMenuOpen = false;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            combat = GetComponent<CombatController>();

            // Find menu references
            characterMenu = FindObjectOfType<CharacterMenu>();
            gameMenu = FindObjectOfType<GameMenu>();
            timelineUI = FindObjectOfType<TimelineUI>();

            // Lock cursor for combat
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Handle menu inputs first
            HandleMenuInput();

            // Only process gameplay inputs if no menu is open
            if (!isMenuOpen)
            {
                HandleMovement();
                HandleRotation();
                HandleInteractions();
                HandleCombat();
            }
        }

        private void HandleMenuInput()
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

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
            rb.MovePosition(transform.position + movement * moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            float mouseX = Input.GetAxis("Mouse X");
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

        private void HandleCombat()
        {
            if (combat != null && Input.GetMouseButtonDown(0))
            {
                combat.Attack();
            }
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
