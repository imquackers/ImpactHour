using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float totalGameTime = 600f;
    public int totalPuzzles = 10;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI puzzleCountText;

    [Header("Effects")]
    public ExplosionEffect explosionEffect;
    public UnityEngine.UI.Image fadeOverlay;
    public float fadeInDuration = 2f;
    public float fadeToBlackDuration = 2f;

    [Header("Meteor")]
    public MeteorMover meteorMover;
    public MeteorExplosionEffect meteorExplosionEffect;

    [Header("Impact Swap")]
    public GameObject earthModel;
    public GameObject destroyedModel;

    [Header("Game Over")]
    public GameOverScreen gameOverScreen;

    private float timeRemaining;
    private int puzzlesSolved = 0;
    private bool gameActive = true;

    // Cached values — UpdateUI only writes to TMP when these change.
    private int cachedMinutes = -1;
    private int cachedSeconds = -1;
    private int cachedPuzzlesSolved = -1;

    private void Awake()
    {
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
        timeRemaining = totalGameTime;
        UpdateUI();
        
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 1);
            StartCoroutine(FadeIn());
        }
    }

    private void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            GameOver();
        }

        UpdateUI();
    }

    public void PuzzleSolved()
    {
        puzzlesSolved++;
        UpdateUI();

        if (puzzlesSolved >= totalPuzzles)
        {
            Victory();
        }
    }

    /// Returns how much time has elapsed since the game started (0 to totalGameTime).
    public float GetElapsedTime() => totalGameTime - timeRemaining;

    /// Returns the total game duration.
    public float GetTotalTime() => totalGameTime;

    public void ApplyTimePenalty(float percentPenalty)
    {
        float timeLost = totalGameTime * percentPenalty;
        timeRemaining -= timeLost;
        
        if (timeRemaining < 0)
        {
            timeRemaining = 0;
        }
        
        Debug.Log($"Time penalty! Lost {timeLost:F1} seconds. Time remaining: {timeRemaining:F1}s");
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);

            if (minutes != cachedMinutes || seconds != cachedSeconds)
            {
                cachedMinutes = minutes;
                cachedSeconds = seconds;
                timerText.text = $"Time: {minutes:00}:{seconds:00}";
            }

            if (timeRemaining <= 60)
                timerText.color = Color.red;
        }

        if (puzzleCountText != null && puzzlesSolved != cachedPuzzlesSolved)
        {
            cachedPuzzlesSolved = puzzlesSolved;
            puzzleCountText.text = $"Puzzles: {puzzlesSolved}/{totalPuzzles}";
        }
    }

    private void GameOver()
    {
        gameActive = false;
        Debug.Log("Time's up! Meteor begins final approach.");

        // Hand off to the meteor — it will fly to Earth and call OnMeteorImpact on collision
        if (meteorMover != null)
            meteorMover.BeginFinalApproach();
    }

    /// Called by MeteorImpact when the meteor collider hits the Earth collider.
    public void OnMeteorImpact()
    {
        Debug.Log("Meteor impacted Earth! Triggering explosion.");

        if (explosionEffect != null)
            explosionEffect.TriggerExplosion();

        StartCoroutine(ImpactSequence());
    }

    private IEnumerator ImpactSequence()
    {
        // Wait for the flash to finish — flashOverlay is now fully opaque black
        yield return new WaitForSeconds(explosionEffect != null ? explosionEffect.flashDuration : 2f);

        // Hand the black screen off to fadeOverlay so FadeIn can control it
        if (fadeOverlay != null)
            fadeOverlay.color = Color.black;

        // Clear flashOverlay — fadeOverlay is now holding the black screen
        if (explosionEffect != null)
            explosionEffect.ClearFlashOverlay();

        // Disable meteor and swap models while screen is black
        if (meteorMover != null)
            meteorMover.gameObject.SetActive(false);

        SwapEarthModels();

        // Small buffer to ensure model swap is complete before revealing
        yield return new WaitForSeconds(0.1f);

        // Fade in to reveal the destroyed Earth
        yield return StartCoroutine(FadeIn());

        // Hold on the destroyed Earth
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeToBlack());

        // Loop GIF and wait for player input
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
    
    /// Hides the intact Earth and shows the Destroyed model.
    private void SwapEarthModels()
    {
        if (earthModel != null)
            earthModel.SetActive(false);
        
        if (destroyedModel != null)
            destroyedModel.SetActive(true);
    }
    
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

    private void Victory()
    {
        gameActive = false;
        Debug.Log("All puzzles solved — triggering meteor explosion.");

        if (timerText != null)
        {
            timerText.text  = "You saved Earth!";
            timerText.color = Color.green;
        }

        if (meteorExplosionEffect != null)
            meteorExplosionEffect.Explode();
        else
            StartCoroutine(VictorySequence(0f));
    }

    /// Called by MeteorExplosionEffect once its particles have fired (≈2 s after detonation).
    public void OnMeteorExploded()
    {
        StartCoroutine(VictorySequence(10f));
    }

    private IEnumerator VictorySequence(float holdDuration)
    {
        // Let the player enjoy the explosion before fading out
        yield return new WaitForSeconds(holdDuration);

        yield return StartCoroutine(FadeToBlack());

        SceneManager.LoadScene(0);   // index 0 = main menu
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
