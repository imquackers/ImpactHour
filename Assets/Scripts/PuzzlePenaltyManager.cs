using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzlePenaltyManager : MonoBehaviour
{
    // singleton (global access)
    public static PuzzlePenaltyManager Instance;

    // settings
    [Header("Penalty Settings")]
    public float penaltyDuration = 1.5f;     // How long the penalty lasts
    public float shakeMagnitude = 0.15f;     // Camera shake strength
    public float timerPenaltyPercent = 0.1f; // % of total time removed

    // audio settings
    [Header("Sound Settings")]
    public float soundPlayDuration = 1f;

    // Refs
    [Header("References")]
    public Image redFlashOverlay; 
    public AudioSource audioSource;
    public AudioClip rumbleSound;

    private void Awake()
    {
        // Ensure only ONE instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Called when player makes a mistake
    public void TriggerPenalty()
    {
        StartCoroutine(PenaltySequence());
    }

    // Main penalty logic
    private IEnumerator PenaltySequence()
    {
        // cam shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(penaltyDuration, shakeMagnitude);
        }

        // audio
        if (audioSource != null && rumbleSound != null)
        {
            audioSource.clip = rumbleSound;
            audioSource.Play();

            // Stop sound early (so it doesn't play too long)
            StartCoroutine(StopSoundAfterDuration(soundPlayDuration));
        }

        // time penalty
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyTimePenalty(timerPenaltyPercent);
        }

        // red flash
        if (redFlashOverlay != null)
        {
            StartCoroutine(RedFlashEffect());
        }

        // Wait for penalty duration before ending
        yield return new WaitForSeconds(penaltyDuration);
    }

    // Stops the sound after a set time
    private IEnumerator StopSoundAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    // Red screen flash that fades out
    private IEnumerator RedFlashEffect()
    {
        float elapsed = 0f;

        Color flashColor = new Color(1f, 0f, 0f, 0.5f); // Strong red
        Color transparent = new Color(1f, 0f, 0f, 0f);  // Invisible

        // Start fully visible
        redFlashOverlay.color = flashColor;
        redFlashOverlay.gameObject.SetActive(true);

        // Fade out over time
        while (elapsed < penaltyDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(0.5f, 0f, elapsed / penaltyDuration);

            redFlashOverlay.color = new Color(1f, 0f, 0f, alpha);

            yield return null;
        }

        // Hide overlay
        redFlashOverlay.color = transparent;
        redFlashOverlay.gameObject.SetActive(false);
    }
}