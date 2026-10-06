public interface IInteractable
{
    // text shown on screen while the player looks at this object
    string GetPrompt(PlayerBag bag);

    // called when the player presses E while looking at this object
    void Interact(PlayerBag bag);
}
