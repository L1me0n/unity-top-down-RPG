public interface IChallengeRuntime
{
    ChallengeType ChallengeType { get; }

    void Initialize(ChallengeRoomContext context);
    void BeginChallenge();
    void CancelChallenge();
    void ResetRuntimeState();
}