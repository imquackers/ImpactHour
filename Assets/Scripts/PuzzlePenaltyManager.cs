using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzlePenaltyManager : MonoBehaviour
{
    public static PuzzlePenaltyManager Instance;

    [Header("Penalty Settings")]
    public float penaltyDuration = 1.5f;
    public float shakeMagnitude = 0.15f;
    public float timerPenaltyPercent = 0.1f;

    [Header("Sound Settings")]
    [Tooltip("How many seconds of the penalty sound to play before stopping it.")]
    public float soundPlayDuration = 1f;

    [Header("References")]
    public Image redFlashOverlay;
    public AudioSource audioSource;
    public AudioClip rumbleSound;

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

    public void TriggerPenalty()
    {
        StartCoroutine(PenaltySequence());
    }

    private IEnumerator PenaltySequence()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(penaltyDuration, shakeMagnitude);
        }

        if (audioSource != null && rumbleSound != null)
        {
            audioSource.clip = rumbleSound;
            audioSource.Play();
            StartCoroutine(StopSoundAfterDuration(soundPlayDuration));
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyTimePenalty(timerPenaltyPercent);
        }

        if (redFlashOverlay != null)
        {
            StartCoroutine(RedFlashEffect());
        }

        yield return new WaitForSeconds(penaltyDuration);
    }

    private IEnumerator StopSoundAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private IEnumerator RedFlashEffect()
    {
        float elapsed = 0f;
        Color flashColor = new Color(1f, 0f, 0f, 0.5f);
        Color transparent = new Color(1f, 0f, 0f, 0f);

        redFlashOverlay.color = flashColor;
        redFlashOverlay.gameObject.SetActive(true);

        while (elapsed < penaltyDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0f, elapsed / penaltyDuration);
            redFlashOverlay.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }

        redFlashOverlay.color = transparent;
        redFlashOverlay.gameObject.SetActive(false);
    }
}
