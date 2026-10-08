using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Esc opens/closes the pause menu: Resume, master volume, Restart, Quit, high score
// lives on HUD

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel; // starts hidden
    [SerializeField] private PopupUI popup; // Esc does nothing while a pop-up is open
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider volumeSlider; // 0 to 1
    [SerializeField] private TMP_Text volumeValueText; // "80%"
    [SerializeField] private TMP_Text highScoreText;

    private const string VolumeKey = "MasterVolume";

    public bool IsOpen => panel.activeSelf;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        panel.SetActive(false);

        // load the saved volume and apply it right away, even if the menu is never opened
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeSlider.SetValueWithoutNotify(volume); // move the handle without calling SetVolume
        SetVolume(volume);

        volumeSlider.onValueChanged.AddListener(SetVolume);
        resumeButton.onClick.AddListener(Close);
        restartButton.onClick.AddListener(GameManager.Instance.Restart);
        quitButton.onClick.AddListener(GameManager.Instance.Quit);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (IsOpen) Close();
        else if (!popup.IsOpen && !GameManager.Instance.IsGameOver) Open();
    }

    private void Open()
    {
        panel.SetActive(true);
        highScoreText.text = $"High score: {GameManager.HighScore}";
        GameManager.Instance.Pause();
    }

    private void Close()
    {
        if (!IsOpen) return;
        panel.SetActive(false);
        PlayerPrefs.Save(); // write the volume to disk
        GameManager.Instance.Resume();
    }

    private void SetVolume(float value)
    {
        AudioListener.volume = value; // master volume for every sound in the game
        PlayerPrefs.SetFloat(VolumeKey, value);
        if (volumeValueText != null) volumeValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }
}
