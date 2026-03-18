using System.Collections.Generic;
using UnityEngine;

public class RoomCombatController : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject hellpuppyPrefab;
    [SerializeField] private GameObject verminPrefab;
    [SerializeField] private GameObject infernoPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform enemySpawnPointsRoot;

    [Header("Respawn")]
    [SerializeField] private bool resetEnemiesToSpawnsOnPlayerRespawn = true;

    [Header("Hellhound Execute")]
    [SerializeField] private bool enableHellhoundExecute = true;
    [SerializeField] private float executeChancePerPoint = 0.005f;

    [Header("Debug")]
    [SerializeField] private bool logExecuteRoll = true;
    [SerializeField] private bool logEncounterFlow = true;

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
            if (logEncounterFlow)
                Debug.Log($"[RoomCombatController] Room {roomCoord} already cleared. No enemies spawned.");

            SetDoorsLocked(false);
            return;
        }

        if (TryHellhoundExecute())
        {
            if (logEncounterFlow)
                Debug.Log($"[RoomCombatController] Hellhound execute triggered in room {roomCoord}.");
            ForceClearRoomInstant();
            return;
        }

        InitializeEncounterIfNeeded();
        SpawnAliveEnemiesFromState();

        if (alive.Count <= 0)
        {
            if (logEncounterFlow)
                Debug.Log($"[RoomCombatController] Room {roomCoord} has no alive enemies after spawn. Clearing room.");
            HandleRoomCleared();
            return;
        }

        if (logEncounterFlow)
            Debug.Log($"[RoomCombatController] Room {roomCoord} spawned {alive.Count} alive enemies.");

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
        if (state.enemyStates == null)
            state.enemyStates = new List<RoomEnemyStateEntry>();
        else
            state.enemyStates.Clear();

        List<RoomEnemyStateEntry> generatedEncounter = EncounterGenerator.Generate(
            roomCoord,
            state.combatLevel,
            state.encounterSeed,
            cachedSpawnPoints.Count
        );

        if (generatedEncounter != null)
            state.enemyStates.AddRange(generatedEncounter);

        state.remainingEnemies = CountAliveEntries();
        state.encounterInitialized = true;

        if (logEncounterFlow)
        {
            Debug.Log(
                $"[RoomCombatController] Initialized encounter for room {roomCoord} | " +
                $"combatLevel={state.combatLevel} | encounterSeed={state.encounterSeed} | " +
                $"entries={state.enemyStates.Count}"
            );
        }
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

            if (logEncounterFlow)
            {
                Debug.Log(
                    $"[RoomCombatController] Spawned {entry.enemyType} " +
                    $"at spawnIndex={spawnIndex} in room {roomCoord}"
                );
            }

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
            if (hellpuppyPrefab == null)
                {
                    Debug.LogError($"[RoomCombatController] Hellpuppy prefab is not assigned.");
                    return null;
                }
                return hellpuppyPrefab;

            case EnemyType.Vermin:
                if (verminPrefab == null)
                {
                    Debug.LogError($"[RoomCombatController] Vermin prefab is not assigned.");
                    return null;
                }
                return verminPrefab;
            
            case EnemyType.Inferno:
                if (infernoPrefab == null)
                {
                    Debug.LogError($"[RoomCombatController] Inferno prefab is not assigned.");
                    return null;
                }
                return infernoPrefab;

            default:
                Debug.LogError($"[RoomCombatController] No prefab mapped for enemy type {type}.");
                return null; 
        }
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
            state.encounterInitialized = true;
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
}