public static class UIInputBlocker
{
    // When true, gameplay input should be ignored.
    public static bool BlockGameplayInput { get; set; }

    // When true, the upgrade menu should ignore its toggle key.
    // Useful when another E-based interaction is active, like the merchant.
    public static bool BlockUpgradeMenuToggle { get; set; }
}