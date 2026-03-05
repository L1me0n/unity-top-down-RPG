using System;
using UnityEngine;

public class BranchProgression : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelSystem levelSystem;

    [Header("Rules")]
    [SerializeField] private int maxPointsPerBranch = 30;

    [Header("Current Points")]
    [SerializeField] private int demon;
    [SerializeField] private int monster;
    [SerializeField] private int fallenGod;
    [SerializeField] private int hellhound;

    public int MaxPointsPerBranch => maxPointsPerBranch;

    public int Demon => demon;
    public int Monster => monster;
    public int FallenGod => fallenGod;
    public int Hellhound => hellhound;

    // Fires whenever any branch value changes (branch, newValue)
    public event Action<BranchType, int> OnBranchChanged;

    // Fires when a spend attempt fails (for UI message)
    public event Action<string> OnSpendFailed;

    private void Awake()
    {
        if (levelSystem == null)
            levelSystem = GetComponent<LevelSystem>();

        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        maxPointsPerBranch = Mathf.Max(1, maxPointsPerBranch);
        ClampAll();
    }

    // Public API
    public int GetPoints(BranchType branch)
    {
        return branch switch
        {
            BranchType.Demon => demon,
            BranchType.Monster => monster,
            BranchType.FallenGod => fallenGod,
            BranchType.Hellhound => hellhound,
            _ => 0
        };
    }

    public bool IsCapped(BranchType branch)
    {
        return GetPoints(branch) >= maxPointsPerBranch;
    }

    // Spend 1 unspent point into a branch if possible.
    // Returns true if spent successfully.
    public bool TrySpendPoint(BranchType branch)
    {
        if (levelSystem == null)
        {
            OnSpendFailed?.Invoke("No LevelSystem reference.");
            return false;
        }

        if (levelSystem.UnspentPoints <= 0)
        {
            OnSpendFailed?.Invoke("No unspent points.");
            return false;
        }

        if (IsCapped(branch))
        {
            OnSpendFailed?.Invoke($"{branch} is capped.");
            return false;
        }

        // 1) Spend from LevelSystem first (single source of truth)
        if (!levelSystem.TrySpendPoint())
        {
            OnSpendFailed?.Invoke("Could not spend point.");
            return false;
        }

        // 2) Apply to branch
        IncrementBranch(branch, 1);

        return true;
    }

    // Useful for debugging
    public void AddBranchPoints(BranchType branch, int amount)
    {
        if (amount == 0) return;
        IncrementBranch(branch, amount);
    }


    private void IncrementBranch(BranchType branch, int delta)
    {
        int newValue;

        switch (branch)
        {
            case BranchType.Demon:
                demon = Mathf.Clamp(demon + delta, 0, maxPointsPerBranch);
                newValue = demon;
                break;

            case BranchType.Monster:
                monster = Mathf.Clamp(monster + delta, 0, maxPointsPerBranch);
                newValue = monster;
                break;

            case BranchType.FallenGod:
                fallenGod = Mathf.Clamp(fallenGod + delta, 0, maxPointsPerBranch);
                newValue = fallenGod;
                break;

            case BranchType.Hellhound:
                hellhound = Mathf.Clamp(hellhound + delta, 0, maxPointsPerBranch);
                newValue = hellhound;
                break;

            default:
                return;
        }

        OnBranchChanged?.Invoke(branch, newValue);
    }

    private void ClampAll()
    {
        demon = Mathf.Clamp(demon, 0, maxPointsPerBranch);
        monster = Mathf.Clamp(monster, 0, maxPointsPerBranch);
        fallenGod = Mathf.Clamp(fallenGod, 0, maxPointsPerBranch);
        hellhound = Mathf.Clamp(hellhound, 0, maxPointsPerBranch);
    }

    public void LoadState(int demonPts, int monsterPts, int fallenPts, int hellhoundPts)
    {
        demon = Mathf.Clamp(demonPts, 0, maxPointsPerBranch);
        monster = Mathf.Clamp(monsterPts, 0, maxPointsPerBranch);
        fallenGod = Mathf.Clamp(fallenPts, 0, maxPointsPerBranch);
        hellhound = Mathf.Clamp(hellhoundPts, 0, maxPointsPerBranch);

        OnBranchChanged?.Invoke(BranchType.Demon, demon);
        OnBranchChanged?.Invoke(BranchType.Monster, monster);
        OnBranchChanged?.Invoke(BranchType.FallenGod, fallenGod);
        OnBranchChanged?.Invoke(BranchType.Hellhound, hellhound);
    }
}