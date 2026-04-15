using System.Collections;
using UnityEngine;

public class LieForcedGluttonyDriver : MonoBehaviour, ILieForcedTrialDriver
{
    [Header("References")]
    [SerializeField] private GluttonyChallengeRuntime gluttonyRuntime;
    [SerializeField] private GluttonyChallengeStage gluttonyStage;
    [SerializeField] private GluttonyChallengePanelUI gluttonyPanelUI;
    [SerializeField] private GluttonyMonsterVisual monsterVisual;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ChallengeEffectManager challengeEffectManager;
    [SerializeField] private PlayerStats playerStats;

    [Header("Ending Sequence")]
    [SerializeField] private float successShowSeconds = 1.8f;
    [SerializeField] private float failShowSeconds = 2.0f;
    [SerializeField] private float guardedFailShowSeconds = 1.6f;

    [Header("Forced Snap")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Transform playerStandPoint;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private LieChallengeRuntime activeLieRuntime;
    private ChallengeRoomContext fakeContext;
    private bool forcedTrialActive;
    private Coroutine resolveRoutine;

    public LieForcedTrialType TrialType => LieForcedTrialType.Gluttony;

    public event System.Action<ILieForcedTrialDriver, ChallengeResult> OnForcedTrialResolved;

    private void Awake()
    {
        if (gluttonyRuntime == null)
            gluttonyRuntime = GetComponentInChildren<GluttonyChallengeRuntime>(true);

        if (gluttonyStage == null)
            gluttonyStage = GetComponentInChildren<GluttonyChallengeStage>(true);

        if (gluttonyPanelUI == null)
            gluttonyPanelUI = FindFirstObjectByType<GluttonyChallengePanelUI>();

        if (monsterVisual == null)
            monsterVisual = GetComponentInChildren<GluttonyMonsterVisual>(true);

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

        if (gluttonyRuntime == null)
        {
            Log("Cannot begin forced Gluttony trial because runtime is missing.");
            return;
        }

        BuildFakeContext();

        SnapPlayerToStandPoint();

        gluttonyRuntime.Initialize(fakeContext);
        gluttonyRuntime.BeginChallenge();

        gluttonyRuntime.OnTurnResolved += HandleTurnResolved;
        gluttonyRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;

        if (monsterVisual != null)
        {
            monsterVisual.ResetVisualInstant();
            monsterVisual.SetFullnessVisual(gluttonyRuntime.FullnessCurrent, gluttonyRuntime.FullnessTarget);
        }

        if (gluttonyPanelUI != null)
            gluttonyPanelUI.ShowForRuntime(gluttonyRuntime);

        Log("Forced Gluttony trial began.");
    }

    public void CancelForcedTrial()
    {
        forcedTrialActive = false;

        if (resolveRoutine != null)
        {
            StopCoroutine(resolveRoutine);
            resolveRoutine = null;
        }

        if (gluttonyRuntime != null)
        {
            gluttonyRuntime.OnTurnResolved -= HandleTurnResolved;
            gluttonyRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
            gluttonyRuntime.ResetRuntimeState();
        }

        if (gluttonyPanelUI != null)
            gluttonyPanelUI.HidePanel();

        if (monsterVisual != null)
            monsterVisual.ResetVisualInstant();

        activeLieRuntime = null;
        fakeContext = null;

        Log("Forced Gluttony trial cancelled/reset.");
    }

    private void BuildFakeContext()
    {
        Vector2Int coord = roomManager != null ? roomManager.CurrentCoord : Vector2Int.zero;

        RoomState fakeState = new RoomState(
            visited: true,
            cleared: false,
            remainingEnemies: -1,
            roomType: RoomType.Challenge,
            challengeType: ChallengeType.Gluttony,
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

    private void HandleTurnResolved(GluttonyChallengeRuntime runtime, GluttonyTurnResult turnResult)
    {
        if (!forcedTrialActive || runtime != gluttonyRuntime)
            return;

        if (monsterVisual != null)
            monsterVisual.SetFullnessVisual(gluttonyRuntime.FullnessCurrent, gluttonyRuntime.FullnessTarget);

        if (runtime.ChallengeState != null && runtime.ChallengeState.IsResolved)
        {
            if (resolveRoutine != null)
                StopCoroutine(resolveRoutine);

            resolveRoutine = StartCoroutine(CoResolveAfterEndPresentation());
        }
    }

    private void HandleRuntimeStateChanged(GluttonyChallengeRuntime runtime, GluttonyChallengeRuntime.GluttonyRuntimeState newState)
    {
        if (!forcedTrialActive || runtime != gluttonyRuntime)
            return;

        if (newState == GluttonyChallengeRuntime.GluttonyRuntimeState.AwaitingInput && monsterVisual != null)
            monsterVisual.SetFullnessVisual(gluttonyRuntime.FullnessCurrent, gluttonyRuntime.FullnessTarget);
    }

    private IEnumerator CoResolveAfterEndPresentation()
    {
        ChallengeResult result = gluttonyRuntime.BuildChallengeResult();
        float holdSeconds = PickHoldSeconds(result);

        yield return new WaitForSecondsRealtime(holdSeconds);

        forcedTrialActive = false;
        resolveRoutine = null;

        if (gluttonyRuntime != null)
        {
            gluttonyRuntime.OnTurnResolved -= HandleTurnResolved;
            gluttonyRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
        }

        if (gluttonyPanelUI != null)
            gluttonyPanelUI.HidePanel();

        Log($"Forced Gluttony resolved | success={result.IsSuccess} | summary={result.summaryText}");
        OnForcedTrialResolved?.Invoke(this, result);
    }

    private float PickHoldSeconds(ChallengeResult result)
    {
        if (result == null)
            return failShowSeconds;

        if (result.IsSuccess)
            return successShowSeconds;

        if (!string.IsNullOrEmpty(result.summaryText) && result.summaryText.Contains("preserve 1 HP"))
            return guardedFailShowSeconds;

        return failShowSeconds;
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

        Debug.Log($"[LieForcedGluttonyDriver] {message}", this);
    }
}