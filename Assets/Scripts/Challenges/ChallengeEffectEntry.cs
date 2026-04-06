[System.Serializable]
public class ChallengeEffectEntry
{
    public ChallengeType sourceChallenge;
    public ChallengeEffectType effectType;

    public float value;

    // For the Phase 8 rule: challenge effects clear when the next challenge room is entered.
    public bool clearsOnNextChallengeEntry;

    public string debugLabel;

    public ChallengeEffectEntry(
        ChallengeType sourceChallenge = ChallengeType.None,
        ChallengeEffectType effectType = ChallengeEffectType.None,
        float value = 0f,
        bool clearsOnNextChallengeEntry = true,
        string debugLabel = "")
    {
        this.sourceChallenge = sourceChallenge;
        this.effectType = effectType;
        this.value = value;
        this.clearsOnNextChallengeEntry = clearsOnNextChallengeEntry;
        this.debugLabel = debugLabel;
    }
}