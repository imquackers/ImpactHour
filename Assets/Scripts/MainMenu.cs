using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    // inspector references
    [Header("UI References")]
    public Button newGameButton; // Button to start game
    public Button quitButton;    // Button to quit game
    public CanvasGroup canvasGroup; // Used for fading UI in/out

    [Header("Controls Screen")]
    public ControlsScreen controlsScreen; // Optional controls/tutorial screen

    [Header("Audio")]
    public AudioSource menuAudioSource; // Background music

    [Header("Fade Settings")]
    public float fadeInDuration = 1.5f; // Time for fade in/out

    private void Start()
    {
        // Fade in the menu when scene starts
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f; // Start invisible
            StartCoroutine(FadeIn());
        }

        // Play menu music if not already playing
        if (menuAudioSource != null && !menuAudioSource.isPlaying)
            menuAudioSource.Play();
    }

    // Called when "New Game" button is pressed
    public void StartNewGame()
    {
        if (controlsScreen != null)
        {
            // Show controls/tutorial screen first
            controlsScreen.Show(canvasGroup);
        }
        else
        {
            // If no controls screen, go straight to game
            StartCoroutine(FadeOutAndLoad("Game"));
        }
    }

    // Called when quit button pressed
    public void QuitGame()
    {
        Application.Quit(); // Closes the application 
        Debug.Log("Quit game");
    }

    // Fade UI from invisible → visible
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;

            // Gradually increase transparency
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);

            yield return null; // Wait next frame
        }

        canvasGroup.alpha = 1f; // Fully visible
    }

    // Fade out UI, then load a new scene
    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        if (canvasGroup != null)
        {
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;

                // Gradually decrease transparency
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);

                yield return null;
            }
        }

        // Load the specified scene
        SceneManager.LoadScene(sceneName);
    }
}