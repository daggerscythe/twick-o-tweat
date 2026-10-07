using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// a message box that pauses the game
// lives on HUD

public class PopupUI : MonoBehaviour
{
    [SerializeField] private GameObject panel; // starts hidden
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonLabel;
    [SerializeField] private float minShowTime = 0.6f; // seconds before it can close

    public bool IsOpen => panel.activeSelf;

    // one pop up waiting to be shown
    private struct Request
    {
        public string Title;
        public string Body;
        public string ButtonText;
        public Action OnClosed; // runs after the player closes it
    }

    private readonly Queue<Request> queue = new Queue<Request>();
    private Request current;
    private float shownAt;

    private void Awake()
    {
        panel.SetActive(false);
        button.onClick.AddListener(Close);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOpen || Keyboard.current == null) return;

        // keyboard shortcut for the button
        Keyboard kb = Keyboard.current;
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) Close();
    }

    public void Show(string title, string body, string buttonText = "Got it", Action onClosed = null)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        queue.Enqueue(new Request { Title = title, Body = body, ButtonText = buttonText, OnClosed = onClosed });

        if (!IsOpen)
        {
            Display(queue.Dequeue());
            panel.SetActive(true);
            GameManager.Instance.Pause();
        }
    }

    private void Display(Request r)
    {
        current = r;
        titleText.text = r.Title;
        bodyText.text = r.Body;
        buttonLabel.text = r.ButtonText;
        shownAt = Time.unscaledTime; // unscaled = keeps counting while timeScale is 0
    }

    public void Close()
    {
        if (!IsOpen || Time.unscaledTime - shownAt < minShowTime) return;

        Action callback = current.OnClosed;

        if (queue.Count > 0)
        {
            Display(queue.Dequeue()); // next pop up, game stays paused
        }
        else
        {
            panel.SetActive(false);
            GameManager.Instance.Resume();
        }

        callback?.Invoke();
    }
}