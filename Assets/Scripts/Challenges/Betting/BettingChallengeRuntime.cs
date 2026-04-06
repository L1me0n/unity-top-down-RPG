using UnityEngine;

public class BettingChallengeRuntime : MonoBehaviour, IChallengeRuntime
{
    public enum BettingRuntimeState
    {
        None,
        Setup,
        AwaitingBet,
        RaceStarting,
        RaceRunning,
        RaceFinishedShowingWinner,
        Resolved,
        Cancelled
    }

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Debug")]
    [SerializeField] private bool logRuntime = true;

    [Header("Optional Test Helpers")]
    [SerializeField] private bool autoEnterAwaitingBetOnBegin = true;

    [Header("Runtime Readonly")]
    [SerializeField] private BettingRuntimeState runtimeState = BettingRuntimeState.None;
    [SerializeField] private BettingWagerState wagerState = new BettingWagerState();
    [SerializeField] private int resolvedWinningLaneIndex = -1;

    private ChallengeRoomContext context;
    private ChallengeRoomController roomController;
    private bool isInitialized;

    public ChallengeType ChallengeType => ChallengeType.Betting;
    public BettingRuntimeState RuntimeState => runtimeState;
    public BettingWagerState WagerState => wagerState;
    public bool IsInitialized => isInitialized;

    public int SnapshotAP => wagerState != null ? wagerState.SnapshotAP : 0;
    public int MinimumTotalWager => wagerState != null ? wagerState.MinimumTotalWager : 0;
    public int TotalWager => wagerState != null ? wagerState.TotalWager : 0;
    public int RemainingAP => wagerState != null ? wagerState.RemainingAP : 0;
    public int ResolvedWinningLaneIndex => resolvedWinningLaneIndex;

    public bool IsAwaitingBet => runtimeState == BettingRuntimeState.AwaitingBet;
    public bool IsRaceStarting => runtimeState == BettingRuntimeState.RaceStarting;
    public bool IsRaceRunning => runtimeState == BettingRuntimeState.RaceRunning;
    public bool IsShowingWinner => runtimeState == BettingRuntimeState.RaceFinishedShowingWinner;
    public bool IsResolved => runtimeState == BettingRuntimeState.Resolved;

    public System.Action<BettingChallengeRuntime, BettingRuntimeState> OnRuntimeStateChanged;
    public System.Action<BettingChallengeRuntime> OnWagerStateChanged;
    public System.Action<BettingChallengeRuntime, int> OnRaceFinished;

    public void Initialize(ChallengeRoomContext context)
    {
        this.context = context;
        roomController = context != null ? context.RoomController : null;
        isInitialized = context != null;

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (wagerState == null)
            wagerState = new BettingWagerState();
        else
            wagerState.InitializeFromSnapshotAP(0);

        resolvedWinningLaneIndex = -1;
        SetRuntimeState(BettingRuntimeState.None, notify: false);

        if (!isInitialized)
        {
            Log("Initialize called with NULL context.");
            return;
        }

        if (context.ChallengeType != ChallengeType.Betting)
        {
            Log(
                $"Initialize ignored because context challengeType is {context.ChallengeType}, " +
                $"but runtime expects {ChallengeType.Betting}."
            );
            return;
        }

        NotifyWagerStateChanged();

        Log(
            $"Initialized Betting runtime | " +
            $"coord={context.Coord} | challengeType={context.ChallengeType}"
        );
    }

    public void BeginChallenge()
    {
        if (!CanOperate())
            return;

        if (IsResolved || IsRaceRunning || IsAwaitingBet)
            return;

        SetRuntimeState(BettingRuntimeState.Setup);
        SnapshotCurrentAPForBetting();

        Log(
            $"Betting challenge begin requested | " +
            $"coord={context.Coord} | " +
            $"{GetWagerDebugSummary()}"
        );

        if (autoEnterAwaitingBetOnBegin)
            EnterAwaitingBet();
    }

    public void CancelChallenge()
    {
        if (!isInitialized)
            return;

        SetRuntimeState(BettingRuntimeState.Cancelled);

        if (roomController != null)
            roomController.SetChallengeUIOpen(false, "Betting challenge cancelled");

        Log("Betting challenge cancelled.");
    }

    public void ResetRuntimeState()
    {
        if (wagerState == null)
            wagerState = new BettingWagerState();

        wagerState.InitializeFromSnapshotAP(0);
        resolvedWinningLaneIndex = -1;

        NotifyWagerStateChanged();

        SetRuntimeState(BettingRuntimeState.None);

        if (roomController != null)
            roomController.SetChallengeUIOpen(false, "Betting runtime reset");

        Log("Betting runtime reset.");
    }

    public void EnterAwaitingBet()
    {
        if (!CanOperate())
            return;

        SetRuntimeState(BettingRuntimeState.AwaitingBet);

        if (roomController != null)
            roomController.SetChallengeUIOpen(true, "Betting panel open");

        Log($"Betting runtime is now awaiting bet input | {GetWagerDebugSummary()}");
    }

    public bool TryStartRace()
    {
        if (!CanOperate())
            return false;

        if (!CanStartRace())
        {
            Log($"TryStartRace failed | invalid wager state | {GetWagerDebugSummary()}");
            return false;
        }

        if (roomController != null)
            roomController.SetChallengeUIOpen(false, "Betting race starting");

        SetRuntimeState(BettingRuntimeState.RaceStarting);

        Log($"Betting race start accepted | {GetWagerDebugSummary()}");

        return true;
    }

    public void EnterRaceRunning()
    {
        if (!CanOperate())
            return;

        SetRuntimeState(BettingRuntimeState.RaceRunning);
        Log("Betting runtime entered RaceRunning.");
    }

    public void NotifyRaceFinished(int winningLaneIndex)
    {
        if (!CanOperate())
            return;

        resolvedWinningLaneIndex = winningLaneIndex;
        SetRuntimeState(BettingRuntimeState.RaceFinishedShowingWinner);

        Log($"Betting race finished | winnerLane={winningLaneIndex}");

        OnRaceFinished?.Invoke(this, winningLaneIndex);
    }

    /// <summary>
    /// 8.3F:
    /// Builds a real ChallengeResult and resolves the room through ChallengeRoomController.
    /// This is the step that makes the room actually complete and persist.
    /// </summary>
    public void FinalizeWinnerPresentationAndResolve()
    {
        if (!CanOperate())
            return;

        if (roomController == null)
        {
            Log("Cannot resolve Betting challenge because RoomController is missing.");
            return;
        }

        ChallengeResult result = BuildChallengeResultFromResolvedRace();

        SetRuntimeState(BettingRuntimeState.Resolved);

        Log($"Resolving Betting challenge with result: {result}");

        roomController.ResolveWithResult(result);
    }

    public ChallengeResult BuildChallengeResultFromResolvedRace()
    {
        int winningBetAP = 0;
        int losingBetAP = 0;

        bool validOutcome = TryGetRaceOutcomeNumbers(
            resolvedWinningLaneIndex,
            out winningBetAP,
            out losingBetAP
        );

        if (!validOutcome)
        {
            ChallengeResult fallback = ChallengeResult.MakeFail(
                ChallengeType.Betting,
                "Betting resolved without a valid outcome."
            );

            return fallback;
        }

        bool wonAnyBet = winningBetAP > 0;

        string summary;
        ChallengeResult result;

        if (wonAnyBet)
        {
            summary =
                $"Bet won. Winner lane {resolvedWinningLaneIndex + 1}. " +
                $"Bonus HP +{winningBetAP}" +
                (losingBetAP > 0 ? $" | Locked AP {losingBetAP}" : "");

            result = ChallengeResult.MakeSuccess(
                ChallengeType.Betting,
                summary,
                markCompleted: true,
                consumeRoom: true
            );
        }
        else
        {
            summary =
                $"Bet lost. Winner lane {resolvedWinningLaneIndex + 1}. " +
                $"Locked AP {losingBetAP}";

            result = ChallengeResult.MakeFail(
                ChallengeType.Betting,
                summary,
                markCompleted: true,
                consumeRoom: true
            );
        }

        if (winningBetAP > 0)
        {
            result.AddEffect(new ChallengeEffectEntry(
                ChallengeType.Betting,
                ChallengeEffectType.BonusMaxHP,
                winningBetAP,
                true,
                $"Betting win: HP +{winningBetAP}"
            ));
        }

        if (losingBetAP > 0)
        {
            result.AddEffect(new ChallengeEffectEntry(
                ChallengeType.Betting,
                ChallengeEffectType.LockedAP,
                losingBetAP,
                true,
                $"Betting loss: Locked AP {losingBetAP}"
            ));
        }

        return result;
    }

    // =========================
    // Wager API
    // =========================

    public void SnapshotCurrentAPForBetting()
    {
        if (wagerState == null)
            wagerState = new BettingWagerState();

        int snapshotAP = playerStats != null ? playerStats.MaxAP : 0;
        wagerState.InitializeFromSnapshotAP(snapshotAP);
        NotifyWagerStateChanged();

        Log(
            $"SnapshotCurrentAPForBetting | " +
            $"playerAP={snapshotAP} | minimumTotalWager={wagerState.MinimumTotalWager}"
        );
    }

    public bool TryIncreaseLaneWager(int laneIndex, int amount = 1)
    {
        if (!CanEditWagers())
            return false;

        bool success = wagerState.TryAddToLane(laneIndex, amount);

        if (success)
            NotifyWagerStateChanged();

        Log(
            $"TryIncreaseLaneWager lane={laneIndex} amount={amount} | " +
            $"success={success} | {GetWagerDebugSummary()}"
        );

        return success;
    }

    public bool TryDecreaseLaneWager(int laneIndex, int amount = 1)
    {
        if (!CanEditWagers())
            return false;

        bool success = wagerState.TryRemoveFromLane(laneIndex, amount);

        if (success)
            NotifyWagerStateChanged();

        Log(
            $"TryDecreaseLaneWager lane={laneIndex} amount={amount} | " +
            $"success={success} | {GetWagerDebugSummary()}"
        );

        return success;
    }

    public int GetLaneWager(int laneIndex)
    {
        if (wagerState == null)
            return 0;

        return wagerState.GetLaneWager(laneIndex);
    }

    public bool CanStartRace()
    {
        return wagerState != null && wagerState.IsValidForRaceStart();
    }

    public bool TryGetRaceOutcomeNumbers(int winningLaneIndex, out int winningBetAP, out int losingBetAP)
    {
        winningBetAP = 0;
        losingBetAP = 0;

        if (wagerState == null)
            return false;

        if (winningLaneIndex < 0 || winningLaneIndex >= BettingWagerState.LaneCount)
            return false;

        if (!wagerState.IsValidForRaceStart())
            return false;

        winningBetAP = wagerState.GetWinningBetAmount(winningLaneIndex);
        losingBetAP = wagerState.GetLosingBetAmount(winningLaneIndex);
        return true;
    }

    public string GetWagerDebugSummary()
    {
        if (wagerState == null)
            return "No wager state.";

        return wagerState.BuildDebugSummary();
    }

    private bool CanEditWagers()
    {
        if (!CanOperate())
            return false;

        if (runtimeState != BettingRuntimeState.AwaitingBet)
        {
            Log(
                $"Wager edit ignored because runtimeState is {runtimeState}, " +
                $"expected {BettingRuntimeState.AwaitingBet}."
            );
            return false;
        }

        return true;
    }

    private bool CanOperate()
    {
        if (!isInitialized || context == null)
        {
            Log("Operation ignored because runtime has not been initialized.");
            return false;
        }

        if (context.ChallengeType != ChallengeType.Betting)
        {
            Log(
                $"Operation ignored because context challengeType is {context.ChallengeType}, " +
                $"expected {ChallengeType.Betting}."
            );
            return false;
        }

        return true;
    }

    private void NotifyWagerStateChanged()
    {
        OnWagerStateChanged?.Invoke(this);
    }

    private void SetRuntimeState(BettingRuntimeState newState, bool notify = true)
    {
        if (runtimeState == newState)
            return;

        runtimeState = newState;

        Log(
            $"Runtime state -> {runtimeState} | " +
            $"coord={(context != null ? context.Coord.ToString() : "NoContext")}"
        );

        if (notify)
            OnRuntimeStateChanged?.Invoke(this, runtimeState);
    }

    private void Log(string message)
    {
        if (!logRuntime)
            return;

        Debug.Log($"[BettingChallengeRuntime] {message}", this);
    }
}