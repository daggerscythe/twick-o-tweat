using System;
using UnityEngine;
using UnityEngine.InputSystem;

// the players health: takes damage, regenerates after a short break, ends the game at 0
// lives on the Player object

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float regenDelay = 4f; // seconds without damage before regen starts
    [SerializeField] private float regenPerSecond = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugKeys = true; // H = take 10 damage TODO: turn off before build

    public float Current { get; private set; }
    public float Max => maxHealth; // => makes a read only property that returns maxHealth
    public bool Invulnerable { get; set; } // green effect turns this on

    public event Action OnHealthChanged; // HUD redraws the bar
    public event Action OnDamaged; // HUD flashes the screen red

    private float lastDamageTime = -999f; // so regen isnt blocked at the start

    private void Awake()
    {
        Current = maxHealth;
    }

    // Update is called once per frame
    private void Update()
    {
        if (debugKeys && Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) TakeDamage(10f);

        // regen: only when hurt, alive, and it's been regenDelay seconds since the last hit
        if (Current <= 0f || Current >= maxHealth) return;
        if (Time.time - lastDamageTime < regenDelay) return;

        Current = Mathf.Min(maxHealth, Current + regenPerSecond * Time.deltaTime);
        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(float amount)
    {
        if (Invulnerable || Current <= 0f) return;

        Current = Mathf.Max(0f, Current - amount);
        lastDamageTime = Time.time; // restarts the regen countdown
        OnDamaged?.Invoke();
        OnHealthChanged?.Invoke();

        if (Current <= 0f) GameManager.Instance.EndGame(GameOverReason.Health);
    }

    // Red effect
    public void HealFull()
    {
        Current = maxHealth;
        OnHealthChanged?.Invoke();
    }
}