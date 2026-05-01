using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

//this allows for a smooth camera shake effect 

public class CameraShake : MonoBehaviour
{

    //static reference so other scripts can easily call it
    public static CameraShake Instance;

    private void Awake()
    {
        //ensures only one CameraShake exists in the scene
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            //if another instance exists, destroy this one
            Destroy(gameObject);
        }
    }
    //keeps track of currently running routine
    private Coroutine shakeCoroutine;

    
    // Starts Camera shake
   
    

    public void Shake(float duration, float magnitude, float frequency = 20f)
    {
        //if a shake is already running, stop it so multiple shakes can't stack
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        //start a new shake coroutine
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude, frequency));
    }
    //handles shake behaviour over time
    private IEnumerator ShakeCoroutine(float duration, float magnitude, float frequency = 20f)
    {
        //stores cameras original position 
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f; //tracks time passed
        float shakeTimer = 0f;

        //run until duration reached
        while (elapsed < duration)
        {
            //increase shake timer based on frequence
            //higher frequency means faster vibration
            shakeTimer += Time.deltaTime * frequency;
            
            //Perlin noise is used instead of random for a smoother shake effect
            float x = Mathf.PerlinNoise(shakeTimer, 0f) * 2f - 1f;
            float y = Mathf.PerlinNoise(0f, shakeTimer) * 2f - 1f;
            
            //graduallty reduce shake over time and starts at full magnitude
            float currentMagnitude = magnitude * (1f - (elapsed / duration));

            //apply shake offset relative to original position
            transform.localPosition = originalPosition + new Vector3(x, y, 0) * currentMagnitude;

            //increase elapsed time
            elapsed += Time.deltaTime;
            //wait until next frame
            yield return null;
        }

        //reset camera position when finished
        transform.localPosition = originalPosition;
        //clear references
        shakeCoroutine = null;
    }
}
