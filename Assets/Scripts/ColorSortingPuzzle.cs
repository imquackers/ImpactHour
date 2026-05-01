using UnityEngine;

// Manages the colour block sorting puzzle.
// Extends PuzzleBase so GameManager counts it — but this puzzle is entirely in world space (no UI panel)

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

   
    protected override void OpenPuzzle()
    {
       
    }

    public override string GetPromptText()
    {
        if (isSolved) return $"{puzzleName} — Completed";
        return $"[E] {puzzleName}: pick up coloured blocks and drop them into the matching boxes";
    }

    // Called by each ColorSortingBox when a block is correctly deposited.
    public void OnBoxFilled(ColorSortingBox box)
    {
        filledBoxes++;

        if (filledBoxes >= boxes.Length)
            CompletePuzzle();
    }
}
