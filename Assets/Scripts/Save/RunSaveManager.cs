using System.IO;
using UnityEngine;

public class RunSaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunCurrency currency;
    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private BranchProgression branches;
    [SerializeField] private DeathPenaltyTracker penalties;

    [Header("Options")]
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool autoSaveOnChanges = true;

    private string SavePath => Path.Combine(Application.persistentDataPath, "run_save.json");

    private void Awake()
    {
        if (currency == null) currency = FindFirstObjectByType<RunCurrency>();
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
        if (branches == null) branches = FindFirstObjectByType<BranchProgression>();
        if (penalties == null) penalties = FindFirstObjectByType<DeathPenaltyTracker>();
    }

    private void Start()
    {
        if (autoLoadOnStart)
            Load();

        if (autoSaveOnChanges)
            Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        // Currency: your RunCurrency might have OnChanged, or OnSoulsChanged/OnXPChanged depending on your current version.
        // If you have OnChanged, hook it. If not, hook both OnSoulsChanged & OnXPChanged.
        if (currency != null)
        {
            // If your RunCurrency has: public System.Action OnChanged;
            currency.OnChanged += Save;
        }

        if (levelSystem != null)
        {
            levelSystem.OnLevelChanged += _ => Save();
            levelSystem.OnUnspentPointsChanged += _ => Save();
            levelSystem.OnProgressChanged += (_, __) => Save();
        }

        if (branches != null)
            branches.OnBranchChanged += (_, __) => Save();
    }

    private void Unsubscribe()
    {
        if (currency != null)
            currency.OnChanged -= Save;

        if (levelSystem != null)
        {
            levelSystem.OnLevelChanged -= _ => Save();
            levelSystem.OnUnspentPointsChanged -= _ => Save();
            levelSystem.OnProgressChanged -= (_, __) => Save();
        }

        if (branches != null)
            branches.OnBranchChanged -= (_, __) => Save();
    }

    public void Save()
    {
        if (currency == null || levelSystem == null || branches == null) return;

        var data = new RunSaveData
        {
            souls = currency.Souls,
            xp = currency.XP,

            level = levelSystem.Level,
            unspentPoints = levelSystem.UnspentPoints,

            demon = branches.Demon,
            monster = branches.Monster,
            fallenGod = branches.FallenGod,
            hellhound = branches.Hellhound,

            maxHPLoss = penalties != null ? penalties.MaxHPLoss : 0,
            maxAPLoss = penalties != null ? penalties.MaxAPLoss : 0,
            dpLoss = penalties != null ? penalties.DPLoss : 0,
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        // Debug.Log($"[RunSaveManager] Saved -> {SavePath}");
    }

    public void Load()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<RunSaveData>(json);
        if (data == null) return;

        // Currency
        currency.SetSouls(data.souls);
        currency.SetXP(data.xp);

        // LevelSystem
        levelSystem.LoadState(data.level, data.unspentPoints);

        // Branches
        branches.LoadState(data.demon, data.monster, data.fallenGod, data.hellhound);

        // Penalties
        if (penalties != null)
        {
            penalties.LoadState(data.maxHPLoss, data.maxAPLoss, data.dpLoss);
        }

         Debug.Log("[RunSaveManager] Loaded run save.");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}