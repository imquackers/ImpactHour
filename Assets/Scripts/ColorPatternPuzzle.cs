using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorPatternPuzzle : PuzzleBase
{
    // inspector settings
    [Header("Pattern Settings")]
    public int patternLength = 5; // How many slots in the pattern
    public int optionCount = 6;   // How many answer buttons

    [Header("UI Elements")]
    public List<Image> patternSlots = new List<Image>(); // UI boxes showing the pattern
    public GameObject questionMarkObject; // The "?" that shows missing slot
    public List<Button> optionButtons = new List<Button>(); // Answer buttons
    public Text instructionText; // Instruction text

    // internal variables
    private List<Color> fullPattern = new List<Color>(); // Full correct pattern
    private int missingIndex; // Index of the missing color
    private Color correctAnswer; // The correct color
    private List<Color> colorPalette = new List<Color>(); // Pool of colors to use

    // Types of patterns the game can generate
    private enum PatternType
    {
        Repeating,
        Alternating,
        Gradient,
        Mirror
    }

    private PatternType currentPatternType;

    private void Start()
    {
        InitializeColorPalette(); // Fill color list
        SetupOptionButtons();     // Add click listeners to buttons
    }

    // Create a list of possible colors
    private void InitializeColorPalette()
    {
        colorPalette = new List<Color>
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
            new Color(0.5f, 0.25f, 0f),
            Color.white,
            new Color(0.5f, 0.5f, 0.5f)
        };
    }

    // Set up button clicks
    private void SetupOptionButtons()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i; // Important: store index properly
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
        }
    }

    // Called when puzzle opens
    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        GeneratePattern(); // Create pattern
        DisplayPattern();  // Show it on screen
        GenerateOptions(); // Create answer buttons
    }

    // Decide which pattern type and build it
    private void GeneratePattern()
    {
        fullPattern.Clear();
        currentPatternType = (PatternType)Random.Range(0, 4);

        // Pick pattern type
        switch (currentPatternType)
        {
            case PatternType.Repeating:
                GenerateRepeatingPattern();
                break;
            case PatternType.Alternating:
                GenerateAlternatingPattern();
                break;
            case PatternType.Gradient:
                GenerateGradientPattern();
                break;
            case PatternType.Mirror:
                GenerateMirrorPattern();
                break;
        }

        // Choose a random slot to hide (not first or last)
        missingIndex = Random.Range(1, patternLength - 1);
        correctAnswer = fullPattern[missingIndex];

        // Set instruction text
        if (instructionText != null)
        {
            instructionText.text = "Find the missing color in the pattern!";
        }
    }

    // pattern types

    // Repeats a sequence like: red, blue, red, blue...
    private void GenerateRepeatingPattern()
    {
        int repeatLength = Random.Range(2, 4);
        List<Color> repeatUnit = new List<Color>();

        for (int i = 0; i < repeatLength; i++)
        {
            repeatUnit.Add(colorPalette[Random.Range(0, colorPalette.Count)]);
        }

        for (int i = 0; i < patternLength; i++)
        {
            fullPattern.Add(repeatUnit[i % repeatLength]);
        }
    }

    // Alternates between two colors
    private void GenerateAlternatingPattern()
    {
        Color colorA = colorPalette[Random.Range(0, colorPalette.Count)];
        Color colorB = colorPalette[Random.Range(0, colorPalette.Count)];

        // Make sure colors are different
        while (colorB == colorA)
        {
            colorB = colorPalette[Random.Range(0, colorPalette.Count)];
        }

        for (int i = 0; i < patternLength; i++)
        {
            fullPattern.Add(i % 2 == 0 ? colorA : colorB);
        }
    }

    // Smooth gradient between two colors
    private void GenerateGradientPattern()
    {
        Color startColor = colorPalette[Random.Range(0, colorPalette.Count)];
        Color endColor = colorPalette[Random.Range(0, colorPalette.Count)];

        for (int i = 0; i < patternLength; i++)
        {
            float t = (float)i / (patternLength - 1);
            fullPattern.Add(Color.Lerp(startColor, endColor, t));
        }
    }

    // Pattern mirrors itself (like symmetry)
    private void GenerateMirrorPattern()
    {
        int halfLength = patternLength / 2;
        List<Color> firstHalf = new List<Color>();

        for (int i = 0; i < halfLength; i++)
        {
            firstHalf.Add(colorPalette[Random.Range(0, colorPalette.Count)]);
        }

        // First half
        for (int i = 0; i < halfLength; i++)
        {
            fullPattern.Add(firstHalf[i]);
        }

        // Middle (if odd length)
        if (patternLength % 2 == 1)
        {
            fullPattern.Add(colorPalette[Random.Range(0, colorPalette.Count)]);
        }

        // Mirror second half
        for (int i = halfLength - 1; i >= 0; i--)
        {
            fullPattern.Add(firstHalf[i]);
        }
    }

    // Show pattern on screen
    private void DisplayPattern()
    {
        for (int i = 0; i < patternSlots.Count && i < patternLength; i++)
        {
            if (i == missingIndex)
            {
                // Hide the missing color (grey box)
                patternSlots[i].color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }
            else
            {
                patternSlots[i].color = fullPattern[i];
            }

            patternSlots[i].gameObject.SetActive(true);
        }

        // Hide extra slots
        for (int i = patternLength; i < patternSlots.Count; i++)
        {
            patternSlots[i].gameObject.SetActive(false);
        }

        // Move the "?" to the missing slot
        if (questionMarkObject != null)
        {
            RectTransform questionRect = questionMarkObject.GetComponent<RectTransform>();
            if (questionRect != null)
            {
                questionRect.anchoredPosition = patternSlots[missingIndex].rectTransform.anchoredPosition;
            }
            questionMarkObject.SetActive(true);
        }
    }

    // Generate answer choices
    private void GenerateOptions()
    {
        List<Color> options = new List<Color>();
        options.Add(correctAnswer); // Always include correct answer

        // Add random wrong answers
        while (options.Count < optionCount && options.Count < colorPalette.Count)
        {
            Color randomColor = colorPalette[Random.Range(0, colorPalette.Count)];
            if (!options.Contains(randomColor))
            {
                options.Add(randomColor);
            }
        }

        // Shuffle options
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Color temp = options[i];
            options[i] = options[j];
            options[j] = temp;
        }

        // Apply colors to buttons
        for (int i = 0; i < optionButtons.Count && i < options.Count; i++)
        {
            Image buttonImage = optionButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = options[i];
            }
            optionButtons[i].gameObject.SetActive(true);
        }

        // Hide unused buttons
        for (int i = options.Count; i < optionButtons.Count; i++)
        {
            optionButtons[i].gameObject.SetActive(false);
        }
    }

    private bool isChecking = false; // Prevent double clicks

    // Called when player clicks an option
    private void OnOptionClicked(int optionIndex)
    {
        if (isSolved || isChecking) return;

        Image buttonImage = optionButtons[optionIndex].GetComponent<Image>();
        if (buttonImage == null) return;

        Color selectedColor = buttonImage.color;

        isChecking = true;
        DisableOptionButtons();
        RemoveOptionListeners();

        // Check if answer is correct
        if (ColorsAreClose(selectedColor, correctAnswer, 0.05f))
        {
            // Correct answer
            patternSlots[missingIndex].color = correctAnswer;

            if (questionMarkObject != null)
            {
                questionMarkObject.SetActive(false);
            }

            CompletePuzzle(); // Finish puzzle
        }
        else
        {
            // Wrong answer
            if (instructionText != null)
            {
                instructionText.text = "Wrong! Look for the pattern...";
            }

            // Trigger penalty system if it exists
            if (PuzzlePenaltyManager.Instance != null)
            {
                PuzzlePenaltyManager.Instance.TriggerPenalty();
            }

            // Re-enable buttons after delay
            StartCoroutine(ReEnableOptionsCoroutine());
        }
    }

    // Wait before letting player try again
    private System.Collections.IEnumerator ReEnableOptionsCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (!isSolved)
        {
            isChecking = false;
            EnableOptionButtons();
            AddOptionListeners();

            ResetInstructionText();
        }
    }

    // Enable/disable buttons
    private void DisableOptionButtons()
    {
        foreach (var button in optionButtons)
        {
            if (button != null) button.interactable = false;
        }
    }

    private void EnableOptionButtons()
    {
        foreach (var button in optionButtons)
        {
            if (button != null) button.interactable = true;
        }
    }

    // Remove and re-add button listeners (prevents duplicate clicks)
    private void RemoveOptionListeners()
    {
        foreach (var button in optionButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }

    private void AddOptionListeners()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
        }
    }

    // Reset instruction text
    private void ResetInstructionText()
    {
        if (instructionText != null)
        {
            instructionText.text = "Find the missing color in the pattern!";
        }
    }

    // Compare colors with small tolerance (important for gradients)
    private bool ColorsAreClose(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    // Called when puzzle closes
    protected override void ClosePuzzle()
    {
        base.ClosePuzzle();

        if (questionMarkObject != null)
        {
            questionMarkObject.SetActive(false);
        }
    }
}