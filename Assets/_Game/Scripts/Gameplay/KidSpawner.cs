using System;
using UnityEngine;

// keeps the door filled with kids
// spawns a new kid when old leaves

public class KidSpawner : MonoBehaviour
{
    [Header("Prefab & Places")]
    [SerializeField] private Kid kidPrefab;
    [SerializeField] private Transform[] doorSlots; // empty GameObjects marking where kids stand
    [SerializeField] private int activeSlotCount = 1;

    [Header("Orders")]
    [SerializeField] private CandyData[] candyPool; // all 5 candy assets
    [SerializeField] private int minItems = 1; // tutorial: 1-2, phase 1: 2-3, ...
    [SerializeField] private int maxItems = 2;

    [Header("Timer: base + perItem x items")]
    [SerializeField] private float baseTime = 20f;
    [SerializeField] private float timePerItem = 6f;

    [Header("Debug")]
    [SerializeField] private bool logOrders = true;

    public event Action OnKidsChanged; // HUG listens to redraw order panel

    private Kid[] kidsInSlots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        kidsInSlots = new Kid[doorSlots.Length];
        for (int i = 0; i < activeSlotCount && i < doorSlots.Length; i++)
        {
            SpawnKid(i);
        }
    }

    // returns the kid standing in a slot, or null if empty
    public Kid GetKid(int slot)
    {
        if (kidsInSlots == null || slot < 0 || slot >= kidsInSlots.Length) return null;
        return kidsInSlots[slot];
    }

    // later the phase system will call this to make orders bigger as night goes on
    public void SetOrderSize(int min, int max)
    {
        minItems = min;
        maxItems = max;
    }

    private void SpawnKid(int slot)
    {
        Transform spot = doorSlots[slot];

        Kid kid = Instantiate(kidPrefab, spot.position, spot.rotation);

        Order order = Order.Generate(candyPool, minItems, maxItems);
        float time = baseTime + timePerItem * order.TotalCount;
        BagColor color = GameColors.RandomColor();
        kid.Setup(order, color, time);

        if (logOrders)
        {
            string text = "";
            foreach (var pair in order.Items) text += $"{pair.Key.name} x{pair.Value}, ";
            Debug.Log($"New kid ({GameColors.ToName(color)}, slot {slot + 1}): {text.TrimEnd(',', ' ')}");
        }

        // += subscribes the method to the kid's event so when the kid leaves HandleKidLeft runs
        kid.OnLeft += HandleKidLeft;

        kidsInSlots[slot] = kid;
        OnKidsChanged?.Invoke();
    }

    private void HandleKidLeft(Kid kid)
    {
        int slot = Array.IndexOf(kidsInSlots, kid);
        if (slot < 0) return;

        kidsInSlots[slot] = null;
        OnKidsChanged?.Invoke();

        if (slot < activeSlotCount) SpawnKid(slot); // spawn next kid right after
    }
}