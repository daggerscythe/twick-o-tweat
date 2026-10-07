using System;
using UnityEngine;

// a normal ghost: flies straight at the player through walls
// lives on the Ghost prefab

public class Ghost : MonoBehaviour
{
    private enum State { Chasing, KnockedBack }

    [Header("Contact")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactRange = 0.9f; // horizontal distance that counts as touching
    [SerializeField] private float chestHeight = 1.2f; // aims at this height above players feet

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 3f;
    [SerializeField] private float knockbackDuration = 0.4f;

    [Header("Hit flash")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashTime = 0.1f;

    public int Health { get; private set; }
    public float Speed { get; private set; }

    public event Action<Ghost> OnDied; // the spawner listens so it can count live ghosts

    private PlayerHealth target;
    private State state = State.Chasing;
    private Vector3 knockStart, knockEnd;
    private float knockTimer;

    private Renderer rend;
    private Color normalColor;
    private float flashTimer;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        normalColor = rend.material.color;
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
        if (target == null) return;

        if (state == State.Chasing) Chase();
        else KnockBack();

        // turn the red flash back off
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) rend.material.color = normalColor;
        }
    }

    private void Chase()
    {
        Vector3 aimPoint = target.transform.position + Vector3.up * chestHeight;

        transform.position = Vector3.MoveTowards(transform.position, aimPoint, Speed * Time.deltaTime);

        // compare only the horizontal distance
        Vector3 away = transform.position - target.transform.position;
        away.y = 0f;

        if (away.magnitude <= contactRange)
        {
            target.TakeDamage(contactDamage); // does nothing while green effect is on
            StartKnockback(away);
        }
    }

    private void StartKnockback(Vector3 away)
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
        float eased = 1f - (1f - t) * (1f - t); // ease-out: fast at first, then slows down
        transform.position = Vector3.Lerp(knockStart, knockEnd, eased);

        if (t >= 1f) state = State.Chasing;
    }

    // the Gun calls this
    public void TakeHit(int damage)
    {
        if (Health <= 0) return; // already dying

        Health -= damage;
        rend.material.color = hitColor;
        flashTimer = flashTime;

        if (Health <= 0)
        {
            OnDied?.Invoke(this);
            Destroy(gameObject); // TODO: death sound + poof
        }
    }
}
