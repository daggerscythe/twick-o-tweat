using UnityEngine;

public class CandyBowl : MonoBehaviour, IInteractable
{
    [SerializeField] private CandyData candy;
    [SerializeField] private SpriteRenderer iconDisplay; // floating icon above the bowl

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // bowl shows its own candy icon automatically
        if (iconDisplay != null && candy != null) iconDisplay.sprite = candy.icon;
    }

    public string GetPrompt(PlayerBag bag)
    {
        return bag.HasBag ? $"[E] Add {candy.displayName}" : "Grab a bag first";
    }

    public void Interact(PlayerBag bag)
    {
        if (bag.HasBag) bag.AddCandy(candy);
    }
}
