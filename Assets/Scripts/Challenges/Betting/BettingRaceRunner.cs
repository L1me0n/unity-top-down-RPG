using System.Collections;
using UnityEngine;

public class BettingRaceRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BettingChallengeRuntime bettingRuntime;
    [SerializeField] private BettingChallengeStage bettingStage;
    [SerializeField] private BettingFeedbackPopup feedbackPopup;

    [Header("Race")]
    [SerializeField] private float finishDistance = 10f;
    [SerializeField] private float preRaceDelay = 0.45f;
    [SerializeField] private float postFinishDelay = 0.4f;
    [SerializeField] private float winnerShowDuration = 1.15f;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    [Header("Readonly")]
    [SerializeField] private int lastWinningLaneIndex = -1;

    [SerializeField] private BettingRaceSimulator simulator = new BettingRaceSimulator();

    private Coroutine raceCoroutine;

    public int LastWinningLaneIndex => lastWinningLaneIndex;
    public bool IsRaceRunning => raceCoroutine != null;

    private void Awake()
    {
        if (bettingRuntime == null)
            bettingRuntime = GetComponent<BettingChallengeRuntime>();

        if (bettingStage == null)
            bettingStage = GetComponent<BettingChallengeStage>();

        if (feedbackPopup == null)
            feedbackPopup = FindFirstObjectByType<BettingFeedbackPopup>();
    }

    private void OnEnable()
    {
        if (bettingRuntime != null)
            bettingRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;
    }

    private void OnDisable()
    {
        if (bettingRuntime != null)
            bettingRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
    }

    private void HandleRuntimeStateChanged(BettingChallengeRuntime runtime, BettingChallengeRuntime.BettingRuntimeState newState)
    {
        if (runtime == null || runtime != bettingRuntime)
            return;

        if (newState == BettingChallengeRuntime.BettingRuntimeState.RaceStarting)
            StartRace();
    }

    private void StartRace()
    {
        if (raceCoroutine != null)
            StopCoroutine(raceCoroutine);

        int seed = BuildRaceSeed();
        simulator.Initialize(seed, finishDistance);
        lastWinningLaneIndex = -1;

        if (bettingStage != null)
        {
            bettingStage.ResetLaneTokensToStart();
            bettingStage.ResetWinnerHighlight();
        }

        raceCoroutine = StartCoroutine(CoRunRace());

        Log($"Race started | seed={seed} | finishDistance={finishDistance}");
    }

    private IEnumerator CoRunRace()
    {
        yield return new WaitForSeconds(preRaceDelay);

        if (bettingRuntime != null)
            bettingRuntime.EnterRaceRunning();

        while (!simulator.Finished)
        {
            simulator.Tick(Time.deltaTime);

            if (bettingStage != null)
            {
                for (int i = 0; i < BettingWagerState.LaneCount; i++)
                {
                    float normalized = simulator.GetNormalizedProgress(i);
                    bettingStage.SetLaneTokenProgress(i, normalized);
                }
            }

            yield return null;
        }

        lastWinningLaneIndex = simulator.WinningLaneIndex;

        if (bettingStage != null)
            bettingStage.HighlightWinningLane(lastWinningLaneIndex);

        Log(
            $"Race finished | winnerLane={lastWinningLaneIndex} | elapsed={simulator.Elapsed:0.00}s"
        );

        yield return new WaitForSeconds(postFinishDelay);

        if (bettingRuntime != null)
            bettingRuntime.NotifyRaceFinished(lastWinningLaneIndex);

        ShowOutcomePopup();

        yield return new WaitForSeconds(winnerShowDuration);

        if (bettingRuntime != null)
            bettingRuntime.FinalizeWinnerPresentationAndResolve();

        raceCoroutine = null;
    }

    private void ShowOutcomePopup()
    {
        if (bettingRuntime == null || feedbackPopup == null)
            return;

        if (!bettingRuntime.TryGetRaceOutcomeNumbers(lastWinningLaneIndex, out int winningBetAP, out int losingBetAP))
        {
            feedbackPopup.ShowMessage("RACE FINISHED");
            return;
        }

        string message;

        if (winningBetAP > 0)
            message = $"BET WON  HP +{winningBetAP}" + (losingBetAP > 0 ? $"  LOCK AP {losingBetAP}" : "");
        else
            message = $"BET LOST  LOCK AP {losingBetAP}";

        feedbackPopup.ShowMessage(message);
    }

    private int BuildRaceSeed()
    {
        int seed = 17;

        if (bettingRuntime != null && bettingRuntime.IsInitialized)
        {
            seed = seed * 31 + bettingRuntime.SnapshotAP;
            seed = seed * 31 + bettingRuntime.TotalWager;
            seed = seed * 31 + bettingRuntime.MinimumTotalWager;

            for (int i = 0; i < BettingWagerState.LaneCount; i++)
                seed = seed * 31 + bettingRuntime.GetLaneWager(i);
        }

        return seed;
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[BettingRaceRunner] {message}", this);
    }
}