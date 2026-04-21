using UnityEngine;
using UnityEngine.InputSystem;

/// A coloured block the player can pick up and throw into a matching ColorSortingBox.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ColorBlock : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    public string colorId = "Red";

    [Header("Glow")]
    [Tooltip("Emission colour applied to this block's material instance at startup.")]
    public Color glowColor = Color.red;
    [Tooltip("HDR intensity multiplier for the emission (higher = brighter glow).")]
    public float glowIntensity = 0.5f;

    [Header("Hold Settings")]
    [Tooltip("Distance in front of the camera to hold the block.")]
    public float holdDistance = 0.15f;
    [Tooltip("How smoothly the held block follows the camera.")]
    public float holdSmoothing = 15f;

    [Header("Throw Settings")]
    [Tooltip("Force applied when the player throws the block.")]
    public float throwForce = 15f;

    private Rigidbody rb;
    private Collider col;
    private bool isHeld = false;
    private PlayerInteraction holder;
    private Camera playerCamera;
    private int originalLayer;

    private static readonly int IgnoreRaycastLayer = 2;
    private static readonly int EmissionColorID    = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalLayer = gameObject.layer;

        ApplyGlow();
    }

    /// Creates a material instance and enables emission with the chosen glow colour.
    private void ApplyGlow()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null) return;

        // Instance the material so only this block is affected
        Material mat = r.material;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor(EmissionColorID, glowColor * glowIntensity);
    }

    private void FixedUpdate()
    {
        if (!isHeld) return;

        // Smoothly track the hold position each physics step
        Vector3 targetPosition = playerCamera.transform.position +
                                 playerCamera.transform.forward * holdDistance;
        Vector3 delta = targetPosition - rb.position;
        rb.linearVelocity = delta * holdSmoothing;
        rb.angularVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (!isHeld) return;

        bool throwInput = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (throwInput) Throw();
    }

    // ── IInteractable ──────────────────────────────────────────────────────────

    public string GetPromptText() => isHeld
        ? "[LMB] Throw  |  [E] Drop"
        : $"[E] Pick up {colorId} block";

    public void Interact(PlayerInteraction player)
    {
        if (isHeld) { Drop(); return; }
        PickUp(player);
    }

    // ── Pick up / Drop / Throw ─────────────────────────────────────────────────

    /// Attaches the block to the player camera.
    public void PickUp(PlayerInteraction player)
    {
        if (isHeld) return;

        isHeld = true;
        holder = player;
        playerCamera = player.GetComponentInChildren<Camera>();

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Disable collider so the held block cannot push the player or anything else
        col.enabled = false;

        // Teleport directly to hold position — no velocity sweep through the player
        rb.position = playerCamera.transform.position +
                      playerCamera.transform.forward * holdDistance;

        // Ignore-raycast layer so the block doesn't block the interaction ray
        gameObject.layer = IgnoreRaycastLayer;
    }

    /// Releases the block in place with no velocity.
    public void Drop()
    {
        if (!isHeld) return;

        isHeld = false;
        holder = null;
        playerCamera = null;

        col.enabled = true;
        rb.useGravity = true;
        gameObject.layer = originalLayer;
    }

    /// Releases the block and launches it forward from the camera.
    public void Throw()
    {
        if (!isHeld) return;

        Vector3 direction = playerCamera.transform.forward;

        // Release state without going through Drop() — keeps collider disabled
        // until the block has traveled clear of the player capsule.
        isHeld = false;
        holder = null;
        playerCamera = null;
        rb.useGravity = true;
        gameObject.layer = originalLayer;

        rb.AddForce(direction * throwForce, ForceMode.Impulse);

        // Re-enable collider after a short delay so it never overlaps the player
        StartCoroutine(ReenableColliderAfterThrow());
    }

    private System.Collections.IEnumerator ReenableColliderAfterThrow()
    {
        // Two fixed-update steps is enough for the block to travel away
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        col.enabled = true;
    }
}
