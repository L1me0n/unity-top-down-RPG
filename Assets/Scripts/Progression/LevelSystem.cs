using System;
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunCurrency currency;

    [Header("Level Tuning")]
    [SerializeField] private int startLevel = 1;

    // XP curve: XP required to go from level L -> L+1
    [SerializeField] private int baseXPToNext = 10; // level 1->2
    [SerializeField] private int stepXPPerLevel = 7; // added each level

    [Header("Debug")]
    [SerializeField] private bool logLevelUps = true;

    public int Level { get; private set; }
    public int UnspentPoints { get; private set; }

    // XP progress within current level
    public int ProgressXP { get; private set; }
    public int XPToNext { get; private set; }

    // Called whenever Level changes
    public event Action<int> OnLevelChanged;        // new level
    public event Action<int> OnUnspentPointsChanged; // new unspent points
    public event Action<int, int> OnProgressChanged; // progressXP, xpToNext

    private void Awake()
    {
        if (currency == null)
            currency = GetComponent<RunCurrency>();

        if (currency == null)
            currency = FindFirstObjectByType<RunCurrency>();

        Level = Mathf.Max(1, startLevel);
        UnspentPoints = 0;

        suppressEvents = true;
        RecomputeFromTotalXP(currency != null ? currency.XP : 0, allowLevelUps: false);
        suppressEvents = false;
    }

    private void OnEnable()
    {
        if (currency != null)
            currency.OnXPChanged += HandleXPChanged;
    }

    private void OnDisable()
    {
        if (currency != null)
            currency.OnXPChanged -= HandleXPChanged;
    }

    private void HandleXPChanged(int newTotalXP)
    {
        RecomputeFromTotalXP(newTotalXP, allowLevelUps: !isLoading);
    }

    // Core logic
    private bool suppressEvents;

    private void RecomputeFromTotalXP(int totalXP, bool allowLevelUps)
    {
        totalXP = Mathf.Max(0, totalXP);

        Level = Mathf.Max(1, Level);

        int requiredToReachCurrentLevel = GetTotalXPRequiredToReachLevel(Level);
        int rawProgress = totalXP - requiredToReachCurrentLevel;
        ProgressXP = Mathf.Max(0, rawProgress);

        XPToNext = GetXPToNext(Level);

        bool leveledUp = false;

        if (allowLevelUps)
        {
            while (ProgressXP >= XPToNext)
            {
                ProgressXP -= XPToNext;
                Level++;
                UnspentPoints++;

                XPToNext = GetXPToNext(Level);
                leveledUp = true;

                if (logLevelUps)
                    Debug.Log($"[LevelSystem] Level up! -> Lv {Level}. Unspent points: {UnspentPoints}");
            }
        }
        else
        {
            // Load-time: never level up automatically, just clamp progress into this level
            // (so saved Level stays authoritative)
            ProgressXP = Mathf.Min(ProgressXP, XPToNext - 1);
        }

        if (!suppressEvents)
        {
            if (leveledUp)
            {
                OnLevelChanged?.Invoke(Level);                // only real mid-session levelups
                OnUnspentPointsChanged?.Invoke(UnspentPoints);
            }

            OnProgressChanged?.Invoke(ProgressXP, XPToNext); // HUD always needs this
        }
    }

    // XP needed for next level at the given current level.
    private int GetXPToNext(int currentLevel)
    {
        currentLevel = Mathf.Max(1, currentLevel);
        int xp = baseXPToNext + (currentLevel - 1) * stepXPPerLevel;
        return Mathf.Max(1, xp);
    }

    // Total XP required to reach a certain level (start of that level).
    private int GetTotalXPRequiredToReachLevel(int targetLevel)
    {
        targetLevel = Mathf.Max(1, targetLevel);

        int total = 0;
        for (int lvl = 1; lvl < targetLevel; lvl++)
            total += GetXPToNext(lvl);

        return total;
    }

    // Public API
    public bool TrySpendPoint()
    {
        if (UnspentPoints <= 0) return false;

        UnspentPoints--;
        OnUnspentPointsChanged?.Invoke(UnspentPoints);
        return true;
    }

    public void LoadState(int level, int unspentPoints)
    {
        Level = Mathf.Max(1, level);
        UnspentPoints = Mathf.Max(0, unspentPoints);

        RecomputeFromTotalXP(currency != null ? currency.XP : 0, allowLevelUps: false);

        // Explicitly update HUD without triggering level-up popup
        OnProgressChanged?.Invoke(ProgressXP, XPToNext);
        OnUnspentPointsChanged?.Invoke(UnspentPoints);
        // DO NOT invoke OnLevelChanged here.
    }

    //loading gate to fix level up sequence on load
    private bool isLoading;

    public void BeginLoad()
    {
        isLoading = true;
        suppressEvents = true;
    }

    public void EndLoad()
    {
        suppressEvents = false;
        isLoading = false;

        // push HUD refresh after load
        OnProgressChanged?.Invoke(ProgressXP, XPToNext);
        OnUnspentPointsChanged?.Invoke(UnspentPoints);
    }
}