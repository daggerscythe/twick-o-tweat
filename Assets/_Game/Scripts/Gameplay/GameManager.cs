using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// holds global game state, scores, strikes

// two ways to lose
public enum GameOverReason
{
    Health, // health reached 0
    Strikes // 3 kids left unhappy
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // singleton

    // true while the night is still going
    public static bool IsPlaying => Instance == null || !Instance.IsGameOver;

    public const int MaxStrikes = 3;

    public int Score { get; private set; }
    public int Strikes { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action OnScoreChanged; // score or strikes changed
    public event Action<DeliveryResult> OnDelivery; // a kid got a bag
    public event Action OnStrike; // a kid left unhappy
    public event Action<GameOverReason> OnGameOver; // the night ended early

    private void Awake()
    {
        // remove if second GM exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // unfreeze timescale when a scene reloads
        Time.timeScale = 1f;
    }

    public void RegisterDelivery(DeliveryResult result)
    {
        if (IsGameOver) return;

        Score += result.Total;
        OnDelivery?.Invoke(result);
        OnScoreChanged?.Invoke();
    }

    public void RegisterTimeout()
    {
        if (IsGameOver) return;

        Strikes++;
        OnStrike?.Invoke();
        OnScoreChanged?.Invoke();

        if (Strikes >= MaxStrikes) EndGame(GameOverReason.Strikes);
    }

    public void EndGame(GameOverReason reason)
    {
        if (IsGameOver) return; // only end once
        IsGameOver = true;

        Time.timeScale = 0f; // freeze everything that uses deltaTime

        // free the mouse so the player can click Restart
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"Game over: {reason}");
        OnGameOver?.Invoke(reason);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // reload scene from scratch
    }
}
