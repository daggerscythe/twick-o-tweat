using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

// spawns ghosts outside the house every few seconds
// picks Normal, Thief or Tank
// lives on Managers

public enum GhostType { Normal, Thief, Tank }

public class GhostSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [FormerlySerializedAs("ghostPrefab")] // keeps the prefab you already dragged in under the old name
    [SerializeField] private Ghost normalPrefab;
    [SerializeField] private Ghost thiefPrefab;
    [SerializeField] private Ghost tankPrefab;

    [Header("References")]
    [SerializeField] private PlayerHealth player;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxAlive = 5;

    [Header("Where (outside the house)")]
    [SerializeField] private Vector2 roomHalfSize = new Vector2(7f, 6f); // room is 14x12, centered on 0,0
    [SerializeField] private float spawnMargin = 3f; // meters outside the walls
    [SerializeField] private float spawnHeight = 1.25f;

    [Header("Scaling: every scoreStep points")]
    [SerializeField] private int scoreStep = 500;
    [SerializeField] private int baseHealth = 3;
    [SerializeField] private int maxHealth = 8;
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float speedPerStep = 0.25f;
    [SerializeField] private float maxSpeed = 4f; // slower than player

    [Header("Special ghosts")]
    [SerializeField, Range(0f, 1f)] private float thiefChance = 0.3f; // per spawn, once thieves are on
    [SerializeField, Range(0f, 1f)] private float tankChance = 0.25f; // per spawn, only while no tank is alive
    [SerializeField] private float thiefSpeedBonus = 1f;
    [SerializeField] private int tankHealthMultiplier = 3;
    [SerializeField] private float tankSpeedMultiplier = 0.6f;

    [Header("Debug")]
    [SerializeField] private bool logSpawns = true;
    [SerializeField] private bool debugKeys = true; // G = normal T = thief Y = tank

    public bool SpawningEnabled { get; set; } = true;
    public bool ThievesEnabled { get; set; }
    public bool TanksEnabled { get; set; }
    public int AliveCount => alive.Count;

    private readonly List<Ghost> alive = new List<Ghost>();
    private readonly Queue<GhostType> forcedNext = new Queue<GhostType>(); // "the next spawn must be a thief"
    private Ghost aliveTank; // null when no tank is in the scene
    private float timer;

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.IsPlaying) return;

        if (debugKeys && Keyboard.current != null)
        {
            if (Keyboard.current.gKey.wasPressedThisFrame) Spawn(GhostType.Normal);
            if (Keyboard.current.tKey.wasPressedThisFrame) Spawn(GhostType.Thief);
            if (Keyboard.current.yKey.wasPressedThisFrame) Spawn(GhostType.Tank);
        }

        if (!SpawningEnabled) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (alive.Count < maxAlive) Spawn(PickType());
        }
    }

    // so a new ghost type shows up right after its pop up
    public void ForceNext(GhostType type)
    {
        forcedNext.Enqueue(type);
    }

    // tutorial step 2: one weak ghost
    public Ghost SpawnTutorialGhost()
    {
        return Spawn(GhostType.Normal, 1);
    }

    private GhostType PickType()
    {
        if (forcedNext.Count > 0) return forcedNext.Dequeue();
        if (TanksEnabled && aliveTank == null && Random.value < tankChance) return GhostType.Tank;
        if (ThievesEnabled && Random.value < thiefChance) return GhostType.Thief;
        return GhostType.Normal;
    }

    // healthOverride > 0 replaces the calculated health
    public Ghost Spawn(GhostType type, int healthOverride = 0)
    {
        if (type == GhostType.Tank && aliveTank != null) type = GhostType.Normal; // only 1 tank at a time

        // normal ghost stats scaled by score
        int steps = GameManager.Instance.Score / scoreStep;
        int health = Mathf.Min(baseHealth + steps, maxHealth);
        float speed = Mathf.Min(baseSpeed + speedPerStep * steps, maxSpeed);
        Ghost prefab = normalPrefab;

        // special ghosts start from the normal stats
        if (type == GhostType.Thief)
        {
            prefab = thiefPrefab;
            health = Mathf.Max(1, health - 1);
            speed += thiefSpeedBonus;
        }
        else if (type == GhostType.Tank)
        {
            prefab = tankPrefab;
            health *= tankHealthMultiplier;
            speed *= tankSpeedMultiplier;
        }

        if (healthOverride > 0) health = healthOverride;

        Ghost ghost = Instantiate(prefab, RandomSpawnPoint(), Quaternion.identity);
        ghost.Init(player, health, speed);
        ghost.OnRemoved += HandleGhostRemoved;
        alive.Add(ghost);
        if (type == GhostType.Tank) aliveTank = ghost;

        if (logSpawns) Debug.Log($"{type} ghost spawned: {health} HP, {speed} m/s ({alive.Count}/{maxAlive} alive)");
        return ghost;
    }

    private void HandleGhostRemoved(Ghost ghost)
    {
        alive.Remove(ghost);
        if (ghost == aliveTank) aliveTank = null;
    }

    // a random point on a rectangle outside the walls
    private Vector3 RandomSpawnPoint()
    {
        float hx = roomHalfSize.x + spawnMargin;
        float hz = roomHalfSize.y + spawnMargin;

        float x, z;
        int side = Random.Range(0, 4); // 0 front, 1 back, 2 left, 3 right
        if (side == 0) { x = Random.Range(-hx, hx); z = hz; }
        else if (side == 1) { x = Random.Range(-hx, hx); z = -hz; }
        else if (side == 2) { x = -hx; z = Random.Range(-hz, hz); }
        else { x = hx; z = Random.Range(-hz, hz); }

        return new Vector3(x, spawnHeight, z);
    }
}