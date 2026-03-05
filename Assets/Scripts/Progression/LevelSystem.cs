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

    private bool isLoaded = true;

    private void Awake()
    {
        if (currency == null)
            currency = GetComponent<RunCurrency>();

        if (currency == null)
            currency = FindFirstObjectByType<RunCurrency>();

        Level = Mathf.Max(1, startLevel);
        UnspentPoints = 0;

        RecomputeFromTotalXP(currency != null ? currency.XP : 0);
    }

    private void Start()
    {
        // We delay apply until Start to ensure that any Start-based initialization in PlayerStats or APRegen happens first, so we don't accidentally overwrite changes from them.
        RecomputeFromTotalXP(currency != null ? currency.XP : 0);

        isLoaded = false; // any changes after this point are not part of initial load, so we can log them and fire events for them.
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
        RecomputeFromTotalXP(newTotalXP);
    }

    // Core logic
    private void RecomputeFromTotalXP(int totalXP)
    {
        totalXP = Mathf.Max(0, totalXP);

        // 1) Ensure our Level is valid
        Level = Mathf.Max(1, Level);

        // 2) Figure out how much XP is "already required" to reach our current level.
        int requiredToReachCurrentLevel = GetTotalXPRequiredToReachLevel(Level);

        // If totalXP is lower than required, progress becomes 0. (No de-leveling.)
        int rawProgress = totalXP - requiredToReachCurrentLevel;
        ProgressXP = Mathf.Max(0, rawProgress);

        // 3) Determine current XPToNext based on our Level
        XPToNext = GetXPToNext(Level);

        // 4) If we have enough progress to level up, level up repeatedly.
        // (This is for big XP gains that skip multiple levels.)
        bool leveledUp = false;
        while (ProgressXP >= XPToNext)
        {
            ProgressXP -= XPToNext;
            Level++;
            UnspentPoints++;

            XPToNext = GetXPToNext(Level);
            if (isLoaded) continue; // don't log or fire events for level-ups during initial load, to avoid spam and potential issues with event order during load.
            leveledUp = true;

            if (logLevelUps)
                Debug.Log($"[LevelSystem] Level up! -> Lv {Level}. Unspent points: {UnspentPoints}");
        }

        // 5) Fire events for UI or other systems
        if (leveledUp)
        {
            OnLevelChanged?.Invoke(Level);
            OnUnspentPointsChanged?.Invoke(UnspentPoints);
        }

        OnProgressChanged?.Invoke(ProgressXP, XPToNext);
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

        isLoaded = true;

        // Recompute progress from current currency XP (if currency exists)
        if (currency != null)
            RecomputeFromTotalXP(currency.XP);

        OnProgressChanged?.Invoke(ProgressXP, XPToNext);
    }
}