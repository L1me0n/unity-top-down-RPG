using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private int baseMaxHP = GameConfig.PlayerStartHP;
    [SerializeField] private int baseMaxAP = GameConfig.PlayerStartAP;
    [SerializeField] private int baseDP = GameConfig.PlayerStartDP;
    [SerializeField] private float baseDisappearDuration = GameConfig.FadeDisappearSeconds;

    [Header("Runtime")]
    [SerializeField] private int maxHP;
    [SerializeField] private int maxAP;
    [SerializeField] private int dp;
    [SerializeField] private int currentHP;
    [SerializeField] private int currentAP;
    [SerializeField] private float disappearDuration;

    public int BaseMaxHP => baseMaxHP;
    public int BaseMaxAP => baseMaxAP;
    public int BaseDP => baseDP;

    public int MaxHP => maxHP;
    public int MaxAP => maxAP;
    public int DP => dp;
    public int HP => currentHP;
    public int AP => currentAP;
    public float DisappearDuration => disappearDuration;

    public System.Action OnChanged;

    private void Awake()
    {
        ResetToBase();
    }

    public void ResetToBase()
    {
        maxHP = Mathf.Max(1, baseMaxHP);
        maxAP = Mathf.Max(1, baseMaxAP);
        dp = Mathf.Max(1, baseDP);

        currentHP = maxHP;
        currentAP = maxAP;

        disappearDuration = Mathf.Max(0.1f, baseDisappearDuration);

        OnChanged?.Invoke();
    }

    public bool CanSpendAP(int amount) => amount <= 0 || currentAP >= amount;

    public bool TrySpendAP(int amount)
    {
        if (amount <= 0) return true;
        if (currentAP < amount) return false;

        currentAP -= amount;
        OnChanged?.Invoke();
        return true;
    }

    public void GainAP(int amount)
    {
        if (amount <= 0) return;
        currentAP = Mathf.Min(maxAP, currentAP + amount);
        OnChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        currentHP = Mathf.Max(0, currentHP - amount);
        OnChanged?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnChanged?.Invoke();
    }

    public void SetMaxHP(int newMax)
    {
        maxHP = Mathf.Max(1, newMax);
        currentHP = Mathf.Min(currentHP, maxHP);
        OnChanged?.Invoke();
    }

    public void SetMaxAP(int newMax)
    {
        maxAP = Mathf.Max(1, newMax);
        currentAP = Mathf.Min(currentAP, maxAP);
        OnChanged?.Invoke();
    }

    public void SetDP(int newDP)
    {
        dp = Mathf.Max(1, newDP);
        OnChanged?.Invoke();
    }

    public void SetDisappearDuration(float newDuration)
    {
        disappearDuration = Mathf.Max(0.1f, newDuration);
        OnChanged?.Invoke();
    }
}