using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorPatternPuzzle : PuzzleBase
{
    [Header("Pattern Settings")]
    public int patternLength = 5;
    public int optionCount = 6;

    [Header("UI Elements")]
    public List<Image> patternSlots = new List<Image>();
    public GameObject questionMarkObject;
    public List<Button> optionButtons = new List<Button>();
    public Text instructionText;

    private List<Color> fullPattern = new List<Color>();
    private int missingIndex;
    private Color correctAnswer;
    private List<Color> colorPalette = new List<Color>();

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
        InitializeColorPalette();
        SetupOptionButtons();
    }

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

    private void SetupOptionButtons()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
        }
    }

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        GeneratePattern();
        DisplayPattern();
        GenerateOptions();
    }

    private void GeneratePattern()
    {
        fullPattern.Clear();
        currentPatternType = (PatternType)Random.Range(0, 4);

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

        missingIndex = Random.Range(1, patternLength - 1);
        correctAnswer = fullPattern[missingIndex];

        Debug.Log($"Pattern Type: {currentPatternType}, Missing Index: {missingIndex}");
        if (instructionText != null)
        {
            instructionText.text = "Find the missing color in the pattern!";
        }
    }

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

    private void GenerateAlternatingPattern()
    {
        Color colorA = colorPalette[Random.Range(0, colorPalette.Count)];
        Color colorB = colorPalette[Random.Range(0, colorPalette.Count)];
        
        while (colorB == colorA)
        {
            colorB = colorPalette[Random.Range(0, colorPalette.Count)];
        }

        for (int i = 0; i < patternLength; i++)
        {
            fullPattern.Add(i % 2 == 0 ? colorA : colorB);
        }
    }

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

    private void GenerateMirrorPattern()
    {
        int halfLength = patternLength / 2;
        List<Color> firstHalf = new List<Color>();
        
        for (int i = 0; i < halfLength; i++)
        {
            firstHalf.Add(colorPalette[Random.Range(0, colorPalette.Count)]);
        }

        for (int i = 0; i < halfLength; i++)
        {
            fullPattern.Add(firstHalf[i]);
        }

        if (patternLength % 2 == 1)
        {
            fullPattern.Add(colorPalette[Random.Range(0, colorPalette.Count)]);
        }

        for (int i = halfLength - 1; i >= 0; i--)
        {
            fullPattern.Add(firstHalf[i]);
        }
    }

    private void DisplayPattern()
    {
        for (int i = 0; i < patternSlots.Count && i < patternLength; i++)
        {
            if (i == missingIndex)
            {
                patternSlots[i].color = new Color(0.3f, 0.3f, 0.3f, 1f);
                patternSlots[i].gameObject.SetActive(true);
            }
            else
            {
                patternSlots[i].color = fullPattern[i];
                patternSlots[i].gameObject.SetActive(true);
            }
        }

        for (int i = patternLength; i < patternSlots.Count; i++)
        {
            patternSlots[i].gameObject.SetActive(false);
        }

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

    private void GenerateOptions()
    {
        List<Color> options = new List<Color>();
        options.Add(correctAnswer);

        while (options.Count < optionCount && options.Count < colorPalette.Count)
        {
            Color randomColor = colorPalette[Random.Range(0, colorPalette.Count)];
            if (!options.Contains(randomColor))
            {
                options.Add(randomColor);
            }
        }

        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Color temp = options[i];
            options[i] = options[j];
            options[j] = temp;
        }

        for (int i = 0; i < optionButtons.Count && i < options.Count; i++)
        {
            Image buttonImage = optionButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = options[i];
            }
            optionButtons[i].gameObject.SetActive(true);
        }

        for (int i = options.Count; i < optionButtons.Count; i++)
        {
            optionButtons[i].gameObject.SetActive(false);
        }
    }

    private bool isChecking = false;
    
    private void OnOptionClicked(int optionIndex)
    {
        if (isSolved)
        {
            Debug.Log("Already solved, ignoring click");
            return;
        }
        
        if (isChecking)
        {
            Debug.Log("Already checking, ignoring duplicate click");
            return;
        }
        
        if (optionIndex >= optionButtons.Count) return;

        Image buttonImage = optionButtons[optionIndex].GetComponent<Image>();
        if (buttonImage == null) return;

        Color selectedColor = buttonImage.color;
        
        Debug.Log($"=== STARTING PATTERN CHECK === Option {optionIndex} clicked");
        isChecking = true;
        DisableOptionButtons();
        RemoveOptionListeners();

        if (ColorsAreClose(selectedColor, correctAnswer, 0.05f))
        {
            Debug.Log("Correct! Pattern completed!");
            patternSlots[missingIndex].color = correctAnswer;
            if (questionMarkObject != null)
            {
                questionMarkObject.SetActive(false);
            }
            CompletePuzzle();
        }
        else
        {
            Debug.Log("Wrong color! Try again.");
            if (instructionText != null)
            {
                instructionText.text = "Wrong! Look for the pattern...";
            }
            
            if (PuzzlePenaltyManager.Instance != null)
            {
                PuzzlePenaltyManager.Instance.TriggerPenalty();
            }
            
            StartCoroutine(ReEnableOptionsCoroutine());
        }
    }
    
    private System.Collections.IEnumerator ReEnableOptionsCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        
        if (!isSolved)
        {
            isChecking = false;
            EnableOptionButtons();
            AddOptionListeners();
            
            if (instructionText != null)
            {
                instructionText.text = "Find the missing color in the pattern!";
            }
        }
    }
    
    private void ReEnableOptions()
    {
        if (isSolved) return;
        
        isChecking = false;
        EnableOptionButtons();
        AddOptionListeners();
        
        if (instructionText != null)
        {
            instructionText.text = "Find the missing color in the pattern!";
        }
    }
    
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
    
    private void RemoveOptionListeners()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
            }
        }
    }
    
    private void AddOptionListeners()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                int index = i;
                optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
            }
        }
    }

    private void ResetInstructionText()
    {
        if (instructionText != null)
        {
            instructionText.text = "Find the missing color in the pattern!";
        }
    }

    private bool ColorsAreClose(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    protected override void ClosePuzzle()
    {
        base.ClosePuzzle();
        if (questionMarkObject != null)
        {
            questionMarkObject.SetActive(false);
        }
    }
}
