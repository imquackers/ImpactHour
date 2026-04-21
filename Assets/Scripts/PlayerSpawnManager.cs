using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public GameObject playerObject;

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (spawnPoint != null && playerObject != null)
        {
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            CharacterController characterController = playerObject.GetComponent<CharacterController>();
            
            if (characterController != null)
            {
                characterController.enabled = false;
                playerObject.transform.position = spawnPosition;
                playerObject.transform.rotation = spawnRotation;
                characterController.enabled = true;
            }
            else
            {
                playerObject.transform.position = spawnPosition;
                playerObject.transform.rotation = spawnRotation;
            }

            Debug.Log($"Player spawned at: {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("PlayerSpawnManager: Missing spawn point or player object!");
        }
    }
}
