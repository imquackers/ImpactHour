using UnityEngine;

/// Slowly rotates the Earth on its tilted axis.
public class EarthRotate : MonoBehaviour
{
    [Tooltip("Degrees per second. Earth's real-day equivalent at game scale.")]
    public float rotationSpeed = 2f;

    [Tooltip("Axis of rotation in local space. Earth is tilted ~23.5 degrees.")]
    public Vector3 rotationAxis = new Vector3(0f, 1f, 0f);

    private void Update()
    {
        transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
