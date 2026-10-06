using UnityEngine;

// empties the whole bag in case of error

public class TrashCan : MonoBehaviour, IInteractable
{
    public string GetPrompt(PlayerBag bag)
    {
        if (!bag.HasBag) return "Trash can";
        if (bag.Contents.Count == 0) return "Bag is already empty";
        return "[E] Empty bag";
    }

    public void Interact(PlayerBag bag)
    {
        if (bag.HasBag) bag.EmptyBag();
    }
}
