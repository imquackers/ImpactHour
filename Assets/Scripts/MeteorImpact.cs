using UnityEngine;

// Detects when the meteor collides with the Earth trigger and notifies GameManager.
[RequireComponent(typeof(Collider))]
public class MeteorImpact : MonoBehaviour
{
    private bool hasImpacted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasImpacted) return;

        if (other.CompareTag("Earth"))
        {
            hasImpacted = true;
            GameManager.Instance?.OnMeteorImpact();
        }
    }
}
