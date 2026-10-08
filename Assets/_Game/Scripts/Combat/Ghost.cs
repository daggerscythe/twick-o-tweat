using System;
using UnityEngine;

// a normal ghost: flies straight at the player through walls
// base for ThiefGhost
// lives on the Ghost prefab

public class Ghost : MonoBehaviour
{
    protected enum State { Chasing, KnockedBack, Fleeing }

    [Header("Contact")]
    [SerializeField] protected float contactDamage = 10f;
    [SerializeField] protected float contactRange = 0.9f; // horizontal distance that counts as touching
    [SerializeField] protected float chestHeight = 1.2f; // aims at this height above players feet

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 3f;
    [SerializeField] private float knockbackDuration = 0.4f;

    [Header("Hit flash")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashTime = 0.1f;

    [Header("Glow (lights out)")]
    [SerializeField] private Color glowColor = new Color(0.6f, 0.8f, 1f);
    [SerializeField] private float glowStrength = 1.5f;

    public int Health { get; protected set; }
    public float Speed { get; protected set; }

    public event Action<Ghost> OnRemoved; // the spawner listens to count live ghosts

    protected PlayerHealth target;
    protected State state = State.Chasing;

    private Vector3 knockStart, knockEnd;
    private float knockTimer;

    private Renderer rend;
    private Color normalColor;
    private float flashTimer;

    private Color normalEmission;
    private bool glowing;

    protected virtual void Awake()
    {
        rend = GetComponent<Renderer>();
        normalColor = rend.material.color;
        normalEmission = rend.material.GetColor("_EmissionColor");
    }

    public void Init(PlayerHealth player, int health, float speed)
    {
        target = player;
        Health = health;
        Speed = speed;
    }

    // Update is called once per frame
    private void Update()
    {
        if (target == null || !GameManager.IsPlaying) return;

        switch (state)
        {
            case State.Chasing: Chase(); break;
            case State.KnockedBack: KnockBack(); break;
            case State.Fleeing: Flee(); break;
        }

        // turn the red flash back off
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) rend.material.color = normalColor;
        }

        UpdateGlow();
    }

    // faint glow while the power is out (only changes the material when the state flips)
    private void UpdateGlow()
    {
        if (glowing == LightsOut.Active) return;
        glowing = LightsOut.Active;
        rend.material.SetColor("_EmissionColor", glowing ? glowColor * glowStrength : normalEmission);
    }

    private void Chase()
    {
        Vector3 aimPoint = target.transform.position + Vector3.up * chestHeight;

        transform.position = Vector3.MoveTowards(transform.position, aimPoint, Speed * Time.deltaTime);

        // compare only the horizontal distance
        Vector3 away = transform.position - target.transform.position;
        away.y = 0f;

        if (away.magnitude <= contactRange) OnTouchPlayer(away);
    }

    // what happens on contact
    protected virtual void OnTouchPlayer(Vector3 away)
    {
        target.TakeDamage(contactDamage); // does nothing while green effect is on
        StartKnockback(away);
    }

    protected void StartKnockback(Vector3 away)
    {
        // push ghost out if its on top of player
        if (away.sqrMagnitude < 0.0001f) away = target.transform.forward;

        state = State.KnockedBack;
        knockTimer = 0f;
        knockStart = transform.position;
        knockEnd = transform.position + away.normalized * knockbackDistance;
    }

    private void KnockBack()
    {
        knockTimer += Time.deltaTime;
        float t = Mathf.Clamp01(knockTimer / knockbackDuration); // 0 -> 1 over the knockback
        float eased = 1f - (1f - t) * (1f - t); // ease out: fast at first, then slows down
        transform.position = Vector3.Lerp(knockStart, knockEnd, eased);

        if (t >= 1f) state = State.Chasing;
    }

    // only the thief flees, so the normal ghost does nothing here
    protected virtual void Flee() { }

    // the Gun calls this
    public void TakeHit(int damage)
    {
        if (Health <= 0) return; // already dying

        Health -= damage;
        rend.material.color = hitColor;
        flashTimer = flashTime;

        if (Health <= 0)
        {
            OnKilled();
            Despawn(); // TODO: death sound + poof
        }
    }

    // extra things to do when shot dead; ThiefGhost returns the candy here
    protected virtual void OnKilled() { }

    // tell the spawner, then remove the ghost from the scene
    protected void Despawn()
    {
        OnRemoved?.Invoke(this);
        Destroy(gameObject);
    }
}