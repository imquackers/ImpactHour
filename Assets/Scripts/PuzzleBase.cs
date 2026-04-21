using UnityEngine;
using System;

public abstract class PuzzleBase : MonoBehaviour, IInteractable
{
    [Header("Puzzle Settings")]
    public string puzzleName = "Puzzle";
    public bool isSolved = false;

    [Header("UI")]
    public GameObject puzzleUI;

    protected PlayerInteraction currentPlayer;
    
    public event Action OnPuzzleCompleted;

    public virtual string GetPromptText()
    {
        if (isSolved)
        {
            return $"{puzzleName} - Completed";
        }
        return $"Press E to interact with {puzzleName}";
    }

    public virtual void Interact(PlayerInteraction player)
    {
        if (isSolved) return;

        currentPlayer = player;
        OpenPuzzle();
    }

    protected virtual void OpenPuzzle()
    {
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Disable camera look and movement so mouse/keys don't affect player during puzzle
            MouseLook mouseLook = FindFirstObjectByType<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = false;
            
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = false;
            
            Debug.Log($"Puzzle opened. Cursor unlocked and visible.");
        }
    }

    protected virtual void ClosePuzzle()
    {
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Re-enable camera look and movement when puzzle closes
            MouseLook mouseLook = FindFirstObjectByType<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = true;
            
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = true;
        }
    }

    protected virtual void CompletePuzzle()
    {
        isSolved = true;
        ClosePuzzle();
        GameManager.Instance.PuzzleSolved();
        OnPuzzleCompleted?.Invoke();
        Debug.Log($"{puzzleName} completed!");
    }

    public void ClosePuzzleButton()
    {
        ClosePuzzle();
    }
}
