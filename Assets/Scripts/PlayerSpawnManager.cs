using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    // settings
    [Header("Spawn Settings")]
    public Transform spawnPoint;     // Where the player should spawn
    public GameObject playerObject;  // Reference to the player

    private void Start()
    {
        // Automatically spawn player when scene starts
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        // If no player is assigned in Inspector, try to find one by tag
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        // Make sure both spawn point and player exist
        if (spawnPoint != null && playerObject != null)
        {
            // Get position & rotation from spawn point
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            // Check if player uses CharacterController
            CharacterController characterController = playerObject.GetComponent<CharacterController>();

            if (characterController != null)
            {
                // Disable controller before moving player
                // prevents physics / teleport issues
                characterController.enabled = false;

                playerObject.transform.position = spawnPosition;
                playerObject.transform.rotation = spawnRotation;

                // Re-enable controller after moving
                characterController.enabled = true;
            }
            else
            {
                // If no CharacterController, just move normally
                playerObject.transform.position = spawnPosition;
                playerObject.transform.rotation = spawnRotation;
            }

            Debug.Log($"Player spawned at: {spawnPosition}");
        }
        else
        {
            // Warning if something is missing
            Debug.LogWarning("PlayerSpawnManager: Missing spawn point or player object");
        }
    }
}