using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
    [SerializeField] private DisappearController disappear;

    public bool IsDead => stats != null && stats.HP <= 0;

    public System.Action OnDied;

    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (disappear == null)
            disappear = GetComponent<DisappearController>();
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0) return;
        if (IsDead) return;

        // Invulnerable while disappeared
        if (disappear != null && disappear.IsDisappeared)
            return;

        stats.TakeDamage(amount);

        if (stats.HP <= 0)
            OnDied?.Invoke();
    }
}