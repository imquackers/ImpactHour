using UnityEngine;
using UnityEngine.UI;

public class ColorMixingPuzzle : PuzzleBase
{
    [Header("Color Mixing Settings")]
    public float colorTolerance = 0.1f; // How close RGB must be to pass

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
        // Add slider listeners for live updates
        if (redSlider != null) redSlider.onValueChanged.AddListener(OnSliderChanged);
        if (greenSlider != null) greenSlider.onValueChanged.AddListener(OnSliderChanged);
        if (blueSlider != null) blueSlider.onValueChanged.AddListener(OnSliderChanged);

        // Button triggers check
        if (submitButton != null) submitButton.onClick.AddListener(CheckColorMatch);
    }

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle(); // Opens UI + disables player control
        GenerateTargetColor();
        ResetPlayerColor();
        UpdatePlayerColorDisplay();
    }

    private void GenerateTargetColor()
    {
        // Random target colour (kept bright by limiting min to 0.2)
        targetColor = new Color(
            Random.Range(0.2f, 1f),
            Random.Range(0.2f, 1f),
            Random.Range(0.2f, 1f),
            1f
        );

        if (targetColorDisplay != null)
        {
            targetColorDisplay.color = targetColor; // Show target colour
        }

        Debug.Log($"Target Color: R={targetColor.r:F2}, G={targetColor.g:F2}, B={targetColor.b:F2}");
    }

    private void ResetPlayerColor()
    {
        // Reset sliders to neutral middle value
        if (redSlider != null) redSlider.value = 0.5f;
        if (greenSlider != null) greenSlider.value = 0.5f;
        if (blueSlider != null) blueSlider.value = 0.5f;
    }

    private void OnSliderChanged(float value)
    {
        UpdatePlayerColorDisplay(); // Any slider change updates preview
    }

    private void UpdatePlayerColorDisplay()
    {
        float r = redSlider != null ? redSlider.value : 0.5f;
        float g = greenSlider != null ? greenSlider.value : 0.5f;
        float b = blueSlider != null ? blueSlider.value : 0.5f;

        currentPlayerColor = new Color(r, g, b, 1f);

        if (playerColorDisplay != null)
        {
            playerColorDisplay.color = currentPlayerColor; // Live preview
        }

        // Convert sliders (0–1) into RGB (0–255) display values
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

        isChecking = true; // Prevent spam clicks

        // Disable button during evaluation (prevents double submission)
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

        if (totalDifference <= colorTolerance * 3)
        {
            Debug.Log("Color match! Puzzle completed!");
            CompletePuzzle(); // Success path
        }
        else
        {
            Debug.Log("Not close enough. Keep adjusting!");
            ShowFeedback(rDiff, gDiff, bDiff);

            if (PuzzlePenaltyManager.Instance != null)
            {
                PuzzlePenaltyManager.Instance.TriggerPenalty(); // Punish wrong attempt
            }

            StartCoroutine(ReEnableSubmitAfterDelay()); // Allow retry
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
                submitButton.onClick.AddListener(CheckColorMatch); // Re-enable input
            }
        }
    }

    private void ShowFeedback(float rDiff, float gDiff, float bDiff)
    {
        string feedback = "Adjust: ";

        // Per-channel guidance based on direction of error
        if (rDiff > colorTolerance)
            feedback += (currentPlayerColor.r < targetColor.r) ? "MORE Red " : "LESS Red ";

        if (gDiff > colorTolerance)
            feedback += (currentPlayerColor.g < targetColor.g) ? "MORE Green " : "LESS Green ";

        if (bDiff > colorTolerance)
            feedback += (currentPlayerColor.b < targetColor.b) ? "MORE Blue " : "LESS Blue ";

        Debug.Log(feedback); // Debug hint system for player
    }

    protected override void ClosePuzzle()
    {
        base.ClosePuzzle(); // Restores player control + hides UI
    }
}