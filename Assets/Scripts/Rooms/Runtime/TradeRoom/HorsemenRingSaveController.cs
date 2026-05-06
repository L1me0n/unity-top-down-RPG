using UnityEngine;

public class HorsemenRingSaveController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private TradeItemInventory inventory;
    [SerializeField] private PlayerLevelUpFlash flash;

    [Header("Flash")]
    [SerializeField] private Color ringSaveFlashColor = new Color(1f, 0.78f, 0.12f, 1f);

    [Header("Debug")]
    [SerializeField] private bool logSaves = true;
    [SerializeField] private bool logFailedSaves = false;

    public bool HasRingAvailable =>
        inventory != null && inventory.GetCount(TradeItemType.HorsemenRing) > 0;

    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<PlayerStats>();

        if (inventory == null)
            inventory = FindFirstObjectByType<TradeItemInventory>();

        if (flash == null)
            flash = GetComponent<PlayerLevelUpFlash>();
    }

    public bool TryPreventDeath()
    {
        if (stats == null)
        {
            if (logFailedSaves)
                Debug.LogWarning("[HorsemenRingSaveController] Cannot save player because PlayerStats is missing.", this);

            return false;
        }

        if (inventory == null)
        {
            if (logFailedSaves)
                Debug.LogWarning("[HorsemenRingSaveController] Cannot save player because TradeItemInventory is missing.", this);

            return false;
        }

        if (!inventory.CanConsume(TradeItemType.HorsemenRing))
        {
            if (logFailedSaves)
                Debug.Log("[HorsemenRingSaveController] No Horsemen Ring available.", this);

            return false;
        }

        bool consumed = inventory.TryConsume(TradeItemType.HorsemenRing);

        if (!consumed)
        {
            if (logFailedSaves)
                Debug.LogWarning("[HorsemenRingSaveController] Ring was available but could not be consumed.", this);

            return false;
        }

        RestoreFullHealth();
        PlayRingSaveFlash();

        if (logSaves)
        {
            Debug.Log(
                $"[HorsemenRingSaveController] Horsemen Ring prevented death. " +
                $"HP restored to {stats.HP}/{stats.MaxHP}. " +
                $"Remaining Rings={inventory.GetCount(TradeItemType.HorsemenRing)}/{inventory.GetMaxStack(TradeItemType.HorsemenRing)}",
                this
            );
        }

        return true;
    }

    private void RestoreFullHealth()
    {
        int missingHP = stats.MaxHP - stats.HP;

        if (missingHP > 0)
            stats.Heal(missingHP);
        else if (stats.HP <= 0)
            stats.Heal(stats.MaxHP);
    }

    private void PlayRingSaveFlash()
    {
        if (flash != null)
            flash.PlayFlashWithColor(ringSaveFlashColor);
    }
}