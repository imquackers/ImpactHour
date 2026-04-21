using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryPuzzle : PuzzleBase
{
    [Header("Memory Puzzle Settings")]
    public int sequenceLength = 5;
    public float flashDuration = 0.5f;
    public float flashDelay = 0.3f;

    [Header("Buttons")]
    public List<Button> colorButtons = new List<Button>();
    public List<Color> buttonColors = new List<Color>();

    private List<int> sequence = new List<int>();
    private List<int> playerSequence = new List<int>();
    private bool isShowingSequence = false;
    private bool isPlayerTurn = false;
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

    private bool buttonsInitialised = false;

    private void Start()
    {
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

        SetupButtons();
    }

    private void SetupButtons()
    {
        // Guard against duplicate listener registration if called more than once.
        if (buttonsInitialised) return;
        buttonsInitialised = true;

        for (int i = 0; i < colorButtons.Count && i < buttonColors.Count; i++)
        {
            Button button = colorButtons[i];
            if (button == null)
            {
                Debug.LogWarning($"[MemoryPuzzle] '{puzzleName}': colorButtons[{i}] is null — skipping.", this);
                continue;
            }

            Color color = buttonColors[i];

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = color;
                originalColors[button] = color;
            }

            // Remove any pre-existing listeners on this button before adding ours,
            // so a second MemoryPuzzle instance cannot stack its own listener on a
            // button that already belongs to another instance.
            button.onClick.RemoveAllListeners();

            int index = i;
            button.onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        StartNewSequence();
    }

    private void StartNewSequence()
    {
        sequence.Clear();
        playerSequence.Clear();
        
        for (int i = 0; i < sequenceLength; i++)
        {
            sequence.Add(Random.Range(0, colorButtons.Count));
        }

        Debug.Log($"New sequence: {string.Join(", ", sequence)}");
        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        isShowingSequence = true;
        isPlayerTurn = false;
        SetButtonsInteractable(false);

        yield return new WaitForSeconds(1f);

        foreach (int index in sequence)
        {
            yield return FlashButton(index);
            yield return new WaitForSeconds(flashDelay);
        }

        isShowingSequence = false;
        isPlayerTurn = true;
        SetButtonsInteractable(true);
        Debug.Log("Your turn! Click the buttons in order.");
    }

    private IEnumerator FlashButton(int index)
    {
        Button button = colorButtons[index];
        Image buttonImage = button.GetComponent<Image>();

        if (buttonImage != null)
        {
            Color original = originalColors[button];
            buttonImage.color = Color.white;
            
            yield return new WaitForSeconds(flashDuration);
            
            buttonImage.color = original;
        }
    }

    private void OnButtonClicked(int index)
    {
        if (isSolved) return;
        if (!isPlayerTurn || isShowingSequence) return;

        Debug.Log($"Player clicked button {index}");
        playerSequence.Add(index);

        StartCoroutine(FlashButton(index));

        if (playerSequence.Count == sequence.Count)
        {
            StartCoroutine(CheckSequence());
        }
    }

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
            Debug.Log("Correct! Puzzle completed!");
            CompletePuzzle();
        }
        else
        {
            Debug.Log("Wrong! Try again...");
            playerSequence.Clear();
            
            if (PuzzlePenaltyManager.Instance != null)
            {
                PuzzlePenaltyManager.Instance.TriggerPenalty();
            }
            
            yield return new WaitForSeconds(1f);
            StartCoroutine(ShowSequence());
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in colorButtons)
        {
            button.interactable = interactable;
        }
    }

    protected override void ClosePuzzle()
    {
        base.ClosePuzzle();
        StopAllCoroutines();
        playerSequence.Clear();
        isPlayerTurn = false;
        isShowingSequence = false;
    }
}
