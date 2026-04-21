using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Placed in the Intro scene. Plays the intro video on a RawImage, then fades
/// to black and loads the MainMenu scene. The player can skip with any key.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class IntroPlayer : MonoBehaviour
{
    [Header("References")]
    public RawImage videoDisplay;
    public CanvasGroup fadeOverlay;

    [Header("Settings")]
    public string mainMenuScene = "MainMenu";
    public float fadeDuration = 1.0f;

    private VideoPlayer videoPlayer;
    private bool skipped = false;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        // Start fully black, fade into the video
        if (fadeOverlay != null)
            fadeOverlay.alpha = 1f;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void Update()
    {
        // Any key or mouse click skips the intro
        if (!skipped && Input.anyKeyDown)
            Skip();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        StartCoroutine(FadeOverlay(1f, 0f, fadeDuration));
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!skipped)
            StartCoroutine(FadeAndLoadMenu());
    }

    /// <summary>Immediately stops the video and transitions to the main menu.</summary>
    private void Skip()
    {
        skipped = true;
        videoPlayer.Stop();
        StartCoroutine(FadeAndLoadMenu());
    }

    private IEnumerator FadeAndLoadMenu()
    {
        yield return StartCoroutine(FadeOverlay(0f, 1f, fadeDuration));
        SceneManager.LoadScene(mainMenuScene);
    }

    /// <summary>Lerps the fade overlay alpha from startAlpha to endAlpha.</summary>
    private IEnumerator FadeOverlay(float startAlpha, float endAlpha, float duration)
    {
        if (fadeOverlay == null) yield break;

        float elapsed = 0f;
        fadeOverlay.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        fadeOverlay.alpha = endAlpha;
    }
}
