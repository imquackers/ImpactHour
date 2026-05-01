using UnityEngine;
using System.Collections;

// Requires a MeshRenderer on the object
[RequireComponent(typeof(MeshRenderer))]
public class PuzzleTerminalScreen : MonoBehaviour, IInteractable
{
    // materials
    [Header("Materials")]
    public Material incompleteMaterial; // Material before puzzle is solved
    public Material completeMaterial;   // Material after puzzle is solved

    // flash settings
    [Header("Flash Settings")]
    [Tooltip("Base colour when active")]
    public Color redColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    [Tooltip("How fast it flashes")]
    public float flashFrequency = 1.5f;

    [Tooltip("Glow strength")]
    public float glowIntensity = 2f;

    // Refs
    [Header("References")]
    public PuzzleBase puzzle; // Linked puzzle
    public string promptText = "Press E to access terminal";

    // internal Variables
    private MeshRenderer meshRenderer;
    private Material instanceMaterial; // Instance (so original isnt modified)
    private bool isComplete = false;

    // Shader property IDs (faster than using strings)
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Create a copy of the material so we can modify it safely
        if (incompleteMaterial != null)
        {
            instanceMaterial = new Material(incompleteMaterial);
            meshRenderer.material = instanceMaterial;
        }

        // Listen for puzzle completion
        if (puzzle != null)
            puzzle.OnPuzzleCompleted += OnPuzzleCompleted;

        // Start flashing effect
        StartCoroutine(FlashEffect());
    }

    private void OnDestroy()
    {
        // Remove event listener
        if (puzzle != null)
            puzzle.OnPuzzleCompleted -= OnPuzzleCompleted;

        // Clean up material instance
        if (instanceMaterial != null)
            Destroy(instanceMaterial);
    }

    // Text shown to player when looking at terminal
    public string GetPromptText() => isComplete ? "" : promptText;

    // Called when player presses interact key
    public void Interact(PlayerInteraction player)
    {
        if (!isComplete && puzzle != null)
            puzzle.Interact(player);
    }

    // Called when puzzle is completed
    private void OnPuzzleCompleted()
    {
        isComplete = true;

        // Stop flashing
        StopAllCoroutines();

        // Swap to completed material
        if (completeMaterial != null && meshRenderer != null)
        {
            if (instanceMaterial != null)
                Destroy(instanceMaterial);

            meshRenderer.material = completeMaterial;
        }
    }

    // flashing effect
    private IEnumerator FlashEffect()
    {
        if (instanceMaterial == null) yield break;

        // Enable emission 
        instanceMaterial.EnableKeyword("_EMISSION");

        while (!isComplete)
        {
            // Half cycle time (red to black to red)
            float halfPeriod = 0.5f / flashFrequency;

            // RED 
            SetScreenColor(redColor, redColor * glowIntensity);
            yield return new WaitForSeconds(halfPeriod);

            if (isComplete) break;

            // BLACK
            SetScreenColor(Color.black, Color.black);
            yield return new WaitForSeconds(halfPeriod);
        }
    }

    // Sets the material color + glow
    private void SetScreenColor(Color baseCol, Color emissionCol)
    {
        if (instanceMaterial == null) return;

        // Change main color
        if (instanceMaterial.HasProperty(BaseColorID))
            instanceMaterial.SetColor(BaseColorID, baseCol);

        // Change emission (glow)
        if (instanceMaterial.HasProperty(EmissionColorID))
            instanceMaterial.SetColor(EmissionColorID, emissionCol);
    }
}