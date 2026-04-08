using UnityEngine;

[System.Serializable]
public class GluttonyTurnResult
{
    public enum TurnOutcome
    {
        None,
        ContinuedSafely,
        ExactSuccess,
        OverfedFail,
        StoppedByOneHPGuard
    }

    public TurnOutcome outcome = TurnOutcome.None;

    [Header("Applied This Turn")]
    public int chosenFeedAmount;
    public int fullnessBefore;
    public int fullnessAfter;
    public int hpLossAppliedThisTurn;

    [Header("Totals After Turn")]
    public int totalSafeFeedsTaken;
    public int totalTempHPLoss;
    public int cappedTempHPLossAtGuard;
    public int projectedRemainingEffectiveHP;

    [Header("Debug / UI")]
    [TextArea]
    public string summaryText = "";
}

[System.Serializable]
public class GluttonyChallengeState
{
    [Header("Identity")]
    [SerializeField] private int fullnessTarget;
    [SerializeField] private int fullnessCurrent;

    [Header("Selection")]
    [SerializeField] private int selectedFeedAmount;
    [SerializeField] private int minSelectableFeedAmount;
    [SerializeField] private int maxSelectableFeedAmount;

    [Header("Turn Tracking")]
    [SerializeField] private int turnCount;
    [SerializeField] private int safeFeedsTaken;
    [SerializeField] private int accumulatedTempHPLoss;

    [Header("Resolution")]
    [SerializeField] private bool isResolved;
    [SerializeField] private bool wasSuccess;
    [SerializeField] private bool wasOverfed;
    [SerializeField] private bool wasStoppedByOneHPGuard;

    public int FullnessTarget => fullnessTarget;
    public int FullnessCurrent => fullnessCurrent;

    public int SelectedFeedAmount => selectedFeedAmount;
    public int MinSelectableFeedAmount => minSelectableFeedAmount;
    public int MaxSelectableFeedAmount => maxSelectableFeedAmount;

    public int TurnCount => turnCount;
    public int SafeFeedsTaken => safeFeedsTaken;
    public int AccumulatedTempHPLoss => accumulatedTempHPLoss;

    public bool IsResolved => isResolved;
    public bool WasSuccess => wasSuccess;
    public bool WasOverfed => wasOverfed;
    public bool WasStoppedByOneHPGuard => wasStoppedByOneHPGuard;

    public int RemainingToTarget => Mathf.Max(0, fullnessTarget - fullnessCurrent);

    public void Initialize(
        int targetFullness,
        int minFeedAmount,
        int maxFeedAmount,
        int initialFeedAmount)
    {
        fullnessTarget = Mathf.Max(1, targetFullness);
        fullnessCurrent = 0;

        minSelectableFeedAmount = Mathf.Max(1, minFeedAmount);
        maxSelectableFeedAmount = Mathf.Max(minSelectableFeedAmount, maxFeedAmount);
        selectedFeedAmount = Mathf.Clamp(initialFeedAmount, minSelectableFeedAmount, maxSelectableFeedAmount);

        turnCount = 0;
        safeFeedsTaken = 0;
        accumulatedTempHPLoss = 0;

        isResolved = false;
        wasSuccess = false;
        wasOverfed = false;
        wasStoppedByOneHPGuard = false;
    }

    public void ForceResetProgressOnly()
    {
        fullnessCurrent = 0;
        turnCount = 0;
        safeFeedsTaken = 0;
        accumulatedTempHPLoss = 0;

        isResolved = false;
        wasSuccess = false;
        wasOverfed = false;
        wasStoppedByOneHPGuard = false;

        selectedFeedAmount = Mathf.Clamp(selectedFeedAmount, minSelectableFeedAmount, maxSelectableFeedAmount);
    }

    public bool TryIncreaseSelectedFeedAmount(int amount = 1)
    {
        if (isResolved)
            return false;

        int delta = Mathf.Max(1, amount);
        int old = selectedFeedAmount;
        selectedFeedAmount = Mathf.Min(maxSelectableFeedAmount, selectedFeedAmount + delta);
        return selectedFeedAmount != old;
    }

    public bool TryDecreaseSelectedFeedAmount(int amount = 1)
    {
        if (isResolved)
            return false;

        int delta = Mathf.Max(1, amount);
        int old = selectedFeedAmount;
        selectedFeedAmount = Mathf.Max(minSelectableFeedAmount, selectedFeedAmount - delta);
        return selectedFeedAmount != old;
    }

    public GluttonyTurnResult ApplySelectedFeed(
        int playerCurrentEffectiveMaxHP,
        int hpLossPerSafeStep)
    {
        return ApplyExactFeedAmount(
            selectedFeedAmount,
            playerCurrentEffectiveMaxHP,
            hpLossPerSafeStep
        );
    }

    public GluttonyTurnResult ApplyExactFeedAmount(
        int feedAmount,
        int playerCurrentEffectiveMaxHP,
        int hpLossPerSafeStep)
    {
        var result = new GluttonyTurnResult();

        if (isResolved)
        {
            result.summaryText = "Gluttony already resolved.";
            return result;
        }

        int chosenFeedAmount = Mathf.Clamp(feedAmount, minSelectableFeedAmount, maxSelectableFeedAmount);
        int clampedCurrentEffectiveHP = Mathf.Max(1, playerCurrentEffectiveMaxHP);
        int clampedHpLossPerSafeStep = Mathf.Max(0, hpLossPerSafeStep);

        turnCount++;

        result.chosenFeedAmount = chosenFeedAmount;
        result.fullnessBefore = fullnessCurrent;

        fullnessCurrent += chosenFeedAmount;
        result.fullnessAfter = fullnessCurrent;

        if (fullnessCurrent == fullnessTarget)
        {
            isResolved = true;
            wasSuccess = true;
            wasOverfed = false;
            wasStoppedByOneHPGuard = false;

            result.outcome = GluttonyTurnResult.TurnOutcome.ExactSuccess;
            result.totalSafeFeedsTaken = safeFeedsTaken;
            result.totalTempHPLoss = accumulatedTempHPLoss;
            result.projectedRemainingEffectiveHP =
                Mathf.Max(1, clampedCurrentEffectiveHP - accumulatedTempHPLoss);

            result.summaryText =
                $"Exact fill. Fed {chosenFeedAmount}. The monster is satisfied at {fullnessCurrent}/{fullnessTarget}.";
            return result;
        }

        if (fullnessCurrent > fullnessTarget)
        {
            isResolved = true;
            wasSuccess = false;
            wasOverfed = true;
            wasStoppedByOneHPGuard = false;

            result.outcome = GluttonyTurnResult.TurnOutcome.OverfedFail;
            result.totalSafeFeedsTaken = safeFeedsTaken;
            result.totalTempHPLoss = accumulatedTempHPLoss;
            result.projectedRemainingEffectiveHP =
                Mathf.Max(1, clampedCurrentEffectiveHP - accumulatedTempHPLoss);

            result.summaryText =
                $"Overfed. Fed {chosenFeedAmount}. The monster exploded at {fullnessCurrent}/{fullnessTarget}.";
            return result;
        }

        int projectedTempHPLoss = accumulatedTempHPLoss + clampedHpLossPerSafeStep;
        int projectedRemainingEffectiveHP = clampedCurrentEffectiveHP - projectedTempHPLoss;

        if (projectedRemainingEffectiveHP < 1)
        {
            isResolved = true;
            wasSuccess = false;
            wasOverfed = false;
            wasStoppedByOneHPGuard = true;

            int maxAllowedLoss = Mathf.Max(0, clampedCurrentEffectiveHP - 1);
            accumulatedTempHPLoss = Mathf.Min(accumulatedTempHPLoss, maxAllowedLoss);

            result.outcome = GluttonyTurnResult.TurnOutcome.StoppedByOneHPGuard;
            result.hpLossAppliedThisTurn = 0;
            result.totalSafeFeedsTaken = safeFeedsTaken;
            result.totalTempHPLoss = accumulatedTempHPLoss;
            result.cappedTempHPLossAtGuard = maxAllowedLoss;
            result.projectedRemainingEffectiveHP = 1;

            result.summaryText =
                $"Fed {chosenFeedAmount}, but Gluttony stops here. Further feeding would reduce you below 1 HP.";
            return result;
        }

        safeFeedsTaken++;
        accumulatedTempHPLoss = projectedTempHPLoss;

        result.outcome = GluttonyTurnResult.TurnOutcome.ContinuedSafely;
        result.hpLossAppliedThisTurn = clampedHpLossPerSafeStep;
        result.totalSafeFeedsTaken = safeFeedsTaken;
        result.totalTempHPLoss = accumulatedTempHPLoss;
        result.projectedRemainingEffectiveHP = projectedRemainingEffectiveHP;

        result.summaryText =
            $"Fed {chosenFeedAmount}. Monster fullness is now {fullnessCurrent}/{fullnessTarget}. " +
            $"Temporary HP loss total: {accumulatedTempHPLoss}.";

        return result;
    }

    public string BuildDebugSummary()
    {
        return
            $"GluttonyState | " +
            $"fullness={fullnessCurrent}/{fullnessTarget} | " +
            $"selectedFeedAmount={selectedFeedAmount} | " +
            $"feedRange={minSelectableFeedAmount}-{maxSelectableFeedAmount} | " +
            $"turns={turnCount} | safeFeedsTaken={safeFeedsTaken} | " +
            $"tempHPLoss={accumulatedTempHPLoss} | " +
            $"resolved={isResolved} | success={wasSuccess} | " +
            $"overfed={wasOverfed} | hpGuard={wasStoppedByOneHPGuard}";
    }
}