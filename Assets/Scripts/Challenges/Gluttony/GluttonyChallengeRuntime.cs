using UnityEngine;

public class GluttonyChallengeRuntime : MonoBehaviour, IChallengeRuntime
{
    public enum GluttonyRuntimeState
    {
        None,
        Setup,
        AwaitingInput,
        ResolvingTurn,
        Resolved,
        Cancelled
    }

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Rules")]
    [SerializeField] private int minTargetFullness = 4;
    [SerializeField] private int maxTargetFullness = 8;
    [SerializeField] private int hpLossPerSafeFeed = 1;
    [SerializeField, Range(0f, 1f)] private float overfeedAPLossPercent = 0.5f;

    [Header("Feed Selection Range")]
    [SerializeField] private int minSelectableFeedAmount = 1;
    [SerializeField] private int maxSelectableFeedAmount = 6;
    [SerializeField] private int initialSelectedFeedAmount = 1;

    [Header("Optional Test Helpers")]
    [SerializeField] private bool autoEnterAwaitingInputOnBegin = true;

    [Header("Debug")]
    [SerializeField] private bool logRuntime = true;

    [Header("Runtime Readonly")]
    [SerializeField] private GluttonyRuntimeState runtimeState = GluttonyRuntimeState.None;
    [SerializeField] private GluttonyChallengeState challengeState = new GluttonyChallengeState();
    [SerializeField, TextArea] private string lastTurnSummary = "";
    [SerializeField] private int snapshotMaxHPOnEntry = 0;

    private ChallengeRoomContext context;
    private ChallengeRoomController roomController;
    private bool isInitialized;

    public ChallengeType ChallengeType => ChallengeType.Gluttony;
    public GluttonyRuntimeState RuntimeState => runtimeState;
    public GluttonyChallengeState ChallengeState => challengeState;
    public string LastTurnSummary => lastTurnSummary;
    public bool IsInitialized => isInitialized;

    public int FullnessTarget => challengeState != null ? challengeState.FullnessTarget : 0;
    public int FullnessCurrent => challengeState != null ? challengeState.FullnessCurrent : 0;
    public int RemainingToTarget => challengeState != null ? challengeState.RemainingToTarget : 0;

    public int SelectedFeedAmount => challengeState != null ? challengeState.SelectedFeedAmount : 0;
    public int MinSelectableFeedAmount => challengeState != null ? challengeState.MinSelectableFeedAmount : 0;
    public int MaxSelectableFeedAmount => challengeState != null ? challengeState.MaxSelectableFeedAmount : 0;

    public int SafeFeedsTaken => challengeState != null ? challengeState.SafeFeedsTaken : 0;
    public int AccumulatedTempHPLoss => challengeState != null ? challengeState.AccumulatedTempHPLoss : 0;

    // Success reward now equals the max HP snapshot taken when the challenge begins.
    public int SuccessBonusMaxHP => Mathf.Max(0, snapshotMaxHPOnEntry);
    public int SnapshotMaxHPOnEntry => snapshotMaxHPOnEntry;

    public bool IsAwaitingInput => runtimeState == GluttonyRuntimeState.AwaitingInput;
    public bool IsResolvingTurn => runtimeState == GluttonyRuntimeState.ResolvingTurn;
    public bool IsResolved => runtimeState == GluttonyRuntimeState.Resolved;

    public System.Action<GluttonyChallengeRuntime, GluttonyRuntimeState> OnRuntimeStateChanged;
    public System.Action<GluttonyChallengeRuntime, GluttonyTurnResult> OnTurnResolved;
    public System.Action<GluttonyChallengeRuntime> OnStateChanged;

    public void Initialize(ChallengeRoomContext context)
    {
        this.context = context;
        roomController = context != null ? context.RoomController : null;
        isInitialized = context != null;

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (challengeState == null)
            challengeState = new GluttonyChallengeState();

        lastTurnSummary = "";
        snapshotMaxHPOnEntry = 0;
        SetRuntimeState(GluttonyRuntimeState.None, notify: false);

        if (!isInitialized)
        {
            Log("Initialize called with NULL context.");
            return;
        }

        if (context.ChallengeType != ChallengeType.Gluttony)
        {
            Log(
                $"Initialize ignored because context challengeType is {context.ChallengeType}, " +
                $"but runtime expects {ChallengeType.Gluttony}."
            );
            return;
        }

        BuildFreshChallengeState();
        NotifyStateChanged();

        Log(
            $"Initialized Gluttony runtime | " +
            $"coord={context.Coord} | targetFullness={challengeState.FullnessTarget} | " +
            $"{challengeState.BuildDebugSummary()}"
        );
    }

    public void BeginChallenge()
    {
        if (!CanOperate())
            return;

        if (IsResolved || runtimeState == GluttonyRuntimeState.AwaitingInput)
            return;

        SetRuntimeState(GluttonyRuntimeState.Setup);

        // Snapshot the player's effective max HP at the moment the challenge begins.
        snapshotMaxHPOnEntry = GetCurrentEffectiveMaxHPForChallenge();

        if (autoEnterAwaitingInputOnBegin)
            EnterAwaitingInput();

        Log(
            $"Gluttony challenge begin requested | " +
            $"coord={context.Coord} | snapshotMaxHPOnEntry={snapshotMaxHPOnEntry} | " +
            $"{challengeState.BuildDebugSummary()}"
        );
    }

    public void CancelChallenge()
    {
        if (!isInitialized)
            return;

        SetRuntimeState(GluttonyRuntimeState.Cancelled);

        if (roomController != null)
            roomController.SetChallengeUIOpen(false, "Gluttony challenge cancelled");

        Log("Gluttony challenge cancelled.");
    }

    public void ResetRuntimeState()
    {
        if (challengeState == null)
            challengeState = new GluttonyChallengeState();

        BuildFreshChallengeState();
        lastTurnSummary = "";
        snapshotMaxHPOnEntry = 0;

        NotifyStateChanged();
        SetRuntimeState(GluttonyRuntimeState.None);

        if (roomController != null)
            roomController.SetChallengeUIOpen(false, "Gluttony runtime reset");

        Log("Gluttony runtime reset.");
    }

    public void EnterAwaitingInput()
    {
        if (!CanOperate())
            return;

        SetRuntimeState(GluttonyRuntimeState.AwaitingInput);

        if (roomController != null)
            roomController.SetChallengeUIOpen(true, "Gluttony panel open");

        Log($"Gluttony runtime is now awaiting input | {challengeState.BuildDebugSummary()}");
    }

    public bool TryIncreaseSelectedFeedAmount(int amount = 1)
    {
        if (!CanEditSelection())
            return false;

        bool changed = challengeState.TryIncreaseSelectedFeedAmount(amount);
        if (changed)
            NotifyStateChanged();

        Log(
            $"TryIncreaseSelectedFeedAmount amount={amount} | changed={changed} | " +
            $"{challengeState.BuildDebugSummary()}"
        );

        return changed;
    }

    public bool TryDecreaseSelectedFeedAmount(int amount = 1)
    {
        if (!CanEditSelection())
            return false;

        bool changed = challengeState.TryDecreaseSelectedFeedAmount(amount);
        if (changed)
            NotifyStateChanged();

        Log(
            $"TryDecreaseSelectedFeedAmount amount={amount} | changed={changed} | " +
            $"{challengeState.BuildDebugSummary()}"
        );

        return changed;
    }

    public bool TryConfirmFeed()
    {
        if (!CanEditSelection())
            return false;

        SetRuntimeState(GluttonyRuntimeState.ResolvingTurn);

        GluttonyTurnResult result = challengeState.ApplySelectedFeed(
            GetCurrentEffectiveMaxHPForChallenge(),
            Mathf.Max(0, hpLossPerSafeFeed)
        );

        lastTurnSummary = result.summaryText;
        NotifyStateChanged();

        Log(
            $"Gluttony turn resolved | selectedFeedAmount={challengeState.SelectedFeedAmount} | " +
            $"outcome={result.outcome} | summary={result.summaryText} | " +
            $"{challengeState.BuildDebugSummary()}"
        );

        OnTurnResolved?.Invoke(this, result);

        if (challengeState.IsResolved)
        {
            FinalizeAndResolveRoom();
            return true;
        }

        SetRuntimeState(GluttonyRuntimeState.AwaitingInput);
        return true;
    }

    public ChallengeResult BuildChallengeResult()
    {
        if (challengeState == null)
        {
            return ChallengeResult.MakeFail(
                ChallengeType.Gluttony,
                "Gluttony resolved without a valid state."
            );
        }

        int tempHPLoss = Mathf.Max(0, challengeState.AccumulatedTempHPLoss);

        if (challengeState.WasSuccess)
        {
            ChallengeResult success = ChallengeResult.MakeSuccess(
                ChallengeType.Gluttony,
                $"Gluttony succeeded. Exact fill at {challengeState.FullnessCurrent}/{challengeState.FullnessTarget}. " +
                $"Bonus HP +{SuccessBonusMaxHP} | Temporary HP loss {tempHPLoss}.",
                markCompleted: true,
                consumeRoom: true
            );

            if (SuccessBonusMaxHP > 0)
            {
                success.AddEffect(new ChallengeEffectEntry(
                    ChallengeType.Gluttony,
                    ChallengeEffectType.BonusMaxHP,
                    SuccessBonusMaxHP,
                    true,
                    $"Gluttony success: HP +{SuccessBonusMaxHP}"
                ));
            }

            if (tempHPLoss > 0)
            {
                success.AddEffect(new ChallengeEffectEntry(
                    ChallengeType.Gluttony,
                    ChallengeEffectType.TempMaxHPLoss,
                    tempHPLoss,
                    true,
                    $"Gluttony feeding cost: HP -{tempHPLoss}"
                ));
            }

            return success;
        }

        if (challengeState.WasOverfed)
        {
            int apLoss = Mathf.Max(
                1,
                Mathf.RoundToInt(GetCurrentEffectiveMaxAPForChallenge() * Mathf.Clamp01(overfeedAPLossPercent))
            );

            ChallengeResult fail = ChallengeResult.MakeFail(
                ChallengeType.Gluttony,
                $"Gluttony failed by overfeeding. Monster exploded at {challengeState.FullnessCurrent}/{challengeState.FullnessTarget}. " +
                $"Temporary HP loss {tempHPLoss} | Temporary AP loss {apLoss}.",
                markCompleted: true,
                consumeRoom: true
            );

            if (tempHPLoss > 0)
            {
                fail.AddEffect(new ChallengeEffectEntry(
                    ChallengeType.Gluttony,
                    ChallengeEffectType.TempMaxHPLoss,
                    tempHPLoss,
                    true,
                    $"Gluttony feeding cost: HP -{tempHPLoss}"
                ));
            }

            if (apLoss > 0)
            {
                fail.AddEffect(new ChallengeEffectEntry(
                    ChallengeType.Gluttony,
                    ChallengeEffectType.TempMaxAPLoss,
                    apLoss,
                    true,
                    $"Gluttony overfeed: AP -{apLoss}"
                ));
            }

            return fail;
        }

        if (challengeState.WasStoppedByOneHPGuard)
        {
            ChallengeResult guardedFail = ChallengeResult.MakeFail(
                ChallengeType.Gluttony,
                $"Gluttony ended early to preserve 1 HP. Temporary HP loss {tempHPLoss}.",
                markCompleted: true,
                consumeRoom: true
            );

            if (tempHPLoss > 0)
            {
                guardedFail.AddEffect(new ChallengeEffectEntry(
                    ChallengeType.Gluttony,
                    ChallengeEffectType.TempMaxHPLoss,
                    tempHPLoss,
                    true,
                    $"Gluttony safe stop: HP -{tempHPLoss}"
                ));
            }

            return guardedFail;
        }

        return ChallengeResult.MakeFail(
            ChallengeType.Gluttony,
            "Gluttony resolved without a recognized outcome."
        );
    }

    private void FinalizeAndResolveRoom()
    {
        if (!CanOperate())
            return;

        if (roomController == null)
        {
            Log("Cannot resolve Gluttony challenge because RoomController is missing.");
            return;
        }

        ChallengeResult result = BuildChallengeResult();

        SetRuntimeState(GluttonyRuntimeState.Resolved);

        Log($"Resolving Gluttony challenge with result: {result}");

        roomController.ResolveWithResult(result);
    }

    private void BuildFreshChallengeState()
    {
        if (challengeState == null)
            challengeState = new GluttonyChallengeState();

        challengeState.Initialize(
            BuildDeterministicTargetFullness(),
            Mathf.Max(1, minSelectableFeedAmount),
            Mathf.Max(Mathf.Max(1, minSelectableFeedAmount), maxSelectableFeedAmount),
            Mathf.Clamp(
                initialSelectedFeedAmount,
                Mathf.Max(1, minSelectableFeedAmount),
                Mathf.Max(Mathf.Max(1, minSelectableFeedAmount), maxSelectableFeedAmount)
            )
        );
    }

    private int BuildDeterministicTargetFullness()
    {
        int minTarget = Mathf.Max(1, minTargetFullness);
        int maxTarget = Mathf.Max(minTarget, maxTargetFullness);

        if (context == null)
            return minTarget;

        int seed =
            Mathf.Abs(
                (context.Coord.x * 73856093) ^
                (context.Coord.y * 19349663) ^
                ((int)ChallengeType.Gluttony * 83492791)
            );

        int range = (maxTarget - minTarget) + 1;
        return minTarget + (seed % range);
    }

    private int GetCurrentEffectiveMaxHPForChallenge()
    {
        if (playerStats == null)
            return 1;

        return Mathf.Max(1, playerStats.MaxHP);
    }

    private int GetCurrentEffectiveMaxAPForChallenge()
    {
        if (playerStats == null)
            return 1;

        return Mathf.Max(1, playerStats.MaxAP);
    }

    private bool CanEditSelection()
    {
        if (!CanOperate())
            return false;

        if (runtimeState != GluttonyRuntimeState.AwaitingInput)
        {
            Log(
                $"Selection input ignored because runtimeState is {runtimeState}, " +
                $"expected {GluttonyRuntimeState.AwaitingInput}."
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

        if (context.ChallengeType != ChallengeType.Gluttony)
        {
            Log(
                $"Operation ignored because context challengeType is {context.ChallengeType}, " +
                $"expected {ChallengeType.Gluttony}."
            );
            return false;
        }

        return true;
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke(this);
    }

    private void SetRuntimeState(GluttonyRuntimeState newState, bool notify = true)
    {
        if (runtimeState == newState)
            return;

        runtimeState = newState;

        Log(
            $"Runtime state -> {runtimeState} | " +
            $"coord={(context != null ? context.Coord.ToString() : "NoContext")} | " +
            $"snapshotMaxHPOnEntry={snapshotMaxHPOnEntry}"
        );

        if (notify)
            OnRuntimeStateChanged?.Invoke(this, runtimeState);
    }

    private void Log(string message)
    {
        if (!logRuntime)
            return;

        Debug.Log($"[GluttonyChallengeRuntime] {message}", this);
    }
}