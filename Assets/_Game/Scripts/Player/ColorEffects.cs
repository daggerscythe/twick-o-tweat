using System;
using UnityEngine;
using UnityEngine.InputSystem;

// one effect at a time: a new match replaces the current one
// lives on the Player object

public class ColorEffects : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float boostedSpeed = 8f; // Light Blue (normal is 5)
    [SerializeField] private int boostedDamage = 2; // Yellow (normal is 1)

    [Header("Debug")]
    [SerializeField] private bool debugKeys = true; // 1-4 = trigger an effect

    public bool HasEffect { get; private set; }
    public BagColor ActiveColor { get; private set; }
    public float TimeLeft { get; private set; }

    public event Action OnEffectChanged; // for sounds / the first-match pop-up 

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

        if (!HasEffect) return;

        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0f) EndEffect();
    }

    private void HandleDelivery(DeliveryResult r)
    {
        // only given for a perfect order in a matching bag
        if (r.ColorBonus > 0) Activate(r.BagColor);
    }

    public void Activate(BagColor color)
    {
        if (HasEffect) EndEffect(); // undo the old one first

        HasEffect = true;
        ActiveColor = color;
        TimeLeft = duration;

        switch (color)
        {
            case BagColor.LightBlue: controller.CurrentSpeed = boostedSpeed; break;
            case BagColor.Yellow: gun.Damage = boostedDamage; break;
            case BagColor.Red: health.HealFull(); break;
            case BagColor.Green: health.Invulnerable = true; break;
        }

        Debug.Log($"Color effect: {EffectName(color)} for {duration}s");
        OnEffectChanged?.Invoke();
    }

    private void EndEffect()
    {
        // put everything back
        controller.CurrentSpeed = normalSpeed;
        gun.Damage = normalDamage;
        health.Invulnerable = false;

        HasEffect = false;
        TimeLeft = 0f;
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
