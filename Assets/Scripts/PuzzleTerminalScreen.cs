using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class PuzzleTerminalScreen : MonoBehaviour, IInteractable
{
    [Header("Materials")]
    public Material incompleteMaterial;
    public Material completeMaterial;

    [Header("Flash Settings")]
    [Tooltip("Base colour of the screen when lit (incomplete state).")]
    public Color redColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    [Tooltip("How many full red→black→red cycles per second.")]
    public float flashFrequency = 1.5f;
    [Tooltip("Emission intensity multiplied onto the red colour while lit.")]
    public float glowIntensity = 2f;

    [Header("References")]
    public PuzzleBase puzzle;
    public string promptText = "Press E to access terminal";

    private MeshRenderer meshRenderer;
    private Material instanceMaterial;
    private bool isComplete = false;

    private static readonly int BaseColorID    = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (incompleteMaterial != null)
        {
            instanceMaterial = new Material(incompleteMaterial);
            meshRenderer.material = instanceMaterial;
        }

        if (puzzle != null)
            puzzle.OnPuzzleCompleted += OnPuzzleCompleted;

        StartCoroutine(FlashEffect());
    }

    private void OnDestroy()
    {
        if (puzzle != null)
            puzzle.OnPuzzleCompleted -= OnPuzzleCompleted;

        if (instanceMaterial != null)
            Destroy(instanceMaterial);
    }

    public string GetPromptText() => isComplete ? "" : promptText;

    public void Interact(PlayerInteraction player)
    {
        if (!isComplete && puzzle != null)
            puzzle.Interact(player);
    }

    private void OnPuzzleCompleted()
    {
        isComplete = true;
        StopAllCoroutines();

        if (completeMaterial != null && meshRenderer != null)
        {
            if (instanceMaterial != null)
                Destroy(instanceMaterial);

            meshRenderer.material = completeMaterial;
        }
    }

    /// Alternates _BaseColor and _EmissionColor between red and black at flashFrequency hz.
    private IEnumerator FlashEffect()
    {
        if (instanceMaterial == null) yield break;

        instanceMaterial.EnableKeyword("_EMISSION");

        while (!isComplete)
        {
            // Each cycle: red half then black half
            float halfPeriod = 0.5f / flashFrequency;

            // Red phase
            SetScreenColor(redColor, redColor * glowIntensity);
            yield return new WaitForSeconds(halfPeriod);

            if (isComplete) break;

            // Black phase
            SetScreenColor(Color.black, Color.black);
            yield return new WaitForSeconds(halfPeriod);
        }
    }

    private void SetScreenColor(Color baseCol, Color emissionCol)
    {
        if (instanceMaterial == null) return;

        if (instanceMaterial.HasProperty(BaseColorID))
            instanceMaterial.SetColor(BaseColorID, baseCol);

        if (instanceMaterial.HasProperty(EmissionColorID))
            instanceMaterial.SetColor(EmissionColorID, emissionCol);
    }
}
