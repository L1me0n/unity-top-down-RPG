using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHP = 5;
    [SerializeField] private int currentHP;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;

    public System.Action<int, int> OnChanged; // (current, max)
    public System.Action OnDied;
    public System.Action OnDamaged;

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = Mathf.Clamp(currentHP <= 0 ? maxHP : currentHP, 0, maxHP);
        OnChanged?.Invoke(currentHP, maxHP);
    }

    public void SetMax(int newMax, bool fill = true)
    {
        maxHP = Mathf.Max(1, newMax);
        currentHP = fill ? maxHP : Mathf.Min(currentHP, maxHP);
        OnChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (currentHP <= 0) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        OnChanged?.Invoke(currentHP, maxHP);
        OnDamaged?.Invoke();

        if (currentHP == 0)
            OnDied?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (currentHP <= 0) return;

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnChanged?.Invoke(currentHP, maxHP);
    }
}