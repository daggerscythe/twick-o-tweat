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
    public event Action<CandyData> OnCandyStolen; // a Thief Ghost took one
    public event Action<CandyData> OnCandyRecovered; // the thief was shot inside the house

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

    // thief removes 1 random candy and returns it
    public CandyData StealRandom()
    {
        if (!HasBag || Contents.Count == 0) return null;

        int index = UnityEngine.Random.Range(0, Contents.Count);
        CandyData candy = Contents[index];
        Contents.RemoveAt(index);

        OnCandyStolen?.Invoke(candy);
        OnBagChanged?.Invoke();
        return candy;
    }

    // the thief was shot in time and the candy goes back into bag
    public void ReturnStolen(CandyData candy)
    {
        if (!HasBag) return; // no bag anymore, the candy is lost
        Contents.Add(candy);
        OnCandyRecovered?.Invoke(candy);
        OnBagChanged?.Invoke();
    }
}