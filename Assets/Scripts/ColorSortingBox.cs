using UnityEngine;

/// A box that accepts exactly one colour of ColorBlock.
/// Notifies its parent ColorSortingPuzzle on a correct or incorrect deposit.
public class ColorSortingBox : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("The colorId that this box accepts.")]
    public string acceptedColorId = "Red";

    [Header("State")]
    public bool isFilled = false;

    [Header("Visual Feedback")]
    [Tooltip("Renderer whose material emissive will change when filled.")]
    public Renderer lidRenderer;
    public Color solvedEmissionColor = Color.green;

    private ColorSortingPuzzle parentPuzzle;

    private void Awake()
    {
        parentPuzzle = GetComponentInParent<ColorSortingPuzzle>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFilled) return;

        ColorBlock block = other.GetComponent<ColorBlock>();
        if (block == null) return;

        if (block.colorId == acceptedColorId)
        {
            AcceptBlock(block);
        }
        else
        {
            RejectBlock(block);
        }
    }

    private void AcceptBlock(ColorBlock block)
    {
        isFilled = true;

        // Snap block into the box and freeze it
        block.Drop();
        Rigidbody blockRb = block.GetComponent<Rigidbody>();
        if (blockRb != null)
        {
            blockRb.linearVelocity = Vector3.zero;
            blockRb.angularVelocity = Vector3.zero;
            blockRb.isKinematic = true;
        }
        block.transform.position = transform.position;
        block.enabled = false;

        // Light up the lid
        if (lidRenderer != null)
        {
            lidRenderer.material.EnableKeyword("_EMISSION");
            lidRenderer.material.SetColor("_EmissionColor", solvedEmissionColor * 2f);
        }

        parentPuzzle?.OnBoxFilled(this);
    }

    private void RejectBlock(ColorBlock block)
    {
        // Bounce the block back out
        Rigidbody blockRb = block.GetComponent<Rigidbody>();
        if (blockRb != null)
            blockRb.AddForce(-block.transform.forward * 300f + Vector3.up * 200f);

        PuzzlePenaltyManager.Instance?.TriggerPenalty();
    }
}
