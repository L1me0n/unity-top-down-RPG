using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LieChallengeRuntime : MonoBehaviour, IChallengeRuntime
{
    [Header("Identity")]
    [SerializeField] private ChallengeType challengeType = ChallengeType.Lie;

    [Header("References")]
    [SerializeField] private ChallengeRoomController controller;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ChallengeEffectManager challengeEffectManager;
    [SerializeField] private LieSneakController lieSneakController;
    [SerializeField] private RunSaveManager runSaveManager;

    [Header("Sneak Settings")]
    [SerializeField, Range(0f, 1f)] private float baseSneakSuccessChance = 0.40f;
    [SerializeField, Range(0f, 1f)] private float successChancePerHellhoundPoint = 0.005f;

    [Header("Trial Settings")]
    [SerializeField] private int minForcedTrials = 1;
    [SerializeField] private int maxForcedTrials = 3;
    [SerializeField] private bool avoidImmediateDuplicateTrials = true;

    [Header("Lie Effects")]
    [SerializeField] private bool lieFailClearsOnNextChallengeEntry = true;

    [Header("Debug")]
    [SerializeField] private bool logRuntime = true;

    private ChallengeRoomContext context;
    private LieChallengeState state = new LieChallengeState();
    private bool isInitialized;
    private bool subscribedToRoomTransition;

    public ChallengeType ChallengeType => challengeType;
    public LieChallengeState State => state;
    public bool IsInitialized => isInitialized;
    public bool IsAwaitingChoice => state != null && state.runtimeState == LieChallengeState.LieRuntimeState.IntroChoice;
    public bool HasChosenRoute => state != null && state.HasChosenRoute;
    public bool IsSneakRoute => state != null && state.IsSneakRoute;
    public bool IsTrialRoute => state != null && state.IsTrialRoute;

    public float SneakSuccessChance01 => GetSneakSuccessChance();
    public float HellhoundLuckPoints => GetHellhoundPointsEstimate();
    public string ForcedTrialSequenceSummary => BuildSequenceSummary(state != null ? state.ForcedTrials : null);

    public System.Action<LieChallengeRuntime> OnStateChanged;
    public System.Action<LieChallengeRuntime, LieChallengeState.LieRuntimeState> OnRuntimeStateChanged;
    public System.Action<LieChallengeRuntime, LieChallengeState.LieRoute> OnRouteChosen;
    public System.Action<LieChallengeRuntime, bool> OnSneakOutcomeRolled;
    public System.Action<LieChallengeRuntime, IReadOnlyList<LieForcedTrialType>> OnTrialSequencePrepared;
    public System.Action<LieChallengeRuntime, string> OnRouteStatusTextChanged;
    public System.Action<LieChallengeRuntime, LieForcedTrialType> OnForcedTrialStarted;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<ChallengeRoomController>();

        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        if (challengeEffectManager == null)
            challengeEffectManager = FindFirstObjectByType<ChallengeEffectManager>();

        if (lieSneakController == null)
            lieSneakController = FindFirstObjectByType<LieSneakController>();

        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();
    }

    private void OnDestroy()
    {
        UnsubscribeFromRoomTransition();
    }

    public void Initialize(ChallengeRoomContext context)
    {
        this.context = context;
        isInitialized = context != null;

        if (controller == null && context != null)
            controller = context.RoomController;

        if (roomManager == null && context != null)
            roomManager = context.RoomManager;

        if (challengeEffectManager == null && context != null)
            challengeEffectManager = context.ChallengeEffectManager;

        if (lieSneakController == null)
            lieSneakController = FindFirstObjectByType<LieSneakController>();

        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();

        SubscribeToRoomTransition();

        state = new LieChallengeState();
        state.ResetForNewRun();

        if (TryRestoreFromRoomState())
        {
            ReapplyLiveSystemsForRestoredState();
            NotifyStateChanged();

            Log(
                $"Initialized Lie runtime from SAVED PROGRESS | coord={context?.Coord} | " +
                $"{state.BuildDebugSummary()}"
            );
            return;
        }

        SetState(LieChallengeState.LieRuntimeState.IntroChoice);
        NotifyStateChanged();

        Log(
            $"Initialized Lie runtime | coord={context?.Coord} | " +
            $"{state.BuildDebugSummary()}"
        );
    }

    public void BeginChallenge()
    {
        if (!CanOperate())
            return;

        if (TryRestoreFromRoomState())
        {
            ReapplyLiveSystemsForRestoredState();
            RestoreChallengeUIHandshakeForCurrentState();
            NotifyStateChanged();

            Log(
                $"Lie challenge resumed from saved progress | coord={context.Coord} | " +
                $"{state.BuildDebugSummary()}"
            );
            return;
        }

        state.ResetForNewRun();

        if (controller != null)
            controller.SetChallengeUIOpen(true, "Lie intro choice open");

        SetState(LieChallengeState.LieRuntimeState.IntroChoice);
        PersistStateAndSave();
        NotifyStateChanged();

        EmitRouteStatus("Lucifer waits. Choose your path.");

        Log(
            $"Lie challenge begin | coord={context.Coord} | " +
            $"baseSneakChance={baseSneakSuccessChance:0.###} | " +
            $"perHellhound={successChancePerHellhoundPoint:0.###}"
        );
    }

    public void CancelChallenge()
    {
        if (state == null)
            state = new LieChallengeState();

        state.Cancel();

        if (lieSneakController != null)
            lieSneakController.EndLieSneak();

        ClearPersistedState();

        if (controller != null)
            controller.SetChallengeUIOpen(false, "Lie challenge cancelled");

        SetState(LieChallengeState.LieRuntimeState.Cancelled);
        NotifyStateChanged();

        Log("Lie challenge cancelled.");
    }

    public void ResetRuntimeState()
    {
        if (state == null)
            state = new LieChallengeState();

        state.ResetForNewRun();

        if (lieSneakController != null)
            lieSneakController.EndLieSneak();

        ClearPersistedState();

        SetState(LieChallengeState.LieRuntimeState.None);

        if (controller != null)
            controller.SetChallengeUIOpen(false, "Lie runtime reset");

        NotifyStateChanged();
        Log("Lie runtime-only state reset.");
    }

    public bool ChooseSneakPast()
    {
        if (!CanChooseRoute())
            return false;

        bool willSucceed = RollSneakOutcome();

        state.ChooseSneakRoute(willSucceed);

        if (controller != null)
            controller.SetChallengeUIOpen(false, "Lie route chosen: Sneak Past");

        SetState(LieChallengeState.LieRuntimeState.SneakPreparing);

        Log(
            $"Lie route chosen: Sneak Past | coord={context?.Coord} | " +
            $"hellhoundPoints={GetHellhoundPointsEstimate():0.###} | " +
            $"successChance={GetSneakSuccessChance():0.###} | " +
            $"rolledOutcome={(willSucceed ? "SUCCESS" : "FAIL")}"
        );

        OnRouteChosen?.Invoke(this, LieChallengeState.LieRoute.SneakPast);
        OnSneakOutcomeRolled?.Invoke(this, willSucceed);

        EmitRouteStatus(
            willSucceed
                ? "You fade into the lie. Leave through a door before it notices."
                : "You fade... and Lucifer lets you believe it worked."
        );

        ActivateSneakRoute();
        PersistStateAndSave();
        NotifyStateChanged();

        return true;
    }

    public bool ChooseAcceptTrials()
    {
        if (!CanChooseRoute())
            return false;

        List<LieForcedTrialType> sequence = BuildForcedTrialSequence();

        state.ChooseTrialRoute(sequence);

        if (controller != null)
            controller.SetChallengeUIOpen(false, "Lie route chosen: Accept Trials");

        SetState(LieChallengeState.LieRuntimeState.TrialPreparing);

        Log(
            $"Lie route chosen: Accept Trials | coord={context?.Coord} | " +
            $"sequence={BuildSequenceSummary(sequence)}"
        );

        OnRouteChosen?.Invoke(this, LieChallengeState.LieRoute.AcceptTrials);
        OnTrialSequencePrepared?.Invoke(this, state.ForcedTrials);
        EmitRouteStatus($"Lucifer chooses: {ForcedTrialSequenceSummary}");

        BeginTrialRoute();
        PersistStateAndSave();
        NotifyStateChanged();

        return true;
    }

    private void ActivateSneakRoute()
    {
        if (lieSneakController != null)
            lieSneakController.BeginLieSneak(!state.sneakWillSucceed);

        SetState(LieChallengeState.LieRuntimeState.SneakActive);
    }

    private void BeginTrialRoute()
    {
        if (TryAdvanceToFirstForcedTrial(out LieForcedTrialType firstTrial))
        {
            OnForcedTrialStarted?.Invoke(this, firstTrial);
            EmitRouteStatus($"First forced trial: {firstTrial}");
        }
        else
        {
            ResolveLieTrialChainComplete();
        }
    }

    public bool TriggerSneakFailureNow()
    {
        if (!CanOperate())
            return false;

        if (!state.IsSneakRoute)
            return false;

        if (state.runtimeState != LieChallengeState.LieRuntimeState.SneakActive)
            return false;

        if (state.sneakWillSucceed)
            return false;

        if (lieSneakController != null)
            lieSneakController.TryTriggerLieSneakFailure();

        state.FinishSneakAttempt();
        SetState(LieChallengeState.LieRuntimeState.SneakResolved);

        ChallengeResult failResult = ChallengeResult
            .MakeFail(
                ChallengeType.Lie,
                summaryText: "Lucifer saw through the sneak. Your stolen courage curdles."
            )
            .AddEffect(new ChallengeEffectEntry(
                ChallengeType.Lie,
                ChallengeEffectType.TempStatHalving,
                1f,
                lieFailClearsOnNextChallengeEntry,
                "Lie sneak failed: temporary stat halving"
            ));

        if (lieSneakController != null)
            lieSneakController.EndLieSneak();

        ClearPersistedState();

        controller.ResolveWithResult(failResult);
        EmitRouteStatus("The lie breaks. Your strength is halved.");

        NotifyStateChanged();
        return true;
    }

    public bool ResolveSneakSuccessNow()
    {
        if (!CanOperate())
            return false;

        if (!state.IsSneakRoute)
            return false;

        if (state.runtimeState != LieChallengeState.LieRuntimeState.SneakActive)
            return false;

        if (!state.sneakWillSucceed)
            return false;

        state.FinishSneakAttempt();
        SetState(LieChallengeState.LieRuntimeState.SneakResolved);

        if (lieSneakController != null)
            lieSneakController.EndLieSneak();

        ChallengeResult successResult = ChallengeResult.MakeSuccess(
            ChallengeType.Lie,
            summaryText: "You slipped through Lucifer's lie."
        );

        ClearPersistedState();

        controller.ResolveWithResult(successResult);
        EmitRouteStatus("You pass unseen.");

        NotifyStateChanged();
        return true;
    }

    public bool ResolveForcedTrialStep(ChallengeResult subResult)
    {
        if (!CanOperate())
            return false;

        if (!state.IsTrialRoute)
            return false;

        if (state.runtimeState != LieChallengeState.LieRuntimeState.TrialRunning)
            return false;

        LieForcedTrialType current = state.CurrentForcedTrial;
        if (current == LieForcedTrialType.None)
            return false;

        if (subResult == null)
            return false;

        ApplyForcedTrialEffects(subResult);

        state.EnterTrialShowingResult($"Forced trial completed: {current}");
        EmitRouteStatus($"Forced trial completed: {current}");

        PersistStateAndSave();

        if (TryAdvanceToNextForcedTrial(out LieForcedTrialType nextTrial))
        {
            OnForcedTrialStarted?.Invoke(this, nextTrial);
            EmitRouteStatus($"Next forced trial: {nextTrial}");

            PersistStateAndSave();
            NotifyStateChanged();
            return true;
        }

        ResolveLieTrialChainComplete();
        NotifyStateChanged();
        return true;
    }

    private void ResolveLieTrialChainComplete()
    {
        state.EnterResolved("Lucifer's trial chain completed.");
        SetState(LieChallengeState.LieRuntimeState.Resolved);

        ChallengeResult result = ChallengeResult.MakeSuccess(
            ChallengeType.Lie,
            summaryText: "Lucifer's forced trials are complete."
        );

        ClearPersistedState();

        controller.ResolveWithResult(result);
        EmitRouteStatus("Lucifer's trial chain is complete.");
    }

    private void ApplyForcedTrialEffects(ChallengeResult subResult)
    {
        if (challengeEffectManager == null || subResult == null || !subResult.HasEffects)
            return;

        for (int i = 0; i < subResult.grantedEffects.Count; i++)
        {
            ChallengeEffectEntry effect = subResult.grantedEffects[i];
            if (effect == null)
                continue;

            if (effect.effectType == ChallengeEffectType.None)
                continue;

            ChallengeEffectEntry appliedEffect = new ChallengeEffectEntry(
                effect.sourceChallenge,
                effect.effectType,
                effect.value,
                effect.clearsOnNextChallengeEntry,
                effect.debugLabel
            );

            challengeEffectManager.AddEffect(appliedEffect);
        }
    }

    public float GetSneakSuccessChance()
    {
        float hellhoundPoints = GetHellhoundPointsEstimate();
        float chance = baseSneakSuccessChance + hellhoundPoints * successChancePerHellhoundPoint;
        return Mathf.Clamp01(chance);
    }

    public bool RollSneakOutcome()
    {
        if (context == null)
            return false;

        float successChance = GetSneakSuccessChance();
        float roll = BuildDeterministicSneakRoll01(context.Coord);

        return roll <= successChance;
    }

    public bool TryAdvanceToFirstForcedTrial(out LieForcedTrialType forcedTrialType)
    {
        forcedTrialType = LieForcedTrialType.None;

        if (!CanOperate())
            return false;

        if (!state.IsTrialRoute)
            return false;

        if (!state.MoveToNextTrial())
            return false;

        forcedTrialType = state.CurrentForcedTrial;

        SetState(LieChallengeState.LieRuntimeState.TrialRunning);

        Log(
            $"Advanced Lie trial sequence to first trial | " +
            $"index={state.currentTrialIndex} | forcedTrial={forcedTrialType}"
        );

        NotifyStateChanged();
        return true;
    }

    public bool TryAdvanceToNextForcedTrial(out LieForcedTrialType forcedTrialType)
    {
        forcedTrialType = LieForcedTrialType.None;

        if (!CanOperate())
            return false;

        if (!state.IsTrialRoute)
            return false;

        if (!state.MoveToNextTrial())
            return false;

        forcedTrialType = state.CurrentForcedTrial;

        SetState(LieChallengeState.LieRuntimeState.TrialRunning);

        Log(
            $"Advanced Lie trial sequence to next trial | " +
            $"index={state.currentTrialIndex} | forcedTrial={forcedTrialType}"
        );

        NotifyStateChanged();
        return true;
    }

    private List<LieForcedTrialType> BuildForcedTrialSequence()
    {
        List<LieForcedTrialType> result = new List<LieForcedTrialType>();

        int minCount = Mathf.Max(1, minForcedTrials);
        int maxCount = Mathf.Max(minCount, maxForcedTrials);

        int count = BuildDeterministicTrialCount(minCount, maxCount, context != null ? context.Coord : Vector2Int.zero);
        int seed = BuildLieSeed(context != null ? context.Coord : Vector2Int.zero);

        LieForcedTrialType previous = LieForcedTrialType.None;

        for (int i = 0; i < count; i++)
        {
            LieForcedTrialType next = BuildTrialTypeFromSeed(seed, i, previous);
            result.Add(next);
            previous = next;
        }

        return result;
    }

    private int BuildDeterministicTrialCount(int minCount, int maxCount, Vector2Int coord)
    {
        int range = (maxCount - minCount) + 1;
        int seed = Mathf.Abs(BuildLieSeed(coord) ^ 0x2F31A1);
        return minCount + (seed % range);
    }

    private LieForcedTrialType BuildTrialTypeFromSeed(int seed, int index, LieForcedTrialType previous)
    {
        LieForcedTrialType[] pool = new LieForcedTrialType[]
        {
            LieForcedTrialType.Betting,
            LieForcedTrialType.Gluttony,
            LieForcedTrialType.Sloth
        };

        if (pool.Length == 0)
            return LieForcedTrialType.None;

        int rolledIndex = Mathf.Abs(seed ^ (index * 92821) ^ 0x51F15) % pool.Length;
        LieForcedTrialType rolled = pool[rolledIndex];

        if (avoidImmediateDuplicateTrials && pool.Length > 1 && rolled == previous)
            rolled = pool[(rolledIndex + 1) % pool.Length];

        return rolled;
    }

    private float GetHellhoundPointsEstimate()
    {
        BranchProgression branchProgression = FindFirstObjectByType<BranchProgression>();
        if (branchProgression == null)
            return 0f;

        return Mathf.Max(0, branchProgression.Hellhound);
    }

    private float BuildDeterministicSneakRoll01(Vector2Int coord)
    {
        int seed = Mathf.Abs(BuildLieSeed(coord) ^ 0x7E13B9);
        int bucket = seed % 10000;
        return bucket / 10000f;
    }

    private int BuildLieSeed(Vector2Int coord)
    {
        int x = coord.x * 73856093;
        int y = coord.y * 19349663;
        int c = ((int)ChallengeType.Lie) * 83492791;
        return x ^ y ^ c;
    }

    private bool TryRestoreFromRoomState()
    {
        RoomState roomState = GetRoomState();
        if (roomState == null)
            return false;

        if (roomState.challengeCompleted)
            return false;

        if (!roomState.lieProgressActive)
            return false;

        state = new LieChallengeState();
        state.ResetForNewRun();

        LieChallengeState.LieRoute restoredRoute =
            System.Enum.IsDefined(typeof(LieChallengeState.LieRoute), roomState.lieChosenRoute)
                ? (LieChallengeState.LieRoute)roomState.lieChosenRoute
                : LieChallengeState.LieRoute.None;

        LieChallengeState.LieRuntimeState restoredRuntimeState =
            System.Enum.IsDefined(typeof(LieChallengeState.LieRuntimeState), roomState.lieRuntimeState)
                ? (LieChallengeState.LieRuntimeState)roomState.lieRuntimeState
                : LieChallengeState.LieRuntimeState.IntroChoice;

        if (restoredRoute == LieChallengeState.LieRoute.SneakPast)
        {
            state.ChooseSneakRoute(roomState.lieSneakWillSucceed);
            state.sneakOutcomeRolled = roomState.lieSneakOutcomeRolled;
            state.sneakAttemptFinished = roomState.lieSneakAttemptFinished;
            state.runtimeState = restoredRuntimeState;
        }
        else if (restoredRoute == LieChallengeState.LieRoute.AcceptTrials)
        {
            List<LieForcedTrialType> sequence = BuildSequenceFromRoomState(roomState);
            state.ChooseTrialRoute(sequence);
            state.trialsPrepared = roomState.lieTrialsPrepared;
            state.currentTrialIndex = roomState.lieCurrentTrialIndex;
            state.runtimeState = restoredRuntimeState;
        }
        else
        {
            state.EnterIntroChoice();
            state.runtimeState = LieChallengeState.LieRuntimeState.IntroChoice;
        }

        return true;
    }

    private void ReapplyLiveSystemsForRestoredState()
    {
        if (state == null)
            return;

        if (state.runtimeState == LieChallengeState.LieRuntimeState.SneakActive && lieSneakController != null)
        {
            lieSneakController.BeginLieSneak(!state.sneakWillSucceed);
        }
    }

    private void RestoreChallengeUIHandshakeForCurrentState()
    {
        if (controller == null || state == null)
            return;

        bool shouldOpenChoiceUI =
            state.runtimeState == LieChallengeState.LieRuntimeState.IntroChoice;

        controller.SetChallengeUIOpen(shouldOpenChoiceUI, "Lie restored runtime state");
    }

    private void PersistStateAndSave()
    {
        SaveStateToRoomState();

        if (runSaveManager != null)
            runSaveManager.Save();
    }

    private void ClearPersistedState()
    {
        RoomState roomState = GetRoomState();
        if (roomState == null)
            return;

        roomState.lieProgressActive = false;
        roomState.lieChosenRoute = 0;
        roomState.lieRuntimeState = 0;
        roomState.lieSneakOutcomeRolled = false;
        roomState.lieSneakWillSucceed = false;
        roomState.lieSneakAttemptFinished = false;
        roomState.lieTrialsPrepared = false;
        roomState.lieForcedTrialCount = 0;
        roomState.lieCurrentTrialIndex = -1;
        roomState.lieForcedTrial0 = 0;
        roomState.lieForcedTrial1 = 0;
        roomState.lieForcedTrial2 = 0;

        if (runSaveManager != null)
            runSaveManager.Save();
    }

    private void SaveStateToRoomState()
    {
        RoomState roomState = GetRoomState();
        if (roomState == null)
            return;

        roomState.lieProgressActive =
            state.runtimeState != LieChallengeState.LieRuntimeState.None &&
            state.runtimeState != LieChallengeState.LieRuntimeState.Resolved &&
            state.runtimeState != LieChallengeState.LieRuntimeState.Cancelled;

        roomState.lieChosenRoute = (int)state.chosenRoute;
        roomState.lieRuntimeState = (int)state.runtimeState;
        roomState.lieSneakOutcomeRolled = state.sneakOutcomeRolled;
        roomState.lieSneakWillSucceed = state.sneakWillSucceed;
        roomState.lieSneakAttemptFinished = state.sneakAttemptFinished;
        roomState.lieTrialsPrepared = state.trialsPrepared;
        roomState.lieForcedTrialCount = state.forcedTrialCount;
        roomState.lieCurrentTrialIndex = state.currentTrialIndex;

        roomState.lieForcedTrial0 = GetForcedTrialAt(0);
        roomState.lieForcedTrial1 = GetForcedTrialAt(1);
        roomState.lieForcedTrial2 = GetForcedTrialAt(2);
    }

    private int GetForcedTrialAt(int index)
    {
        if (state == null || state.ForcedTrials == null)
            return 0;

        if (index < 0 || index >= state.ForcedTrials.Count)
            return 0;

        return (int)state.ForcedTrials[index];
    }

    private List<LieForcedTrialType> BuildSequenceFromRoomState(RoomState roomState)
    {
        List<LieForcedTrialType> result = new List<LieForcedTrialType>();

        int count = Mathf.Clamp(roomState.lieForcedTrialCount, 0, 3);
        int[] raw = new int[]
        {
            roomState.lieForcedTrial0,
            roomState.lieForcedTrial1,
            roomState.lieForcedTrial2
        };

        for (int i = 0; i < count; i++)
        {
            if (!System.Enum.IsDefined(typeof(LieForcedTrialType), raw[i]))
                continue;

            LieForcedTrialType trial = (LieForcedTrialType)raw[i];
            if (trial == LieForcedTrialType.None)
                continue;

            result.Add(trial);
        }

        return result;
    }

    private RoomState GetRoomState()
    {
        if (context == null)
            return null;

        return context.RoomState;
    }

    private void SubscribeToRoomTransition()
    {
        if (roomManager == null || subscribedToRoomTransition)
            return;

        roomManager.OnBeforeTransitionRequested += HandleBeforeTransitionRequested;
        subscribedToRoomTransition = true;
    }

    private void UnsubscribeFromRoomTransition()
    {
        if (roomManager == null || !subscribedToRoomTransition)
            return;

        roomManager.OnBeforeTransitionRequested -= HandleBeforeTransitionRequested;
        subscribedToRoomTransition = false;
    }

    private void HandleBeforeTransitionRequested(Vector2Int fromCoord, RoomDirection _)
    {
        if (!CanOperate())
            return;

        if (context == null)
            return;

        if (fromCoord != context.Coord)
            return;

        if (!state.IsSneakRoute)
            return;

        if (state.runtimeState != LieChallengeState.LieRuntimeState.SneakActive)
            return;

        if (!state.sneakWillSucceed)
            return;

        ResolveSneakSuccessNow();
    }

    private bool CanChooseRoute()
    {
        if (!CanOperate())
            return false;

        if (state == null)
            return false;

        if (state.runtimeState != LieChallengeState.LieRuntimeState.IntroChoice)
        {
            Log(
                $"Route choice ignored because runtimeState={state.runtimeState}, " +
                $"expected IntroChoice."
            );
            return false;
        }

        if (state.HasChosenRoute)
        {
            Log("Route choice ignored because Lie route was already chosen.");
            return false;
        }

        return true;
    }

    private bool CanOperate()
    {
        if (!isInitialized || context == null)
        {
            Log("Lie operation ignored because runtime is not initialized.");
            return false;
        }

        if (context.ChallengeType != ChallengeType.Lie)
        {
            Log(
                $"Lie operation ignored because context challengeType is {context.ChallengeType}, " +
                $"expected {ChallengeType.Lie}."
            );
            return false;
        }

        return true;
    }

    private void EmitRouteStatus(string text)
    {
        OnRouteStatusTextChanged?.Invoke(this, text);
    }

    private void SetState(LieChallengeState.LieRuntimeState newState)
    {
        if (state == null)
            state = new LieChallengeState();

        if (state.runtimeState == newState)
            return;

        state.runtimeState = newState;

        Log(
            $"Lie runtime state -> {state.runtimeState} | " +
            $"coord={(context != null ? context.Coord.ToString() : "NoContext")}"
        );

        OnRuntimeStateChanged?.Invoke(this, state.runtimeState);
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke(this);
    }

    private string BuildSequenceSummary(IReadOnlyList<LieForcedTrialType> sequence)
    {
        if (sequence == null || sequence.Count == 0)
            return "None";

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < sequence.Count; i++)
        {
            if (i > 0)
                sb.Append(" -> ");

            sb.Append(sequence[i]);
        }

        return sb.ToString();
    }

    private void Log(string message)
    {
        if (!logRuntime)
            return;

        Debug.Log($"[LieChallengeRuntime] {message}", this);
    }
}