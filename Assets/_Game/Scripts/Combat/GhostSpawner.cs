using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// spawns ghosts outside the house every few seconds
// lives on Managers

public class GhostSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ghost ghostPrefab;
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

    [Header("Debug")]
    [SerializeField] private bool logSpawns = true;
    [SerializeField] private bool debugKeys = true; // G = spawn a ghost now

    public bool SpawningEnabled { get; set; } = true;
    public int AliveCount => alive.Count;

    private readonly List<Ghost> alive = new List<Ghost>();
    private float timer;

    // Update is called once per frame
    private void Update()
    {
        if (debugKeys && Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame) Spawn();

        if (!SpawningEnabled) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (alive.Count < maxAlive) Spawn();
        }
    }

    public Ghost Spawn()
    {
        int steps = GameManager.Instance.Score / scoreStep;
        int health = Mathf.Min(baseHealth + steps, maxHealth);
        float speed = Mathf.Min(baseSpeed + speedPerStep * steps, maxSpeed);

        Ghost ghost = Instantiate(ghostPrefab, RandomSpawnPoint(), Quaternion.identity);
        ghost.Init(player, health, speed);
        ghost.OnDied += HandleGhostDied;
        alive.Add(ghost);

        if (logSpawns) Debug.Log($"Ghost spawned: {health} HP, {speed} m/s ({alive.Count}/{maxAlive} alive)");
        return ghost;
    }

    private void HandleGhostDied(Ghost ghost)
    {
        alive.Remove(ghost);
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
