using System;
using UnityEngine;

public class RunCurrency : MonoBehaviour
{
    [SerializeField] private int souls;
    [SerializeField] private int xp;

    public int Souls => souls;
    public int XP => xp;

    public event Action<int> OnSoulsChanged;
    public event Action<int> OnXPChanged;
    public event Action OnChanged;

    public void AddSouls(int amount)
    {
        if (amount <= 0) return;

        souls += amount;

        OnSoulsChanged?.Invoke(souls);
        OnChanged?.Invoke();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        xp += amount;

        OnXPChanged?.Invoke(xp);
        OnChanged?.Invoke();
    }

    public bool CanSpendSouls(int amount)
    {
        if (amount <= 0)
            return false;

        return souls >= amount;
    }

    public bool TrySpendSouls(int amount)
    {
        if (!CanSpendSouls(amount))
            return false;

        souls = Mathf.Max(0, souls - amount);

        OnSoulsChanged?.Invoke(souls);
        OnChanged?.Invoke();

        return true;
    }

    // Percentage-based soul loss.
    // Keep this for death penalties.
    public int TakeSouls(float amount)
    {
        if (amount <= 0.0f) return 0;

        int lost = Mathf.FloorToInt(souls * amount);
        souls = Mathf.Max(0, souls - lost);

        OnSoulsChanged?.Invoke(souls);
        OnChanged?.Invoke();

        return lost;
    }

    // Percentage-based XP loss.
    // Kept for compatibility, even if current death rules no longer use XP loss.
    public int TakeXP(float amount)
    {
        if (amount <= 0.0f) return 0;

        int lost = Mathf.FloorToInt(xp * amount);
        xp = Mathf.Max(0, xp - lost);

        OnXPChanged?.Invoke(xp);
        OnChanged?.Invoke();

        return lost;
    }

    public void SetSouls(int newSouls)
    {
        souls = Mathf.Max(0, newSouls);

        OnSoulsChanged?.Invoke(souls);
        OnChanged?.Invoke();
    }

    public void SetXP(int newXP)
    {
        xp = Mathf.Max(0, newXP);

        OnXPChanged?.Invoke(xp);
        OnChanged?.Invoke();
    }

    public void ResetAll()
    {
        souls = 0;
        xp = 0;

        OnSoulsChanged?.Invoke(souls);
        OnXPChanged?.Invoke(xp);
        OnChanged?.Invoke();
    }

    [ContextMenu("Debug Add 100 Souls")]
    private void DebugAdd100Souls()
    {
        AddSouls(100);
    }

    [ContextMenu("Debug Add 500 Souls")]
    private void DebugAdd500Souls()
    {
        AddSouls(500);
    }
}