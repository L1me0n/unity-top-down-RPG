using System;
using UnityEngine;

public class TradeItemEffectManager : MonoBehaviour
{
    public static TradeItemEffectManager Instance { get; private set; }

    [Header("Durations")]
    [SerializeField] private float chronosDuration = 5f;
    [SerializeField] private float bloodlustDuration = 15f;
    [SerializeField] private float ectoplasmDuration = 30f;

    [Header("Effect Values")]
    [SerializeField] private float ectoplasmDisappearBonusSeconds = 3f;

    [Header("Debug")]
    [SerializeField] private bool logEffects = true;

    private float chronosRemaining;
    private float bloodlustRemaining;
    private float ectoplasmRemaining;

    private bool wasChronosActive;
    private bool wasBloodlustActive;
    private bool wasEctoplasmActive;

    public bool IsChronosActive => chronosRemaining > 0f;
    public bool IsBloodlustActive => bloodlustRemaining > 0f;
    public bool IsEctoplasmActive => ectoplasmRemaining > 0f;

    public float ChronosRemaining => Mathf.Max(0f, chronosRemaining);
    public float BloodlustRemaining => Mathf.Max(0f, bloodlustRemaining);
    public float EctoplasmRemaining => Mathf.Max(0f, ectoplasmRemaining);

    public float ChronosDuration => Mathf.Max(0.01f, chronosDuration);
    public float BloodlustDuration => Mathf.Max(0.01f, bloodlustDuration);
    public float EctoplasmDuration => Mathf.Max(0.01f, ectoplasmDuration);

    public float ChronosProgress01 => GetProgress01(ChronosRemaining, ChronosDuration);
    public float BloodlustProgress01 => GetProgress01(BloodlustRemaining, BloodlustDuration);
    public float EctoplasmProgress01 => GetProgress01(EctoplasmRemaining, EctoplasmDuration);

    public float EctoplasmDisappearBonusSeconds =>
        IsEctoplasmActive ? ectoplasmDisappearBonusSeconds : 0f;

    public event Action OnEffectsChanged;
    public event Action<TradeItemType, bool> OnEffectActiveChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[TradeItemEffectManager] Duplicate instance found. Destroying duplicate.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        bool changed = false;

        changed |= TickEffect(ref chronosRemaining, TradeItemType.ChronosSpell, ref wasChronosActive);
        changed |= TickEffect(ref bloodlustRemaining, TradeItemType.BloodlustPotion, ref wasBloodlustActive);
        changed |= TickEffect(ref ectoplasmRemaining, TradeItemType.EctoplasmPotion, ref wasEctoplasmActive);

        if (changed)
            OnEffectsChanged?.Invoke();
    }

    public bool Activate(TradeItemType itemType)
    {
        switch (itemType)
        {
            case TradeItemType.ChronosSpell:
                return ActivateChronos();

            case TradeItemType.BloodlustPotion:
                return ActivateBloodlust();

            case TradeItemType.EctoplasmPotion:
                return ActivateEctoplasm();

            case TradeItemType.HorsemenRing:
                if (logEffects)
                {
                    Debug.Log(
                        "[TradeItemEffectManager] Horsemen Ring is passive. " +
                        "It should not be activated manually. Death-save comes in 9.6.",
                        this
                    );
                }

                return false;

            case TradeItemType.None:
            default:
                if (logEffects)
                    Debug.LogWarning($"[TradeItemEffectManager] Cannot activate unsupported item type: {itemType}", this);

                return false;
        }
    }

    public bool ActivateChronos()
    {
        return ActivateTimer(
            TradeItemType.ChronosSpell,
            ref chronosRemaining,
            ref wasChronosActive,
            chronosDuration
        );
    }

    public bool ActivateBloodlust()
    {
        return ActivateTimer(
            TradeItemType.BloodlustPotion,
            ref bloodlustRemaining,
            ref wasBloodlustActive,
            bloodlustDuration
        );
    }

    public bool ActivateEctoplasm()
    {
        return ActivateTimer(
            TradeItemType.EctoplasmPotion,
            ref ectoplasmRemaining,
            ref wasEctoplasmActive,
            ectoplasmDuration
        );
    }

    public void ClearAllEffects()
    {
        bool changed = false;

        changed |= ForceEndEffect(TradeItemType.ChronosSpell, ref chronosRemaining, ref wasChronosActive);
        changed |= ForceEndEffect(TradeItemType.BloodlustPotion, ref bloodlustRemaining, ref wasBloodlustActive);
        changed |= ForceEndEffect(TradeItemType.EctoplasmPotion, ref ectoplasmRemaining, ref wasEctoplasmActive);

        if (changed)
            OnEffectsChanged?.Invoke();
    }

    public float GetRemaining(TradeItemType itemType)
    {
        switch (itemType)
        {
            case TradeItemType.ChronosSpell:
                return ChronosRemaining;

            case TradeItemType.BloodlustPotion:
                return BloodlustRemaining;

            case TradeItemType.EctoplasmPotion:
                return EctoplasmRemaining;

            default:
                return 0f;
        }
    }

    public float GetDuration(TradeItemType itemType)
    {
        switch (itemType)
        {
            case TradeItemType.ChronosSpell:
                return ChronosDuration;

            case TradeItemType.BloodlustPotion:
                return BloodlustDuration;

            case TradeItemType.EctoplasmPotion:
                return EctoplasmDuration;

            default:
                return 0.01f;
        }
    }

    public float GetProgress01(TradeItemType itemType)
    {
        return GetProgress01(GetRemaining(itemType), GetDuration(itemType));
    }

    public bool IsActive(TradeItemType itemType)
    {
        switch (itemType)
        {
            case TradeItemType.ChronosSpell:
                return IsChronosActive;

            case TradeItemType.BloodlustPotion:
                return IsBloodlustActive;

            case TradeItemType.EctoplasmPotion:
                return IsEctoplasmActive;

            default:
                return false;
        }
    }

    public string GetDebugSummary()
    {
        return
            $"Chronos={ChronosRemaining:0.00}s active={IsChronosActive}, " +
            $"Bloodlust={BloodlustRemaining:0.00}s active={IsBloodlustActive}, " +
            $"Ectoplasm={EctoplasmRemaining:0.00}s active={IsEctoplasmActive}, " +
            $"EctoplasmBonus={EctoplasmDisappearBonusSeconds:0.00}s";
    }

    private bool ActivateTimer(
        TradeItemType itemType,
        ref float remaining,
        ref bool wasActiveFlag,
        float duration)
    {
        if (duration <= 0f)
        {
            Debug.LogWarning($"[TradeItemEffectManager] Cannot activate {itemType}. Duration must be greater than 0.", this);
            return false;
        }

        bool wasAlreadyActive = remaining > 0f;

        remaining = duration;

        if (!wasActiveFlag)
        {
            wasActiveFlag = true;
            OnEffectActiveChanged?.Invoke(itemType, true);
        }

        OnEffectsChanged?.Invoke();

        if (logEffects)
        {
            string displayName = TradeItemCatalog.GetDisplayName(itemType);
            string action = wasAlreadyActive ? "refreshed" : "activated";

            Debug.Log(
                $"[TradeItemEffectManager] {displayName} {action}. " +
                $"Duration={duration:0.00}s",
                this
            );
        }

        return true;
    }

    private bool TickEffect(ref float remaining, TradeItemType itemType, ref bool wasActiveFlag)
    {
        if (remaining <= 0f)
            return false;

        remaining -= Time.deltaTime;

        if (remaining > 0f)
            return false;

        remaining = 0f;

        if (wasActiveFlag)
        {
            wasActiveFlag = false;
            OnEffectActiveChanged?.Invoke(itemType, false);

            if (logEffects)
                Debug.Log($"[TradeItemEffectManager] {TradeItemCatalog.GetDisplayName(itemType)} ended.", this);
        }

        return true;
    }

    private bool ForceEndEffect(TradeItemType itemType, ref float remaining, ref bool wasActiveFlag)
    {
        bool wasActive = remaining > 0f || wasActiveFlag;

        remaining = 0f;

        if (wasActiveFlag)
        {
            wasActiveFlag = false;
            OnEffectActiveChanged?.Invoke(itemType, false);
        }

        if (wasActive && logEffects)
            Debug.Log($"[TradeItemEffectManager] {TradeItemCatalog.GetDisplayName(itemType)} force-ended.", this);

        return wasActive;
    }

    private float GetProgress01(float remaining, float duration)
    {
        if (duration <= 0f)
            return 0f;

        return Mathf.Clamp01(remaining / duration);
    }

    [ContextMenu("Debug Activate Chronos")]
    private void DebugActivateChronos()
    {
        ActivateChronos();
    }

    [ContextMenu("Debug Activate Bloodlust")]
    private void DebugActivateBloodlust()
    {
        ActivateBloodlust();
    }

    [ContextMenu("Debug Activate Ectoplasm")]
    private void DebugActivateEctoplasm()
    {
        ActivateEctoplasm();
    }

    [ContextMenu("Debug Clear All Effects")]
    private void DebugClearAllEffects()
    {
        ClearAllEffects();
    }

    [ContextMenu("Debug Print Summary")]
    private void DebugPrintSummary()
    {
        Debug.Log($"[TradeItemEffectManager] {GetDebugSummary()}", this);
    }
}