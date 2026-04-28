using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    // inspector settings
    [Header("Interaction Settings")]
    public float interactionRange = 3f; // How far the player can interact
    public LayerMask interactableLayer; // Only objects on this layer can be interacted with
    public KeyCode interactKey = KeyCode.E; // Key used to interact

    [Header("UI References")]
    public TMPro.TextMeshProUGUI interactionPrompt; // UI text that shows interaction message

    // internel variables
    private Camera playerCamera; // Reference to player's camera
    private IInteractable currentInteractable; // The object player is currently looking at

    private void Start()
    {
        // Get the camera attached to the player
        playerCamera = GetComponentInChildren<Camera>();

        // Hide interaction text at the start
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
       
        CheckForInteractable(); // Check if player is looking at something interactable
        HandleInteraction();   // Check if player presses interact key
    }

    private void CheckForInteractable()
    {
        // Create a ray from the camera going forward
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Shoot the ray and check if it hits something in range and on correct layer
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            // Try to get an interactable component from what we hit
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // If valid interactable found, set it as current
                SetCurrentInteractable(interactable);
                return;
            }
        }

        // If nothing valid hit, clear interactable
        SetCurrentInteractable(null);
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        // Only update if it's a new object (prevents unnecessary UI updates)
        if (currentInteractable != interactable)
        {
            currentInteractable = interactable;

            if (interactionPrompt != null)
            {
                if (currentInteractable != null)
                {
                    // Show prompt text from the interactable object
                    interactionPrompt.text = currentInteractable.GetPromptText();
                    interactionPrompt.gameObject.SetActive(true);
                }
                else
                {
                    // Hide prompt if nothing interactable
                    interactionPrompt.gameObject.SetActive(false);
                }
            }
        }
    }

    private void HandleInteraction()
    {
        // If looking at something AND player presses interact key
        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            // Call the interact function on that object
            currentInteractable.Interact(this);
        }
    }
}