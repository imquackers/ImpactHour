using UnityEngine;

public class MouseLook : MonoBehaviour
{
    // inspector settings
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f; // How fast the camera moves

    [Header("References")]
    public Transform playerBody; // The player object (rotates left/right)

    // internal variables
    private float xRotation = 0f; // Stores up/down rotation

    private void Start()
    {
        // Lock the mouse to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Get mouse movement input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate camera up/down (invert because mouse Y is reversed)
        xRotation -= mouseY;

        // Clamp rotation so you can't look too far up/down
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply vertical rotation to the camera ONLY
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player left/right (horizontal movement)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}