using UnityEngine;

public class LieForcedBettingDriver : MonoBehaviour, ILieForcedTrialDriver
{
    [Header("References")]
    [SerializeField] private BettingChallengeRuntime bettingRuntime;
    [SerializeField] private BettingChallengeStage bettingStage;
    [SerializeField] private BettingChallengePanelUI bettingPanelUI;
    [SerializeField] private BettingRaceRunner bettingRaceRunner;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ChallengeEffectManager challengeEffectManager;
    [SerializeField] private PlayerStats playerStats;

    [Header("Forced Snap")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Transform playerStandPoint;
    

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private LieChallengeRuntime activeLieRuntime;
    private ChallengeRoomContext fakeContext;
    private bool forcedTrialActive;
    private bool raceResolved;

    public LieForcedTrialType TrialType => LieForcedTrialType.Betting;

    public event System.Action<ILieForcedTrialDriver, ChallengeResult> OnForcedTrialResolved;

    private void Awake()
    {
        if (bettingRuntime == null)
            bettingRuntime = GetComponentInChildren<BettingChallengeRuntime>(true);

        if (bettingStage == null)
            bettingStage = GetComponentInChildren<BettingChallengeStage>(true);

        if (bettingPanelUI == null)
            bettingPanelUI = FindFirstObjectByType<BettingChallengePanelUI>();

        if (bettingRaceRunner == null)
            bettingRaceRunner = GetComponentInChildren<BettingRaceRunner>(true);

        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        if (challengeEffectManager == null)
            challengeEffectManager = FindFirstObjectByType<ChallengeEffectManager>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    public void BeginForcedTrial(LieChallengeRuntime lieRuntime)
    {
        activeLieRuntime = lieRuntime;
        forcedTrialActive = true;
        raceResolved = false;

        if (bettingRuntime == null || bettingRaceRunner == null)
        {
            Log("Cannot begin forced Betting trial because Betting runtime or race runner is missing.");
            return;
        }

        BuildFakeContext();

        SnapPlayerToStandPoint();

        bettingRuntime.Initialize(fakeContext);
        bettingRuntime.BeginChallenge();

        bettingRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;
        bettingRuntime.OnRaceFinished += HandleRaceFinished;

        if (bettingPanelUI != null)
            bettingPanelUI.ShowForRuntime(bettingRuntime);

        Log("Forced Betting trial began.");
    }

    public void CancelForcedTrial()
    {
        forcedTrialActive = false;
        raceResolved = false;

        if (bettingRuntime != null)
        {
            bettingRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
            bettingRuntime.OnRaceFinished -= HandleRaceFinished;
            bettingRuntime.ResetRuntimeState();
        }

        if (bettingPanelUI != null)
            bettingPanelUI.HidePanel();

        if (bettingStage != null)
        {
            bettingStage.ResetLaneTokensToStart();
            bettingStage.ResetWinnerHighlight();
        }

        activeLieRuntime = null;
        fakeContext = null;

        Log("Forced Betting trial cancelled/reset.");
    }

    private void BuildFakeContext()
    {
        Vector2Int coord = roomManager != null ? roomManager.CurrentCoord : Vector2Int.zero;

        RoomState fakeState = new RoomState(
            visited: true,
            cleared: false,
            remainingEnemies: -1,
            roomType: RoomType.Challenge,
            challengeType: ChallengeType.Betting,
            challengeCompleted: false
        );

        fakeContext = new ChallengeRoomContext(
            roomManager,
            challengeEffectManager,
            roomController: null,
            roomState: fakeState,
            coord: coord
        );
    }

    private void HandleRuntimeStateChanged(BettingChallengeRuntime runtime, BettingChallengeRuntime.BettingRuntimeState newState)
    {
        if (!forcedTrialActive || runtime != bettingRuntime)
            return;

        if (newState == BettingChallengeRuntime.BettingRuntimeState.RaceFinishedShowingWinner && !raceResolved)
        {
            ChallengeResult result = bettingRuntime.BuildChallengeResultFromResolvedRace();
            raceResolved = true;
            forcedTrialActive = false;

            if (bettingPanelUI != null)
                bettingPanelUI.HidePanel();

            Log($"Forced Betting resolved with result: {result.summaryText}");
            OnForcedTrialResolved?.Invoke(this, result);
        }
    }

    private void HandleRaceFinished(BettingChallengeRuntime runtime, int winningLaneIndex)
    {
        if (!forcedTrialActive || runtime != bettingRuntime)
            return;

        Log($"Forced Betting race finished | winnerLane={winningLaneIndex}");
    }

    private void SnapPlayerToStandPoint()
    {
        if (playerTransform == null || playerStandPoint == null)
            return;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        playerTransform.position = playerStandPoint.position;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[LieForcedBettingDriver] {message}", this);
    }
}