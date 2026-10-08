using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Screen overlay: score, strikes, current order + kid timer, interaction prompt, held bag (color + candy count) and a short message after each delivery

public class HUD : MonoBehaviour
{
    [Header("Things to watch")]
    [SerializeField] private PlayerBag bag;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private KidSpawner spawner;

    [Header("Top right")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text strikesText;

    [Header("Order cards (right side), one per door slot")]
    [SerializeField] private OrderCardUI[] orderCards;

    [Header("Center")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float feedbackDuration = 3f;

    [Header("Bottom left")]
    [SerializeField] private Image bagIcon;
    [SerializeField] private TMP_Text bagText;

    [Header("Top left: health")]
    [SerializeField] private PlayerHealth health;
    [SerializeField] private Image healthFill; // Image Type = Filled, Horizontal
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image damageFlash; // full-screen red image, starts invisible
    [SerializeField] private float flashAlpha = 0.35f;
    [SerializeField] private float flashFadeSpeed = 1.5f; // alpha per second

    [Header("Gun")]
    [SerializeField] private Gun gun;
    [SerializeField] private Image crosshair;

    [Header("Color effect")]
    [SerializeField] private ColorEffects effects;
    [SerializeField] private GameObject effectPanel;
    [SerializeField] private Image effectIcon;
    [SerializeField] private TMP_Text effectText;

    private float feedbackHideTime;
    private Color crosshairColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // subscribe to events
        bag.OnBagChanged += RefreshBag;
        bag.OnCandyStolen += ShowStolen;
        bag.OnCandyRecovered += ShowRecovered;
        spawner.OnKidsChanged += RefreshOrder;
        GameManager.Instance.OnScoreChanged += RefreshScore;
        GameManager.Instance.OnDelivery += ShowDelivery;
        GameManager.Instance.OnStrike += ShowStrike;
        health.OnHealthChanged += RefreshHealth;
        health.OnDamaged += FlashDamage;
        damageFlash.color = new Color(1f, 0f, 0f, 0f);
        crosshairColor = crosshair.color;

        // draw once now in case things happened before subscription
        RefreshBag();
        RefreshOrder();
        RefreshScore();
        RefreshHealth();
        feedbackText.text = "";
    }

    // unsubscribe when destroyed
    private void OnDestroy()
    {
        if (bag != null)
        {
            bag.OnBagChanged -= RefreshBag;
            bag.OnCandyStolen -= ShowStolen;
            bag.OnCandyRecovered -= ShowRecovered;
        }
        if (spawner != null) spawner.OnKidsChanged -= RefreshOrder;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= RefreshScore;
            GameManager.Instance.OnDelivery -= ShowDelivery;
            GameManager.Instance.OnStrike -= ShowStrike;
        }
        if (health != null)
        {
            health.OnHealthChanged -= RefreshHealth;
            health.OnDamaged -= FlashDamage;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        promptText.text = interactor.CurrentPrompt;

        if (feedbackText.text != "" && Time.time > feedbackHideTime) feedbackText.text = "";
        UpdateEffect();
        UpdateDamageFlash();
        // faded crosshair = gun is lowered
        crosshair.color = gun.IsLowered ? new Color(crosshairColor.r, crosshairColor.g, crosshairColor.b, 0.25f) : crosshairColor;
    }

    private void RefreshScore()
    {
        scoreText.text = $"Score: {GameManager.Instance.Score}";
        strikesText.text = $"Strikes: {GameManager.Instance.Strikes}/{GameManager.MaxStrikes}";
    }

    private void RefreshBag()
    {
        if (!bag.HasBag)
        {
            bagIcon.enabled = false;
            bagText.text = "No bag";
            return;
        }
        bagIcon.enabled = true;
        bagIcon.color = GameColors.ToColor(bag.CurrentColor);
        bagText.text = $"{GameColors.ToName(bag.CurrentColor)} bag: {bag.Contents.Count} candy";
    }

    // one card per door slot; an empty slot hides its card
    private void RefreshOrder()
    {
        for (int i = 0; i < orderCards.Length; i++)
        {
            orderCards[i].Show(spawner.GetKid(i), i + 1);
        }
    }

    private void RefreshHealth()
    {
        float fraction = health.Current / health.Max;
        healthFill.fillAmount = fraction;
        healthFill.color = Color.Lerp(Color.red, Color.green, fraction); // red when low
        healthText.text = $"{Mathf.CeilToInt(health.Current)} / {health.Max}";
    }

    private void FlashDamage()
    {
        damageFlash.color = new Color(1f, 0f, 0f, flashAlpha);
    }

    private void UpdateEffect()
    {
        if (effectPanel.activeSelf != effects.HasEffect) effectPanel.SetActive(effects.HasEffect);
        if (!effects.HasEffect) return;

        effectIcon.color = GameColors.ToColor(effects.ActiveColor);
        effectText.text = $"{ColorEffects.EffectName(effects.ActiveColor)}  {effects.TimeLeft:0.0}s";
    }

    private void UpdateDamageFlash()
    {
        Color c = damageFlash.color;
        if (c.a <= 0f) return;
        c.a = Mathf.MoveTowards(c.a, 0f, flashFadeSpeed * Time.deltaTime);
        damageFlash.color = c;
    }

    private void ShowDelivery(DeliveryResult r)
    {
        string text = $"+{r.Total}   ({r.Correct} correct, {r.Wrong} wrong, {r.Missing} missing)";
        if (r.SpeedBonus > 0) text += $"\nSpeed +{r.SpeedBonus}";
        if (r.Perfect) text += "   PERFECT +" + r.PerfectBonus;
        if (r.ColorBonus > 0) text += "   COLOR MATCH +" + r.ColorBonus;
        if (!r.Perfect && r.Correct > 0) text += "\nNot perfect: no bonuses";
        ShowFeedback(text);
    }

    private void ShowStrike()
    {
        ShowFeedback($"The kid left unhappy! Strike {GameManager.Instance.Strikes}/{GameManager.MaxStrikes}");
    }

    private void ShowStolen(CandyData candy)
    {
        ShowFeedback($"A Thief Ghost stole your {candy.displayName}! Shoot it before it escapes!");
    }

    private void ShowRecovered(CandyData candy)
    {
        ShowFeedback($"Got the {candy.displayName} back!");
    }

    private void ShowFeedback(string text)
    {
        feedbackText.text = text;
        feedbackHideTime = Time.time + feedbackDuration;
    }
}
