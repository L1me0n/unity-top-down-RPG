using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class LieChallengeState
{
    public enum LieRoute
    {
        None,
        SneakPast,
        AcceptTrials
    }

    public enum LieRuntimeState
    {
        None,
        Setup,
        IntroChoice,
        SneakPreparing,
        SneakActive,
        SneakResolved,
        TrialPreparing,
        TrialRunning,
        TrialShowingResult,
        Resolved,
        Cancelled
    }

    [Header("Route / Flow")]
    public LieRoute chosenRoute = LieRoute.None;
    public LieRuntimeState runtimeState = LieRuntimeState.None;

    [Header("Sneak")]
    public bool sneakOutcomeRolled = false;
    public bool sneakWillSucceed = false;
    public bool sneakAttemptFinished = false;

    [Header("Trials")]
    public int forcedTrialCount = 0;
    public int currentTrialIndex = -1;
    public bool trialsPrepared = false;

    [SerializeField] private List<LieForcedTrialType> forcedTrials = new List<LieForcedTrialType>();

    [Header("Debug / Summary")]
    [TextArea]
    public string lastSummary = "";

    public IReadOnlyList<LieForcedTrialType> ForcedTrials => forcedTrials;
    public bool HasChosenRoute => chosenRoute != LieRoute.None;
    public bool IsSneakRoute => chosenRoute == LieRoute.SneakPast;
    public bool IsTrialRoute => chosenRoute == LieRoute.AcceptTrials;
    public bool HasForcedTrials => forcedTrials != null && forcedTrials.Count > 0;
    public bool HasMoreTrials =>
        trialsPrepared &&
        forcedTrials != null &&
        currentTrialIndex + 1 < forcedTrials.Count;

    public LieForcedTrialType CurrentForcedTrial
    {
        get
        {
            if (forcedTrials == null)
                return LieForcedTrialType.None;

            if (currentTrialIndex < 0 || currentTrialIndex >= forcedTrials.Count)
                return LieForcedTrialType.None;

            return forcedTrials[currentTrialIndex];
        }
    }

    public void ResetForNewRun()
    {
        chosenRoute = LieRoute.None;
        runtimeState = LieRuntimeState.Setup;

        sneakOutcomeRolled = false;
        sneakWillSucceed = false;
        sneakAttemptFinished = false;

        forcedTrialCount = 0;
        currentTrialIndex = -1;
        trialsPrepared = false;

        if (forcedTrials == null)
            forcedTrials = new List<LieForcedTrialType>();
        else
            forcedTrials.Clear();

        lastSummary = "";
    }

    public void EnterIntroChoice()
    {
        runtimeState = LieRuntimeState.IntroChoice;
    }

    public void ChooseSneakRoute(bool willSucceed)
    {
        chosenRoute = LieRoute.SneakPast;
        sneakOutcomeRolled = true;
        sneakWillSucceed = willSucceed;
        sneakAttemptFinished = false;
        runtimeState = LieRuntimeState.SneakPreparing;
    }

    public void ChooseTrialRoute(List<LieForcedTrialType> sequence)
    {
        chosenRoute = LieRoute.AcceptTrials;

        if (forcedTrials == null)
            forcedTrials = new List<LieForcedTrialType>();
        else
            forcedTrials.Clear();

        if (sequence != null)
            forcedTrials.AddRange(sequence);

        forcedTrialCount = forcedTrials.Count;
        currentTrialIndex = -1;
        trialsPrepared = forcedTrialCount > 0;

        runtimeState = LieRuntimeState.TrialPreparing;
    }

    public void EnterSneakActive()
    {
        runtimeState = LieRuntimeState.SneakActive;
    }

    public void FinishSneakAttempt()
    {
        sneakAttemptFinished = true;
        runtimeState = LieRuntimeState.SneakResolved;
    }

    public bool MoveToNextTrial()
    {
        if (!HasMoreTrials)
            return false;

        currentTrialIndex++;
        runtimeState = LieRuntimeState.TrialRunning;
        return true;
    }

    public void EnterTrialShowingResult(string summaryText)
    {
        lastSummary = summaryText ?? "";
        runtimeState = LieRuntimeState.TrialShowingResult;
    }

    public void EnterResolved(string summaryText)
    {
        lastSummary = summaryText ?? "";
        runtimeState = LieRuntimeState.Resolved;
    }

    public void Cancel()
    {
        runtimeState = LieRuntimeState.Cancelled;
    }

    public string BuildDebugSummary()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("LieChallengeState | ");
        sb.Append("route=").Append(chosenRoute).Append(" | ");
        sb.Append("state=").Append(runtimeState).Append(" | ");
        sb.Append("sneakRolled=").Append(sneakOutcomeRolled).Append(" | ");
        sb.Append("sneakSuccess=").Append(sneakWillSucceed).Append(" | ");
        sb.Append("forcedTrialCount=").Append(forcedTrialCount).Append(" | ");
        sb.Append("currentTrialIndex=").Append(currentTrialIndex).Append(" | ");
        sb.Append("currentTrial=").Append(CurrentForcedTrial);

        return sb.ToString();
    }
}