using UnityEngine;

public class BagPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private BagColor color;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // paint the bag w the right color
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = GameColors.ToColor(color);
    }

    public string GetPrompt(PlayerBag bag)
    {
        string colorName = GameColors.ToName(color);
        if (bag.HasBag && bag.Contents.Count > 0) return $"[E] Swap for {colorName} bag (empties current)";
        return $"[E] Grab {colorName} bag";
    }

    public void Interact(PlayerBag bag)
    {
        bag.GrabBag(color);
    }
}
