using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    public Image flashOverlay;
    public float flashDuration = 2f;
    public Color flashColor = Color.white;
    
    [Header("Shake Settings")]
    public float shakeDuration = 3f;
    public float shakeIntensity = 2f;
    public float shakeFrequency = 50f;
    
    [Header("Skybox Settings")]
    public Material explosionSkybox;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip explosionSound;
    
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        }
    }
    
    public void TriggerExplosion()
    {
        StartCoroutine(ExplosionSequence());
    }

    /// Clears the flash overlay so another system can take over the black screen.
    public void ClearFlashOverlay()
    {
        if (flashOverlay != null)
            flashOverlay.color = Color.clear;
    }
    
    private IEnumerator ExplosionSequence()
    {
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        
        StartCoroutine(FlashEffect());
        
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeDuration, shakeIntensity, shakeFrequency);
        }
        
        yield return null;
    }
    
    private IEnumerator FlashEffect()
    {
        if (flashOverlay == null) yield break;

        bool skyboxChanged = false;

        // Phase 1 (0–10%): flash overlay fades in from transparent to full flash colour
        float flashInDuration = flashDuration * 0.1f;
        float elapsed = 0f;
        while (elapsed < flashInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / flashInDuration;
            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // Phase 2 (10–30%): hold at full flash colour, swap skybox
        float holdDuration = flashDuration * 0.2f;
        elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;

            if (!skyboxChanged && explosionSkybox != null)
            {
                RenderSettings.skybox = explosionSkybox;
                DynamicGI.UpdateEnvironment();
                skyboxChanged = true;
            }

            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1f);
            yield return null;
        }

        // Phase 3 (30–100%): cross-fade flash colour → black (keep alpha at 1 throughout)
        float fadeToBlackDuration = flashDuration * 0.7f;
        elapsed = 0f;
        while (elapsed < fadeToBlackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeToBlackDuration;
            float r = Mathf.Lerp(flashColor.r, 0f, t);
            float g = Mathf.Lerp(flashColor.g, 0f, t);
            float b = Mathf.Lerp(flashColor.b, 0f, t);
            flashOverlay.color = new Color(r, g, b, 1f);
            yield return null;
        }

        // End state: fully opaque black — GameManager.ImpactSequence takes over from here
        flashOverlay.color = Color.black;
    }
}
