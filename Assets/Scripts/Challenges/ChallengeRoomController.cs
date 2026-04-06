using System.Collections.Generic;
using UnityEngine;

public class ChallengeRoomController : MonoBehaviour
{
    public enum ChallengeRoomFlowState
    {
        None,
        Intro,
        AwaitingStart,
        Running,
        Resolved,
        Exiting
    }

    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ChallengeEffectManager challengeEffectManager;
    [SerializeField] private RunSaveManager runSaveManager;

    [Header("Debug")]
    [SerializeField] private bool logFlow = true;

    [Header("Test Runtime")]
    [SerializeField] private bool autoBeginWhenAwaitingStart = true;

    private Vector2Int roomCoord;
    private RoomState roomState;
    private ChallengeType challengeType = ChallengeType.None;
    private ChallengeRoomFlowState flowState = ChallengeRoomFlowState.None;
    private ChallengeResult lastResult;

    private readonly List<IChallengeRuntime> discoveredRuntimes = new List<IChallengeRuntime>();
    private IChallengeRuntime activeChallengeRuntime;
    private ChallengeRoomContext activeContext;

    private bool isChallengeUIOpen;

    public Vector2Int RoomCoord => roomCoord;
    public RoomState RoomState => roomState;
    public ChallengeType ChallengeType => challengeType;
    public ChallengeRoomFlowState FlowState => flowState;
    public ChallengeResult LastResult => lastResult;
    public ChallengeRoomContext ActiveContext => activeContext;
    public IChallengeRuntime ActiveChallengeRuntime => activeChallengeRuntime;

    public bool HasValidRoomState => roomState != null;
    public bool IsChallengeRoom => roomState != null && roomState.roomType == RoomType.Challenge;
    public bool IsChallengeCompleted => roomState != null && roomState.challengeCompleted;
    public bool IsRunning => flowState == ChallengeRoomFlowState.Running;
    public bool IsResolved => flowState == ChallengeRoomFlowState.Resolved;
    public bool HasActiveRuntime => activeChallengeRuntime != null;
    public bool IsChallengeUIOpen => isChallengeUIOpen;

    public System.Action<ChallengeRoomController, ChallengeRoomFlowState> OnFlowStateChanged;
    public System.Action<ChallengeRoomController> OnChallengeRoomInitialized;
    public System.Action<ChallengeRoomController, ChallengeResult> OnChallengeResolved;
    public System.Action<ChallengeRoomController, bool> OnChallengeUIOpenChanged;

    private void Awake()
    {
        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        if (challengeEffectManager == null)
            challengeEffectManager = FindFirstObjectByType<ChallengeEffectManager>();

        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();

        RefreshRuntimeCache();
    }

    /// <summary>
    /// 8.2 entry point. RoomManager will call this when a Challenge room is entered.
    /// </summary>
    public void Initialize(RoomManager manager, Vector2Int coord, RoomState state)
    {
        roomManager = manager != null ? manager : roomManager;
        roomCoord = coord;
        roomState = state;
        challengeType = state != null ? state.challengeType : ChallengeType.None;
        lastResult = null;
        activeContext = null;
        activeChallengeRuntime = null;
        isChallengeUIOpen = false;

        RefreshRuntimeCache();

        if (roomState == null)
        {
            SetFlowState(ChallengeRoomFlowState.None);
            Log($"Initialize called with NULL RoomState at {coord}.");
            return;
        }

        if (roomState.roomType != RoomType.Challenge)
        {
            SetFlowState(ChallengeRoomFlowState.None);
            Log(
                $"Initialize called for non-challenge room at {coord}. " +
                $"roomType={roomState.roomType}"
            );
            return;
        }

        InitializeChallengeRuntime();

        if (roomState.challengeCompleted)
        {
            SetFlowState(ChallengeRoomFlowState.Resolved);
            Log(
                $"Initialized COMPLETED challenge room at {coord} | " +
                $"challengeType={challengeType}"
            );
        }
        else
        {
            SetFlowState(ChallengeRoomFlowState.Intro);
            Log(
                $"Initialized active challenge room at {coord} | " +
                $"challengeType={challengeType} | challengeCompleted={roomState.challengeCompleted}"
            );
        }

        OnChallengeRoomInitialized?.Invoke(this);
    }

    public void EnterAwaitingStart()
    {
        if (!CanOperateOnChallengeRoom())
            return;

        if (roomState.challengeCompleted)
        {
            Log($"Ignored EnterAwaitingStart in completed room {roomCoord}.");
            return;
        }

        SetFlowState(ChallengeRoomFlowState.AwaitingStart);

        if (autoBeginWhenAwaitingStart)
            StartCoroutine(CoBeginChallengeNextFrame());
    }

    private System.Collections.IEnumerator CoBeginChallengeNextFrame()
    {
        yield return null;
        BeginChallenge();
    }

    public void BeginChallenge()
    {
        if (!CanOperateOnChallengeRoom())
            return;

        if (roomState.challengeCompleted)
        {
            Log($"Ignored BeginChallenge in completed room {roomCoord}.");
            return;
        }

        if (isChallengeUIOpen)
        {
            Log(
                $"Ignored BeginChallenge in room {roomCoord} because challenge UI is still open."
            );
            return;
        }

        if (activeChallengeRuntime == null)
        {
            Log(
                $"BeginChallenge requested in room {roomCoord}, but no runtime matched " +
                $"challengeType={challengeType}."
            );
            return;
        }

        SetFlowState(ChallengeRoomFlowState.Running);
        activeChallengeRuntime.BeginChallenge();
    }

    /// <summary>
    /// Central room-owned UI blocking handshake for Challenge UI.
    /// Cursor is handled separately by CursorHider.
    /// </summary>
    public void SetChallengeUIOpen(bool isOpen, string reason = "")
    {
        if (isChallengeUIOpen == isOpen)
            return;

        isChallengeUIOpen = isOpen;
        UIInputBlocker.BlockGameplayInput = isChallengeUIOpen;

        Log(
            $"Challenge UI open -> {isChallengeUIOpen}" +
            $"{(string.IsNullOrWhiteSpace(reason) ? "" : $" | reason={reason}")} | " +
            $"coord={roomCoord} | challengeType={challengeType}"
        );

        OnChallengeUIOpenChanged?.Invoke(this, isChallengeUIOpen);
    }

    /// <summary>
    /// Legacy/simple resolution path kept temporarily for compatibility.
    /// Prefer ResolveWithResult(...) going forward.
    /// </summary>
    public void MarkResolved(bool markCompleted)
    {
        ChallengeOutcome outcome = markCompleted
            ? ChallengeOutcome.Success
            : ChallengeOutcome.None;

        ChallengeResult fallbackResult = new ChallengeResult(
            challengeType,
            outcome,
            markCompleted,
            true,
            markCompleted ? "Challenge resolved." : "Challenge state changed."
        );

        ResolveWithResult(fallbackResult);
    }

    public void ResolveWithResult(ChallengeResult result)
    {
        if (!CanOperateOnChallengeRoom())
            return;

        if (result == null)
        {
            Log($"Ignored ResolveWithResult in room {roomCoord} because result is null.");
            return;
        }

        if (IsResolved)
        {
            Log($"Ignored ResolveWithResult in room {roomCoord} because the room is already resolved.");
            return;
        }

        if (result.challengeType == ChallengeType.None)
            result.challengeType = challengeType;

        ApplyGrantedEffects(result);

        lastResult = result;

        if (result.markCompleted)
        {
            if (roomManager != null)
                roomManager.MarkChallengeRoomCompleted(roomCoord);
            else
                roomState.challengeCompleted = true;
        }

        SetChallengeUIOpen(false, "Challenge resolved");
        SetFlowState(ChallengeRoomFlowState.Resolved);

        ForceImmediateSave();

        Log(
            $"Challenge room resolved at {roomCoord} | " +
            $"result={result}"
        );

        OnChallengeResolved?.Invoke(this, result);
    }

    public void EnterExiting()
    {
        if (!CanOperateOnChallengeRoom())
            return;

        SetChallengeUIOpen(false, "Entering exiting state");
        SetFlowState(ChallengeRoomFlowState.Exiting);
    }

    public void CancelActiveChallengeRuntime()
    {
        if (activeChallengeRuntime == null)
            return;

        activeChallengeRuntime.CancelChallenge();
        SetChallengeUIOpen(false, "Challenge runtime cancelled");

        Log($"Cancelled active runtime for challengeType={challengeType} in room {roomCoord}.");
    }

    public void ResetRuntimeOnlyState()
    {
        lastResult = null;

        if (activeChallengeRuntime != null)
            activeChallengeRuntime.ResetRuntimeState();

        SetChallengeUIOpen(false, "Runtime-only reset");

        if (!HasValidRoomState)
        {
            SetFlowState(ChallengeRoomFlowState.None);
            return;
        }

        if (!IsChallengeRoom)
        {
            SetFlowState(ChallengeRoomFlowState.None);
            return;
        }

        SetFlowState(roomState.challengeCompleted
            ? ChallengeRoomFlowState.Resolved
            : ChallengeRoomFlowState.Intro);
    }

    private void ApplyGrantedEffects(ChallengeResult result)
    {
        if (result == null)
            return;

        if (!result.HasEffects)
        {
            Log($"No granted effects to apply for challengeType={challengeType} in room {roomCoord}.");
            return;
        }

        if (challengeEffectManager == null)
        {
            Log($"Cannot apply challenge effects in room {roomCoord} because ChallengeEffectManager is missing.");
            return;
        }

        for (int i = 0; i < result.grantedEffects.Count; i++)
        {
            ChallengeEffectEntry effect = result.grantedEffects[i];
            if (effect == null)
                continue;

            ChallengeEffectEntry appliedEffect = new ChallengeEffectEntry(
                effect.sourceChallenge == ChallengeType.None ? challengeType : effect.sourceChallenge,
                effect.effectType,
                effect.value,
                effect.clearsOnNextChallengeEntry,
                effect.debugLabel
            );

            challengeEffectManager.AddEffect(appliedEffect);

            Log(
                $"Applied challenge effect | " +
                $"source={appliedEffect.sourceChallenge} | " +
                $"type={appliedEffect.effectType} | value={appliedEffect.value} | " +
                $"clearsOnNextChallengeEntry={appliedEffect.clearsOnNextChallengeEntry} | " +
                $"label={appliedEffect.debugLabel}"
            );
        }
    }

    private void ForceImmediateSave()
    {
        if (runSaveManager == null)
        {
            Log($"RunSaveManager not found. Could not force immediate save after resolving room {roomCoord}.");
            return;
        }

        runSaveManager.Save();
        Log($"Forced immediate save after resolving challenge room {roomCoord}.");
    }

    private void RefreshRuntimeCache()
    {
        discoveredRuntimes.Clear();

        MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IChallengeRuntime runtime)
                discoveredRuntimes.Add(runtime);
        }

        Log($"Discovered {discoveredRuntimes.Count} challenge runtime(s) on room object / children.");
    }

    private void InitializeChallengeRuntime()
    {
        activeChallengeRuntime = null;
        activeContext = null;

        for (int i = 0; i < discoveredRuntimes.Count; i++)
        {
            IChallengeRuntime runtime = discoveredRuntimes[i];
            if (runtime == null)
                continue;

            if (runtime.ChallengeType != challengeType)
                continue;

            activeChallengeRuntime = runtime;
            break;
        }

        if (activeChallengeRuntime == null)
        {
            Log(
                $"No matching challenge runtime found for challengeType={challengeType} " +
                $"in room {roomCoord}."
            );
            return;
        }

        activeContext = new ChallengeRoomContext(
            roomManager,
            challengeEffectManager,
            this,
            roomState,
            roomCoord
        );

        activeChallengeRuntime.Initialize(activeContext);

        Log(
            $"Initialized challenge runtime {activeChallengeRuntime.GetType().Name} " +
            $"for challengeType={challengeType} in room {roomCoord}."
        );
    }

    private bool CanOperateOnChallengeRoom()
    {
        if (roomState == null)
        {
            Log("Ignored operation because roomState is null.");
            return false;
        }

        if (roomState.roomType != RoomType.Challenge)
        {
            Log($"Ignored operation because room {roomCoord} is not Challenge. roomType={roomState.roomType}");
            return false;
        }

        return true;
    }

    private void SetFlowState(ChallengeRoomFlowState newState)
    {
        if (flowState == newState)
            return;

        flowState = newState;

        Log(
            $"Flow state -> {flowState} | " +
            $"coord={roomCoord} | challengeType={challengeType}"
        );

        OnFlowStateChanged?.Invoke(this, flowState);
    }

    private void Log(string message)
    {
        if (!logFlow)
            return;

        Debug.Log($"[ChallengeRoomController] {message}", this);
    }
}