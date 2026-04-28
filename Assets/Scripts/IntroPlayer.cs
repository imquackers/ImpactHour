using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;


// Plays the intro video, fades in and then out and then loads the main menu.
// Player can skip with any key.


// Ensures this GameObject always has a VideoPlayer
[RequireComponent(typeof(VideoPlayer))]
public class IntroPlayer : MonoBehaviour
{
    // inspector references
    [Header("References")]
    public RawImage videoDisplay; // Where the video is shown (UI)
    public CanvasGroup fadeOverlay; // Black overlay used for fading

    // settings
    [Header("Settings")]
    public string mainMenuScene = "MainMenu"; // Scene to load after intro
    public float fadeDuration = 1.0f; // How long fades take

    // internal variables
    private VideoPlayer videoPlayer; // Plays the video
    private bool skipped = false;    // Prevents double triggering

    private void Awake()
    {
        // Get VideoPlayer component on this object
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        // Start fully black (fade in later)
        if (fadeOverlay != null)
            fadeOverlay.alpha = 1f;

        // When video ends → call OnVideoFinished
        videoPlayer.loopPointReached += OnVideoFinished;

        // Prepare the video before playing (avoids lag)
        videoPlayer.Prepare();

        // When ready → call OnVideoPrepared
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void Update()
    {
        // If player presses any key, skip intro
        if (!skipped && Input.anyKeyDown)
            Skip();
    }

    // Called when video is fully loaded and ready
    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play(); // Start playing video

        // Fade from black → visible
        StartCoroutine(FadeOverlay(1f, 0f, fadeDuration));
    }

    // Called when video finishes normally
    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!skipped)
            StartCoroutine(FadeAndLoadMenu());
    }

   
    // Skip intro manually
   
    private void Skip()
    {
        skipped = true; // Prevent double triggering

        videoPlayer.Stop(); // Stop video immediately

        // Fade out and go to menu
        StartCoroutine(FadeAndLoadMenu());
    }

    // Fade to black, then load next scene
    private IEnumerator FadeAndLoadMenu()
    {
        // Fade from visible → black
        yield return StartCoroutine(FadeOverlay(0f, 1f, fadeDuration));

        // Load the main menu scene
        SceneManager.LoadScene(mainMenuScene);
    }


    // Smoothly fades overlay alpha between two values
  
    private IEnumerator FadeOverlay(float startAlpha, float endAlpha, float duration)
    {
        if (fadeOverlay == null) yield break;

        float elapsed = 0f;

        // Set starting transparency
        fadeOverlay.alpha = startAlpha;

        // Gradually change alpha over time
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Lerp for smooth transition between values
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);

            yield return null; // Wait until next frame
        }

        // Ensure final value is exact
        fadeOverlay.alpha = endAlpha;
    }
}