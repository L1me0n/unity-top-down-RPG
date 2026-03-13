using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject roomPrefab; // RoomBase

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Collider2D playerCollider;

    [Header("Transition Tuning")]
    [SerializeField] private float transitionLockSeconds = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool logTransitions = true;

    [Header("Camera Clamping")]
    [SerializeField] private CameraFollow cameraClampBehaviour;

    [Header("Map Bounds")]
    [SerializeField] private bool useBoundedMap = true;
    [SerializeField] private Vector2Int minCoord = new Vector2Int(-2, -2);
    [SerializeField] private Vector2Int maxCoord = new Vector2Int(2, 2);

    [SerializeField] private float currencyLossOnDeath = 0.25f;   // lose 25% of souls/xp on death

    private readonly System.Collections.Generic.Dictionary<Vector2Int, RoomState> states
        = new System.Collections.Generic.Dictionary<Vector2Int, RoomState>();

    public Vector2Int CurrentCoord => currentCoord;
    public RoomInstance CurrentRoom => currentRoom;

    private Vector2Int currentCoord;
    private RoomInstance currentRoom;
    private bool isTransitioning;
    public bool SkipInitialLoad { get; set; }

    public System.Action<RoomInstance> OnRoomEntered;

    private void Awake()
    {
        if (playerRb == null && player != null) playerRb = player.GetComponent<Rigidbody2D>();
        if (playerCollider == null && player != null) playerCollider = player.GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (SkipInitialLoad) return;
        // Start at (0,0)
        LoadRoom(Vector2Int.zero, enteredFrom: null);
    }

    public void RequestTransition(RoomDirection viaDoorDirection)
    {
        if (isTransitioning) return;
        if (roomPrefab == null || player == null) return;

        Vector2Int next = currentCoord + DirToDelta(viaDoorDirection);

        if (!IsCoordAllowed(next))
        {
            if (logTransitions)
                Debug.Log($"[RoomManager] Blocked transition to {next} (out of bounds).");
            return;
        }

        // When we go NORTH through a door, the next room is entered FROM SOUTH.
        RoomDirection enteredFromSideInNewRoom = Opposite(viaDoorDirection);

        if (logTransitions)
            Debug.Log($"[RoomManager] Transition {currentCoord} -> {next} via {viaDoorDirection} (enter new room from {enteredFromSideInNewRoom})");

        StartCoroutine(DoTransition(next, enteredFromSideInNewRoom));
    }

    private IEnumerator DoTransition(Vector2Int nextCoord, RoomDirection enteredFrom)
    {
        isTransitioning = true;

        // Stop motion so we don't “skate” into the next trigger.
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        // Disable collider briefly to avoid instant retrigger.
        if (playerCollider != null) playerCollider.enabled = false;

        LoadRoom(nextCoord, enteredFrom);

        // Wait a tiny bit to clear overlaps, then re-enable.
        yield return new WaitForSeconds(transitionLockSeconds);

        if (playerCollider != null) playerCollider.enabled = true;

        isTransitioning = false;
    }

    private void LoadRoom(Vector2Int coord, RoomDirection? enteredFrom)
    {
        // Destroy old
        if (currentRoom != null)
        {
            Destroy(currentRoom.gameObject);
            currentRoom = null;
        }

        // Spawn new
        GameObject roomGo = Instantiate(roomPrefab);
        roomGo.name = $"Room_{coord.x}_{coord.y}";

        currentRoom = roomGo.GetComponent<RoomInstance>();
        if (currentRoom == null)
        {
            Debug.LogError("[RoomManager] Spawned room prefab has no RoomInstance component.");
            return;
        }

        cameraClampBehaviour.SetRoomBounds(currentRoom.RoomBounds);

        // Update coord
        currentCoord = coord;

        

        // Hook doors to this manager
        var doors = currentRoom.Doors;
        for (int i = 0; i < doors.Length; i++)
            doors[i].SetRoomManager(this);

        var state = GetOrCreateState(coord);
        if (logTransitions)
        {
            int ring = WorldDifficultyService.GetRing(coord);
            Debug.Log($"[RoomManager] Loaded room {coord} | ring={ring} | combatLevel={state.combatLevel}");
        }

        var combat = currentRoom.GetComponent<RoomCombatController>();
        if (combat != null)
        {
            combat.OnRoomEntered(this, currentCoord, state);
        }  
        
        UpdateDoorAvailability(doors);

        // Place player at the correct spawn
        Vector3 spawnPos = currentRoom.GetSpawnPosition(enteredFrom);
        player.position = spawnPos;

        OnRoomEntered?.Invoke(currentRoom);
    }

    private Vector2Int DirToDelta(RoomDirection d)
    {
        switch (d)
        {
            case RoomDirection.North: return new Vector2Int(0, 1);
            case RoomDirection.East:  return new Vector2Int(1, 0);
            case RoomDirection.South: return new Vector2Int(0, -1);
            case RoomDirection.West:  return new Vector2Int(-1, 0);
        }
        return Vector2Int.zero;
    }

    private RoomDirection Opposite(RoomDirection d)
    {
        switch (d)
        {
            case RoomDirection.North: return RoomDirection.South;
            case RoomDirection.East:  return RoomDirection.West;
            case RoomDirection.South: return RoomDirection.North;
            case RoomDirection.West:  return RoomDirection.East;
        }
        return d;
    }

    private bool IsCoordAllowed(Vector2Int c)
    {
        if (!useBoundedMap) return true;
        return c.x >= minCoord.x && c.x <= maxCoord.x && c.y >= minCoord.y && c.y <= maxCoord.y;
    }

    private RoomState GetOrCreateState(Vector2Int c)
    {
        if (!states.TryGetValue(c, out var s))
        {
            s = new RoomState(visited: false, cleared: false);
            s.combatLevel = WorldDifficultyService.GetCombatLevel(c);
            states.Add(c, s);
            if (logTransitions)
                Debug.Log($"[RoomManager] Created state for {c}, combatLevel = {s.combatLevel}");
        }
        else if (s.combatLevel <= 0)
        {
            s.combatLevel = WorldDifficultyService.GetCombatLevel(c);
        }

        return s;
    }

    private void UpdateDoorAvailability(RoomDoor[] doors)
    {
        for (int i = 0; i < doors.Length; i++)
        {
            RoomDoor d = doors[i];
            Vector2Int target = currentCoord + DirToDelta(d.Direction);
            bool allowed = IsCoordAllowed(target);

            // Disable the whole door object so trigger + visual both disappear.
            d.gameObject.SetActive(allowed);
        }
    }

    public void RespawnInCurrentRoom()
    {
        if (currentRoom == null || player == null) return;

        // 1) restore stats
        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.Heal(999999);
            stats.GainAP(999999);
        }

        // 2) apply death penalty
        var deathPenalty = player.GetComponent<DeathPenaltyTracker>();
        if (deathPenalty != null)
        {
            deathPenalty.AddDeathPenalty(1, 1, 1, 1);
        }

        // 3) reset enemies to their original spawn points (NO new spawns)
        var combat = currentRoom.GetComponent<RoomCombatController>();
        if (combat != null)
            combat.ResetAliveEnemiesToSpawnPoints();

        // 4) take a portion of currency on death
        var currency = player.GetComponent<RunCurrency>();
        if (currency != null)
        {
            currency.TakeSouls(currencyLossOnDeath);
            currency.TakeXP(currencyLossOnDeath);
        }

        // 5) teleport player to center spawn (enteredFrom null -> Spawn_Center)
        player.position = currentRoom.GetSpawnPosition(null);

        // 6) clear motion
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
    }

    public void MarkCurrentRoomCleared()
    {
        var state = GetOrCreateState(currentCoord);
        state.cleared = true;
    }


    public RoomStateSaveEntry[] ExportRoomStates()
    {
        var list = new List<RoomStateSaveEntry>();

        foreach (var kv in states)
        {
            var c = kv.Key;
            var s = kv.Value;

            RoomStateSaveEntry entry = new RoomStateSaveEntry();
            entry.x = c.x;
            entry.y = c.y;

            entry.visited = s.visited;
            entry.cleared = s.cleared;
            entry.remainingEnemies = s.remainingEnemies;

            entry.encounterInitialized = s.encounterInitialized;
            entry.combatLevel = s.combatLevel;
            entry.encounterSeed = s.encounterSeed;

            if (s.enemyStates != null)
            {
                for (int i = 0; i < s.enemyStates.Count; i++)
                {
                    RoomEnemyStateEntry enemyState = s.enemyStates[i];

                    RoomEnemyStateSaveEntry enemyEntry = new RoomEnemyStateSaveEntry();
                    enemyEntry.enemyType = (int)enemyState.enemyType;
                    enemyEntry.spawnPointIndex = enemyState.spawnPointIndex;
                    enemyEntry.alive = enemyState.alive;

                    entry.enemyStates.Add(enemyEntry);
                }
            }

            list.Add(entry);
        }

        return list.ToArray();
    }

    public void ImportRoomStates(RoomStateSaveEntry[] entries, Vector2Int startCoord)
    {
        states.Clear();

        if (entries != null)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                RoomStateSaveEntry e = entries[i];
                Vector2Int coord = new Vector2Int(e.x, e.y);

                RoomState s = new RoomState(e.visited, e.cleared);
                s.remainingEnemies = e.remainingEnemies;

                s.encounterInitialized = e.encounterInitialized;
                s.combatLevel = e.combatLevel;
                s.encounterSeed = e.encounterSeed;

                s.enemyStates = new List<RoomEnemyStateEntry>();

                if (e.enemyStates != null)
                {
                    for (int j = 0; j < e.enemyStates.Count; j++)
                    {
                        RoomEnemyStateSaveEntry savedEnemy = e.enemyStates[j];

                        RoomEnemyStateEntry restoredEnemy = new RoomEnemyStateEntry();
                        restoredEnemy.enemyType = (EnemyType)savedEnemy.enemyType;
                        restoredEnemy.spawnPointIndex = savedEnemy.spawnPointIndex;
                        restoredEnemy.alive = savedEnemy.alive;

                        s.enemyStates.Add(restoredEnemy);
                    }
                }

                states[coord] = s;
            }
        }

        LoadRoom(startCoord, enteredFrom: null);
    }
}