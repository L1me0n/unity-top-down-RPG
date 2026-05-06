using UnityEngine;

public class TradeEffectTimerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TradeItemEffectManager effectManager;

    [Header("Slots")]
    [SerializeField] private TradeEffectTimerSlotUI chronosSlot;
    [SerializeField] private TradeEffectTimerSlotUI bloodlustSlot;
    [SerializeField] private TradeEffectTimerSlotUI ectoplasmSlot;

    [Header("Labels")]
    [SerializeField] private string chronosLabel = "1";
    [SerializeField] private string bloodlustLabel = "2";
    [SerializeField] private string ectoplasmLabel = "3";

    [Header("Colors")]
    [SerializeField] private Color chronosColor = new Color(0.35f, 0.75f, 1f, 1f);
    [SerializeField] private Color bloodlustColor = new Color(1f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color ectoplasmColor = new Color(0.45f, 1f, 0.55f, 1f);

    private void Awake()
    {
        if (effectManager == null)
            effectManager = FindFirstObjectByType<TradeItemEffectManager>();

        SetupSlots();
    }

    private void OnEnable()
    {
        if (effectManager == null)
            effectManager = FindFirstObjectByType<TradeItemEffectManager>();

        if (effectManager != null)
            effectManager.OnEffectsChanged += HandleEffectsChanged;

        SetupSlots();
        RefreshAll();
    }

    private void OnDisable()
    {
        if (effectManager != null)
            effectManager.OnEffectsChanged -= HandleEffectsChanged;
    }

    private void Update()
    {
        if (effectManager == null)
            return;

        RefreshAll();
    }

    private void SetupSlots()
    {
        if (chronosSlot != null)
            chronosSlot.Setup(chronosLabel, chronosColor);

        if (bloodlustSlot != null)
            bloodlustSlot.Setup(bloodlustLabel, bloodlustColor);

        if (ectoplasmSlot != null)
            ectoplasmSlot.Setup(ectoplasmLabel, ectoplasmColor);
    }

    private void RefreshAll()
    {
        if (effectManager == null)
        {
            HideAll();
            return;
        }

        RefreshSlot(chronosSlot, TradeItemType.ChronosSpell);
        RefreshSlot(bloodlustSlot, TradeItemType.BloodlustPotion);
        RefreshSlot(ectoplasmSlot, TradeItemType.EctoplasmPotion);
    }

    private void RefreshSlot(TradeEffectTimerSlotUI slot, TradeItemType itemType)
    {
        if (slot == null)
            return;

        bool active = effectManager.IsActive(itemType);
        float progress = effectManager.GetProgress01(itemType);
        float remaining = effectManager.GetRemaining(itemType);

        slot.Refresh(active, progress, remaining);
    }

    private void HideAll()
    {
        if (chronosSlot != null)
            chronosSlot.SetVisible(false);

        if (bloodlustSlot != null)
            bloodlustSlot.SetVisible(false);

        if (ectoplasmSlot != null)
            ectoplasmSlot.SetVisible(false);
    }

    private void HandleEffectsChanged()
    {
        RefreshAll();
    }
}