using TMPro;
using UnityEngine;
using UnityEngine.UI;

// shows the Game Over panel when GameManager says the night ended early
// lives on the HUD

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel; // starts hidden
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Button restartButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        panel.SetActive(false);
        GameManager.Instance.OnGameOver += Show;

        restartButton.onClick.AddListener(GameManager.Instance.Restart);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnGameOver -= Show;
    }

    private void Show(GameOverReason reason)
    {
        panel.SetActive(true);
        reasonText.text = reason == GameOverReason.Health
            ? "The ghosts got you!"
            : "Three kids went home without candy!";
        finalScoreText.text = $"Final score: {GameManager.Instance.Score}";
    }
}
