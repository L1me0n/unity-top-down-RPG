using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
    [SerializeField] private DisappearController disappear;
    [SerializeField] private HorsemenRingSaveController horsemenRingSave;

    public bool IsDead => stats != null && stats.HP <= 0;

    public System.Action OnDied;

    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (disappear == null)
            disappear = GetComponent<DisappearController>();

        if (horsemenRingSave == null)
            horsemenRingSave = GetComponent<HorsemenRingSaveController>();
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0) return;
        if (IsDead) return;

        // Invulnerable while disappeared.
        if (disappear != null && disappear.IsDisappeared)
            return;

        stats.TakeDamage(amount);

        var hpRegen = GetComponent<HPRegen>();
        if (hpRegen != null)
            hpRegen.NotifyDamaged();

        if (stats.HP <= 0)
        {
            if (horsemenRingSave != null && horsemenRingSave.TryPreventDeath())
                return;

            OnDied?.Invoke();
        }
    }
}