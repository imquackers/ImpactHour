using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

// Displays an animated looping game-over GIF and a pulsing prompt.
// Waits for any key press before signalling the caller to restart.
[RequireComponent(typeof(RawImage))]
public class GameOverScreen : MonoBehaviour
{
    [System.Serializable]
    public struct GifFrame
    {
        public Texture2D texture;
        [Tooltip("How long to display this frame in seconds.")]
        public float delay;
    }

    [Header("Frames")]
    public GifFrame[] frames;

    [Header("Prompt")]
    public TextMeshProUGUI promptText;
    [Tooltip("How fast the prompt pulses (cycles per second).")]
    public float pulseSpeed = 1.5f;

    private RawImage rawImage;
    private bool inputReceived;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        gameObject.SetActive(false);
    }

    // Shows the screen, loops the GIF, and waits until the player presses any key.
    public IEnumerator PlayAndWaitForInput()
    {
        inputReceived = false;
        gameObject.SetActive(true);

        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.alpha = 0f;
        }

        StartCoroutine(LoopGif());
        StartCoroutine(PulsePrompt());

        // Wait for any key — skip the very first frame so the key that triggered
        // this screen (if any) doesn't immediately dismiss it
        yield return null;
        yield return new WaitUntil(() =>
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Gamepad.current != null && (
                Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.startButton.wasPressedThisFrame)));

        inputReceived = true;
    }

    // Hides the screen and stops all coroutines.
    public void Hide()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    private IEnumerator LoopGif()
    {
        if (frames == null || frames.Length == 0) yield break;

        int index = 0;
        while (!inputReceived)
        {
            rawImage.texture = frames[index].texture;
            yield return new WaitForSeconds(frames[index].delay);
            index = (index + 1) % frames.Length;
        }
    }

    private IEnumerator PulsePrompt()
    {
        if (promptText == null) yield break;

        // Brief delay before prompt appears
        yield return new WaitForSeconds(0.5f);

        while (!inputReceived)
        {
            float alpha = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f;
            promptText.alpha = alpha;
            yield return null;
        }

        promptText.alpha = 0f;
    }
}
