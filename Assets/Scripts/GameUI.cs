using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    public Text interactionText;
    public GameObject gameOverPanel;
    public Text gameOverText;

    private PlayerInteraction playerInteraction;

    private void Start()
    {
        playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateInteractionPrompt();
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionText == null) return;

        interactionText.text = "";
    }
}
