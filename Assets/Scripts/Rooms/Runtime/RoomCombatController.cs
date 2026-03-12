using System.Collections.Generic;
using UnityEngine;

public class RoomCombatController : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject hellpuppyPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform enemySpawnPointsRoot;
    [SerializeField] private bool shuffleSpawns = true;

    [Header("Spawn Count (v1)")]
    [SerializeField] private int minEnemies = 2;
    [SerializeField] private int maxEnemies = 5;

    [Header("Respawn")]
    [SerializeField] private bool resetEnemiesToSpawnsOnPlayerRespawn = true;

    [Header("Hellhound Execute")]
    [SerializeField] private bool enableHellhoundExecute = true;
    [SerializeField] private float executeChancePerPoint = 0.005f;

    private bool logExecuteRoll = true;
    private BranchProgression branches;

    private class SpawnedEnemyRuntime
    {
        public Transform spawnPoint;
        public GameObject enemyObject;
        public RoomEnemyStateEntry stateEntry;
    }

    private readonly List<SpawnedEnemyRuntime> alive = new List<SpawnedEnemyRuntime>();
    private readonly List<Transform> cachedSpawnPoints = new List<Transform>();

    private RoomManager roomManager;
    private Vector2Int roomCoord;
    private RoomState state;

    public int AliveCount => alive.Count;

    private void Awake()
    {
        if (enemySpawnPointsRoot == null)
        {
            Transform t = transform.Find("EnemySpawnPoints");
            if (t != null)
                enemySpawnPointsRoot = t;
        }

        if (branches == null)
            branches = FindFirstObjectByType<BranchProgression>();

        CacheSpawnPoints();
    }

    public void OnRoomEntered(RoomManager manager, Vector2Int coord, RoomState roomState)
    {
        roomManager = manager;
        roomCoord = coord;
        state = roomState;

        PruneNullEnemies();
        ClearRuntimeList();

        if (state != null)
            state.visited = true;

        if (state != null && state.cleared)
        {
            SetDoorsLocked(false);
            return;
        }

        if (TryHellhoundExecute())
        {
            ForceClearRoomInstant();
            return;
        }

        InitializeEncounterIfNeeded();
        SpawnAliveEnemiesFromState();

        if (alive.Count <= 0)
        {
            HandleRoomCleared();
            return;
        }

        SetDoorsLocked(true);
    }

    private void CacheSpawnPoints()
    {
        cachedSpawnPoints.Clear();

        if (enemySpawnPointsRoot == null)
            return;

        Transform[] points = enemySpawnPointsRoot.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == enemySpawnPointsRoot)
                continue;

            cachedSpawnPoints.Add(points[i]);
        }
    }

    private void InitializeEncounterIfNeeded()
    {
        if (state == null)
            return;

        if (state.encounterInitialized)
            return;

        if (cachedSpawnPoints.Count == 0)
        {
            Debug.LogWarning("[RoomCombatController] No enemy spawn points found.");
            state.encounterInitialized = true;
            state.remainingEnemies = 0;
            return;
        }

        List<int> spawnIndices = new List<int>();
        for (int i = 0; i < cachedSpawnPoints.Count; i++)
            spawnIndices.Add(i);

        if (shuffleSpawns)
            Shuffle(spawnIndices);

        int count = Random.Range(minEnemies, maxEnemies + 1);
        count = Mathf.Clamp(count, 1, cachedSpawnPoints.Count);

        state.enemyStates.Clear();

        for (int i = 0; i < count; i++)
        {
            RoomEnemyStateEntry entry = new RoomEnemyStateEntry();
            entry.enemyType = EnemyType.Hellpuppy;
            entry.spawnPointIndex = spawnIndices[i];
            entry.alive = true;

            state.enemyStates.Add(entry);
        }

        state.remainingEnemies = state.enemyStates.Count;
        state.encounterInitialized = true;
    }

    private void SpawnAliveEnemiesFromState()
    {
        if (state == null)
            return;

        if (state.enemyStates == null)
            return;

        for (int i = 0; i < state.enemyStates.Count; i++)
        {
            RoomEnemyStateEntry entry = state.enemyStates[i];

            if (!entry.alive)
                continue;

            int spawnIndex = entry.spawnPointIndex;
            if (spawnIndex < 0 || spawnIndex >= cachedSpawnPoints.Count)
            {
                Debug.LogWarning($"[RoomCombatController] Invalid spawn index {spawnIndex} in room {roomCoord}");
                continue;
            }

            GameObject prefab = GetPrefabForEnemyType(entry.enemyType);
            if (prefab == null)
            {
                Debug.LogWarning($"[RoomCombatController] No prefab assigned for enemy type {entry.enemyType}");
                continue;
            }

            Transform spawnPoint = cachedSpawnPoints[spawnIndex];
            GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity, transform);

            EnemyRoomLink link = enemy.GetComponent<EnemyRoomLink>();
            if (link == null)
                link = enemy.AddComponent<EnemyRoomLink>();

            link.Init(this);

            SpawnedEnemyRuntime runtime = new SpawnedEnemyRuntime();
            runtime.spawnPoint = spawnPoint;
            runtime.enemyObject = enemy;
            runtime.stateEntry = entry;

            alive.Add(runtime);
        }

        if (state != null)
            state.remainingEnemies = CountAliveEntries();
    }

    private GameObject GetPrefabForEnemyType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Hellpuppy:
                return hellpuppyPrefab;
        }

        return null;
    }

    private bool TryHellhoundExecute()
    {
        if (!enableHellhoundExecute)
            return false;

        if (branches == null)
            return false;

        int points = branches.Hellhound;
        if (points <= 0)
            return false;

        float chance = Mathf.Clamp01(points * executeChancePerPoint);
        float roll = Random.value;

        if (logExecuteRoll)
            Debug.Log($"[Hellhound] roll {roll:0.000} vs chance {chance:0.000} (points={points})");

        return roll < chance;
    }

    private void ForceClearRoomInstant()
    {
        if (state != null)
        {
            if (state.enemyStates != null)
            {
                for (int i = 0; i < state.enemyStates.Count; i++)
                    state.enemyStates[i].alive = false;
            }

            state.remainingEnemies = 0;
            state.cleared = true;
        }

        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i] != null && alive[i].enemyObject != null)
                Destroy(alive[i].enemyObject);
        }

        alive.Clear();
        HandleRoomCleared();
    }

    public void NotifyEnemyDead(GameObject enemy)
    {
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i].enemyObject == enemy)
            {
                if (alive[i].stateEntry != null)
                    alive[i].stateEntry.alive = false;

                alive.RemoveAt(i);
                break;
            }
        }

        PruneNullEnemies();

        if (state != null)
            state.remainingEnemies = CountAliveEntries();

        if (state != null && state.remainingEnemies <= 0)
        {
            HandleRoomCleared();
        }
    }

    private int CountAliveEntries()
    {
        if (state == null || state.enemyStates == null)
            return 0;

        int count = 0;

        for (int i = 0; i < state.enemyStates.Count; i++)
        {
            if (state.enemyStates[i].alive)
                count++;
        }

        return count;
    }

    private void HandleRoomCleared()
    {
        SetDoorsLocked(false);

        if (state != null)
        {
            state.cleared = true;
            state.remainingEnemies = 0;
        }

        if (roomManager != null)
            roomManager.MarkCurrentRoomCleared();
    }

    private void SetDoorsLocked(bool locked)
    {
        if (roomManager == null || roomManager.CurrentRoom == null)
            return;

        RoomDoor[] doors = roomManager.CurrentRoom.Doors;
        if (doors == null)
            return;

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i] == null)
                continue;

            RoomDoorLockable lockable = doors[i].GetComponent<RoomDoorLockable>();
            if (lockable != null)
            {
                lockable.SetLocked(locked);
            }
            else
            {
                Collider2D col = doors[i].GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = !locked;
            }
        }
    }

    public void ResetAliveEnemiesToSpawnPoints()
    {
        if (!resetEnemiesToSpawnsOnPlayerRespawn)
            return;

        for (int i = 0; i < alive.Count; i++)
        {
            SpawnedEnemyRuntime entry = alive[i];

            if (entry == null || entry.enemyObject == null || entry.spawnPoint == null)
                continue;

            entry.enemyObject.transform.position = entry.spawnPoint.position;

            Rigidbody2D rb = entry.enemyObject.GetComponent<Rigidbody2D>();
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
            if (alive[i] == null || alive[i].enemyObject == null)
                alive.RemoveAt(i);
        }
    }

    private void ClearRuntimeList()
    {
        alive.Clear();
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            int tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}