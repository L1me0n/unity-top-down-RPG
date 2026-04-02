using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject combatRoomPrefab;
    [SerializeField] private GameObject campfireRoomPrefab;

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

    [SerializeField] private float currencyLossOnDeath = 0.25f;   // lose 25% of souls on death

    [Header("Repopulation")]
    [SerializeField] private int repopulationDelaySteps = 50;

    private readonly Dictionary<Vector2Int, RoomState> states
        = new Dictionary<Vector2Int, RoomState>();

    public Vector2Int CurrentCoord => currentCoord;
    public RoomInstance CurrentRoom => currentRoom;

    private Vector2Int currentCoord;
    private RoomInstance currentRoom;
    private bool isTransitioning;
    public bool SkipInitialLoad { get; set; }

    private Vector2Int lastActivatedCampfireCoord;
    private bool hasActivatedCampfireCheckpoint;

    // 7.5: world-progress clock for repopulation timing.
    // This counts only normal room-to-room travel, not load/respawn.
    private int runStepCount;
    public int CurrentRunStep => runStepCount;

    public bool HasActivatedCampfireCheckpoint => hasActivatedCampfireCheckpoint;
    public Vector2Int LastActivatedCampfireCoord => lastActivatedCampfireCoord;

    // 7.6 helper: true only during the checkpoint-room load triggered by death respawn.
    // Campfire systems can read this to suppress celebratory feedback like restore popups.
    public bool SuppressNextCampfireRecoveryFeedback { get; private set; }

    public System.Action<RoomInstance> OnRoomEntered;
    public System.Action<Vector2Int, RoomState> OnCampfireEntered;
    
    // 7.6: checkpoint activation event (for campfire feedback popups).
    public System.Action<Vector2Int> OnCheckpointActivated;

    private void Awake()
    {
        if (playerRb == null && player != null) playerRb = player.GetComponent<Rigidbody2D>();
        if (playerCollider == null && player != null) playerCollider = player.GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (SkipInitialLoad) return;

        // Initial spawn into the start room should not advance world step.
        LoadRoom(Vector2Int.zero, enteredFrom: null, countAsRunStep: false);
    }

    public void RequestTransition(RoomDirection viaDoorDirection)
    {
        if (isTransitioning) return;
        if (player == null) return;

        if (combatRoomPrefab == null)
        {
            Debug.LogError("[RoomManager] Combat room prefab is not assigned.");
            return;
        }

        Vector2Int next = currentCoord + DirToDelta(viaDoorDirection);

        if (!IsCoordAllowed(next))
        {
            if (logTransitions)
                Debug.Log($"[RoomManager] Blocked transition to {next} (out of bounds).");
            return;
        }

        RoomDirection enteredFromSideInNewRoom = Opposite(viaDoorDirection);

        if (logTransitions)
            Debug.Log($"[RoomManager] Transition {currentCoord} -> {next} via {viaDoorDirection} (enter new room from {enteredFromSideInNewRoom})");

        StartCoroutine(DoTransition(next, enteredFromSideInNewRoom));
    }

    private IEnumerator DoTransition(Vector2Int nextCoord, RoomDirection enteredFrom)
    {
        isTransitioning = true;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        if (playerCollider != null) playerCollider.enabled = false;

        // Normal door travel should advance world step.
        LoadRoom(nextCoord, enteredFrom, countAsRunStep: true);

        yield return new WaitForSeconds(transitionLockSeconds);

        if (playerCollider != null) playerCollider.enabled = true;

        isTransitioning = false;
    }

    private GameObject GetPrefabForRoomType(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Campfire:
                if (campfireRoomPrefab != null)
                    return campfireRoomPrefab;

                Debug.LogWarning("[RoomManager] Campfire room prefab is missing. Falling back to combat room prefab.");
                return combatRoomPrefab;

            case RoomType.Combat:
            default:
                if (combatRoomPrefab != null)
                    return combatRoomPrefab;

                Debug.LogError("[RoomManager] Combat room prefab is not assigned.");
                return null;
        }
    }

    private void HandleCampfireRoomEntered(Vector2Int coord, RoomState state, bool allowCheckpointFeedback)
    {
        if (state == null)
            return;

        if (state.roomType != RoomType.Campfire)
            return;

        if (logTransitions)
            Debug.Log($"[RoomManager] Entered campfire room at {coord}.");

        bool alreadyActiveCheckpoint =
            hasActivatedCampfireCheckpoint &&
            lastActivatedCampfireCoord == coord;

        if (!alreadyActiveCheckpoint)
        {
            SetActivatedCampfireCheckpoint(coord, emitFeedbackEvent: allowCheckpointFeedback);
        }
        else if (logTransitions)
        {
            Debug.Log($"[RoomManager] Campfire at {coord} is already the active checkpoint.");
        }

        OnCampfireEntered?.Invoke(coord, state);
    }

    public void SetActivatedCampfireCheckpoint(Vector2Int coord, bool emitFeedbackEvent = true)
    {
        if (hasActivatedCampfireCheckpoint && lastActivatedCampfireCoord == coord)
            return;

        lastActivatedCampfireCoord = coord;
        hasActivatedCampfireCheckpoint = true;

        if (logTransitions)
        {
            Debug.Log(
                $"[RoomManager] Active campfire checkpoint set to {coord} " +
                $"(emitFeedbackEvent={emitFeedbackEvent}).");
        }

        if (emitFeedbackEvent)
            OnCheckpointActivated?.Invoke(coord);
    }

    private void AdvanceRunStep()
    {
        runStepCount++;

        if (logTransitions)
            Debug.Log($"[RoomManager] Run step advanced -> {runStepCount}");
    }

    public void LoadRunStepState(int savedRunStep)
    {
        runStepCount = Mathf.Max(0, savedRunStep);

        if (logTransitions)
            Debug.Log($"[RoomManager] Loaded run step count -> {runStepCount}");
    }

    private void LoadRoom(Vector2Int coord, RoomDirection? enteredFrom, bool countAsRunStep)
    {
        if (countAsRunStep)
        {
            AdvanceRunStep();
        }

        if (currentRoom != null)
        {
            Destroy(currentRoom.gameObject);
            currentRoom = null;
        }

        var state = GetOrCreateState(coord);

        // 7.5:
        // If this is an old cleared combat room, wake it back up before combat flow runs.
        TryRepopulateRoomState(coord, state);

        // Stamp room visit history using the current run step.
        state.visited = true;
        state.lastVisitedStep = runStepCount;

        GameObject prefabToSpawn = GetPrefabForRoomType(state.roomType);
        if (prefabToSpawn == null)
        {
            Debug.LogError($"[RoomManager] No valid prefab found for room type {state.roomType} at {coord}.");
            return;
        }

        GameObject roomGo = Instantiate(prefabToSpawn);
        roomGo.name = $"Room_{coord.x}_{coord.y}";

        currentRoom = roomGo.GetComponent<RoomInstance>();
        if (currentRoom == null)
        {
            Debug.LogError("[RoomManager] Spawned room prefab has no RoomInstance component.");
            return;
        }

        cameraClampBehaviour.SetRoomBounds(currentRoom.RoomBounds);

        currentCoord = coord;

        var doors = currentRoom.Doors;
        for (int i = 0; i < doors.Length; i++)
            doors[i].SetRoomManager(this);

        if (logTransitions)
        {
            int ring = WorldDifficultyService.GetRing(coord);
            int roomAgeSinceClear = GetRoomAgeSinceClear(state);
            bool eligibleToRepopulate = CanRoomRepopulateNow(state);

            Debug.Log(
                $"[RoomManager] Loaded room {coord} | roomType={state.roomType} | ring={ring} | " +
                $"combatLevel={state.combatLevel} | encounterSeed={state.encounterSeed} | " +
                $"encounterInitialized={state.encounterInitialized} | visited={state.visited} | cleared={state.cleared} | " +
                $"runStep={runStepCount} | lastVisitedStep={state.lastVisitedStep} | lastClearedStep={state.lastClearedStep} | " +
                $"repopBlockedUntil={state.repopulationBlockedUntilStep} | timesRepopulated={state.timesRepopulated} | " +
                $"roomAgeSinceClear={roomAgeSinceClear} | canRepopulateNow={eligibleToRepopulate}"
            );
        }

        var combat = currentRoom.GetComponent<RoomCombatController>();
        if (combat != null)
        {
            combat.OnRoomEntered(this, currentCoord, state);
        }

        UpdateDoorAvailability(doors);

        Vector3 spawnPos = currentRoom.GetSpawnPosition(enteredFrom);
        player.position = spawnPos;

        // 7.6 
        HandleCampfireRoomEntered(coord, state, allowCheckpointFeedback: countAsRunStep);

        // 7.6: one-shot flag, only meant for the current room-load cycle.
        SuppressNextCampfireRecoveryFeedback = false;

        OnRoomEntered?.Invoke(currentRoom);
    }

    private Vector2Int DirToDelta(RoomDirection d)
    {
        switch (d)
        {
            case RoomDirection.North: return new Vector2Int(0, 1);
            case RoomDirection.East: return new Vector2Int(1, 0);
            case RoomDirection.South: return new Vector2Int(0, -1);
            case RoomDirection.West: return new Vector2Int(-1, 0);
        }
        return Vector2Int.zero;
    }

    private RoomDirection Opposite(RoomDirection d)
    {
        switch (d)
        {
            case RoomDirection.North: return RoomDirection.South;
            case RoomDirection.East: return RoomDirection.West;
            case RoomDirection.South: return RoomDirection.North;
            case RoomDirection.West: return RoomDirection.East;
        }
        return d;
    }

    private bool IsCoordAllowed(Vector2Int c)
    {
        if (!useBoundedMap) return true;
        return c.x >= minCoord.x && c.x <= maxCoord.x && c.y >= minCoord.y && c.y <= maxCoord.y;
    }

    private RoomType DetermineRoomTypeForNewState(Vector2Int coord)
    {
        if (coord == Vector2Int.zero)
            return RoomType.Campfire;

        int ring = WorldDifficultyService.GetRing(coord);

        if (ring < 2)
            return RoomType.Combat;

        int hash = Mathf.Abs((coord.x * 73856093) ^ (coord.y * 19349663));

        if (hash % 8 == 0)
            return RoomType.Campfire;

        return RoomType.Combat;
    }

    private RoomState GetOrCreateState(Vector2Int c)
    {
        if (!states.TryGetValue(c, out var s))
        {
            RoomType roomType = DetermineRoomTypeForNewState(c);

            s = new RoomState(
                visited: false,
                cleared: false,
                remainingEnemies: -1,
                roomType: roomType,
                lastVisitedStep: -1,
                lastClearedStep: -1,
                repopulationBlockedUntilStep: 0,
                timesRepopulated: 0
            );

            s.combatLevel = WorldDifficultyService.GetCombatLevel(c);
            s.encounterSeed = EncounterGenerator.BuildEncounterSeed(c, s.combatLevel);
            states.Add(c, s);

            if (logTransitions)
                Debug.Log($"[RoomManager] Created state for {c}, roomType = {s.roomType}, combatLevel = {s.combatLevel}, encounterSeed = {s.encounterSeed}");
        }
        else
        {
            if (!System.Enum.IsDefined(typeof(RoomType), s.roomType))
            {
                s.roomType = DetermineRoomTypeForNewState(c);

                if (logTransitions)
                    Debug.Log($"[RoomManager] Repaired missing/invalid roomType for {c} -> {s.roomType}");
            }

            if (s.combatLevel <= 0)
            {
                s.combatLevel = WorldDifficultyService.GetCombatLevel(c);

                if (logTransitions)
                    Debug.Log($"[RoomManager] Repaired missing combatLevel for {c} -> {s.combatLevel}");
            }

            if (s.encounterSeed == 0)
            {
                s.encounterSeed = EncounterGenerator.BuildEncounterSeed(c, s.combatLevel);

                if (logTransitions)
                    Debug.Log($"[RoomManager] Repaired missing encounterSeed for {c} -> {s.encounterSeed}");
            }

            if (s.lastVisitedStep < -1) s.lastVisitedStep = -1;
            if (s.lastClearedStep < -1) s.lastClearedStep = -1;
            if (s.repopulationBlockedUntilStep < 0) s.repopulationBlockedUntilStep = 0;
            if (s.timesRepopulated < 0) s.timesRepopulated = 0;
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
            d.gameObject.SetActive(allowed);
        }
    }

    public void RespawnInCurrentRoom()
    {
        if (currentRoom == null || player == null) return;

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.Heal(999999);
            stats.GainAP(999999);
        }

        var currency = player.GetComponent<RunCurrency>();
        if (currency != null)
        {
            currency.TakeSouls(currencyLossOnDeath);
        }

        player.position = currentRoom.GetSpawnPosition(null);

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        if (playerCollider != null && !playerCollider.enabled)
        {
            playerCollider.enabled = true;
        }

        if (logTransitions)
            Debug.Log("[RoomManager] RespawnInCurrentRoom() used as legacy fallback. No local enemy reset was performed.");
    }

    public void RespawnAtCheckpoint()
    {
        if (player == null)
            return;

        if (isTransitioning)
        {
            if (logTransitions)
                Debug.Log("[RoomManager] Ignored checkpoint respawn request because a transition is already in progress.");
            return;
        }

        Vector2Int checkpointCoord = hasActivatedCampfireCheckpoint
            ? lastActivatedCampfireCoord
            : Vector2Int.zero;

        if (logTransitions)
        {
            if (hasActivatedCampfireCheckpoint)
                Debug.Log($"[RoomManager] Respawning player at active checkpoint {checkpointCoord}.");
            else
                Debug.Log($"[RoomManager] No active checkpoint found. Falling back to start room {checkpointCoord}.");
        }

        var currency = player.GetComponent<RunCurrency>();
        if (currency != null)
        {
            currency.TakeSouls(currencyLossOnDeath);
            if (logTransitions)
                Debug.Log($"[RoomManager] Applied death soul loss at checkpoint respawn ({currencyLossOnDeath * 100f}% souls).");
        }

        var deathPenalty = player.GetComponent<DeathPenaltyTracker>();
        if (deathPenalty != null)
        {
            deathPenalty.AddDeathPenalty(1, 1, 1, 1);

            if (logTransitions)
                Debug.Log("[RoomManager] Applied death stat penalty at checkpoint respawn.");
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        if (playerCollider != null && !playerCollider.enabled)
            playerCollider.enabled = true;

        // 7.6: checkpoint respawn should still restore the player,
        // but we suppress the "restored" popup because this is a death-return flow.
        SuppressNextCampfireRecoveryFeedback = true;

        // Respawn / checkpoint return should not advance world step.
        LoadRoom(checkpointCoord, enteredFrom: null, countAsRunStep: false);

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        if (playerCollider != null && !playerCollider.enabled)
            playerCollider.enabled = true;
    }

    public void MarkCurrentRoomCleared()
    {
        var state = GetOrCreateState(currentCoord);

        state.cleared = true;

        // stamp the clear history at the current run step.
        state.lastClearedStep = runStepCount;

        if (logTransitions)
            Debug.Log($"[RoomManager] Marked room {currentCoord} cleared at run step {runStepCount}.");
    }

    public void LoadCheckpointState(bool hasCheckpoint, Vector2Int checkpointCoord)
    {
        hasActivatedCampfireCheckpoint = hasCheckpoint;
        lastActivatedCampfireCoord = checkpointCoord;

        if (logTransitions && hasCheckpoint)
            Debug.Log($"[RoomManager] Loaded active campfire checkpoint at {checkpointCoord}.");
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

            entry.roomType = (int)s.roomType;

            entry.encounterInitialized = s.encounterInitialized;
            entry.combatLevel = s.combatLevel;
            entry.encounterSeed = s.encounterSeed;

            entry.lastVisitedStep = s.lastVisitedStep;
            entry.lastClearedStep = s.lastClearedStep;
            entry.repopulationBlockedUntilStep = s.repopulationBlockedUntilStep;
            entry.timesRepopulated = s.timesRepopulated;

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

                RoomType restoredRoomType = System.Enum.IsDefined(typeof(RoomType), e.roomType)
                    ? (RoomType)e.roomType
                    : RoomType.Combat;

                RoomState s = new RoomState(
                    e.visited,
                    e.cleared,
                    e.remainingEnemies,
                    restoredRoomType,
                    e.lastVisitedStep,
                    e.lastClearedStep,
                    e.repopulationBlockedUntilStep,
                    e.timesRepopulated
                );

                s.encounterInitialized = e.encounterInitialized;
                s.combatLevel = e.combatLevel;
                s.encounterSeed = e.encounterSeed;

                if (s.combatLevel <= 0)
                    s.combatLevel = WorldDifficultyService.GetCombatLevel(coord);

                if (s.encounterSeed == 0)
                    s.encounterSeed = EncounterGenerator.BuildEncounterSeed(coord, s.combatLevel);

                if (s.lastVisitedStep < -1) s.lastVisitedStep = -1;
                if (s.lastClearedStep < -1) s.lastClearedStep = -1;
                if (s.repopulationBlockedUntilStep < 0) s.repopulationBlockedUntilStep = 0;
                if (s.timesRepopulated < 0) s.timesRepopulated = 0;

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

        // Loading a save should restore the room, not advance time.
        LoadRoom(startCoord, enteredFrom: null, countAsRunStep: false);
    }

    // -----------------------------
    // 7.5: repopulation helpers
    // -----------------------------

    public int GetRoomAgeSinceClear(RoomState state)
    {
        if (state == null)
            return -1;

        if (!state.cleared)
            return -1;

        if (state.lastClearedStep < 0)
            return -1;

        return runStepCount - state.lastClearedStep;
    }

    public bool CanRoomRepopulateNow(RoomState state)
    {
        if (state == null)
            return false;

        if (state.roomType != RoomType.Combat)
            return false;

        if (!state.cleared)
            return false;

        if (state.lastClearedStep < 0)
            return false;

        int roomAgeSinceClear = GetRoomAgeSinceClear(state);
        if (roomAgeSinceClear < 0)
            return false;

        return roomAgeSinceClear >= repopulationDelaySteps;
    }

    private bool TryRepopulateRoomState(Vector2Int coord, RoomState state)
    {
        if (!CanRoomRepopulateNow(state))
            return false;

        int oldLastClearedStep = state.lastClearedStep;
        int roomAgeSinceClear = GetRoomAgeSinceClear(state);

        // Wake the room back up.
        state.cleared = false;
        state.remainingEnemies = -1;
        state.encounterInitialized = false;

        if (state.enemyStates == null)
            state.enemyStates = new List<RoomEnemyStateEntry>();
        else
            state.enemyStates.Clear();

        state.timesRepopulated++;

        if (logTransitions)
        {
            Debug.Log(
                $"[RoomManager] Repopulated room {coord} | " +
                $"oldLastClearedStep={oldLastClearedStep} | roomAgeSinceClear={roomAgeSinceClear} | " +
                $"timesRepopulated={state.timesRepopulated}"
            );
        }

        return true;
    }
}