using System;
using System.Collections.Generic;
using UnityEngine;

// the bag the palyer is holding, lives on the player object

public class PlayerBag : MonoBehaviour
{
    public bool HasBag { get; private set; }
    public BagColor CurrentColor { get; private set; }
    public List<CandyData> Contents { get; } = new List<CandyData>();

    public event Action OnBagChanged; // HUD subscribes to this

    // grabbing a bag always gives a fresh, empty one
    public void GrabBag(BagColor color)
    {
        HasBag = true;
        CurrentColor = color;
        Contents.Clear();
        OnBagChanged?.Invoke(); // ?. will only call it if somebody is listening
    }

    public void AddCandy(CandyData candy)
    {
        if (!HasBag) return;
        Contents.Add(candy);
        OnBagChanged?.Invoke();
    }

    // trash can empties the bag but you keep holding it in the same color
    public void EmptyBag()
    {
        Contents.Clear();
        OnBagChanged?.Invoke();
    }

    // delivery gives away a COPY of the contents and leaves the player without a bag
    public List<CandyData> HandOver()
    {
        List<CandyData> given = new List<CandyData>(Contents);
        Contents.Clear();
        HasBag = false;
        OnBagChanged?.Invoke();
        return given;
    }
}
