using System.Collections.Generic;
using UnityEngine;

public class ChallengeEffectManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool logEffectChanges = true;

    [SerializeField] private List<ChallengeEffectEntry> activeEffects = new List<ChallengeEffectEntry>();

    public System.Action OnEffectsChanged;

    public IReadOnlyList<ChallengeEffectEntry> ActiveEffects => activeEffects;
    public int ActiveEffectCount => activeEffects.Count;

    public void AddEffect(ChallengeEffectEntry effect)
    {
        if (effect == null)
            return;

        if (effect.effectType == ChallengeEffectType.None)
            return;

        ChallengeEffectEntry stored = new ChallengeEffectEntry(
            effect.sourceChallenge,
            effect.effectType,
            effect.value,
            effect.clearsOnNextChallengeEntry,
            effect.debugLabel
        );

        activeEffects.Add(stored);

        if (logEffectChanges)
        {
            Debug.Log(
                $"[ChallengeEffectManager] Added effect | " +
                $"source={stored.sourceChallenge} | type={stored.effectType} | value={stored.value} | " +
                $"clearsOnNextChallengeEntry={stored.clearsOnNextChallengeEntry} | label={stored.debugLabel}"
            );
        }

        OnEffectsChanged?.Invoke();
    }

    public void ClearAllEffects()
    {
        if (activeEffects.Count == 0)
            return;

        activeEffects.Clear();

        if (logEffectChanges)
            Debug.Log("[ChallengeEffectManager] Cleared all challenge effects.");

        OnEffectsChanged?.Invoke();
    }

    public void ClearEffectsThatExpireOnNextChallengeEntry()
    {
        int removed = activeEffects.RemoveAll(effect => effect != null && effect.clearsOnNextChallengeEntry);

        if (removed <= 0)
            return;

        if (logEffectChanges)
            Debug.Log($"[ChallengeEffectManager] Cleared {removed} effect(s) on challenge-room entry.");

        OnEffectsChanged?.Invoke();
    }

    public void RemoveEffectsFromSourceChallenge(ChallengeType sourceChallenge)
    {
        int removed = activeEffects.RemoveAll(effect => effect != null && effect.sourceChallenge == sourceChallenge);

        if (removed <= 0)
            return;

        if (logEffectChanges)
            Debug.Log($"[ChallengeEffectManager] Removed {removed} effect(s) from source {sourceChallenge}.");

        OnEffectsChanged?.Invoke();
    }

    public float GetTotalValue(ChallengeEffectType effectType)
    {
        float total = 0f;

        for (int i = 0; i < activeEffects.Count; i++)
        {
            ChallengeEffectEntry effect = activeEffects[i];
            if (effect == null)
                continue;

            if (effect.effectType != effectType)
                continue;

            total += effect.value;
        }

        return total;
    }

    public float GetTotalBonusMaxHP()
    {
        return GetTotalValue(ChallengeEffectType.BonusMaxHP);
    }

    public float GetTotalLockedAP()
    {
        return GetTotalValue(ChallengeEffectType.LockedAP);
    }

    public float GetTotalTempMaxHPLoss()
    {
        return GetTotalValue(ChallengeEffectType.TempMaxHPLoss);
    }

    public float GetTotalTempMaxAPLoss()
    {
        return GetTotalValue(ChallengeEffectType.TempMaxAPLoss);
    }

    public float GetTotalTempDPBonus()
    {
        return GetTotalValue(ChallengeEffectType.TempDPBonus);
    }

    public float GetCombinedTempDPMultiplier()
    {
        float multiplier = 1f;

        for (int i = 0; i < activeEffects.Count; i++)
        {
            ChallengeEffectEntry effect = activeEffects[i];
            if (effect == null)
                continue;

            if (effect.effectType != ChallengeEffectType.TempDPMultiplier)
                continue;

            multiplier *= effect.value;
        }

        return multiplier;
    }

    public bool HasTempStatHalving()
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            ChallengeEffectEntry effect = activeEffects[i];
            if (effect == null)
                continue;

            if (effect.effectType == ChallengeEffectType.TempStatHalving)
                return true;
        }

        return false;
    }

    public string GetDebugSummary()
    {
        if (activeEffects.Count == 0)
            return "No active challenge effects.";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Active challenge effects: {activeEffects.Count}");

        for (int i = 0; i < activeEffects.Count; i++)
        {
            ChallengeEffectEntry effect = activeEffects[i];
            if (effect == null)
                continue;

            sb.Append("- ");
            sb.Append(effect.sourceChallenge);
            sb.Append(" | ");
            sb.Append(effect.effectType);
            sb.Append(" | value=");
            sb.Append(effect.value);

            if (!string.IsNullOrWhiteSpace(effect.debugLabel))
            {
                sb.Append(" | ");
                sb.Append(effect.debugLabel);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public ChallengeEffectEntry[] ExportActiveEffects()
    {
        if (activeEffects == null || activeEffects.Count == 0)
            return System.Array.Empty<ChallengeEffectEntry>();

        ChallengeEffectEntry[] result = new ChallengeEffectEntry[activeEffects.Count];

        for (int i = 0; i < activeEffects.Count; i++)
        {
            ChallengeEffectEntry effect = activeEffects[i];

            if (effect == null)
            {
                result[i] = new ChallengeEffectEntry();
                continue;
            }

            result[i] = new ChallengeEffectEntry(
                effect.sourceChallenge,
                effect.effectType,
                effect.value,
                effect.clearsOnNextChallengeEntry,
                effect.debugLabel
            );
        }

        return result;
    }

    public void ImportActiveEffects(ChallengeEffectEntry[] effects)
    {
        activeEffects.Clear();

        if (effects != null)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                ChallengeEffectEntry effect = effects[i];
                if (effect == null)
                    continue;

                if (effect.effectType == ChallengeEffectType.None)
                    continue;

                activeEffects.Add(new ChallengeEffectEntry(
                    effect.sourceChallenge,
                    effect.effectType,
                    effect.value,
                    effect.clearsOnNextChallengeEntry,
                    effect.debugLabel
                ));
            }
        }

        if (logEffectChanges)
            Debug.Log($"[ChallengeEffectManager] Imported {activeEffects.Count} active challenge effect(s).");

        OnEffectsChanged?.Invoke();
    }
}