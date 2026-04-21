using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI interactionPrompt;

    private Camera playerCamera;
    private IInteractable currentInteractable;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckForInteractable();
        HandleInteraction();
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                SetCurrentInteractable(interactable);
                return;
            }
        }

        SetCurrentInteractable(null);
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        if (currentInteractable != interactable)
        {
            currentInteractable = interactable;

            if (interactionPrompt != null)
            {
                if (currentInteractable != null)
                {
                    interactionPrompt.text = currentInteractable.GetPromptText();
                    interactionPrompt.gameObject.SetActive(true);
                }
                else
                {
                    interactionPrompt.gameObject.SetActive(false);
                }
            }
        }
    }

    private void HandleInteraction()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact(this);
        }
    }
}
