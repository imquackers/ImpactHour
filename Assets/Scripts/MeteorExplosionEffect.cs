using System.Collections;
using UnityEngine;

/// Spawned on the Meteor GameObject when all puzzles are solved.
/// Creates layered particle systems to simulate a mid-space explosion,
/// hides the meteor mesh, then notifies GameManager to begin the victory fade.
[RequireComponent(typeof(AudioSource))]
public class MeteorExplosionEffect : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip explosionClip;
    [Tooltip("Volume of the explosion sound.")]
    public float explosionVolume = 1f;

    [Header("Flash")]
    [Tooltip("UI Image used to flash the screen white on detonation.")]
    public UnityEngine.UI.Image flashOverlay;
    public float flashInDuration  = 0.15f;
    public float flashHoldDuration = 0.2f;
    public float flashOutDuration  = 0.6f;

    // Each particle layer: scale relative to meteor size
    private const float CoreRadius      = 500f;
    private const float ShockwaveRadius = 1200f;
    private const float DebrisRadius    = 900f;
    private const float SmokeRadius     = 700f;

    private AudioSource audioSource;
    private bool hasExploded = false;

    // Shared URP-compatible particle material, built once and reused across all layers.
    private Material particleMaterial;

    private static readonly string URPParticleShader = "Universal Render Pipeline/Particles/Unlit";
    private static readonly string FallbackShader    = "Particles/Standard Unlit";

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        BuildParticleMaterial();
    }

    private void BuildParticleMaterial()
    {
        Shader shader = Shader.Find(URPParticleShader) ?? Shader.Find(FallbackShader);
        if (shader == null)
        {
            Debug.LogWarning("[MeteorExplosionEffect] Could not find a particle shader — particles may appear magenta.");
            return;
        }

        particleMaterial = new Material(shader);
        // White base so particle vertex colours are applied without tinting
        particleMaterial.SetColor("_BaseColor", Color.white);
    }

    /// Assigns the shared particle material to a ParticleSystemRenderer.
    private void ApplyMaterial(GameObject go)
    {
        if (particleMaterial == null) return;
        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        if (r != null)
        {
            r.material         = particleMaterial;
            r.trailMaterial    = particleMaterial;
            r.renderMode       = ParticleSystemRenderMode.Billboard;
        }
    }

    /// Called by GameManager when all puzzles are solved.
    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        // ── 1. Flash screen white ─────────────────────────────────────────────
        if (flashOverlay != null)
            StartCoroutine(FlashScreen());

        // ── 2. Play sound ─────────────────────────────────────────────────────
        if (explosionClip != null)
        {
            audioSource.PlayOneShot(explosionClip, explosionVolume);
        }

        // ── 3. Hide the meteor mesh ───────────────────────────────────────────
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

        // Disable collider so nothing reacts to the now-invisible rock
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null) col.enabled = false;

        // ── 4. Spawn all particle layers at the meteor's position ─────────────
        Vector3 pos = transform.position;

        SpawnCoreBlast(pos);
        SpawnShockwave(pos);
        SpawnDebris(pos);
        SpawnSmoke(pos);

        // ── 5. Wait then notify GameManager to do the victory fade ────────────
        yield return new WaitForSeconds(2f);
        GameManager.Instance?.OnMeteorExploded();
    }

    // ── Particle builders ─────────────────────────────────────────────────────

    /// Bright orange/yellow fireball core burst.
    private void SpawnCoreBlast(Vector3 pos)
    {
        GameObject go = new GameObject("FX_CoreBlast");
        go.transform.position = pos;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ApplyMaterial(go);
        Destroy(go, 8f);

        var main = ps.main;
        main.duration           = 0.5f;
        main.loop               = false;
        main.startLifetime      = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startSpeed         = new ParticleSystem.MinMaxCurve(CoreRadius * 0.3f, CoreRadius * 1.2f);
        main.startSize          = new ParticleSystem.MinMaxCurve(CoreRadius * 0.3f, CoreRadius * 0.8f);
        main.startColor         = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.7f, 0.1f, 1f),
            new Color(1f, 0.2f, 0.0f, 0.8f));
        main.gravityModifier    = 0f;
        main.simulationSpace    = ParticleSystemSimulationSpace.World;
        main.maxParticles       = 200;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 150) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = CoreRadius * 0.1f;

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled  = true;
        velocityOverLifetime.radial   = new ParticleSystem.MinMaxCurve(-CoreRadius * 0.05f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f), new GradientColorKey(new Color(0.4f, 0.1f, 0f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(g);

        ps.Play();
    }

    /// Thin expanding ring shockwave.
    private void SpawnShockwave(Vector3 pos)
    {
        GameObject go = new GameObject("FX_Shockwave");
        go.transform.position = pos;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ApplyMaterial(go);
        Destroy(go, 5f);

        var main = ps.main;
        main.duration        = 0.05f;
        main.loop            = false;
        main.startLifetime   = 1.2f;
        main.startSpeed      = ShockwaveRadius * 0.8f;
        main.startSize       = new ParticleSystem.MinMaxCurve(ShockwaveRadius * 0.05f, ShockwaveRadius * 0.15f);
        main.startColor      = new Color(1f, 0.85f, 0.5f, 0.7f);
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 300;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 250) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = ShockwaveRadius * 0.05f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.4f, 0f), 0.4f), new GradientColorKey(Color.grey, 1f) },
            new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(g);

        ps.Play();
    }

    /// Rocky debris chunks flying outward.
    private void SpawnDebris(Vector3 pos)
    {
        GameObject go = new GameObject("FX_Debris");
        go.transform.position = pos;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ApplyMaterial(go);
        Destroy(go, 10f);

        var main = ps.main;
        main.duration        = 0.3f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(DebrisRadius * 0.2f, DebrisRadius * 0.9f);
        main.startSize       = new ParticleSystem.MinMaxCurve(DebrisRadius * 0.03f, DebrisRadius * 0.12f);
        main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(0.55f, 0.35f, 0.15f),
            new Color(0.8f,  0.5f,  0.2f));
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 80;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = DebrisRadius * 0.05f;

        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z       = new ParticleSystem.MinMaxCurve(-180f, 180f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(0.9f, 0.6f, 0.1f), 0f), new GradientColorKey(new Color(0.4f, 0.25f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(g);

        ps.Play();
    }

    /// Dark billowing smoke cloud that lingers.
    private void SpawnSmoke(Vector3 pos)
    {
        GameObject go = new GameObject("FX_Smoke");
        go.transform.position = pos;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ApplyMaterial(go);
        Destroy(go, 12f);

        var main = ps.main;
        main.duration        = 1f;
        main.loop            = false;
        main.startDelay      = 0.3f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(SmokeRadius * 0.05f, SmokeRadius * 0.25f);
        main.startSize       = new ParticleSystem.MinMaxCurve(SmokeRadius * 0.4f, SmokeRadius);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(0.15f, 0.1f, 0.08f, 0.6f),
            new Color(0.4f,  0.3f, 0.25f, 0.4f));
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 60;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = SmokeRadius * 0.2f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(0.5f, 0.3f, 0.1f), 0f), new GradientColorKey(new Color(0.15f, 0.1f, 0.08f), 0.3f), new GradientColorKey(Color.black, 1f) },
            new[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0.4f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(g);

        ps.Play();
    }

    // ── Screen flash ──────────────────────────────────────────────────────────

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

        // Hold
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

        flashOverlay.color = Color.clear;
        flashOverlay.gameObject.SetActive(false);
    }
}
