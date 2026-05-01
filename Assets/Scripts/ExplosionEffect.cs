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
    public Material explosionSkybox; // Sky changes during explosion

  
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip explosionSound;

    private Camera mainCamera;

    private void Start()
    {
        // Get main camera
        mainCamera = Camera.main;

        // Make sure overlay starts invisible
        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        }
    }

    // Called to start explosion effect
    public void TriggerExplosion()
    {
        StartCoroutine(ExplosionSequence());
    }

    // Clears overlay (used by GameManager later)
    public void ClearFlashOverlay()
    {
        if (flashOverlay != null)
            flashOverlay.color = Color.clear;
    }

    private IEnumerator ExplosionSequence()
    {
        // Play explosion sound
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Start screen flash
        StartCoroutine(FlashEffect());

        // Start camera shake
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

        // phase 1: flash in
        // Fade from transparent to full colour
        float flashInDuration = flashDuration * 0.1f;
        float elapsed = 0f;

        while (elapsed < flashInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / flashInDuration;

            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // phase 2: hold + skybox change
        float holdDuration = flashDuration * 0.2f;
        elapsed = 0f;

        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;

            // Change skybox once during flash
            if (!skyboxChanged && explosionSkybox != null)
            {
                RenderSettings.skybox = explosionSkybox;
                DynamicGI.UpdateEnvironment(); // Update lighting
                skyboxChanged = true;
            }

            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1f);
            yield return null;
        }

        // Phase3: fade to black
        float fadeToBlackDuration = flashDuration * 0.7f;
        elapsed = 0f;

        while (elapsed < fadeToBlackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeToBlackDuration;

            // Gradually change colour to black
            float r = Mathf.Lerp(flashColor.r, 0f, t);
            float g = Mathf.Lerp(flashColor.g, 0f, t);
            float b = Mathf.Lerp(flashColor.b, 0f, t);

            flashOverlay.color = new Color(r, g, b, 1f);
            yield return null;
        }

        // End state = fully black screen
        flashOverlay.color = Color.black;
    }
}