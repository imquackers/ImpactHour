using UnityEngine;

/// Manages the colour block sorting puzzle.
/// Extends PuzzleBase so GameManager counts it — but this puzzle lives entirely
/// in world space (no UI panel). Interacting with the puzzle terminal gives a
/// brief hint; the player then picks up blocks and throws them into matching boxes.
public class ColorSortingPuzzle : PuzzleBase
{
    [Header("Sorting Puzzle")]
    [Tooltip("All ColorSortingBox children in this puzzle.")]
    public ColorSortingBox[] boxes;

    private int filledBoxes = 0;

    private void Start()
    {
        // Auto-discover boxes if not assigned
        if (boxes == null || boxes.Length == 0)
            boxes = GetComponentsInChildren<ColorSortingBox>();

        puzzleName = "Colour Sorting";
    }

    /// Called by Interact() on the terminal — show a brief hint via the prompt.
    protected override void OpenPuzzle()
    {
        // No UI panel — just unlock cursor briefly then let the player move freely.
        // The hint is shown via the interaction prompt text already.
    }

    public override string GetPromptText()
    {
        if (isSolved) return $"{puzzleName} — Completed";
        return $"[E] {puzzleName}: pick up coloured blocks and drop them into the matching boxes";
    }

    /// Called by each ColorSortingBox when a block is correctly deposited.
    public void OnBoxFilled(ColorSortingBox box)
    {
        filledBoxes++;

        if (filledBoxes >= boxes.Length)
            CompletePuzzle();
    }
}
