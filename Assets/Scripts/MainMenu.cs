using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Button newGameButton;
    public Button quitButton;
    public CanvasGroup canvasGroup;

    [Header("Controls Screen")]
    public ControlsScreen controlsScreen;

    [Header("Audio")]
    public AudioSource menuAudioSource;

    [Header("Fade Settings")]
    public float fadeInDuration = 1.5f;

    private void Start()
    {
        // Fade in the menu on start
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }

        // Play menu music
        if (menuAudioSource != null && !menuAudioSource.isPlaying)
            menuAudioSource.Play();
    }

    /// <summary>Shows the controls screen, then loads the game when the player continues.</summary>
    public void StartNewGame()
    {
        if (controlsScreen != null)
        {
            controlsScreen.Show(canvasGroup);
        }
        else
        {
            StartCoroutine(FadeOutAndLoad("Game"));
        }
    }

    /// <summary>Quits the application.</summary>
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game");
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene(sceneName);
    }
}
