public interface ILieForcedTrialDriver
{
    LieForcedTrialType TrialType { get; }

    event System.Action<ILieForcedTrialDriver, ChallengeResult> OnForcedTrialResolved;

    void BeginForcedTrial(LieChallengeRuntime lieRuntime);
    void CancelForcedTrial();
}