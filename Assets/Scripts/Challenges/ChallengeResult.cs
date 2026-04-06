using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChallengeResult
{
    [Header("Identity")]
    public ChallengeType challengeType = ChallengeType.None;
    public ChallengeOutcome outcome = ChallengeOutcome.None;

    [Header("Room Resolution")]
    public bool markCompleted = true;
    public bool consumeRoom = true;

    [Header("Effects")]
    public List<ChallengeEffectEntry> grantedEffects = new List<ChallengeEffectEntry>();

    [Header("UI / Debug")]
    [TextArea]
    public string summaryText = "";

    public bool IsSuccess => outcome == ChallengeOutcome.Success;
    public bool IsFail => outcome == ChallengeOutcome.Fail;
    public bool HasEffects => grantedEffects != null && grantedEffects.Count > 0;

    public ChallengeResult()
    {
    }

    public ChallengeResult(
        ChallengeType challengeType,
        ChallengeOutcome outcome,
        bool markCompleted = true,
        bool consumeRoom = true,
        string summaryText = "")
    {
        this.challengeType = challengeType;
        this.outcome = outcome;
        this.markCompleted = markCompleted;
        this.consumeRoom = consumeRoom;
        this.summaryText = summaryText;
    }

    public ChallengeResult AddEffect(ChallengeEffectEntry effect)
    {
        if (effect == null)
            return this;

        if (effect.effectType == ChallengeEffectType.None)
            return this;

        if (grantedEffects == null)
            grantedEffects = new List<ChallengeEffectEntry>();

        grantedEffects.Add(new ChallengeEffectEntry(
            effect.sourceChallenge,
            effect.effectType,
            effect.value,
            effect.clearsOnNextChallengeEntry,
            effect.debugLabel
        ));

        return this;
    }

    public static ChallengeResult MakeSuccess(
        ChallengeType challengeType,
        string summaryText = "",
        bool markCompleted = true,
        bool consumeRoom = true)
    {
        return new ChallengeResult(
            challengeType,
            ChallengeOutcome.Success,
            markCompleted,
            consumeRoom,
            summaryText
        );
    }

    public static ChallengeResult MakeFail(
        ChallengeType challengeType,
        string summaryText = "",
        bool markCompleted = true,
        bool consumeRoom = true)
    {
        return new ChallengeResult(
            challengeType,
            ChallengeOutcome.Fail,
            markCompleted,
            consumeRoom,
            summaryText
        );
    }

    public override string ToString()
    {
        int effectCount = grantedEffects != null ? grantedEffects.Count : 0;

        return
            $"ChallengeResult | " +
            $"challengeType={challengeType} | " +
            $"outcome={outcome} | " +
            $"markCompleted={markCompleted} | " +
            $"consumeRoom={consumeRoom} | " +
            $"effectCount={effectCount} | " +
            $"summary={summaryText}";
    }
}