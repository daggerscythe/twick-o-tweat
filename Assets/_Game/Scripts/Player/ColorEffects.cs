using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// color match power-ups
// effects STACK: each color has its own timer
// lives on the Player object

public class ColorEffects : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float boostedSpeed = 8f; // Light Blue (normal is 5)
    [SerializeField] private int boostedDamage = 2; // Yellow (normal is 1)

    [Header("Debug")]
    [SerializeField] private bool debugKeys = true; // 1-4 = trigger an effect

    // the 4 colors in a fixed order, handy for looping
    public static readonly BagColor[] AllColors = { BagColor.LightBlue, BagColor.Yellow, BagColor.Red, BagColor.Green };

    public event Action OnEffectChanged; // an effect started or ended

    // seconds left for every RUNNING effect; a color that isn't in here is off
    private readonly Dictionary<BagColor, float> timeLeft = new Dictionary<BagColor, float>();

    private PlayerController controller;
    private PlayerHealth health;
    private Gun gun;
    private float normalSpeed;
    private int normalDamage;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        health = GetComponent<PlayerHealth>();
        gun = GetComponent<Gun>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        normalSpeed = controller.CurrentSpeed;
        normalDamage = gun.Damage;

        GameManager.Instance.OnDelivery += HandleDelivery;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnDelivery -= HandleDelivery;
    }

    // Update is called once per frame
    private void Update()
    {
        if (debugKeys && Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) Activate(BagColor.LightBlue);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) Activate(BagColor.Yellow);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) Activate(BagColor.Red);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) Activate(BagColor.Green);
        }

        // count down every running effect
        foreach (BagColor c in AllColors)
        {
            if (!timeLeft.TryGetValue(c, out float t)) continue; // not running
            t -= Time.deltaTime;
            if (t <= 0f) EndEffect(c);
            else timeLeft[c] = t;
        }
    }

    public bool IsActive(BagColor color) => timeLeft.ContainsKey(color);

    public float GetTimeLeft(BagColor color) => timeLeft.TryGetValue(color, out float t) ? t : 0f;

    private void HandleDelivery(DeliveryResult r)
    {
        // only given for a perfect order in a matching bag
        if (r.ColorBonus > 0) Activate(r.BagColor);
    }

    public void Activate(BagColor color)
    {
        timeLeft[color] = duration; // starts this color, or restarts it if it's already running; other colors keep going

        switch (color)
        {
            case BagColor.LightBlue: controller.CurrentSpeed = boostedSpeed; break;
            case BagColor.Yellow: gun.Damage = boostedDamage; break;
            case BagColor.Red: health.HealFull(); break; // instant, so a repeat match heals again
            case BagColor.Green: health.Invulnerable = true; break;
        }

        Debug.Log($"Color effect: {EffectName(color)} for {duration}s");
        AudioManager.Play(Sfx.ColorEffect);
        OnEffectChanged?.Invoke();
    }

    private void EndEffect(BagColor color)
    {
        timeLeft.Remove(color);

        // undo only THIS color's effect
        switch (color)
        {
            case BagColor.LightBlue: controller.CurrentSpeed = normalSpeed; break;
            case BagColor.Yellow: gun.Damage = normalDamage; break;
            case BagColor.Green: health.Invulnerable = false; break;
                // Red's heal was instant: nothing to undo
        }

        OnEffectChanged?.Invoke();
    }

    public static string EffectName(BagColor c)
    {
        switch (c)
        {
            case BagColor.LightBlue: return "Speed Boost";
            case BagColor.Yellow: return "Double Damage";
            case BagColor.Red: return "Full Heal";
            case BagColor.Green: return "Ghost Shield";
            default: return "";
        }
    }
}