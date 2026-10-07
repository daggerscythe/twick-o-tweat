using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// holds global game state, scores, strikes

// two ways to lose
public enum GameOverReason
{
    Health, // health reached 0
    Strikes, // 3 kids left unhappy
    Survived  // made it to 9 PM (the win)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // singleton

    // true while the night is still going
    public static bool IsPlaying => Instance == null || (!Instance.IsGameOver && !Instance.IsPaused);

    public const int MaxStrikes = 3;
    private const string HighScoreKey = "HighScore";

    // best score ever saved on disk
    public static int HighScore => PlayerPrefs.GetInt(HighScoreKey, 0);

    public int Score { get; private set; }
    public int Strikes { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused => pauseRequests > 0;
    public bool NewHighScore { get; private set; } // this night beat the old high score

    // tutorial: a kid timing out does not give a strike
    public bool PracticeMode { get; set; }

    public event Action OnScoreChanged; // score or strikes changed
    public event Action<DeliveryResult> OnDelivery; // a kid got a bag
    public event Action OnStrike; // a kid left unhappy
    public event Action<GameOverReason> OnGameOver; // the night ended early

    private int pauseRequests; // how many things want the game paused right now

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

    // freeze the game and free the mouse
    public void Pause()
    {
        pauseRequests++;
        Time.timeScale = 0f;
        SetCursorFree(true);
    }

    // undo one pause the game only runs again when nobody wants it paused
    public void Resume()
    {
        if (pauseRequests == 0) return;
        pauseRequests--;

        if (pauseRequests > 0 || IsGameOver) return;
        Time.timeScale = 1f;
        SetCursorFree(false);
    }

    public void EndGame(GameOverReason reason)
    {
        if (IsGameOver) return; // only end once
        IsGameOver = true;

        Time.timeScale = 0f; // freeze everything that uses deltaTime
        SetCursorFree(true);

        // save a new best score
        if (Score > HighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, Score);
            PlayerPrefs.Save();
            NewHighScore = true;
        }

        Debug.Log($"Night over: {reason}, score {Score}");
        OnGameOver?.Invoke(reason);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // reload scene from scratch
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Application.Quit does nothing in the editor
#else
        Application.Quit();
#endif
    }

    private static void SetCursorFree(bool free)
    {
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
    }
}
