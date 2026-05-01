using UnityEngine;

// Moves the meteor from MeteorWaypoint toward MeteorWaypoint2 over the level timer.
// When BeginFinalApproach() is called (timer hits 0), it switches to direct velocity
// flight so it physically travels to Earth and triggers a collision.
public class MeteorMover : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform startWaypoint;
    public Transform endWaypoint;

    [Header("Rush Settings")]
    [Tooltip("How many seconds before the end the meteor starts its rapid acceleration.")]
    public float finalRushSeconds = 15f;
    [Tooltip("Fraction of total travel covered BEFORE the final rush (0–1).")]
    [Range(0f, 0.99f)]
    public float slowPhaseCoverage = 0.35f;

    [Header("Final Approach")]
    [Tooltip("Speed in world units per second once the timer reaches 0.")]
    public float approachSpeed = 800f;

    [Header("Rotation")]
    public Vector3 rotationSpeed = new Vector3(4f, 11f, 7f);
    public float wobbleAmount = 2.5f;
    public float wobbleFrequency = 0.4f;

    private Vector3 startPos;
    private Vector3 endPos;
    private float wobbleTimer;
    private bool finalApproachActive = false;

    private void Start()
    {
        if (startWaypoint != null)
            startPos = startWaypoint.position;

        if (endWaypoint != null)
            endPos = endWaypoint.position;

        transform.position = startPos;
    }

    private void Update()
    {
        if (finalApproachActive)
            FlyTowardEarth();
        else
            UpdateTimerPosition();

        UpdateRotation();
    }

    // Called by GameManager when the timer reaches 0.
    // Switches from lerp-based movement to direct velocity flight toward Earth.
    public void BeginFinalApproach()
    {
        finalApproachActive = true;
    }

    private void UpdateTimerPosition()
    {
        if (GameManager.Instance == null) return;

        float total = GameManager.Instance.GetTotalTime();
        float elapsed = GameManager.Instance.GetElapsedTime();
        float remaining = total - elapsed;

        if (total <= 0f) return;

        float t;

        if (remaining >= finalRushSeconds)
        {
            float slowDuration = total - finalRushSeconds;
            float slowElapsed = Mathf.Clamp(elapsed, 0f, slowDuration);
            float slowT = (slowDuration > 0f) ? slowElapsed / slowDuration : 0f;
            t = slowPhaseCoverage * (slowT * slowT);
        }
        else
        {
            float rushT = 1f - Mathf.Clamp01(remaining / finalRushSeconds);
            float eased = rushT * rushT * rushT;
            t = Mathf.Lerp(slowPhaseCoverage, 1f, eased);
        }

        transform.position = Vector3.Lerp(startPos, endPos, t);
    }

    private void FlyTowardEarth()
    {
        if (endWaypoint == null) return;

        Vector3 direction = (endWaypoint.position - transform.position).normalized;
        transform.position += direction * approachSpeed * Time.deltaTime;
    }

    private void UpdateRotation()
    {
        wobbleTimer += Time.deltaTime;

        Vector3 spin = rotationSpeed * Time.deltaTime;
        float wobble = Mathf.Sin(wobbleTimer * wobbleFrequency * Mathf.PI * 2f) * wobbleAmount;
        spin.x += wobble * Time.deltaTime;

        transform.Rotate(spin, Space.Self);
    }
}
