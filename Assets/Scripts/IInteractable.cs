public interface IInteractable
{
    // This function should return the text shown on screen for interactable objects
    string GetPromptText();

    //  runs when the player interacts (presses E)
    // "player" gives a reference to who interacted (useful if needed)
    void Interact(PlayerInteraction player);
}