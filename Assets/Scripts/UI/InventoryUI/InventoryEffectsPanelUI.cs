using System.Text;
using UnityEngine;
using TMPro;

public class InventoryEffectsPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChallengeEffectManager challengeEffectManager;

    [Header("Text")]
    [SerializeField] private TMP_Text effectsTitleText;
    [SerializeField] private TMP_Text effectsListText;

    [Header("Options")]
    [SerializeField] private bool autoFindReferences = true;

    private void Awake()
    {
        FindReferencesIfNeeded();

        if (effectsTitleText != null)
            effectsTitleText.text = "Active Challenge Effects";
    }

    private void OnEnable()
    {
        if (challengeEffectManager != null)
            challengeEffectManager.OnEffectsChanged += HandleEffectsChanged;
    }

    private void OnDisable()
    {
        if (challengeEffectManager != null)
            challengeEffectManager.OnEffectsChanged -= HandleEffectsChanged;
    }

    public void Refresh()
    {
        FindReferencesIfNeeded();

        if (effectsListText == null)
            return;

        if (challengeEffectManager == null)
        {
            effectsListText.text = "No effect manager found.";
            return;
        }

        if (challengeEffectManager.ActiveEffectCount <= 0)
        {
            effectsListText.text = "No active challenge effects.";
            return;
        }

        StringBuilder sb = new StringBuilder();

        foreach (ChallengeEffectEntry effect in challengeEffectManager.ActiveEffects)
        {
            if (effect == null || effect.effectType == ChallengeEffectType.None)
                continue;

            sb.Append("• ");
            sb.Append(FormatEffect(effect));
            sb.AppendLine();
        }

        effectsListText.text = sb.Length > 0
            ? sb.ToString()
            : "No active challenge effects.";
    }

    private void HandleEffectsChanged()
    {
        Refresh();
    }

    private void FindReferencesIfNeeded()
    {
        if (!autoFindReferences)
            return;

        if (challengeEffectManager == null)
            challengeEffectManager = FindFirstObjectByType<ChallengeEffectManager>();
    }

    private string FormatEffect(ChallengeEffectEntry effect)
    {
        if (!string.IsNullOrWhiteSpace(effect.debugLabel))
            return effect.debugLabel;

        string source = effect.sourceChallenge.ToString();

        switch (effect.effectType)
        {
            case ChallengeEffectType.BonusMaxHP:
                return $"{source}: +{FormatValue(effect.value)} Max HP until next Challenge";

            case ChallengeEffectType.LockedAP:
                return $"{source}: {FormatValue(effect.value)} AP locked until next Challenge";

            case ChallengeEffectType.TempMaxHPLoss:
                return $"{source}: -{FormatValue(effect.value)} Max HP until next Challenge";

            case ChallengeEffectType.TempMaxAPLoss:
                return $"{source}: -{FormatValue(effect.value)} Max AP until next Challenge";

            case ChallengeEffectType.TempDPBonus:
                return $"{source}: +{FormatValue(effect.value)} DMG until next Challenge";

            case ChallengeEffectType.TempDPMultiplier:
                return $"{source}: DMG x{effect.value:0.##} until next Challenge";

            case ChallengeEffectType.TempStatHalving:
                return $"{source}: Stats halved until next Challenge";

            case ChallengeEffectType.None:
            default:
                return $"{source}: {effect.effectType} {effect.value:0.##}";
        }
    }

    private string FormatValue(float value)
    {
        if (Mathf.Approximately(value, Mathf.Round(value)))
            return Mathf.RoundToInt(value).ToString();

        return value.ToString("0.##");
    }
}