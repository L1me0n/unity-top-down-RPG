using System.Collections;
using UnityEngine;

public class LieForcedSlothDriver : MonoBehaviour, ILieForcedTrialDriver
{
    [Header("References")]
    [SerializeField] private SlothChallengeRuntime slothRuntime;
    [SerializeField] private SlothChallengeStage slothStage;
    [SerializeField] private SlothChallengePanelUI slothPanelUI;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ChallengeEffectManager challengeEffectManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerDamageReceiver playerDamageReceiver;

    [Header("Forced Snap")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Transform playerStandPoint;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private LieChallengeRuntime activeLieRuntime;
    private ChallengeRoomContext fakeContext;
    private bool forcedTrialActive;

    public LieForcedTrialType TrialType => LieForcedTrialType.Sloth;

    public event System.Action<ILieForcedTrialDriver, ChallengeResult> OnForcedTrialResolved;

    private void Awake()
    {
        if (slothRuntime == null)
            slothRuntime = GetComponentInChildren<SlothChallengeRuntime>(true);

        if (slothStage == null)
            slothStage = GetComponentInChildren<SlothChallengeStage>(true);

        if (slothPanelUI == null)
            slothPanelUI = FindFirstObjectByType<SlothChallengePanelUI>();

        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        if (challengeEffectManager == null)
            challengeEffectManager = FindFirstObjectByType<ChallengeEffectManager>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerDamageReceiver == null)
            playerDamageReceiver = FindFirstObjectByType<PlayerDamageReceiver>();
    }

    public void BeginForcedTrial(LieChallengeRuntime lieRuntime)
    {
        activeLieRuntime = lieRuntime;
        forcedTrialActive = true;

        if (slothRuntime == null)
        {
            Log("Cannot begin forced Sloth trial because SlothChallengeRuntime is missing.");
            return;
        }

        BuildFakeContext();

        SnapPlayerToStandPoint();

        slothRuntime.Initialize(fakeContext);
        slothRuntime.BeginChallenge();

        slothRuntime.OnTimingStopped += HandleTimingStopped;

        if (slothPanelUI != null)
            slothPanelUI.ShowForRuntime(slothRuntime);

        Log("Forced Sloth trial began.");
    }

    public void CancelForcedTrial()
    {
        forcedTrialActive = false;

        if (slothRuntime != null)
        {
            slothRuntime.OnTimingStopped -= HandleTimingStopped;
            slothRuntime.ResetRuntimeState();
        }

        if (slothPanelUI != null)
            slothPanelUI.HidePanel();

        activeLieRuntime = null;
        fakeContext = null;

        Log("Forced Sloth trial cancelled/reset.");
    }

    private void BuildFakeContext()
    {
        Vector2Int coord = roomManager != null ? roomManager.CurrentCoord : Vector2Int.zero;

        RoomState fakeState = new RoomState(
            visited: true,
            cleared: false,
            remainingEnemies: -1,
            roomType: RoomType.Challenge,
            challengeType: ChallengeType.Sloth,
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

    private void HandleTimingStopped(SlothChallengeRuntime runtime, bool success)
    {
        if (!forcedTrialActive)
            return;

        if (runtime != slothRuntime)
            return;

        StartCoroutine(CoResolveAfterStop(success));
    }

    private IEnumerator CoResolveAfterStop(bool success)
    {
        yield return new WaitForSecondsRealtime(success ? 1.4f : 1.2f);

        ChallengeResult result;

        if (success)
        {
            result = ChallengeResult
                .MakeSuccess(
                    ChallengeType.Sloth,
                    summaryText: $"Sloth success. Marker stopped at {slothRuntime.MarkerNormalized:0.000}. DP multiplier granted."
                )
                .AddEffect(new ChallengeEffectEntry(
                    ChallengeType.Sloth,
                    ChallengeEffectType.TempDPMultiplier,
                    1.5f,
                    true,
                    "Sloth success x1.5 DP"
                ));
        }
        else
        {
            result = ChallengeResult.MakeFail(
                ChallengeType.Sloth,
                summaryText: "Sloth failed inside Lucifer's chain."
            );
        }

        slothRuntime.OnTimingStopped -= HandleTimingStopped;
        forcedTrialActive = false;

        if (slothPanelUI != null)
            slothPanelUI.HidePanel();

        Log($"Forced Sloth resolved | success={success}");
        OnForcedTrialResolved?.Invoke(this, result);
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

        Debug.Log($"[LieForcedSlothDriver] {message}", this);
    }
}