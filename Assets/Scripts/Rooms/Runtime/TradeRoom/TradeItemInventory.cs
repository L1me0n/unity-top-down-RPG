using System;
using UnityEngine;

public class TradeItemInventory : MonoBehaviour
{
    [Header("Starting Counts")]
    [SerializeField] private int chronosSpellCount;
    [SerializeField] private int bloodlustPotionCount;
    [SerializeField] private int ectoplasmPotionCount;
    [SerializeField] private int horsemenRingCount;

    [Header("Debug")]
    [SerializeField] private bool logChanges = true;

    public event Action OnInventoryChanged;
    public event Action<TradeItemType, int> OnItemCountChanged;

    public int ChronosSpellCount => chronosSpellCount;
    public int BloodlustPotionCount => bloodlustPotionCount;
    public int EctoplasmPotionCount => ectoplasmPotionCount;
    public int HorsemenRingCount => horsemenRingCount;

    private void Awake()
    {
        ClampAllCounts();
    }

    public int GetCount(TradeItemType itemType)
    {
        switch (itemType)
        {
            case TradeItemType.ChronosSpell:
                return chronosSpellCount;

            case TradeItemType.BloodlustPotion:
                return bloodlustPotionCount;

            case TradeItemType.EctoplasmPotion:
                return ectoplasmPotionCount;

            case TradeItemType.HorsemenRing:
                return horsemenRingCount;

            case TradeItemType.None:
            default:
                return 0;
        }
    }

    public int GetMaxStack(TradeItemType itemType)
    {
        return TradeItemCatalog.GetMaxStack(itemType);
    }

    public bool IsFull(TradeItemType itemType)
    {
        TradeItemDefinition definition = TradeItemCatalog.Get(itemType);

        if (definition == null)
            return true;

        return GetCount(itemType) >= definition.maxStack;
    }

    public bool CanAdd(TradeItemType itemType, int amount = 1)
    {
        if (amount <= 0)
            return false;

        TradeItemDefinition definition = TradeItemCatalog.Get(itemType);

        if (definition == null)
            return false;

        int current = GetCount(itemType);
        return current + amount <= definition.maxStack;
    }

    public bool TryAdd(TradeItemType itemType, int amount = 1)
    {
        if (!CanAdd(itemType, amount))
        {
            if (logChanges)
            {
                Debug.Log(
                    $"[TradeItemInventory] Cannot add {amount} {itemType}. " +
                    $"Current={GetCount(itemType)}, Max={GetMaxStack(itemType)}",
                    this
                );
            }

            return false;
        }

        int newCount = GetCount(itemType) + amount;
        SetCountInternal(itemType, newCount, notify: true);
        return true;
    }

    public bool CanConsume(TradeItemType itemType, int amount = 1)
    {
        if (amount <= 0)
            return false;

        TradeItemDefinition definition = TradeItemCatalog.Get(itemType);

        if (definition == null)
            return false;

        return GetCount(itemType) >= amount;
    }

    public bool TryConsume(TradeItemType itemType, int amount = 1)
    {
        if (!CanConsume(itemType, amount))
        {
            if (logChanges)
            {
                Debug.Log(
                    $"[TradeItemInventory] Cannot consume {amount} {itemType}. " +
                    $"Current={GetCount(itemType)}",
                    this
                );
            }

            return false;
        }

        int newCount = GetCount(itemType) - amount;
        SetCountInternal(itemType, newCount, notify: true);
        return true;
    }

    public void SetCountFromSave(TradeItemType itemType, int savedCount)
    {
        SetCountInternal(itemType, savedCount, notify: true);
    }

    public void ResetInventory()
    {
        bool changed = false;

        changed |= SetCountInternal(TradeItemType.ChronosSpell, 0, notify: false);
        changed |= SetCountInternal(TradeItemType.BloodlustPotion, 0, notify: false);
        changed |= SetCountInternal(TradeItemType.EctoplasmPotion, 0, notify: false);
        changed |= SetCountInternal(TradeItemType.HorsemenRing, 0, notify: false);

        if (changed)
            NotifyInventoryChanged();
    }

    public string GetDebugSummary()
    {
        return
            $"Chronos={chronosSpellCount}/{GetMaxStack(TradeItemType.ChronosSpell)}, " +
            $"Bloodlust={bloodlustPotionCount}/{GetMaxStack(TradeItemType.BloodlustPotion)}, " +
            $"Ectoplasm={ectoplasmPotionCount}/{GetMaxStack(TradeItemType.EctoplasmPotion)}, " +
            $"Ring={horsemenRingCount}/{GetMaxStack(TradeItemType.HorsemenRing)}";
    }

    private bool SetCountInternal(TradeItemType itemType, int desiredCount, bool notify)
    {
        TradeItemDefinition definition = TradeItemCatalog.Get(itemType);

        if (definition == null)
            return false;

        int clamped = Mathf.Clamp(desiredCount, 0, definition.maxStack);
        int oldCount = GetCount(itemType);

        if (oldCount == clamped)
            return false;

        switch (itemType)
        {
            case TradeItemType.ChronosSpell:
                chronosSpellCount = clamped;
                break;

            case TradeItemType.BloodlustPotion:
                bloodlustPotionCount = clamped;
                break;

            case TradeItemType.EctoplasmPotion:
                ectoplasmPotionCount = clamped;
                break;

            case TradeItemType.HorsemenRing:
                horsemenRingCount = clamped;
                break;

            default:
                return false;
        }

        if (logChanges)
        {
            Debug.Log(
                $"[TradeItemInventory] {TradeItemCatalog.GetDisplayName(itemType)} count changed: " +
                $"{oldCount} -> {clamped}",
                this
            );
        }

        if (notify)
        {
            OnItemCountChanged?.Invoke(itemType, clamped);
            NotifyInventoryChanged();
        }

        return true;
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    private void ClampAllCounts()
    {
        SetCountInternal(TradeItemType.ChronosSpell, chronosSpellCount, notify: false);
        SetCountInternal(TradeItemType.BloodlustPotion, bloodlustPotionCount, notify: false);
        SetCountInternal(TradeItemType.EctoplasmPotion, ectoplasmPotionCount, notify: false);
        SetCountInternal(TradeItemType.HorsemenRing, horsemenRingCount, notify: false);
    }

    [ContextMenu("Debug Add One Of Each")]
    private void DebugAddOneOfEach()
    {
        TryAdd(TradeItemType.ChronosSpell);
        TryAdd(TradeItemType.BloodlustPotion);
        TryAdd(TradeItemType.EctoplasmPotion);
        TryAdd(TradeItemType.HorsemenRing);

        Debug.Log($"[TradeItemInventory] {GetDebugSummary()}", this);
    }

    [ContextMenu("Debug Fill All")]
    private void DebugFillAll()
    {
        SetCountInternal(TradeItemType.ChronosSpell, GetMaxStack(TradeItemType.ChronosSpell), notify: true);
        SetCountInternal(TradeItemType.BloodlustPotion, GetMaxStack(TradeItemType.BloodlustPotion), notify: true);
        SetCountInternal(TradeItemType.EctoplasmPotion, GetMaxStack(TradeItemType.EctoplasmPotion), notify: true);
        SetCountInternal(TradeItemType.HorsemenRing, GetMaxStack(TradeItemType.HorsemenRing), notify: true);

        Debug.Log($"[TradeItemInventory] {GetDebugSummary()}", this);
    }

    [ContextMenu("Debug Clear All")]
    private void DebugClearAll()
    {
        ResetInventory();
        Debug.Log($"[TradeItemInventory] {GetDebugSummary()}", this);
    }

    [ContextMenu("Debug Print Summary")]
    private void DebugPrintSummary()
    {
        Debug.Log($"[TradeItemInventory] {GetDebugSummary()}", this);
    }
}