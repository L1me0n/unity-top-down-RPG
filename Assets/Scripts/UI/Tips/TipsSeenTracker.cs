using UnityEngine;

public class TipsSeenTracker : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool log = false;

    [Header("Challenge Tips Seen")]
    [SerializeField] private bool hasSeenGeneralChallengeTips;
    [SerializeField] private bool hasSeenBettingTips;
    [SerializeField] private bool hasSeenGluttonyTips;
    [SerializeField] private bool hasSeenSlothTips;
    [SerializeField] private bool hasSeenLieTips;

    public bool HasSeenGeneralChallengeTips => hasSeenGeneralChallengeTips;

    public bool HasSeenChallengeTypeTips(ChallengeType challengeType)
    {
        switch (challengeType)
        {
            case ChallengeType.Betting:
                return hasSeenBettingTips;

            case ChallengeType.Gluttony:
                return hasSeenGluttonyTips;

            case ChallengeType.Sloth:
                return hasSeenSlothTips;

            case ChallengeType.Lie:
                return hasSeenLieTips;

            default:
                return false;
        }
    }

    public void MarkGeneralChallengeTipsSeen()
    {
        if (hasSeenGeneralChallengeTips)
            return;

        hasSeenGeneralChallengeTips = true;

        if (log)
            Debug.Log("[TipsSeenTracker] Marked general challenge tips as seen.", this);
    }

    public void MarkChallengeTypeTipsSeen(ChallengeType challengeType)
    {
        bool changed = false;

        switch (challengeType)
        {
            case ChallengeType.Betting:
                if (!hasSeenBettingTips)
                {
                    hasSeenBettingTips = true;
                    changed = true;
                }
                break;

            case ChallengeType.Gluttony:
                if (!hasSeenGluttonyTips)
                {
                    hasSeenGluttonyTips = true;
                    changed = true;
                }
                break;

            case ChallengeType.Sloth:
                if (!hasSeenSlothTips)
                {
                    hasSeenSlothTips = true;
                    changed = true;
                }
                break;

            case ChallengeType.Lie:
                if (!hasSeenLieTips)
                {
                    hasSeenLieTips = true;
                    changed = true;
                }
                break;
        }

        if (changed && log)
            Debug.Log($"[TipsSeenTracker] Marked {challengeType} tips as seen.", this);
    }

    public void LoadChallengeTipsState(
        bool generalSeen,
        bool bettingSeen,
        bool gluttonySeen,
        bool slothSeen,
        bool lieSeen)
    {
        hasSeenGeneralChallengeTips = generalSeen;
        hasSeenBettingTips = bettingSeen;
        hasSeenGluttonyTips = gluttonySeen;
        hasSeenSlothTips = slothSeen;
        hasSeenLieTips = lieSeen;

        if (log)
        {
            Debug.Log(
                "[TipsSeenTracker] Loaded challenge tips state | " +
                $"general={hasSeenGeneralChallengeTips} | " +
                $"betting={hasSeenBettingTips} | " +
                $"gluttony={hasSeenGluttonyTips} | " +
                $"sloth={hasSeenSlothTips} | " +
                $"lie={hasSeenLieTips}",
                this
            );
        }
    }
}