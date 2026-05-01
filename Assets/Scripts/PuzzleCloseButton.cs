using UnityEngine;
using UnityEngine.UI;

// Ensures this object has a Button component
[RequireComponent(typeof(Button))]
public class PuzzleCloseButton : MonoBehaviour
{
    private void Start()
    {
        // Get the Button component on this object
        Button button = GetComponent<Button>();

        // Add a listener so when the button is clicked, OnCloseClicked runs
        button.onClick.AddListener(OnCloseClicked);
    }

    // Called when the button is clicked
    private void OnCloseClicked()
    {
        // Find ALL puzzles in the scene
        PuzzleBase[] allPuzzles = FindObjectsByType<PuzzleBase>(FindObjectsSortMode.None);

        // Loop through them
        foreach (PuzzleBase puzzle in allPuzzles)
        {
            // Check if this puzzle UI is currently open
            if (puzzle.puzzleUI != null && puzzle.puzzleUI.activeSelf)
            {
                // Close the active puzzle
                puzzle.ClosePuzzleButton();

                Debug.Log($"Closing puzzle: {puzzle.puzzleName}");
                return; // Stop after closing one
            }
        }

        // If no active puzzle was found
        Debug.LogWarning("No active puzzle found to close!");
    }
}