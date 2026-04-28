using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryPuzzle : PuzzleBase
{
    // inspector settings
    [Header("Memory Puzzle Settings")]
    public int sequenceLength = 5;     // How many buttons in the sequence
    public float flashDuration = 0.5f; // How long each button lights up
    public float flashDelay = 0.3f;    // Delay between flashes

    [Header("Buttons")]
    public List<Button> colorButtons = new List<Button>(); // Buttons player presses
    public List<Color> buttonColors = new List<Color>();   // Colors for each button

    // internal variables
    private List<int> sequence = new List<int>();      // Correct sequence (indexes)
    private List<int> playerSequence = new List<int>(); // Player input
    private bool isShowingSequence = false; // Are we showing the pattern?
    private bool isPlayerTurn = false;      // Can player click yet?

    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>(); // Store original colors

    private bool buttonsInitialised = false; // Prevent duplicate setup

    private void Start()
    {
        // If no colors assigned, create default colors
        if (buttonColors.Count == 0)
        {
            buttonColors = new List<Color>
            {
                Color.red,
                Color.blue,
                Color.yellow,
                Color.green,
                Color.magenta,
                Color.cyan,
                new Color(1f, 0.5f, 0f),
                new Color(0.5f, 0f, 0.5f),
                new Color(1f, 0.75f, 0.8f),
                new Color(0.5f, 0.25f, 0f)
            };
        }

        SetupButtons(); // Prepare buttons
    }

    private void SetupButtons()
    {
        // Prevent running twice (important if multiple puzzles exist)
        if (buttonsInitialised) return;
        buttonsInitialised = true;

        for (int i = 0; i < colorButtons.Count && i < buttonColors.Count; i++)
        {
            Button button = colorButtons[i];

            if (button == null)
            {
                Debug.LogWarning($"[MemoryPuzzle] '{puzzleName}': button {i} is null");
                continue;
            }

            Color color = buttonColors[i];

            // Set button color
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = color;
                originalColors[button] = color; // Store original color
            }

            // Remove old listeners (prevents duplicates)
            button.onClick.RemoveAllListeners();

            int index = i;
            button.onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    // Called when puzzle opens
    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        StartNewSequence(); // Start game
    }

    // Generate a new random sequence
    private void StartNewSequence()
    {
        sequence.Clear();
        playerSequence.Clear();

        for (int i = 0; i < sequenceLength; i++)
        {
            sequence.Add(Random.Range(0, colorButtons.Count));
        }

        Debug.Log($"Sequence: {string.Join(", ", sequence)}");

        StartCoroutine(ShowSequence());
    }

    // Show the sequence to the player
    private IEnumerator ShowSequence()
    {
        isShowingSequence = true;
        isPlayerTurn = false;

        SetButtonsInteractable(false); // Disable clicking

        yield return new WaitForSeconds(1f);

        // Flash each button in order
        foreach (int index in sequence)
        {
            yield return FlashButton(index);
            yield return new WaitForSeconds(flashDelay);
        }

        // Now it's the player's turn
        isShowingSequence = false;
        isPlayerTurn = true;

        SetButtonsInteractable(true);

        Debug.Log("Your turn!");
    }

    // Flash a button (turn white then back)
    private IEnumerator FlashButton(int index)
    {
        Button button = colorButtons[index];
        Image buttonImage = button.GetComponent<Image>();

        if (buttonImage != null)
        {
            Color original = originalColors[button];

            buttonImage.color = Color.white; // Highlight

            yield return new WaitForSeconds(flashDuration);

            buttonImage.color = original; // Restore
        }
    }

    // Called when player clicks a button
    private void OnButtonClicked(int index)
    {
        if (isSolved) return;
        if (!isPlayerTurn || isShowingSequence) return;

        playerSequence.Add(index); // Save input

        StartCoroutine(FlashButton(index)); // Visual feedback

        // If player finished entering sequence
        if (playerSequence.Count == sequence.Count)
        {
            StartCoroutine(CheckSequence());
        }
    }

    // Check if player's sequence is correct
    private IEnumerator CheckSequence()
    {
        yield return new WaitForSeconds(0.5f);

        bool correct = true;

        for (int i = 0; i < sequence.Count; i++)
        {
            if (playerSequence[i] != sequence[i])
            {
                correct = false;
                break;
            }
        }

        if (correct)
        {
            Debug.Log("Correct!");
            CompletePuzzle(); // Puzzle solved
        }
        else
        {
            Debug.Log("Wrong!");
            playerSequence.Clear();

            // Trigger penalty if system exists
            if (PuzzlePenaltyManager.Instance != null)
            {
                PuzzlePenaltyManager.Instance.TriggerPenalty();
            }

            yield return new WaitForSeconds(1f);

            // Show sequence again
            StartCoroutine(ShowSequence());
        }
    }

    // Enable/disable all buttons
    private void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in colorButtons)
        {
            button.interactable = interactable;
        }
    }

    // Reset when puzzle closes
    protected override void ClosePuzzle()
    {
        base.ClosePuzzle();

        StopAllCoroutines(); // Stop flashing/sequence
        playerSequence.Clear();

        isPlayerTurn = false;
        isShowingSequence = false;
    }
}