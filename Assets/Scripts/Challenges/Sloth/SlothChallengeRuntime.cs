using UnityEngine;

public class SlothChallengeRuntime : MonoBehaviour, IChallengeRuntime
{
    public enum LocalFlowState
    {
        None,
        Setup,
        AwaitingStart,
        Running,
        Resolved,
        Cancelled
    }

    [Header("Identity")]
    [SerializeField] private ChallengeType challengeType = ChallengeType.Sloth;

    [Header("Runtime References")]
    [SerializeField] private ChallengeRoomController controller;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private PlayerDamageReceiver playerDamageReceiver;
    [SerializeField] private PlayerStats playerStats;

    [Header("Timing Config")]
    [SerializeField] private float markerStartNormalized = 0f;
    [SerializeField] private float markerStartDirection = 1f;
    [SerializeField] private float markerSpeed = 0.75f;

    [Header("Success Zone")]
    [SerializeField] private float successZoneCenter = 0.5f;
    [SerializeField] private float successZoneWidth = 0.16f;

    [Header("Reward")]
    [SerializeField] private float successDPMultiplier = 1.5f;
    [SerializeField] private bool clearsOnNextChallengeEntry = true;

    [Header("Debug / Temporary Testing")]
    [SerializeField] private bool logFlow = true;
    [SerializeField] private bool autoStartOnBegin = true;
    [SerializeField] private bool useKeyboardStopForTesting = true;
    [SerializeField] private KeyCode stopKey = KeyCode.Space;

    private ChallengeRoomContext context;
    private SlothTimingState timingState = new SlothTimingState();
    private LocalFlowState localFlowState = LocalFlowState.None;
    private bool lastStopWasSuccess;
    private bool failureDeathTriggered;

    public ChallengeType ChallengeType => challengeType;
    public SlothTimingState TimingState => timingState;
    public LocalFlowState FlowState => localFlowState;

    public bool IsRunning => localFlowState == LocalFlowState.Running;
    public bool IsAwaitingStart => localFlowState == LocalFlowState.AwaitingStart;
    public bool IsResolved => localFlowState == LocalFlowState.Resolved;
    public bool CanStartRun => localFlowState == LocalFlowState.AwaitingStart;
    public bool CanStopRun => localFlowState == LocalFlowState.Running;
    public bool LastStopWasSuccess => lastStopWasSuccess;

    public float MarkerNormalized => timingState != null ? timingState.markerNormalized : 0f;
    public float SuccessZoneCenter => timingState != null ? timingState.successZoneCenter : 0.5f;
    public float SuccessZoneWidth => timingState != null ? timingState.successZoneWidth : 0.16f;
    public float SuccessZoneMin => timingState != null ? timingState.SuccessZoneMin : 0.42f;
    public float SuccessZoneMax => timingState != null ? timingState.SuccessZoneMax : 0.58f;
    public float MarkerSpeed => timingState != null ? timingState.markerSpeed : markerSpeed;

    public System.Action<SlothChallengeRuntime, LocalFlowState> OnRuntimeStateChanged;
    public System.Action<SlothChallengeRuntime> OnStateChanged;
    public System.Action<SlothChallengeRuntime, bool> OnTimingStopped;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<ChallengeRoomController>();

        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        if (playerDamageReceiver == null)
            playerDamageReceiver = FindFirstObjectByType<PlayerDamageReceiver>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    private void Update()
    {
        if (localFlowState != LocalFlowState.Running)
            return;

        timingState.Tick(Time.deltaTime);
        NotifyStateChanged();

        if (useKeyboardStopForTesting && Input.GetKeyDown(stopKey))
            TryStopAndResolve();
    }

    public void Initialize(ChallengeRoomContext context)
    {
        this.context = context;

        if (controller == null && context != null)
            controller = context.RoomController;

        if (roomManager == null && context != null)
            roomManager = context.RoomManager;

        if (playerDamageReceiver == null)
            playerDamageReceiver = FindFirstObjectByType<PlayerDamageReceiver>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        timingState = new SlothTimingState();
        timingState.ConfigureSpeed(markerSpeed);
        timingState.ConfigureSuccessZone(successZoneCenter, successZoneWidth);
        timingState.ResetForNewRun(markerStartNormalized, markerStartDirection);
        timingState.EnterAwaitingStart();

        lastStopWasSuccess = false;
        failureDeathTriggered = false;

        SetLocalFlow(LocalFlowState.AwaitingStart);
        NotifyStateChanged();

        Log(
            $"Initialized Sloth runtime | coord={context?.Coord} | " +
            $"{timingState.BuildSummary()}"
        );
    }

    public void BeginChallenge()
    {
        if (context == null)
        {
            Log("BeginChallenge ignored because context is null.");
            return;
        }

        if (controller == null)
        {
            Log("BeginChallenge ignored because ChallengeRoomController is missing.");
            return;
        }

        timingState.ConfigureSpeed(markerSpeed);
        timingState.ConfigureSuccessZone(successZoneCenter, successZoneWidth);
        timingState.ResetForNewRun(markerStartNormalized, markerStartDirection);
        timingState.EnterAwaitingStart();

        lastStopWasSuccess = false;
        failureDeathTriggered = false;

        SetLocalFlow(LocalFlowState.Setup);

        controller.SetChallengeUIOpen(true, "Sloth challenge begin");

        Log(
            $"Sloth challenge setup complete | coord={context.Coord} | " +
            $"speed={markerSpeed:0.###} | zoneCenter={successZoneCenter:0.###} | zoneWidth={successZoneWidth:0.###}"
        );

        SetLocalFlow(LocalFlowState.AwaitingStart);
        NotifyStateChanged();

        if (autoStartOnBegin)
            StartTimingRun();
    }

    public void CancelChallenge()
    {
        timingState.Cancel();
        SetLocalFlow(LocalFlowState.Cancelled);

        if (controller != null)
            controller.SetChallengeUIOpen(false, "Sloth challenge cancelled");

        NotifyStateChanged();
        Log("Sloth challenge cancelled.");
    }

    public void ResetRuntimeState()
    {
        timingState = new SlothTimingState();
        timingState.ConfigureSpeed(markerSpeed);
        timingState.ConfigureSuccessZone(successZoneCenter, successZoneWidth);
        timingState.ResetForNewRun(markerStartNormalized, markerStartDirection);
        timingState.EnterAwaitingStart();

        lastStopWasSuccess = false;
        failureDeathTriggered = false;

        SetLocalFlow(LocalFlowState.None);
        NotifyStateChanged();

        Log("Sloth runtime-only state reset.");
    }

    public void StartTimingRun()
    {
        if (context == null)
        {
            Log("StartTimingRun ignored because context is null.");
            return;
        }

        if (localFlowState == LocalFlowState.Resolved || localFlowState == LocalFlowState.Cancelled)
        {
            Log($"StartTimingRun ignored because runtime is in state {localFlowState}.");
            return;
        }

        if (localFlowState != LocalFlowState.AwaitingStart)
        {
            Log($"StartTimingRun ignored because localFlowState={localFlowState}, expected AwaitingStart.");
            return;
        }

        timingState.StartRun();
        SetLocalFlow(LocalFlowState.Running);
        NotifyStateChanged();

        Log($"Sloth timing run started | {timingState.BuildSummary()}");
    }

    public bool TryRequestStart()
    {
        if (!CanStartRun)
            return false;

        StartTimingRun();
        return true;
    }

    public bool TryRequestStop()
    {
        if (!CanStopRun)
            return false;

        TryStopAndResolve();
        return true;
    }

    public void TryStopAndResolve()
    {
        if (localFlowState != LocalFlowState.Running)
        {
            Log($"TryStopAndResolve ignored because localFlowState={localFlowState}.");
            return;
        }

        bool success = timingState.StopAndEvaluate();
        lastStopWasSuccess = success;

        SetLocalFlow(LocalFlowState.Resolved);
        NotifyStateChanged();

        Log($"Sloth timing stopped | {timingState.BuildSummary()}");

        OnTimingStopped?.Invoke(this, success);

        if (success)
            ResolveSuccess();
    }

    public void ExecuteFailureDeath()
    {
        if (failureDeathTriggered)
        {
            Log("ExecuteFailureDeath ignored because failure death already triggered.");
            return;
        }

        if (lastStopWasSuccess)
        {
            Log("ExecuteFailureDeath ignored because last stop was success.");
            return;
        }

        failureDeathTriggered = true;

        if (controller != null)
            controller.SetChallengeUIOpen(false, "Sloth fail -> real death");

        if (playerDamageReceiver == null)
            playerDamageReceiver = FindFirstObjectByType<PlayerDamageReceiver>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerDamageReceiver == null || playerStats == null)
        {
            Log("ExecuteFailureDeath failed because PlayerDamageReceiver or PlayerStats is missing.");
            return;
        }

        int lethalDamage = Mathf.Max(1, playerStats.HP);

        Log(
            $"Executing Sloth fail death | coord={context?.Coord} | " +
            $"currentHP={playerStats.HP} | lethalDamage={lethalDamage}"
        );

        playerDamageReceiver.ApplyDamage(lethalDamage);
    }

    private void ResolveSuccess()
    {
        if (controller == null)
        {
            Log("ResolveSuccess failed because controller is null.");
            return;
        }

        ChallengeResult result = ChallengeResult
            .MakeSuccess(
                ChallengeType.Sloth,
                summaryText: $"Sloth success. Marker stopped at {timingState.markerNormalized:0.000}. DP multiplier granted."
            )
            .AddEffect(new ChallengeEffectEntry(
                ChallengeType.Sloth,
                ChallengeEffectType.TempDPMultiplier,
                successDPMultiplier,
                clearsOnNextChallengeEntry,
                $"Sloth success x{successDPMultiplier:0.##} DP"
            ));

        controller.ResolveWithResult(result);

        Log(
            $"Sloth success resolved | coord={context?.Coord} | " +
            $"multiplier={successDPMultiplier:0.##}"
        );
    }

    private void SetLocalFlow(LocalFlowState newState)
    {
        if (localFlowState == newState)
            return;

        localFlowState = newState;
        Log($"Local flow -> {localFlowState}");

        OnRuntimeStateChanged?.Invoke(this, localFlowState);
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke(this);
    }

    private void Log(string message)
    {
        if (!logFlow)
            return;

        Debug.Log($"[SlothChallengeRuntime] {message}", this);
    }
}