using System.Collections.Generic;
using UnityEngine;

public class RoomCombatController : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject hellpuppyPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform enemySpawnPointsRoot; // EnemySpawnPoints container
    [SerializeField] private bool shuffleSpawns = true;

    [Header("Spawn Count (v1)")]
    [SerializeField] private int minEnemies = 2;
    [SerializeField] private int maxEnemies = 5;

    [SerializeField] private bool resetEnemiesToSpawnsOnPlayerRespawn = true;

    [Header("Hellhound Execute")]
    private BranchProgression branches;
    [SerializeField] private bool enableHellhoundExecute = true;
    [SerializeField] private float executeChancePerPoint = 0.005f; // 0.5%

    private bool logExecuteRoll = true;


    private struct SpawnedEnemy
    {
        public Transform spawn;
        public GameObject enemy;
    }

    private List<SpawnedEnemy> alive = new List<SpawnedEnemy>();

    private RoomManager roomManager;
    private Vector2Int roomCoord;

    public int AliveCount => alive.Count;

    private void Awake()
    {
        if (enemySpawnPointsRoot == null)
        {
            var t = transform.Find("EnemySpawnPoints");
            if (t != null) enemySpawnPointsRoot = t;
        }
        if (branches == null) branches = FindFirstObjectByType<BranchProgression>();
    }

    // Called by RoomManager right after room is spawned
    public void OnRoomEntered(RoomManager manager, Vector2Int coord, bool isClearedAlready)
    {
        roomManager = manager;
        roomCoord = coord;

        PruneNullEnemies();

        if (isClearedAlready)
        {
            SetDoorsLocked(false);
            return;
        }

        // Hellhound instant-clear roll BEFORE spawning
        if (TryHellhoundExecute())
        {
            HandleRoomCleared();
            return;
        }

        SpawnWaveV1();
        SetDoorsLocked(true);

        // Edge-case: if spawn config results in 0 enemies
        if (alive.Count == 0)
        {
            HandleRoomCleared();
        }
    }

    private bool TryHellhoundExecute()
    {
        if (!enableHellhoundExecute) return false;
        if (branches == null) return false;

        int points = branches.Hellhound;
        if (points <= 0) return false;

        float chance = Mathf.Clamp01(points * executeChancePerPoint);
        float roll = Random.value;

        if (logExecuteRoll) Debug.Log($"[Hellhound] roll {roll:0.000} vs chance {chance:0.000} (points={points})");

        return roll < chance;
    }

    private void SpawnWaveV1()
    {
        if (hellpuppyPrefab == null)
        {
            Debug.LogError("[RoomCombatController] Missing hellpuppyPrefab!");
            return;
        }
        if (enemySpawnPointsRoot == null)
        {
            Debug.LogError("[RoomCombatController] Missing EnemySpawnPoints root!");
            return;
        }

        Transform[] points = enemySpawnPointsRoot.GetComponentsInChildren<Transform>(true);
        // points[0] is root; skip it
        List<Transform> usable = new List<Transform>();
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == enemySpawnPointsRoot) continue;
            usable.Add(points[i]);
        }

        if (usable.Count == 0) return;

        if (shuffleSpawns) Shuffle(usable);

        int count = Random.Range(minEnemies, maxEnemies + 1);
        count = Mathf.Clamp(count, 1, usable.Count); // v1: no reuse

        for (int i = 0; i < count; i++)
        {
            Transform sp = usable[i];
            GameObject e = Instantiate(hellpuppyPrefab, sp.position, Quaternion.identity);

            // Link enemy death back to this room
            var link = e.AddComponent<EnemyRoomLink>();
            link.Init(this);

            alive.Add(new SpawnedEnemy { spawn = sp, enemy = e });
        }
    }

    // Called by EnemyRoomLink when enemy dies/destroys
    public void NotifyEnemyDead(GameObject enemy)
    {
        for (int i = alive.Count - 1; i >= 0; i--)
    {
        if (alive[i].enemy == enemy)
        {
            alive.RemoveAt(i);
            break;
        }
    }

        PruneNullEnemies();

        if (alive.Count <= 0)
        {
            HandleRoomCleared();
        }
    }

    private void HandleRoomCleared()
    {
        SetDoorsLocked(false);

        if (roomManager != null)
        {
            roomManager.MarkCurrentRoomCleared();
        }
    }

    private void SetDoorsLocked(bool locked)
    {
        if (roomManager == null || roomManager.CurrentRoom == null) return;
        

        var doors = roomManager.CurrentRoom.Doors; 
        if (doors == null) return;

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i] == null) continue;

            var lockable = doors[i].GetComponent<RoomDoorLockable>();
            if (lockable != null)
            {
                lockable.SetLocked(locked);
            }
            else
            {
                // Fallback: disable collider if no lockable script
                var col = doors[i].GetComponent<Collider2D>();
                if (col != null) col.enabled = !locked;
            }
        }
    }

    private void Shuffle(List<Transform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    public void ResetAliveEnemiesToSpawnPoints()
    {
        if (!resetEnemiesToSpawnsOnPlayerRespawn) return;

        for (int i = 0; i < alive.Count; i++)
        {
            var entry = alive[i];
            if (entry.enemy == null || entry.spawn == null) continue;

            entry.enemy.transform.position = entry.spawn.position;

            // reset physics so they don’t “slide” from old velocity
            var rb = entry.enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    private void PruneNullEnemies()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i].enemy == null)
                alive.RemoveAt(i);
        }
    }
}