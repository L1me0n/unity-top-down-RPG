using UnityEngine;

public class TradeItemHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TradeItemInventory inventory;

    [Header("Slots")]
    [SerializeField] private TradeItemHUDSlotUI chronosSlot;
    [SerializeField] private TradeItemHUDSlotUI bloodlustSlot;
    [SerializeField] private TradeItemHUDSlotUI ectoplasmSlot;
    [SerializeField] private TradeItemHUDSlotUI ringSlot;

    [Header("Display")]
    [Tooltip("If true, item slots with 0 count are hidden. If false, they show as 0/5.")]
    [SerializeField] private bool hideEmptySlots = false;

    [Header("Debug")]
    [SerializeField] private bool logRefresh;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<TradeItemInventory>();
    }

    private void OnEnable()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<TradeItemInventory>();

        if (inventory != null)
        {
            inventory.OnInventoryChanged += HandleInventoryChanged;
            inventory.OnItemCountChanged += HandleItemCountChanged;
        }

        RefreshAll();
    }

    private void Start()
    {
        RefreshAll();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= HandleInventoryChanged;
            inventory.OnItemCountChanged -= HandleItemCountChanged;
        }
    }

    public void RefreshAll()
    {
        bool showWhenEmpty = !hideEmptySlots;

        RefreshSlot(chronosSlot, TradeItemType.ChronosSpell, showWhenEmpty);
        RefreshSlot(bloodlustSlot, TradeItemType.BloodlustPotion, showWhenEmpty);
        RefreshSlot(ectoplasmSlot, TradeItemType.EctoplasmPotion, showWhenEmpty);
        RefreshSlot(ringSlot, TradeItemType.HorsemenRing, showWhenEmpty);

        if (logRefresh && inventory != null)
            Debug.Log($"[TradeItemHUD] Refreshed: {inventory.GetDebugSummary()}", this);
    }

    private void RefreshSlot(TradeItemHUDSlotUI slot, TradeItemType itemType, bool showWhenEmpty)
    {
        if (slot == null)
            return;

        int count = inventory != null ? inventory.GetCount(itemType) : 0;
        slot.Setup(itemType, count, showWhenEmpty);
    }

    private void HandleInventoryChanged()
    {
        RefreshAll();
    }

    private void HandleItemCountChanged(TradeItemType itemType, int count)
    {
        bool showWhenEmpty = !hideEmptySlots;

        switch (itemType)
        {
            case TradeItemType.ChronosSpell:
                if (chronosSlot != null)
                    chronosSlot.Setup(itemType, count, showWhenEmpty);
                break;

            case TradeItemType.BloodlustPotion:
                if (bloodlustSlot != null)
                    bloodlustSlot.Setup(itemType, count, showWhenEmpty);
                break;

            case TradeItemType.EctoplasmPotion:
                if (ectoplasmSlot != null)
                    ectoplasmSlot.Setup(itemType, count, showWhenEmpty);
                break;

            case TradeItemType.HorsemenRing:
                if (ringSlot != null)
                    ringSlot.Setup(itemType, count, showWhenEmpty);
                break;
        }
    }

    [ContextMenu("Debug Refresh HUD")]
    private void DebugRefreshHUD()
    {
        RefreshAll();
    }
}