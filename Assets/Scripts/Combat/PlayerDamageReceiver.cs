using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
    [SerializeField] private DisappearController disappear;
    [SerializeField] private HorsemenRingSaveController horsemenRingSave;

    public bool IsDead => stats != null && stats.HP <= 0;

    public System.Action OnDied;
    public System.Action<int> OnMaxHPLost;

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

    public bool TryApplyMaxHPLoss(int amount)
    {
        if (amount <= 0)
            return false;

        if (IsDead)
            return false;

        // Eating Wave is avoidable with Disappear.
        if (disappear != null && disappear.IsDisappeared)
            return false;

        if (stats == null)
            stats = GetComponent<PlayerStats>();

        if (stats == null)
        {
            Debug.LogWarning("[PlayerDamageReceiver] Missing PlayerStats. Cannot apply Max HP loss.", this);
            return false;
        }

        bool lost = stats.TryLoseMaxHP(amount);

        if (lost)
            OnMaxHPLost?.Invoke(amount);

        return lost;
    }
}