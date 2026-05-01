using UnityEngine;
using System;

// Base class for ALL puzzles
public abstract class PuzzleBase : MonoBehaviour, IInteractable
{
    // settings
    [Header("Puzzle Settings")]
    public string puzzleName = "Puzzle"; // Name shown to player
    public bool isSolved = false;        // Has this puzzle been completed?

    // UI
    [Header("UI")]
    public GameObject puzzleUI; // The UI panel for this puzzle

    // internal
    protected PlayerInteraction currentPlayer; // Player interacting with puzzle

    // Event triggered when puzzle is completed
    public event Action OnPuzzleCompleted;

    // Text shown when player looks at puzzle
    public virtual string GetPromptText()
    {
        if (isSolved)
        {
            return $"{puzzleName} - Completed";
        }

        return $"Press E to interact with {puzzleName}";
    }

    // Called when player presses interact key
    public virtual void Interact(PlayerInteraction player)
    {
        if (isSolved) return;

        currentPlayer = player;
        OpenPuzzle();
    }

    // Open Puzzle
    protected virtual void OpenPuzzle()
    {
        if (puzzleUI != null)
        {
            // Show puzzle UI
            puzzleUI.SetActive(true);

            // Unlock mouse so player can use UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable player controls while solving puzzle
            MouseLook mouseLook = FindFirstObjectByType<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = false;

            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = false;

            Debug.Log("Puzzle opened");
        }
    }

    // Close Puzzle
    protected virtual void ClosePuzzle()
    {
        if (puzzleUI != null)
        {
            // Hide UI
            puzzleUI.SetActive(false);

            // Lock mouse back to game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Re-enable player controls
            MouseLook mouseLook = FindFirstObjectByType<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = true;

            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = true;
        }
    }

    // Complete Puzzle
    protected virtual void CompletePuzzle()
    {
        isSolved = true;

        ClosePuzzle(); // Close UI

        // Notify GameManager
        GameManager.Instance.PuzzleSolved();

        // Trigger event (used by things like terminals, doors, etc.)
        OnPuzzleCompleted?.Invoke();

        Debug.Log($"{puzzleName} completed!");
    }

    // Called by UI button to close puzzle
    public void ClosePuzzleButton()
    {
        ClosePuzzle();
    }
}