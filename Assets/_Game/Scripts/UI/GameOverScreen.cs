using TMPro;
using UnityEngine;
using UnityEngine.UI;

// the end-of-night screen for both endings:
// win (9 PM) = "You survived!" + pumpkin rating, lose = "Game Over" + why
// always shows final score, high score, Restart and Quit
// lives on the HUD

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel; // starts hidden
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Rating (win only)")]
    [SerializeField] private GameObject ratingRow;
    [SerializeField] private Image[] pumpkins; // 3 images, left to right
    [SerializeField] private Color pumpkinOn = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private Color pumpkinOff = new Color(0.25f, 0.25f, 0.25f, 0.6f);
    [SerializeField] private int twoPumpkinScore = 4000; // TBD in playtesting
    [SerializeField] private int threePumpkinScore = 7000; // TBD in playtesting

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        panel.SetActive(false);
        GameManager.Instance.OnGameOver += Show;

        restartButton.onClick.AddListener(GameManager.Instance.Restart);
        quitButton.onClick.AddListener(GameManager.Instance.Quit);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnGameOver -= Show;
    }

    private void Show(GameOverReason reason)
    {
        int score = GameManager.Instance.Score;
        bool won = reason == GameOverReason.Survived;

        panel.SetActive(true);
        titleText.text = won ? "You survived the night!" : "Game Over";

        switch (reason)
        {
            case GameOverReason.Survived: reasonText.text = "It's 9 PM. The last kid waves goodbye and the ghosts drift back into the walls."; break;
            case GameOverReason.Health: reasonText.text = "The ghosts got you!"; break;
            case GameOverReason.Strikes: reasonText.text = "Three kids went home without candy!"; break;
        }

        finalScoreText.text = $"Final score: {score}";
        highScoreText.text = GameManager.Instance.NewHighScore
            ? "NEW HIGH SCORE!"
            : $"High score: {GameManager.HighScore}";

        // pumpkins only for finishing the night
        ratingRow.SetActive(won);
        if (won)
        {
            int count = 1; // finishing = 1 pumpkin
            if (score >= twoPumpkinScore) count = 2;
            if (score >= threePumpkinScore) count = 3;

            for (int i = 0; i < pumpkins.Length; i++)
            {
                pumpkins[i].color = i < count ? pumpkinOn : pumpkinOff;
            }
        }
    }
}