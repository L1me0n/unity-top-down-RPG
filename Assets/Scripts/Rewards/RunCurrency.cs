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

    public int TakeSouls(float amount)
    {
        if (amount <= 0.0f) return 0;
        int lost = Mathf.FloorToInt(souls * amount);
        souls = Mathf.Max(0, souls - lost);
        OnSoulsChanged?.Invoke(souls);
        OnChanged?.Invoke();
        return lost;
    }

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
}