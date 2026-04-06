using System.Collections;
using UnityEngine;

public class TestChallengeRuntime : MonoBehaviour, IChallengeRuntime
{
    [Header("Identity")]
    [SerializeField] private ChallengeType challengeType = ChallengeType.Betting;

    [Header("Test Behavior")]
    [SerializeField] private bool autoResolve = true;
    [SerializeField] private float resolveDelaySeconds = 1.5f;
    [SerializeField] private bool resolveAsSuccess = true;

    [Header("Test Reward")]
    [SerializeField] private ChallengeEffectType rewardEffectType = ChallengeEffectType.TempDPBonus;
    [SerializeField] private float rewardValue = 2f;
    [SerializeField] private bool clearsOnNextChallengeEntry = true;
    [SerializeField] private string rewardDebugLabel = "Test runtime reward";

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private ChallengeRoomContext context;
    private Coroutine runningCoroutine;
    private bool initialized;
    private bool started;

    public ChallengeType ChallengeType => challengeType;

    public void Initialize(ChallengeRoomContext context)
    {
        this.context = context;
        initialized = true;
        started = false;

        StopRunningCoroutine();

        if (log)
        {
            Debug.Log(
                $"[TestChallengeRuntime] Initialized | " +
                $"challengeType={challengeType} | coord={context?.Coord}",
                this
            );
        }
    }

    public void BeginChallenge()
    {
        if (!initialized)
        {
            if (log)
                Debug.LogWarning("[TestChallengeRuntime] BeginChallenge ignored because runtime is not initialized.", this);
            return;
        }

        if (started)
        {
            if (log)
                Debug.LogWarning("[TestChallengeRuntime] BeginChallenge ignored because runtime already started.", this);
            return;
        }

        if (context == null || context.RoomController == null)
        {
            if (log)
                Debug.LogWarning("[TestChallengeRuntime] BeginChallenge ignored because context/controller is missing.", this);
            return;
        }

        started = true;

        if (log)
        {
            Debug.Log(
                $"[TestChallengeRuntime] BeginChallenge | " +
                $"challengeType={challengeType} | autoResolve={autoResolve} | delay={resolveDelaySeconds}",
                this
            );
        }

        if (autoResolve)
            runningCoroutine = StartCoroutine(CoAutoResolve());
    }

    public void CancelChallenge()
    {
        StopRunningCoroutine();
        started = false;

        if (log)
            Debug.Log("[TestChallengeRuntime] Challenge cancelled.", this);
    }

    public void ResetRuntimeState()
    {
        StopRunningCoroutine();
        started = false;
        initialized = false;
        context = null;

        if (log)
            Debug.Log("[TestChallengeRuntime] Runtime state reset.", this);
    }

    private IEnumerator CoAutoResolve()
    {
        yield return new WaitForSeconds(resolveDelaySeconds);

        if (context == null || context.RoomController == null)
            yield break;

        ChallengeResult result = resolveAsSuccess
            ? ChallengeResult.MakeSuccess(
                challengeType,
                "Test challenge succeeded."
            )
            : ChallengeResult.MakeFail(
                challengeType,
                "Test challenge failed."
            );

        if (rewardEffectType != ChallengeEffectType.None)
        {
            result.AddEffect(new ChallengeEffectEntry(
                challengeType,
                rewardEffectType,
                rewardValue,
                clearsOnNextChallengeEntry,
                rewardDebugLabel
            ));
        }

        if (log)
        {
            Debug.Log(
                $"[TestChallengeRuntime] Auto resolving | " +
                $"challengeType={challengeType} | outcome={result.outcome} | " +
                $"rewardType={rewardEffectType} | rewardValue={rewardValue}",
                this
            );
        }

        context.RoomController.ResolveWithResult(result);

        runningCoroutine = null;
    }

    private void StopRunningCoroutine()
    {
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }
    }
}