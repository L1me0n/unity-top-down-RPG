using System.Collections.Generic;
using UnityEngine;

public static class UIInputBlocker
{
    public const string LockUpgradeMenu = "UpgradeMenu";
    public const string LockTradeMerchant = "TradeMerchant";
    public const string LockTradeShop = "TradeShop";
    public const string LockClueMenu = "ClueMenu";
    public const string LockGluttonyVictory = "GluttonyVictory";
    public const string LockInventoryMenu = "InventoryMenu";
    public const string LockPauseMenu = "PauseMenu";
    public const string LockTipsMenu = "TipsMenu";
    public const string LockSavesMenu = "SavesMenu";
    public const string LockOptionsMenu = "OptionsMenu";
    public const string LockMainMenu = "MainMenu";
    public const string LockDebugPanel = "DebugPanel";

    private static readonly HashSet<string> gameplayLocks = new HashSet<string>();
    private static readonly HashSet<string> upgradeMenuLocks = new HashSet<string>();
    private static readonly HashSet<string> pauseToggleLocks = new HashSet<string>();
    private static readonly HashSet<string> inventoryToggleLocks = new HashSet<string>();
    private static readonly HashSet<string> clueMenuToggleLocks = new HashSet<string>();
    private static readonly HashSet<string> tradeItemHotkeyLocks = new HashSet<string>();

    private static bool legacyBlockGameplayInput;
    private static bool legacyBlockUpgradeMenuToggle;
    private static bool legacyBlockPauseToggle;
    private static bool legacyBlockInventoryToggle;
    private static bool legacyBlockClueMenuToggle;
    private static bool legacyBlockTradeItemHotkeys;

    public static bool BlockGameplayInput
    {
        get => legacyBlockGameplayInput || gameplayLocks.Count > 0;
        set => legacyBlockGameplayInput = value;
    }

    public static bool BlockUpgradeMenuToggle
    {
        get => legacyBlockUpgradeMenuToggle || upgradeMenuLocks.Count > 0;
        set => legacyBlockUpgradeMenuToggle = value;
    }

    public static bool BlockPauseToggle
    {
        get => legacyBlockPauseToggle || pauseToggleLocks.Count > 0;
        set => legacyBlockPauseToggle = value;
    }

    public static bool BlockInventoryToggle
    {
        get => legacyBlockInventoryToggle || inventoryToggleLocks.Count > 0;
        set => legacyBlockInventoryToggle = value;
    }

    public static bool BlockClueMenuToggle
    {
        get => legacyBlockClueMenuToggle || clueMenuToggleLocks.Count > 0;
        set => legacyBlockClueMenuToggle = value;
    }

    public static bool BlockTradeItemHotkeys
    {
        get => legacyBlockTradeItemHotkeys || tradeItemHotkeyLocks.Count > 0;
        set => legacyBlockTradeItemHotkeys = value;
    }

    public static bool AnyBlockingUIOpen => BlockGameplayInput;

    public static void SetGameplayBlocked(string ownerKey, bool blocked)
    {
        SetLock(gameplayLocks, ownerKey, blocked);
    }

    public static void SetUpgradeMenuBlocked(string ownerKey, bool blocked)
    {
        SetLock(upgradeMenuLocks, ownerKey, blocked);
    }

    public static void SetPauseToggleBlocked(string ownerKey, bool blocked)
    {
        SetLock(pauseToggleLocks, ownerKey, blocked);
    }

    public static void SetInventoryToggleBlocked(string ownerKey, bool blocked)
    {
        SetLock(inventoryToggleLocks, ownerKey, blocked);
    }

    public static void SetClueMenuToggleBlocked(string ownerKey, bool blocked)
    {
        SetLock(clueMenuToggleLocks, ownerKey, blocked);
    }

    public static void SetTradeItemHotkeysBlocked(string ownerKey, bool blocked)
    {
        SetLock(tradeItemHotkeyLocks, ownerKey, blocked);
    }

    public static bool IsGameplayBlockedBy(string ownerKey)
    {
        return IsLockedBy(gameplayLocks, ownerKey);
    }

    public static bool IsUpgradeMenuBlockedBy(string ownerKey)
    {
        return IsLockedBy(upgradeMenuLocks, ownerKey);
    }

    public static void ReleaseOwner(string ownerKey)
    {
        if (string.IsNullOrWhiteSpace(ownerKey))
            return;

        gameplayLocks.Remove(ownerKey);
        upgradeMenuLocks.Remove(ownerKey);
        pauseToggleLocks.Remove(ownerKey);
        inventoryToggleLocks.Remove(ownerKey);
        clueMenuToggleLocks.Remove(ownerKey);
        tradeItemHotkeyLocks.Remove(ownerKey);
    }

    public static void ClearAll()
    {
        legacyBlockGameplayInput = false;
        legacyBlockUpgradeMenuToggle = false;
        legacyBlockPauseToggle = false;
        legacyBlockInventoryToggle = false;
        legacyBlockClueMenuToggle = false;
        legacyBlockTradeItemHotkeys = false;

        gameplayLocks.Clear();
        upgradeMenuLocks.Clear();
        pauseToggleLocks.Clear();
        inventoryToggleLocks.Clear();
        clueMenuToggleLocks.Clear();
        tradeItemHotkeyLocks.Clear();
    }

    public static string GetDebugSummary()
    {
        return
            $"Gameplay={BlockGameplayInput} [{string.Join(", ", gameplayLocks)}] | " +
            $"Upgrade={BlockUpgradeMenuToggle} [{string.Join(", ", upgradeMenuLocks)}] | " +
            $"Pause={BlockPauseToggle} [{string.Join(", ", pauseToggleLocks)}] | " +
            $"Inventory={BlockInventoryToggle} [{string.Join(", ", inventoryToggleLocks)}] | " +
            $"Clue={BlockClueMenuToggle} [{string.Join(", ", clueMenuToggleLocks)}] | " +
            $"TradeHotkeys={BlockTradeItemHotkeys} [{string.Join(", ", tradeItemHotkeyLocks)}]";
    }

    private static void SetLock(HashSet<string> targetSet, string ownerKey, bool blocked)
    {
        if (string.IsNullOrWhiteSpace(ownerKey))
        {
            Debug.LogWarning("[UIInputBlocker] Ignored lock request with empty owner key.");
            return;
        }

        if (blocked)
            targetSet.Add(ownerKey);
        else
            targetSet.Remove(ownerKey);
    }

    private static bool IsLockedBy(HashSet<string> targetSet, string ownerKey)
    {
        if (string.IsNullOrWhiteSpace(ownerKey))
            return false;

        return targetSet.Contains(ownerKey);
    }
}