using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // singleton
    public static GameManager Instance { get; private set; }

    // game settings
    [Header("Game Settings")]
    public float totalGameTime = 600f; // Total time (seconds)
    public int totalPuzzles = 10;      // How many puzzles to win

    // UI
    [Header("UI References")]
    public TMPro.TextMeshProUGUI timerText;       // Timer display
    public TMPro.TextMeshProUGUI puzzleCountText; // Puzzle progress

    // Effects
    [Header("Effects")]
    public ExplosionEffect explosionEffect; // particle effect (can add our own later)
    public UnityEngine.UI.Image fadeOverlay; // Black screen for fades
    public float fadeInDuration = 2f;        // Fade in time
    public float fadeToBlackDuration = 2f;   // Fade out time

    // meteor
    [Header("Meteor")]
    public MeteorMover meteorMover; // Controls meteor movement
    public MeteorExplosionEffect meteorExplosionEffect; // Explosion when destroyed

    // earth models
    [Header("Impact Swap")]
    public GameObject earthModel;      // Normal Earth
    public GameObject destroyedModel; // Destroyed Earth

    // game over screen
    [Header("Game Over")]
    public GameOverScreen gameOverScreen;

    // internal variables
    private float timeRemaining;
    private int puzzlesSolved = 0;
    private bool gameActive = true;

    // Cached values (prevents updating UI every frame unnecessarily)
    private int cachedMinutes = -1;
    private int cachedSeconds = -1;
    private int cachedPuzzlesSolved = -1;

    private void Awake()
    {
        // Ensure only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        timeRemaining = totalGameTime; // Start timer
        UpdateUI();

        // Start screen fully black, then fade in
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 1);
            StartCoroutine(FadeIn());
        }
    }

    private void Update()
    {
        if (!gameActive) return;

        // Countdown timer
        timeRemaining -= Time.deltaTime;

        // If time runs out then game over
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            GameOver();
        }

        UpdateUI();
    }

    // Called when a puzzle is completed
    public void PuzzleSolved()
    {
        puzzlesSolved++;
        UpdateUI();

        // If all puzzles done then win
        if (puzzlesSolved >= totalPuzzles)
        {
            Victory();
        }
    }

    // Returns time passed since start
    public float GetElapsedTime() => totalGameTime - timeRemaining;

    // Returns total time allowed
    public float GetTotalTime() => totalGameTime;

    // Removes time as punishment
    public void ApplyTimePenalty(float percentPenalty)
    {
        float timeLost = totalGameTime * percentPenalty;
        timeRemaining -= timeLost;

        if (timeRemaining < 0)
            timeRemaining = 0;

        Debug.Log($"Lost {timeLost:F1} seconds");
        UpdateUI();
    }

    // Update timer + puzzle UI (only when values change)
    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);

            // Only update text if time changed
            if (minutes != cachedMinutes || seconds != cachedSeconds)
            {
                cachedMinutes = minutes;
                cachedSeconds = seconds;
                timerText.text = $"Time: {minutes:00}:{seconds:00}";
            }

            // Turn red when low time
            if (timeRemaining <= 60)
                timerText.color = Color.red;
        }

        // Update puzzle counter
        if (puzzleCountText != null && puzzlesSolved != cachedPuzzlesSolved)
        {
            cachedPuzzlesSolved = puzzlesSolved;
            puzzleCountText.text = $"Puzzles: {puzzlesSolved}/{totalPuzzles}";
        }
    }

    // game over flow
    private void GameOver()
    {
        gameActive = false;
        Debug.Log("game over");

        // Start meteor crash sequence
        if (meteorMover != null)
            meteorMover.BeginFinalApproach();
    }

    // Called when meteor hits Earth
    public void OnMeteorImpact()
    {
        Debug.Log("Meteor hit");

        if (explosionEffect != null)
            explosionEffect.TriggerExplosion();

        StartCoroutine(ImpactSequence());
    }

    private IEnumerator ImpactSequence()
    {
        // Wait for explosion flash
        yield return new WaitForSeconds(explosionEffect != null ? explosionEffect.flashDuration : 2f);

        // Keep screen black using fade overlay
        if (fadeOverlay != null)
            fadeOverlay.color = Color.black;

        // Remove flash overlay
        if (explosionEffect != null)
            explosionEffect.ClearFlashOverlay();

        // Disable meteor + swap Earth model
        if (meteorMover != null)
            meteorMover.gameObject.SetActive(false);

        SwapEarthModels();

        yield return new WaitForSeconds(0.1f);

        // Fade in to destroyed Earth
        yield return StartCoroutine(FadeIn());

        // Show result for a few seconds
        yield return new WaitForSeconds(3f);

        // Fade back to black
        yield return StartCoroutine(FadeToBlack());

        // Show game over screen
        if (gameOverScreen != null)
        {
            yield return StartCoroutine(gameOverScreen.PlayAndWaitForInput());
            gameOverScreen.Hide();
        }
        else
        {
            yield return new WaitForSeconds(5f);
        }

        RestartLevel();
    }

    // Swap Earth models
    private void SwapEarthModels()
    {
        if (earthModel != null)
            earthModel.SetActive(false);

        if (destroyedModel != null)
            destroyedModel.SetActive(true);
    }

    // Fade from black to visible
    private IEnumerator FadeIn()
    {
        if (fadeOverlay == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeInDuration);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeOverlay.color = new Color(0, 0, 0, 0);
    }

    // Fade from visible to black
    private IEnumerator FadeToBlack()
    {
        if (fadeOverlay == null) yield break;

        float elapsed = 0f;
        Color fadeColor = fadeOverlay.color;

        while (elapsed < fadeToBlackDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / fadeToBlackDuration;
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }

    // win flow
    private void Victory()
    {
        gameActive = false;

        if (timerText != null)
        {
            timerText.text = "You saved Earth!";
            timerText.color = Color.green;
        }

        // Trigger meteor explosion in space
        if (meteorExplosionEffect != null)
            meteorExplosionEffect.Explode();
        else
            StartCoroutine(VictorySequence(0f));
    }

    // Called after explosion effect finishes
    public void OnMeteorExploded()
    {
        StartCoroutine(VictorySequence(10f));
    }

    private IEnumerator VictorySequence(float holdDuration)
    {
        yield return new WaitForSeconds(holdDuration);

        yield return StartCoroutine(FadeToBlack());

        SceneManager.LoadScene(0); // Load main menu
    }

    // Restart current level
    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}