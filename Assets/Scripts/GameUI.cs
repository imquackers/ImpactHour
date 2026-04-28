using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    // inspector references
    [Header("UI References")]
    public Text interactionText;     // Text that shows interaction prompts
    public GameObject gameOverPanel; // Panel shown when game ends
    public Text gameOverText;        // Text inside the game over panel

    // Reference to player interaction system
    private PlayerInteraction playerInteraction;

    private void Start()
    {
        // Find the PlayerInteraction script in the scene
        playerInteraction = FindFirstObjectByType<PlayerInteraction>();

        // Make sure game over screen starts hidden
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        // Update UI every frame
        UpdateInteractionPrompt();
    }

    private void UpdateInteractionPrompt()
    {
        // If no UI text assigned, do nothing
        if (interactionText == null) return;

        // Currently clears the text every frame
        //replaced by interaction prompt
        interactionText.text = "";
    }
}