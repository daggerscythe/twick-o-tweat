using System;
using UnityEngine;

// keeps the open door slots filled with kids
// spawns a new kid when old leaves
// NightDirector opens slot 2 in Phase 3 and makes orders bigger each phase

public class KidSpawner : MonoBehaviour
{
    [Header("Prefab & Places")]
    [SerializeField] private Kid kidPrefab;
    [SerializeField] private Transform[] doorSlots; // empty GameObjects marking where kids stand
    [SerializeField] private int activeSlotCount = 1; // how many slots are open at the start

    [Header("Orders")]
    [SerializeField] private CandyData[] candyPool; // all 5 candy assets
    [SerializeField] private int minItems = 1; // tutorial: 1-2, phase 1: 2-3, ...
    [SerializeField] private int maxItems = 2;

    [Header("Timer: base + perItem x items")]
    [SerializeField] private float baseTime = 20f;
    [SerializeField] private float timePerItem = 6f;

    [Header("Debug")]
    [SerializeField] private bool logOrders = true;

    public event Action OnKidsChanged; // HUD listens to redraw the order cards
    public event Action<Kid> OnKidSpawned; // NightDirector listens (turns the timer off in tutorial step 1)

    public int ActiveSlotCount => activeSlotCount;

    private Kid[] kidsInSlots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        kidsInSlots = new Kid[doorSlots.Length];
        SetActiveSlots(activeSlotCount);
    }

    // returns the kid standing in a slot, or null if empty
    public Kid GetKid(int slot)
    {
        if (kidsInSlots == null || slot < 0 || slot >= kidsInSlots.Length) return null;
        return kidsInSlots[slot];
    }

    // the phase system calls this to make orders bigger as night goes on
    public void SetOrderSize(int min, int max)
    {
        minItems = min;
        maxItems = max;
    }

    // open more door slots; every open slot without a kid gets one right away
    public void SetActiveSlots(int count)
    {
        activeSlotCount = Mathf.Clamp(count, 0, doorSlots.Length);
        if (kidsInSlots == null) return; // Start hasn't run yet; it will spawn them

        for (int i = 0; i < activeSlotCount; i++)
        {
            if (kidsInSlots[i] == null) SpawnKid(i);
        }
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
        OnKidSpawned?.Invoke(kid);
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
