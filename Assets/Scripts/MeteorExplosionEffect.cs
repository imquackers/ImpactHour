using System.Collections;
using UnityEngine;

// Handles the big meteor explosion effect when player wins
[RequireComponent(typeof(AudioSource))]
public class MeteorExplosionEffect : MonoBehaviour
{
    // Audio
    [Header("Audio")]
    public AudioClip explosionClip;     
    public float explosionVolume = 1f;  

    // screen flash
    [Header("Flash")]
    public UnityEngine.UI.Image flashOverlay; 
    public float flashInDuration = 0.15f;   
    public float flashHoldDuration = 0.2f;   
    public float flashOutDuration = 0.6f;    

    // particle sizes
    // 
    private const float CoreRadius = 500f;
    private const float ShockwaveRadius = 1200f;
    private const float DebrisRadius = 900f;
    private const float SmokeRadius = 700f;

    private AudioSource audioSource;
    private bool hasExploded = false;

    // Shared material for all particle systems
    private Material particleMaterial;

    // Shader names (URP + fallback)
    private static readonly string URPParticleShader = "Universal Render Pipeline/Particles/Unlit";
    private static readonly string FallbackShader = "Particles/Standard Unlit";

    private void Awake()
    {
        // Get AudioSource on this object
        audioSource = GetComponent<AudioSource>();

        // Create particle material
        BuildParticleMaterial();
    }

    private void BuildParticleMaterial()
    {
        // Try URP shader first, fallback if needed
        Shader shader = Shader.Find(URPParticleShader) ?? Shader.Find(FallbackShader);

        if (shader == null)
        {
            Debug.LogWarning("No particle shader found!");
            return;
        }

        // Create material (white so particle colors show correctly)
        particleMaterial = new Material(shader);
        particleMaterial.SetColor("_BaseColor", Color.white);
    }

    // Applies the material to a particle system
    private void ApplyMaterial(GameObject go)
    {
        if (particleMaterial == null) return;

        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        if (r != null)
        {
            r.material = particleMaterial;
            r.trailMaterial = particleMaterial;
            r.renderMode = ParticleSystemRenderMode.Billboard;
        }
    }

    // main explosion
    public void Explode()
    {
        // Prevent double explosion
        if (hasExploded) return;

        hasExploded = true;
        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        // Flash screen white
        if (flashOverlay != null)
            StartCoroutine(FlashScreen());

        // Play explosion sound
        if (explosionClip != null)
        {
            audioSource.PlayOneShot(explosionClip, explosionVolume);
        }

        // Hide meteor mesh
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

        // Disable collider
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null) col.enabled = false;

        // Spawn all explosion effects at meteor position
        Vector3 pos = transform.position;

        SpawnCoreBlast(pos);   // Fireball
        SpawnShockwave(pos);   // Expanding ring
        SpawnDebris(pos);      // Flying chunks
        SpawnSmoke(pos);       // Smoke cloud

        // Wait, then tell GameManager victory is ready
        yield return new WaitForSeconds(2f);
        GameManager.Instance?.OnMeteorExploded();
    }

    // particle effects

    // Main fireball explosion
    private void SpawnCoreBlast(Vector3 pos)
    {
        GameObject go = new GameObject("FX_CoreBlast");
        go.transform.position = pos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ApplyMaterial(go);

        Destroy(go, 8f); // Clean up after time

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(CoreRadius * 0.3f, CoreRadius * 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(CoreRadius * 0.3f, CoreRadius * 0.8f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.7f, 0.1f),
            new Color(1f, 0.2f, 0f)
        );

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 150) });

        ps.Play();
    }

    // Expanding shockwave ring
    private void SpawnShockwave(Vector3 pos)
    {
        GameObject go = new GameObject("FX_Shockwave");
        go.transform.position = pos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ApplyMaterial(go);

        Destroy(go, 5f);

        var main = ps.main;
        main.startSpeed = ShockwaveRadius * 0.8f;
        main.startSize = ShockwaveRadius * 0.1f;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 250) });

        ps.Play();
    }

    // Flying debris pieces
    private void SpawnDebris(Vector3 pos)
    {
        GameObject go = new GameObject("FX_Debris");
        go.transform.position = pos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ApplyMaterial(go);

        Destroy(go, 10f);

        var main = ps.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(DebrisRadius * 0.2f, DebrisRadius * 0.9f);

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        ps.Play();
    }

    // Smoke cloud
    private void SpawnSmoke(Vector3 pos)
    {
        GameObject go = new GameObject("FX_Smoke");
        go.transform.position = pos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ApplyMaterial(go);

        Destroy(go, 12f);

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

        ps.Play();
    }

    // screen flash effect
    private IEnumerator FlashScreen()
    {
        if (flashOverlay == null) yield break;

        flashOverlay.gameObject.SetActive(true);

        // Fade in
        float elapsed = 0f;
        while (elapsed < flashInDuration)
        {
            elapsed += Time.deltaTime;
            flashOverlay.color = new Color(1f, 1f, 1f, elapsed / flashInDuration);
            yield return null;
        }

        // Hold white
        flashOverlay.color = Color.white;
        yield return new WaitForSeconds(flashHoldDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < flashOutDuration)
        {
            elapsed += Time.deltaTime;
            flashOverlay.color = new Color(1f, 1f, 1f, 1f - elapsed / flashOutDuration);
            yield return null;
        }

        // Hide overlay
        flashOverlay.color = Color.clear;
        flashOverlay.gameObject.SetActive(false);
    }
}