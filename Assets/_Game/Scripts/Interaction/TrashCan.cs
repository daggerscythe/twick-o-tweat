using UnityEngine;

// empties the whole bag in case of error

public class TrashCan : MonoBehaviour, IInteractable
{
    private TrashCanLid lid;

    private void Awake()
    {
        lid = GetComponent<TrashCanLid>();
    }

    public string GetPrompt(PlayerBag bag)
    {
        if (!bag.HasBag) return "Trash can";
        if (bag.Contents.Count == 0) return "Bag is already empty";
        return "[E] Empty bag";
    }

    public void Interact(PlayerBag bag)
    {
        if (!bag.HasBag || bag.Contents.Count == 0) return; // nothing to dump: no sound, no lid
        bag.EmptyBag();
        AudioManager.Play(Sfx.TrashDump);
        if (lid != null) lid.Play();
    }
}
