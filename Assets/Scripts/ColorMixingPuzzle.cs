using UnityEngine;
using UnityEngine.UI;

public class ColorMixingPuzzle : PuzzleBase
{
    [Header("Color Mixing Settings")]
    public float colorTolerance = 0.1f;

    [Header("UI Elements")]
    public Image targetColorDisplay;
    public Image playerColorDisplay;
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;
    public Text redValueText;
    public Text greenValueText;
    public Text blueValueText;
    public Button submitButton;

    private Color targetColor;
    private Color currentPlayerColor;

    private void Start()
    {
        if (redSlider != null) redSlider.onValueChanged.AddListener(OnSliderChanged);
        if (greenSlider != null) greenSlider.onValueChanged.AddListener(OnSliderChanged);
        if (blueSlider != null) blueSlider.onValueChanged.AddListener(OnSliderChanged);
        if (submitButton != null) submitButton.onClick.AddListener(CheckColorMatch);
    }

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        GenerateTargetColor();
        ResetPlayerColor();
        UpdatePlayerColorDisplay();
    }

    private void GenerateTargetColor()
    {
        targetColor = new Color(
            Random.Range(0.2f, 1f),
            Random.Range(0.2f, 1f),
            Random.Range(0.2f, 1f),
            1f
        );

        if (targetColorDisplay != null)
        {
            targetColorDisplay.color = targetColor;
        }

        Debug.Log($"Target Color: R={targetColor.r:F2}, G={targetColor.g:F2}, B={targetColor.b:F2}");
    }

    private void ResetPlayerColor()
    {
        if (redSlider != null) redSlider.value = 0.5f;
        if (greenSlider != null) greenSlider.value = 0.5f;
        if (blueSlider != null) blueSlider.value = 0.5f;
    }

    private void OnSliderChanged(float value)
    {
        UpdatePlayerColorDisplay();
    }

    private void UpdatePlayerColorDisplay()
    {
        float r = redSlider != null ? redSlider.value : 0.5f;
        float g = greenSlider != null ? greenSlider.value : 0.5f;
        float b = blueSlider != null ? blueSlider.value : 0.5f;

        currentPlayerColor = new Color(r, g, b, 1f);

        if (playerColorDisplay != null)
        {
            playerColorDisplay.color = currentPlayerColor;
        }

        if (redValueText != null) redValueText.text = Mathf.RoundToInt(r * 255).ToString();
        if (greenValueText != null) greenValueText.text = Mathf.RoundToInt(g * 255).ToString();
        if (blueValueText != null) blueValueText.text = Mathf.RoundToInt(b * 255).ToString();
    }

    private bool isChecking = false;
    
    private void CheckColorMatch()
    {
        if (isSolved)
        {
            Debug.Log("Already solved, ignoring check");
            return;
        }
        
        if (isChecking)
        {
            Debug.Log("Already checking, ignoring duplicate call");
            return;
        }
        
        Debug.Log("=== STARTING COLOR CHECK ===");
        isChecking = true;
        
        // Remove listener to prevent multiple calls
        if (submitButton != null)
        {
            submitButton.interactable = false;
            submitButton.onClick.RemoveListener(CheckColorMatch);
        }
        
        float rDiff = Mathf.Abs(targetColor.r - currentPlayerColor.r);
        float gDiff = Mathf.Abs(targetColor.g - currentPlayerColor.g);
        float bDiff = Mathf.Abs(targetColor.b - currentPlayerColor.b);

        float totalDifference = rDiff + gDiff + bDiff;

        Debug.Log($"Color Difference: {totalDifference:F3} (Tolerance: {colorTolerance * 3:F3})");
        Debug.Log($"Target: R={targetColor.r:F2} G={targetColor.g:F2} B={targetColor.b:F2}");
        Debug.Log($"Player: R={currentPlayerColor.r:F2} G={currentPlayerColor.g:F2} B={currentPlayerColor.b:F2}");

        if (totalDifference <= colorTolerance * 3)
        {
            Debug.Log("Color match! Puzzle completed!");
            CompletePuzzle();
        }
        else
        {
            Debug.Log("Not close enough. Keep adjusting!");
            ShowFeedback(rDiff, gDiff, bDiff);
            
            if (PuzzlePenaltyManager.Instance != null)
            {
                PuzzlePenaltyManager.Instance.TriggerPenalty();
            }
            
            // Re-add listener for next attempt
            StartCoroutine(ReEnableSubmitAfterDelay());
        }
    }
    
    private System.Collections.IEnumerator ReEnableSubmitAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        
        if (!isSolved)
        {
            isChecking = false;
            if (submitButton != null)
            {
                submitButton.interactable = true;
                submitButton.onClick.AddListener(CheckColorMatch);
            }
        }
    }

    private void ShowFeedback(float rDiff, float gDiff, float bDiff)
    {
        string feedback = "Adjust: ";
        
        if (rDiff > colorTolerance)
        {
            if (currentPlayerColor.r < targetColor.r)
                feedback += "MORE Red ";
            else
                feedback += "LESS Red ";
        }
        
        if (gDiff > colorTolerance)
        {
            if (currentPlayerColor.g < targetColor.g)
                feedback += "MORE Green ";
            else
                feedback += "LESS Green ";
        }
        
        if (bDiff > colorTolerance)
        {
            if (currentPlayerColor.b < targetColor.b)
                feedback += "MORE Blue ";
            else
                feedback += "LESS Blue ";
        }

        Debug.Log(feedback);
    }

    protected override void ClosePuzzle()
    {
        base.ClosePuzzle();
    }
}
