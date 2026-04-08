using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject combatRoomPrefab;
    [SerializeField] private GameObject campfireRoomPrefab;
    [SerializeField] private GameObject bettingRoomPrefab;
    [SerializeField] private GameObject gluttonyRoomPrefab;
    [SerializeField] private GameObject slothRoomPrefab;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private ChallengeEffectManager challengeEffectManager;

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

    [Header("Challenge Reset")]
    [SerializeField] private int challengeResetDelaySteps = 50;

    [Header("Hostage Campfire Capacity")]
    [SerializeField] private int maxStoredHostageGhostsPerCampfire = 10;

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
    public System.Action<Vector2Int, RoomState> OnHostagesRescued;

    private void Awake()
    {
        if (playerRb == null && player != null) playerRb = player.GetComponent<Rigidbody2D>();
        if (playerCollider == null && player != null) playerCollider = player.GetComponent<Collider2D>();
        if (challengeEffectManager == null) challengeEffectManager = FindFirstObjectByType<ChallengeEffectManager>();
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

    private GameObject GetPrefabForRoomState(RoomState state)
    {
        if (state == null)
        {
            Debug.LogError("[RoomManager] GetPrefabForRoomState called with null RoomState.");
            return null;
        }

        switch (state.roomType)
        {
            case RoomType.Campfire:
                if (campfireRoomPrefab != null)
                    return campfireRoomPrefab;

                Debug.LogWarning("[RoomManager] Campfire room prefab is missing. Falling back to combat room prefab.");
                return combatRoomPrefab;

            case RoomType.Combat:
                if (combatRoomPrefab != null)
                    return combatRoomPrefab;

                Debug.LogError("[RoomManager] Combat room prefab is not assigned.");
                return null;

            case RoomType.Challenge:
                switch (state.challengeType)
                {
                    case ChallengeType.Betting:
                        if (bettingRoomPrefab != null)
                            return bettingRoomPrefab;

                        Debug.LogError("[RoomManager] Betting room prefab is not assigned.");
                        return null;

                    case ChallengeType.Gluttony:
                        if (gluttonyRoomPrefab != null)
                            return gluttonyRoomPrefab;

                        Debug.LogError("[RoomManager] Gluttony room prefab is not assigned.");
                        return null;

                    case ChallengeType.Sloth:
                        if (slothRoomPrefab != null)
                            return slothRoomPrefab;

                        Debug.LogError("[RoomManager] Sloth room prefab is not assigned.");
                        return null;
                    case ChallengeType.Lie:
                    case ChallengeType.None:
                    default:
                        Debug.LogWarning(
                            $"[RoomManager] No dedicated prefab assigned yet for challengeType={state.challengeType}. " +
                            $"Falling back to betting room prefab."
                        );

                        if (bettingRoomPrefab != null)
                            return bettingRoomPrefab;

                        Debug.LogError("[RoomManager] Betting room prefab fallback is not assigned.");
                        return null;
                }

            default:
                Debug.LogError($"[RoomManager] Unsupported roomType={state.roomType}");
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

        // 8.2:
        // If this is an old cleared challenge room, wake it back up.
        TryResetChallengeRoomState(coord, state);

        // Stamp room visit history using the current run step.
        state.visited = true;
        state.lastVisitedStep = runStepCount;

        GameObject prefabToSpawn = GetPrefabForRoomState(state);
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
        
        HandleChallengeRoomEntered(coord, state, allowEffectClear: countAsRunStep);

        if (logTransitions)
        {
            int ring = WorldDifficultyService.GetRing(coord);
            int roomAgeSinceClear = GetRoomAgeSinceClear(state);
            bool eligibleToRepopulate = CanRoomRepopulateNow(state);

            Debug.Log(
                $"[RoomManager] Loaded room {coord} | roomType={state.roomType} | " +
                $"challengeType={state.challengeType} | challengeCompleted={state.challengeCompleted} | " +
                $"ring={ring} | combatLevel={state.combatLevel} | encounterSeed={state.encounterSeed} | " +
                $"encounterInitialized={state.encounterInitialized} | visited={state.visited} | cleared={state.cleared} | " +
                $"runStep={runStepCount} | lastVisitedStep={state.lastVisitedStep} | lastClearedStep={state.lastClearedStep} | " +
                $"repopBlockedUntil={state.repopulationBlockedUntilStep} | timesRepopulated={state.timesRepopulated} | " +
                $"roomAgeSinceClear={roomAgeSinceClear} | canRepopulateNow={eligibleToRepopulate} | " +
                $"hasHostageGhosts={state.hasHostageGhosts} | hostageGhostCount={state.hostageGhostCount} | " +
                $"hostageGhostsRescued={state.hostageGhostsRescued} | storedHostageGhostCount={state.storedHostageGhostCount}"
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

        //Campfire rooms
        if (ring < 2)
            return RoomType.Combat;

        int campfireHash = Mathf.Abs((coord.x * 73856093) ^ (coord.y * 19349663));

        if (campfireHash % 8 == 0)
            return RoomType.Campfire;

        // Challenge rooms        
        if (ring < 3)
            return RoomType.Combat;
        
        int challengeHash = Mathf.Abs((coord.x * 83492791) ^ (coord.y * 297121507));

        if (challengeHash % 4 == 0)
            return RoomType.Challenge;

        return RoomType.Combat;
    }

    private bool ShouldAssignHostagesToNewRoom(Vector2Int coord, RoomType roomType)
    {
        if (roomType != RoomType.Combat)
            return false;

        if (coord == Vector2Int.zero)
            return false;

        int ring = WorldDifficultyService.GetRing(coord);
        if (ring < 2)
            return false;

        if (hasActivatedCampfireCheckpoint)
        {
            Vector2Int activeCampfireCoord = GetCurrentHostageDestinationCampfireCoord();

            if (IsCampfireAtHostageCapacity(activeCampfireCoord))
            {
                if (logTransitions)
                {
                    Debug.Log(
                        $"[RoomManager] Suppressed hostage assignment for new room {coord} " +
                        $"because active checkpoint campfire {activeCampfireCoord} is full " +
                        $"({GetStoredHostageGhostCount(activeCampfireCoord)}/{GetCampfireHostageCapacity()})."
                    );
                }

                return false;
            }
        }

        int hash = Mathf.Abs((coord.x * 83492791) ^ (coord.y * 297121507));

        // First-pass rarity: about 1 in 5 eligible combat rooms.
        return hash % 5 == 0;
    }

    private int GetInitialHostageGhostCount(Vector2Int coord)
    {
        int hash = Mathf.Abs((coord.x * 19349663) ^ (coord.y * 83492791));

        // Deterministic count in range 1..3
        return 1 + (hash % 3);
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

            if (roomType == RoomType.Challenge)
            {
                s.challengeType = DetermineChallengeTypeForNewState(c);
                s.challengeCompleted = false;
            }
            else
            {
                s.challengeType = ChallengeType.None;
                s.challengeCompleted = false;
            }

            s.combatLevel = WorldDifficultyService.GetCombatLevel(c);
            s.encounterSeed = EncounterGenerator.BuildEncounterSeed(c, s.combatLevel);

            if (ShouldAssignHostagesToNewRoom(c, roomType))
            {
                s.hasHostageGhosts = true;
                s.hostageGhostCount = GetInitialHostageGhostCount(c);
                s.hostageGhostsRescued = false;
            }

            states.Add(c, s);

            if (logTransitions)
            {
                Debug.Log(
                    $"[RoomManager] Created state for {c}, roomType = {s.roomType}, " +
                    $"challengeType = {s.challengeType}, challengeCompleted = {s.challengeCompleted}, " +
                    $"combatLevel = {s.combatLevel}, encounterSeed = {s.encounterSeed}, " +
                    $"hasHostageGhosts = {s.hasHostageGhosts}, hostageGhostCount = {s.hostageGhostCount}"
                );
            }
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

            if (s.roomType != RoomType.Combat)
            {
                s.hasHostageGhosts = false;
                s.hostageGhostCount = 0;
                s.hostageGhostsRescued = false;
            }

            if (s.roomType != RoomType.Campfire)
            {
                s.storedHostageGhostCount = 0;
            }

            if (s.roomType == RoomType.Challenge)
            {
                if (!s.challengeCompleted)
                {
                    s.lastChallengeCompletedStep = -1;
                }
                if (!System.Enum.IsDefined(typeof(ChallengeType), s.challengeType) || s.challengeType == ChallengeType.None)
                {
                    s.challengeType = DetermineChallengeTypeForNewState(c);
                    s.challengeCompleted = false;

                    if (logTransitions)
                        Debug.Log($"[RoomManager] Repaired missing/invalid challengeType for {c} -> {s.challengeType}");
                }
            }

            if (s.roomType != RoomType.Challenge)
            {
                s.challengeType = ChallengeType.None;
                s.challengeCompleted = false;
                s.lastChallengeCompletedStep = -1;
            }

            if (!s.hasHostageGhosts)
            {
                s.hostageGhostCount = 0;
                s.hostageGhostsRescued = false;
            }

            if (s.hasHostageGhosts && s.hostageGhostCount <= 0)
            {
                s.hasHostageGhosts = false;
                s.hostageGhostCount = 0;
                s.hostageGhostsRescued = false;
            }

            if (s.lastVisitedStep < -1) s.lastVisitedStep = -1;
            if (s.lastClearedStep < -1) s.lastClearedStep = -1;
            if (s.repopulationBlockedUntilStep < 0) s.repopulationBlockedUntilStep = 0;
            if (s.timesRepopulated < 0) s.timesRepopulated = 0;
            if (s.hostageGhostCount < 0) s.hostageGhostCount = 0;
            if (s.storedHostageGhostCount < 0) s.storedHostageGhostCount = 0;
            if (s.lastChallengeCompletedStep < -1) s.lastChallengeCompletedStep = -1;
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

    public void MarkChallengeRoomCompleted(Vector2Int coord)
    {
        RoomState state = GetOrCreateState(coord);
        if (state == null)
            return;

        if (state.roomType != RoomType.Challenge)
            return;

        state.challengeCompleted = true;
        state.lastChallengeCompletedStep = runStepCount;

        if (logTransitions)
        {
            Debug.Log(
                $"[RoomManager] Marked challenge room {coord} completed at run step {runStepCount}."
            );
        }
    }

    public bool TryGetRoomState(Vector2Int coord, out RoomState state)
    {
        return states.TryGetValue(coord, out state);
    }

    public bool TryRescueHostagesInRoom(Vector2Int coord)
    {
        RoomState state = GetOrCreateState(coord);
        if (state == null)
            return false;

        if (state.roomType != RoomType.Combat)
            return false;

        if (!state.hasHostageGhosts)
            return false;

        if (state.hostageGhostsRescued)
            return false;

        if (state.hostageGhostCount <= 0)
            return false;

        state.hostageGhostsRescued = true;

        TransferRescuedHostagesToCheckpoint(coord, state);

        if (logTransitions)
        {
            Debug.Log(
                $"[RoomManager] Hostages rescued in room {coord} | " +
                $"hostageGhostCount={state.hostageGhostCount}"
            );
        }

        OnHostagesRescued?.Invoke(coord, state);
        return true;
    }

    private void TransferRescuedHostagesToCheckpoint(Vector2Int sourceRoomCoord, RoomState rescuedRoomState)
    {
        if (rescuedRoomState == null)
            return;

        int requestedCount = rescuedRoomState.hostageGhostCount;
        if (requestedCount <= 0)
            return;

        Vector2Int destinationCoord = GetCurrentHostageDestinationCampfireCoord();
        RoomState destinationState = GetOrCreateState(destinationCoord);

        if (destinationState.roomType != RoomType.Campfire)
        {
            Debug.LogWarning(
                $"[RoomManager] Checkpoint destination {destinationCoord} is not Campfire. Falling back to start room."
            );

            destinationCoord = Vector2Int.zero;
            destinationState = GetOrCreateState(destinationCoord);
        }

        int remainingSpace = GetRemainingCampfireHostageSpace(destinationCoord);
        int acceptedCount = Mathf.Min(requestedCount, remainingSpace);
        int rejectedCount = requestedCount - acceptedCount;

        if (acceptedCount > 0)
        {
            destinationState.storedHostageGhostCount += acceptedCount;

            int capacity = GetCampfireHostageCapacity();
            if (destinationState.storedHostageGhostCount > capacity)
                destinationState.storedHostageGhostCount = capacity;
        }

        if (logTransitions)
        {
            Debug.Log(
                $"[RoomManager] Transferred rescued hostages | " +
                $"sourceRoom={sourceRoomCoord} | requested={requestedCount} | accepted={acceptedCount} | rejected={rejectedCount} | " +
                $"destinationCampfire={destinationCoord} | storedNow={destinationState.storedHostageGhostCount} | " +
                $"remainingSpaceNow={GetRemainingCampfireHostageSpace(destinationCoord)} | capacity={GetCampfireHostageCapacity()}"
            );
        }
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

            // Challenge rooms
            entry.challengeType = (int)s.challengeType;
            entry.challengeCompleted = s.challengeCompleted;
            entry.lastChallengeCompletedStep = s.lastChallengeCompletedStep;

            entry.encounterInitialized = s.encounterInitialized;
            entry.combatLevel = s.combatLevel;
            entry.encounterSeed = s.encounterSeed;

            entry.lastVisitedStep = s.lastVisitedStep;
            entry.lastClearedStep = s.lastClearedStep;
            entry.repopulationBlockedUntilStep = s.repopulationBlockedUntilStep;
            entry.timesRepopulated = s.timesRepopulated;

            entry.hasHostageGhosts = s.hasHostageGhosts;
            entry.hostageGhostCount = s.hostageGhostCount;
            entry.hostageGhostsRescued = s.hostageGhostsRescued;
            entry.storedHostageGhostCount = s.storedHostageGhostCount;

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
                
                ChallengeType restoredChallengeType = System.Enum.IsDefined(typeof(ChallengeType), e.challengeType)
                    ? (ChallengeType)e.challengeType
                    : ChallengeType.None;

                RoomState s = new RoomState(
                    e.visited,
                    e.cleared,
                    e.remainingEnemies,
                    restoredRoomType,
                    e.lastVisitedStep,
                    e.lastClearedStep,
                    e.repopulationBlockedUntilStep,
                    e.timesRepopulated,
                    e.hasHostageGhosts,
                    e.hostageGhostCount,
                    e.hostageGhostsRescued,
                    e.storedHostageGhostCount,
                    restoredChallengeType,
                    e.challengeCompleted,
                    e.lastChallengeCompletedStep
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

    // -----------------------------
    // H6: hostage campfire capacity helpers
    // -----------------------------

    public int GetCampfireHostageCapacity()
    {
        return Mathf.Max(0, maxStoredHostageGhostsPerCampfire);
    }

    public int GetRemainingCampfireHostageSpace(Vector2Int campfireCoord)
    {
        RoomState state = GetOrCreateState(campfireCoord);
        if (state == null)
            return 0;

        if (state.roomType != RoomType.Campfire)
            return 0;

        int capacity = GetCampfireHostageCapacity();
        int stored = Mathf.Max(0, state.storedHostageGhostCount);

        return Mathf.Max(0, capacity - stored);
    }

    public bool IsCampfireAtHostageCapacity(Vector2Int campfireCoord)
    {
        RoomState state = GetOrCreateState(campfireCoord);
        if (state == null)
            return true;

        if (state.roomType != RoomType.Campfire)
            return true;

        int capacity = GetCampfireHostageCapacity();
        int stored = Mathf.Max(0, state.storedHostageGhostCount);

        return stored >= capacity;
    }

    public Vector2Int GetCurrentHostageDestinationCampfireCoord()
    {
        return hasActivatedCampfireCheckpoint
            ? lastActivatedCampfireCoord
            : Vector2Int.zero;
    }

    public int GetStoredHostageGhostCount(Vector2Int campfireCoord)
    {
        RoomState state = GetOrCreateState(campfireCoord);
        if (state == null)
            return 0;

        if (state.roomType != RoomType.Campfire)
            return 0;

        return Mathf.Max(0, state.storedHostageGhostCount);
    }

    public bool ConsumeCampfireRecoveryFeedbackSuppression()
    {
        bool wasSuppressed = SuppressNextCampfireRecoveryFeedback;
        SuppressNextCampfireRecoveryFeedback = false;
        return wasSuppressed;
    }

    public void SuppressNextCampfireRecoveryPopup()
    {
        SuppressNextCampfireRecoveryFeedback = true;
    }

    // -----------------------------
    // 8.0-8.2: challenge room helpers
    // -----------------------------

    private ChallengeType DetermineChallengeTypeForNewState(Vector2Int coord)
    {
        int hash = Mathf.Abs((coord.x * 73856093) ^ (coord.y * 19349663) ^ 83492791);

        int roll = hash % 10;

        if (roll <= 3) return ChallengeType.Betting;   // 0,1,2,3 = 40%
        if (roll <= 6) return ChallengeType.Gluttony;  // 4,5,6 = 30%
        if (roll <= 8) return ChallengeType.Sloth;     // 7,8 = 20%
        return ChallengeType.Lie;                      // 9 = 10%
    }

    private void HandleChallengeRoomEntered(Vector2Int coord, RoomState state, bool allowEffectClear)
    {
        if (state == null)
            return;

        if (state.roomType != RoomType.Challenge)
            return;

        if (logTransitions)
        {
            Debug.Log(
                $"[RoomManager] Entered challenge room at {coord} " +
                $"| challengeType={state.challengeType} | " +
                $"challengeCompleted={state.challengeCompleted} | " +
                $"allowEffectClear={allowEffectClear}"
            );
        }

        if (allowEffectClear)
        {
            if (challengeEffectManager == null)
            {
                if (logTransitions)
                    Debug.LogWarning("[RoomManager] ChallengeEffectManager not found while entering challenge room.");
            }
            else
            {
                challengeEffectManager.ClearEffectsThatExpireOnNextChallengeEntry();
            }
        }

        if (currentRoom == null)
        {
            if (logTransitions)
                Debug.LogWarning($"[RoomManager] currentRoom is null while handling challenge room {coord}.");
            return;
        }

        ChallengeRoomController controller = currentRoom.GetComponentInChildren<ChallengeRoomController>(true);
        if (controller == null)
        {
            if (logTransitions)
            {
                Debug.LogWarning(
                    $"[RoomManager] Challenge room at {coord} has no ChallengeRoomController on prefab " +
                    $"'{currentRoom.name}'."
                );
            }
            return;
        }

        controller.Initialize(this, coord, state);

        if (logTransitions)
        {
            Debug.Log(
                $"[RoomManager] Initialized ChallengeRoomController for room {coord} " +
                $"| challengeType={state.challengeType} | challengeCompleted={state.challengeCompleted}"
            );
        }
    }

    public int GetChallengeAgeSinceCompletion(RoomState state)
    {
        if (state == null)
            return -1;

        if (state.roomType != RoomType.Challenge)
            return -1;

        if (!state.challengeCompleted)
            return -1;

        if (state.lastChallengeCompletedStep < 0)
            return -1;

        return runStepCount - state.lastChallengeCompletedStep;
    }

    public bool CanChallengeRoomResetNow(RoomState state)
    {
        if (state == null)
            return false;

        if (state.roomType != RoomType.Challenge)
            return false;

        if (!state.challengeCompleted)
            return false;

        if (state.lastChallengeCompletedStep < 0)
            return false;

        int ageSinceCompletion = GetChallengeAgeSinceCompletion(state);
        if (ageSinceCompletion < 0)
            return false;

        return ageSinceCompletion >= challengeResetDelaySteps;
    }

    private bool TryResetChallengeRoomState(Vector2Int coord, RoomState state)
    {
        if (!CanChallengeRoomResetNow(state))
            return false;

        int oldCompletedStep = state.lastChallengeCompletedStep;
        int ageSinceCompletion = GetChallengeAgeSinceCompletion(state);

        state.challengeCompleted = false;
        state.lastChallengeCompletedStep = -1;

        if (logTransitions)
        {
            Debug.Log(
                $"[RoomManager] Reset challenge room {coord} | " +
                $"oldLastChallengeCompletedStep={oldCompletedStep} | " +
                $"ageSinceCompletion={ageSinceCompletion} | " +
                $"resetDelay={challengeResetDelaySteps}"
            );
        }

        return true;
    }
}