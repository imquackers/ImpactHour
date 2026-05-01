using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class ColorGradientPuzzle : PuzzleBase
{
    [Header("Gradient Settings")]
    [Tooltip("Total number of gradient swatches shown.")]
    public int swatchCount = 6;
    [Tooltip("Number of answer choices presented.")]
    public int optionCount = 5;

    [Header("UI Elements")]
    public List<Image> swatchSlots = new List<Image>();
    public List<Button> optionButtons = new List<Button>();
    public Text instructionText;

    // Runtime state
    private Color startColor;
    private Color endColor;
    private int missingIndex;
    private Color correctColor;
    private bool isChecking = false;

    private static readonly Color MissingSlotColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    private void Start()
    {
        SetupOptionListeners();
    }

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        isChecking = false;
        GenerateGradient();
        DisplaySwatches();
        GenerateOptions();
        SetInstruction("Find the missing colour in the gradient!");
    }

    // Generation 

    private void GenerateGradient()
    {
        // Pick two visually distinct hues
        float h1 = Random.value;
        float h2 = h1 + Random.Range(0.25f, 0.55f);
        if (h2 > 1f) h2 -= 1f;

        startColor = Color.HSVToRGB(h1, 0.85f, 0.9f);
        endColor   = Color.HSVToRGB(h2, 0.85f, 0.9f);

        // Missing slot is never the first or last so context is always visible
        missingIndex = Random.Range(1, swatchCount - 1);
        correctColor = GradientColorAt(missingIndex);
    }

    private Color GradientColorAt(int index)
    {
        float t = (float)index / (swatchCount - 1);
        return Color.Lerp(startColor, endColor, t);
    }

    private void DisplaySwatches()
    {
        for (int i = 0; i < swatchSlots.Count; i++)
        {
            if (swatchSlots[i] == null) continue;

            if (i < swatchCount)
            {
                swatchSlots[i].gameObject.SetActive(true);
                swatchSlots[i].color = (i == missingIndex) ? MissingSlotColor : GradientColorAt(i);
            }
            else
            {
                swatchSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // Options 

    private void GenerateOptions()
    {
        // Build option list: correct + nearby hue distractors
        List<Color> options = new List<Color> { correctColor };

        float correctH, correctS, correctV;
        Color.RGBToHSV(correctColor, out correctH, out correctS, out correctV);

        int attempts = 0;
        while (options.Count < optionCount && attempts < 50)
        {
            attempts++;
            float offsetH = correctH + Random.Range(-0.22f, 0.22f);
            if (offsetH < 0f) offsetH += 1f;
            if (offsetH > 1f) offsetH -= 1f;

            Color candidate = Color.HSVToRGB(offsetH, correctS, correctV);

            // Reject if too close to any existing option
            bool tooClose = false;
            foreach (Color existing in options)
            {
                if (ColorsClose(candidate, existing, 0.05f)) { tooClose = true; break; }
            }
            if (!tooClose) options.Add(candidate);
        }

        // Shuffle
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }

        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] == null) continue;

            if (i < options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                Image img = optionButtons[i].GetComponent<Image>();
                if (img != null) img.color = options[i];
                optionButtons[i].interactable = true;
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    //Interaction

    private void SetupOptionListeners()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i;
            if (optionButtons[i] != null)
                optionButtons[i].onClick.AddListener(() => OnOptionClicked(index));
        }
    }

    private void OnOptionClicked(int index)
    {
        if (isSolved || isChecking) return;
        if (index >= optionButtons.Count || optionButtons[index] == null) return;

        Image img = optionButtons[index].GetComponent<Image>();
        if (img == null) return;

        isChecking = true;
        SetOptionsInteractable(false);

        if (ColorsClose(img.color, correctColor, 0.05f))
        {
            // Correct = reveal the missing swatch and complete
            if (missingIndex < swatchSlots.Count && swatchSlots[missingIndex] != null)
                swatchSlots[missingIndex].color = correctColor;

            CompletePuzzle();
        }
        else
        {
            // Wrong = penalty then re-enable after a short delay
            PuzzlePenaltyManager.Instance?.TriggerPenalty();
            SetInstruction("Wrong! Look more carefully at the gradient...");
            StartCoroutine(ResetAfterPenalty());
        }
    }

    private IEnumerator ResetAfterPenalty()
    {
        yield return new WaitForSeconds(1.5f);
        if (!isSolved)
        {
            isChecking = false;
            SetOptionsInteractable(true);
            SetInstruction("Find the missing colour in the gradient!");
        }
    }

    private void SetOptionsInteractable(bool value)
    {
        foreach (Button b in optionButtons)
            if (b != null) b.interactable = value;
    }

    private void SetInstruction(string text)
    {
        if (instructionText != null)
            instructionText.text = text;
    }

    // Helpers 

    private bool ColorsClose(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}
