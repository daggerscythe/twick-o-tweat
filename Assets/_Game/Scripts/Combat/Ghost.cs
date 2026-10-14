using System;
using UnityEngine;

// a normal ghost: flies straight at the player through walls
// lives on the Ghost prefab

public class Ghost : MonoBehaviour
{
    protected enum State { Chasing, KnockedBack, Fleeing }

    [Header("Contact")]
    [SerializeField] protected float contactDamage = 8f;
    [SerializeField] protected float contactRange = 0.9f; // horizontal distance that counts as touching
    [SerializeField] protected float chestHeight = 1.2f; // aims at this height above players feet

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 3f;
    [SerializeField] private float knockbackDuration = 0.4f;

    [Header("Hit flash")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashTime = 0.1f;
    [SerializeField] private float hitGlowStrength = 2f;

    [Header("Glow (lights out)")]
    [SerializeField] private Color glowColor = new Color(0.6f, 0.8f, 1f);
    [SerializeField] private float glowStrength = 1.5f;

    [Header("House")]
    [SerializeField] protected Vector2 houseHalfSize = new Vector2(7f, 6f); // room is 14x12, centered on 0,0

    [Header("Sound")]
    [SerializeField] private Sfx appearSound = Sfx.GhostAppear; // Ghost_Tank: TankAppear

    [Header("Model")]
    [SerializeField] private Renderer bodyRenderer; // the body mesh on the imported model
    [SerializeField] private int bodyMaterialSlot = 0; // which material slot is the body
    [SerializeField] private float turnSpeed = 10f; // how fast the ghost turns to face you
    
    public int Health { get; protected set; }
    public float Speed { get; protected set; }

    public event Action<Ghost> OnRemoved; // the spawner listens to count live ghosts

    protected PlayerHealth target;
    protected State state = State.Chasing;

    private Vector3 knockStart, knockEnd;
    private float knockTimer;

    private Material bodyMat; // this ghost's own copy of the body material

    private Color normalColor;
    private float flashTimer;

    private Color normalEmission;
    private bool glowing;

    private bool hasAppeared; // the appear sound already played (only once per ghost, even after knockbacks)

    protected virtual void Awake()
    {
        bodyMat = bodyRenderer.materials[bodyMaterialSlot];
        normalColor = bodyMat.color;
        normalEmission = bodyMat.GetColor("_EmissionColor");
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

        FaceTarget();

        CheckAppeared();

        // turn the red flash back off
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                bodyMat.color = normalColor;
                bodyMat.SetColor("_EmissionColor", CurrentEmission());
            }
        }

        UpdateGlow();
    }

    // turn to look at the player (a fleeing thief looks where it's going)
    private void FaceTarget()
    {
        Vector3 look = target.transform.position - transform.position;
        if (state == State.Fleeing) look = -look;
        look.y = 0f; // only turn left/right, never tilt
        if (look.sqrMagnitude < 0.0001f) return;

        Quaternion wanted = Quaternion.LookRotation(look);
        transform.rotation = Quaternion.Slerp(transform.rotation, wanted, turnSpeed * Time.deltaTime);
    }

    // plays the appear sound the first time the ghost floats into the room
    private void CheckAppeared()
    {
        if (hasAppeared || !InsideHouse()) return;
        hasAppeared = true;
        AudioManager.Play(appearSound);
    }

    // true while the ghost's center is inside the walls
    protected bool InsideHouse()
    {
        Vector3 p = transform.position;
        return Mathf.Abs(p.x) < houseHalfSize.x && Mathf.Abs(p.z) < houseHalfSize.y;
    }

    // faint glow while the power is out
    private void UpdateGlow()
    {
        if (glowing == LightsOut.Active) return;
        glowing = LightsOut.Active;
        if (flashTimer <= 0f) bodyMat.SetColor("_EmissionColor", CurrentEmission()); // a running flash resets it when it ends
    }

    // the emission the ghost should have when it isn't flashing
    private Color CurrentEmission()
    {
        return glowing ? glowColor * glowStrength : normalEmission;
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
        target.TakeDamage(contactDamage); // does nothing while green effect or hit cooldown is on
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
        bodyMat.color = hitColor;
        bodyMat.SetColor("_EmissionColor", hitColor * hitGlowStrength);
        flashTimer = flashTime;

        if (Health <= 0)
        {
            AudioManager.Play(Sfx.GhostDeath); // the killing shot plays ONLY the death sound
            OnKilled();
            Despawn();
        }
        else
        {
            AudioManager.Play(Sfx.GhostHit);
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