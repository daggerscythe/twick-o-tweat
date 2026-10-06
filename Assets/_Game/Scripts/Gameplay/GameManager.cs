using System;
using UnityEngine;

// holds global game state, scores, strikes

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // singleton

    public const int MaxStrikes = 3;

    public int Score { get; private set; }
    public int Strikes { get; private set; }

    public event Action OnScoreChanged; // score or strikes changed
    public event Action<DeliveryResult> OnDelivery; // a kid got a bag
    public event Action OnStrike; // a kid left unhappy

    private void Awake()
    {
        // remove if second GM exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterDelivery(DeliveryResult result)
    {
        Score += result.Total;
        OnDelivery?.Invoke(result);
        OnScoreChanged?.Invoke();
    }

    public void RegisterTimeout()
    {
        Strikes++;
        OnStrike?.Invoke();
        OnScoreChanged?.Invoke();

        if (Strikes >= MaxStrikes)
        {
            // TODO: add game over
            Debug.Log("3 strikes - Game Over (screen not built yet)");
        }
    }
}
