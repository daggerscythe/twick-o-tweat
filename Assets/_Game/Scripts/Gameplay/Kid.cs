using System;
using UnityEngine;

// a trick-or-treater, lives on the Kid prefab
// kidspawner creates one and calls setup to give it an order, color, and time limit

public class Kid : MonoBehaviour, IInteractable
{
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private SpeechBubble bubble;

    public Order Order { get; private set; }
    public BagColor KidColor { get; private set; }
    public float TimeLeft { get; private set; }
    public float TotalTime { get; private set; }

    // no time limit on first step of tutorial
    public bool TimerRunning { get; set; } = true;

    // announces 'leaving' so the spawner can send next kid
    public event Action<Kid> OnLeft;

    private bool finished; // stops from leaving twice

    public void Setup(Order order, BagColor color, float totalTime)
    {
        Order = order;
        KidColor = color;
        TotalTime = totalTime;
        TimeLeft = totalTime;

        bodyRenderer.material.color = GameColors.ToColor(color);
        bubble.Show(order);
        bubble.SetTimer(1f);
    }

    // Update is called once per frame
    private void Update()
    {
        if (finished || !TimerRunning) return;

        TimeLeft -= Time.deltaTime;
        bubble.SetTimer(TimeLeft / TotalTime);

        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            finished = true;
            GameManager.Instance.RegisterTimeout(); // 0 points + 1 strike
            Leave();
        }
    }

    public string GetPrompt(PlayerBag bag)
    {
        return bag.HasBag ? "[E] Give bag" : "Grab a bag and fill my order!";
    }

    public void Interact(PlayerBag bag)
    {
        if (finished || !bag.HasBag) return;
        finished = true;

        BagColor bagColor = bag.CurrentColor;
        var candies = bag.HandOver();

        DeliveryResult result = ScoreCalculator.Calculate(Order, candies, bagColor, KidColor, TimeLeft, TotalTime);
        GameManager.Instance.RegisterDelivery(result);
        Leave();
    }

    public void Leave()
    {
        OnLeft?.Invoke(this);
        Destroy(gameObject); // TODO: walk away / fade out animation
    }
}
