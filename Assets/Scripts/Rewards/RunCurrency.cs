using System;
using UnityEngine;

public class RunCurrency : MonoBehaviour
{
    public event Action OnChanged;

    [SerializeField] private int souls;
    [SerializeField] private int xp;

    public int Souls => souls;
    public int XP => xp;

    public void AddSouls(int amount)
    {
        if (amount <= 0) return;
        souls += amount;
        OnChanged?.Invoke();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        xp += amount;
        OnChanged?.Invoke();
    }

    public void TakeSouls(float amount)
    {
        if (amount <= 0) return;
        souls = Mathf.FloorToInt(souls - souls*amount);
        OnChanged?.Invoke();
    }

    public void TakeXP(float amount)
    {
        if (amount <= 0) return;
        xp = Mathf.FloorToInt(xp - xp*amount);
        OnChanged?.Invoke();
    }

    public void ResetAll()
    {
        souls = 0;
        xp = 0;
        OnChanged?.Invoke();
    }
}