using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// Shows the controls image after the player clicks New Game.
// Waits for any key, then fades out and loads the game scene.


public class ControlsScreen : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup controlsCanvasGroup;
    public CanvasGroup menuCanvasGroup;

    [Header("Settings")]
    public string gameScene = "Game";
    public float fadeDuration = 0.75f;

    private bool waitingForKey = false;

    private void Update()
    {
        if (waitingForKey && Input.anyKeyDown)
        {
            waitingForKey = false;
            StartCoroutine(FadeOutAndLoad());
        }
    }


    // Called by MainMenu.StartNewGame(). Fades the menu out, fades the controls screen in, then waits for the player to press any key.
   

    public void Show(CanvasGroup menuFadeGroup)
    {
        StartCoroutine(ShowSequence(menuFadeGroup));
    }

    private IEnumerator ShowSequence(CanvasGroup menuFadeGroup)
    {
        // Fade the main menu out
        if (menuFadeGroup != null)
            yield return StartCoroutine(Fade(menuFadeGroup, 1f, 0f, fadeDuration));

        // Activate and fade the controls screen in
        controlsCanvasGroup.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(controlsCanvasGroup, 0f, 1f, fadeDuration));

        waitingForKey = true;
    }

    private IEnumerator FadeOutAndLoad()
    {
        yield return StartCoroutine(Fade(controlsCanvasGroup, 1f, 0f, fadeDuration));
        SceneManager.LoadScene(gameScene);
    }

    // Lerps a CanvasGroup alpha between two values.
    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;
    }
}
