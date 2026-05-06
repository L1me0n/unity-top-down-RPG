using UnityEngine;

public class TradeItemHotkeyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TradeItemInventory inventory;
    [SerializeField] private TradeItemEffectManager effectManager;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode chronosKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode bloodlustKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode ectoplasmKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode horsemenRingKey = KeyCode.Alpha4;

    [Header("Options")]
    [SerializeField] private bool allowNumpadKeys = true;

    [Header("Debug")]
    [SerializeField] private bool logUses = true;
    [SerializeField] private bool logFailedUses = true;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<TradeItemInventory>();

        if (effectManager == null)
            effectManager = FindFirstObjectByType<TradeItemEffectManager>();
    }

    private void Update()
    {
        if (UIInputBlocker.BlockGameplayInput)
            return;

        if (inventory == null)
            return;

        if (effectManager == null)
            effectManager = TradeItemEffectManager.Instance;

        if (effectManager == null)
            return;

        if (WasPressed(chronosKey, KeyCode.Keypad1))
        {
            TryUseActiveItem(TradeItemType.ChronosSpell);
            return;
        }

        if (WasPressed(bloodlustKey, KeyCode.Keypad2))
        {
            TryUseActiveItem(TradeItemType.BloodlustPotion);
            return;
        }

        if (WasPressed(ectoplasmKey, KeyCode.Keypad3))
        {
            TryUseActiveItem(TradeItemType.EctoplasmPotion);
            return;
        }

        if (WasPressed(horsemenRingKey, KeyCode.Keypad4))
        {
            HandleHorsemenRingHotkey();
            return;
        }
    }

    private bool WasPressed(KeyCode mainKey, KeyCode keypadKey)
    {
        if (Input.GetKeyDown(mainKey))
            return true;

        if (allowNumpadKeys && Input.GetKeyDown(keypadKey))
            return true;

        return false;
    }

    private void TryUseActiveItem(TradeItemType itemType)
    {
        TradeItemDefinition definition = TradeItemCatalog.Get(itemType);

        if (definition == null)
        {
            if (logFailedUses)
                Debug.LogWarning($"[TradeItemHotkeyController] Unknown item type: {itemType}", this);

            return;
        }

        if (!inventory.CanConsume(itemType))
        {
            if (logFailedUses)
            {
                Debug.Log(
                    $"[TradeItemHotkeyController] Cannot use {definition.displayName}. " +
                    $"Count is 0.",
                    this
                );
            }

            return;
        }

        bool activated = effectManager.Activate(itemType);

        if (!activated)
        {
            if (logFailedUses)
            {
                Debug.Log(
                    $"[TradeItemHotkeyController] {definition.displayName} was not activated, so item was not consumed.",
                    this
                );
            }

            return;
        }

        bool consumed = inventory.TryConsume(itemType);

        if (!consumed)
        {
            // This should almost never happen because CanConsume was checked before activation.
            // If it does happen, clear all effects would be too aggressive, so we only warn.
            // Later, if needed, TradeItemEffectManager can support canceling one specific effect.
            if (logFailedUses)
            {
                Debug.LogWarning(
                    $"[TradeItemHotkeyController] {definition.displayName} activated, but inventory consumption failed. " +
                    $"This should not happen.",
                    this
                );
            }

            return;
        }

        if (logUses)
        {
            Debug.Log(
                $"[TradeItemHotkeyController] Used {definition.displayName}. " +
                $"Remaining items={inventory.GetCount(itemType)}/{inventory.GetMaxStack(itemType)} | " +
                $"Effects: {effectManager.GetDebugSummary()}",
                this
            );
        }
    }

    private void HandleHorsemenRingHotkey()
    {
        TradeItemDefinition definition = TradeItemCatalog.Get(TradeItemType.HorsemenRing);

        if (definition == null)
        {
            if (logFailedUses)
                Debug.LogWarning("[TradeItemHotkeyController] Horsemen Ring definition missing.", this);

            return;
        }

        int count = inventory.GetCount(TradeItemType.HorsemenRing);

        if (count <= 0)
        {
            if (logFailedUses)
            {
                Debug.Log(
                    "[TradeItemHotkeyController] Horsemen Ring count is 0. Nothing passive is available.",
                    this
                );
            }

            return;
        }

        if (logUses)
        {
            Debug.Log(
                $"[TradeItemHotkeyController] Horsemen Ring is passive. " +
                $"Death-save behavior comes in 9.6. Current rings={count}/{definition.maxStack}",
                this
            );
        }
    }
}